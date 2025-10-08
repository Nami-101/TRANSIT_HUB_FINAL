using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitHub.Models.DTOs;
using TransitHub.Services.Interfaces;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Admin login with role selection endpoint
        /// </summary>
        /// <param name="request">Login credentials with selected role</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login-with-role")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> LoginWithRole([FromBody] LoginWithRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.LoginWithRoleAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// User logout endpoint
        /// </summary>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            var userId = User?.FindFirst("sub")?.Value ?? User?.FindFirst("id")?.Value ?? string.Empty;
            var result = await _authService.LogoutAsync(userId);

            return Ok(result);
        }

        /// <summary>
        /// Refresh JWT token endpoint
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Refresh token is required"));
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Verify email with OTP
        /// </summary>
        /// <param name="request">Email verification request</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse>> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.VerifyEmailAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Resend OTP for email verification
        /// </summary>
        /// <param name="request">Resend OTP request</param>
        /// <returns>Resend result</returns>
        [HttpPost("resend-otp")]
        public async Task<ActionResult<ApiResponse>> ResendOtp([FromBody] ResendOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.ResendOtpAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Forgot password endpoint
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Forgot password result</returns>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
            }

            // Get the frontend URL from the request
            var origin = Request.Headers["Origin"].FirstOrDefault() ?? "http://localhost:4200";
            var result = await _authService.ForgotPasswordAsync(request, origin);
            return Ok(result);
        }

        /// <summary>
        /// Reset password endpoint
        /// </summary>
        /// <param name="request">Reset password request</param>
        /// <returns>Reset password result</returns>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.ResetPasswordAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<UserInfo>> GetCurrentUser()
        {
            try
            {
                var userInfo = new UserInfo
                {
                    Id = User?.FindFirst("sub")?.Value ?? User?.FindFirst("id")?.Value ?? string.Empty,
                    Email = User?.FindFirst("email")?.Value ?? string.Empty,
                    FirstName = User?.FindFirst("given_name")?.Value ?? User?.FindFirst("name")?.Value ?? string.Empty,
                    LastName = User?.FindFirst("family_name")?.Value ?? string.Empty,
                    Roles = User?.FindAll("role")?.Select(c => c.Value).ToList() ?? new List<string>()
                };

                return Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "User information retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user information");
                return StatusCode(500, ApiResponse<UserInfo>.ErrorResult("An error occurred while retrieving user information"));
            }
        }
    }
}