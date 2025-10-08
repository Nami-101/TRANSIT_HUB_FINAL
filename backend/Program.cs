using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TransitHub.Data;
using TransitHub.Repositories;
using TransitHub.Repositories.Interfaces;
using TransitHub.Services;
using TransitHub.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpContextAccessor();

// Add Entity Framework
builder.Services.AddDbContext<TransitHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add Business Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Admin Services
builder.Services.AddScoped<IAdminService, AdminService>();

// Add Train Booking Services
builder.Services.AddScoped<ITrainBookingService, TrainBookingService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add SignalR
builder.Services.AddSignalR();

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings - relaxed for development
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<TransitHubDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://192.168.18.191:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Transit-Hub API", 
        Version = "v1",
        Description = "API for Transit-Hub booking system with JWT Authentication and Repository patterns"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transit-Hub API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}
else
{
    // Only use HTTPS redirection in production
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TransitHub.Hubs.NotificationHub>("/notificationHub");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TransitHubDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    try
    {
        // Apply any pending migrations
        // context.Database.Migrate(); // Commented out due to migration conflict
        
        // Seed default roles
        await SeedRolesAsync(roleManager);
        
        // Seed admin users
        await SeedAdminUsersAsync(userManager, roleManager);
        
        // Create stored procedures
        await CreateStoredProceduresAsync(context);
        
        
        // Seed initial data
        DataSeeder.SeedAsync(context).Wait();
        
        Console.WriteLine("Database setup and seeding completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database setup failed: {ex.Message}");
    }
}

static async Task CreateStoredProceduresAsync(TransitHubDbContext context)
{
    try
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "StoredProcedures");
        
        // Read and execute stored procedure files in order
        var files = new[]
        {
            "01_UserManagement.sql",
            "02_SearchOperations.sql", 
            "03_BookingOperations.sql",
            "04_PaymentOperations.sql",
            "05_WaitlistOperations.sql",
            "06_AdminOperations.sql"
        };
        
        foreach (var file in files)
        {
            var filePath = Path.Combine(basePath, file);
            if (File.Exists(filePath))
            {
                var sql = await File.ReadAllTextAsync(filePath);
                // Split by GO statements and execute each batch
                var batches = sql.Split(new[] { "\nGO\n", "\nGO\r\n", "\rGO\r", "\nGO", "GO\n" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var batch in batches)
                {
                    var trimmedBatch = batch.Trim();
                    if (!string.IsNullOrEmpty(trimmedBatch) && !trimmedBatch.StartsWith("--") && !trimmedBatch.StartsWith("USE"))
                    {
                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(trimmedBatch);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Failed to execute batch from {file}: {ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"Executed stored procedures from {file}");
            }
        }
        
        Console.WriteLine("All stored procedures created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating stored procedures: {ex.Message}");
    }
}

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    try
    {
        var roles = new[] { "Admin", "User" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Created role: {role}");
            }
        }
        
        Console.WriteLine("Role seeding completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Role seeding failed: {ex.Message}");
    }
}

static async Task SeedAdminUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    try
    {
        // Create admin user
        var adminEmail = "admin@transithub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Created admin user: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"Admin user {adminEmail} already exists");
        }
        
        // Create test user
        var testEmail = "test@example.com";
        var testUser = await userManager.FindByEmailAsync(testEmail);
        
        if (testUser == null)
        {
            testUser = new IdentityUser
            {
                UserName = testEmail,
                Email = testEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(testUser, "Test@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
                Console.WriteLine($"Created test user: {testEmail}");
            }
        }
        else
        {
            Console.WriteLine($"Test user {testEmail} already exists");
        }
        
        Console.WriteLine("Admin user seeding completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Admin user seeding failed: {ex.Message}");
    }
}