using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitHub.Migrations
{
    /// <inheritdoc />
    public partial class RestoreBookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationFee",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "Cancellations");

            migrationBuilder.RenameColumn(
                name: "CancelledAt",
                table: "Cancellations",
                newName: "CancellationDate");

            migrationBuilder.RenameColumn(
                name: "CancellationReason",
                table: "Cancellations",
                newName: "RefundNotes");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Cancellations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Cancellations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundProcessedDate",
                table: "Cancellations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundStatus",
                table: "Cancellations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefundTransactionRef",
                table: "Cancellations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Cancellations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Cancellations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainBookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TrainId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TravelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PassengerCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SelectedQuota = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AutoAssignSeats = table.Column<bool>(type: "bit", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainBookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_TrainBookings_TrainSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "TrainSchedules",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainBookings_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "TrainID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainBookingPassengers",
                columns: table => new
                {
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CoachNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SeatPreference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsSeniorCitizen = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainBookingPassengers", x => x.PassengerId);
                    table.ForeignKey(
                        name: "FK_TrainBookingPassengers_TrainBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "TrainBookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookingPassengers_BookingId",
                table: "TrainBookingPassengers",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_ScheduleId",
                table: "TrainBookings",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_TrainId",
                table: "TrainBookings",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_UserId",
                table: "TrainBookings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainBookingPassengers");

            migrationBuilder.DropTable(
                name: "TrainBookings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "RefundProcessedDate",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "RefundStatus",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "RefundTransactionRef",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Cancellations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Cancellations");

            migrationBuilder.RenameColumn(
                name: "RefundNotes",
                table: "Cancellations",
                newName: "CancellationReason");

            migrationBuilder.RenameColumn(
                name: "CancellationDate",
                table: "Cancellations",
                newName: "CancelledAt");

            migrationBuilder.AddColumn<decimal>(
                name: "CancellationFee",
                table: "Cancellations",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "Cancellations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
