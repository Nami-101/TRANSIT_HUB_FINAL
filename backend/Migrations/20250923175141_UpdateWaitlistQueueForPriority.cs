using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWaitlistQueueForPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueue_Bookings_BookingID",
                table: "WaitlistQueue");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueue_FlightSchedules_FlightScheduleID",
                table: "WaitlistQueue");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueue_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WaitlistQueue",
                table: "WaitlistQueue");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "WaitlistQueue");

            migrationBuilder.RenameTable(
                name: "WaitlistQueue",
                newName: "WaitlistQueues");

            migrationBuilder.RenameColumn(
                name: "WaitlistID",
                table: "WaitlistQueues",
                newName: "WaitlistId");

            migrationBuilder.RenameColumn(
                name: "QueuePosition",
                table: "WaitlistQueues",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "FlightScheduleID",
                table: "WaitlistQueues",
                newName: "FlightScheduleScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_TrainScheduleID",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_TrainScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_QueuePosition",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_Position");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_FlightScheduleID",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_FlightScheduleScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_BookingID",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_BookingID");

            migrationBuilder.AlterColumn<int>(
                name: "TrainScheduleID",
                table: "WaitlistQueues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "WaitlistQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QueuedAt",
                table: "WaitlistQueues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_WaitlistQueues",
                table: "WaitlistQueues",
                column: "WaitlistId");

            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    CoachID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainID = table.Column<int>(type: "int", nullable: false),
                    CoachNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TrainClassID = table.Column<int>(type: "int", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    BaseFare = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TrainID1 = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.CoachID);
                    table.ForeignKey(
                        name: "FK_Coaches_TrainClasses_TrainClassID",
                        column: x => x.TrainClassID,
                        principalTable: "TrainClasses",
                        principalColumn: "TrainClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Trains_TrainID",
                        column: x => x.TrainID,
                        principalTable: "Trains",
                        principalColumn: "TrainID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coaches_Trains_TrainID1",
                        column: x => x.TrainID1,
                        principalTable: "Trains",
                        principalColumn: "TrainID");
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    SeatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachID = table.Column<int>(type: "int", nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SeatType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsWindowSeat = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.SeatID);
                    table.ForeignKey(
                        name: "FK_Seats_Coaches_CoachID",
                        column: x => x.CoachID,
                        principalTable: "Coaches",
                        principalColumn: "CoachID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_TrainClassID",
                table: "Coaches",
                column: "TrainClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_TrainID_CoachNumber",
                table: "Coaches",
                columns: new[] { "TrainID", "CoachNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_TrainID1",
                table: "Coaches",
                column: "TrainID1");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_CoachID_SeatNumber",
                table: "Seats",
                columns: new[] { "CoachID", "SeatNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueues_Bookings_BookingID",
                table: "WaitlistQueues",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueues_FlightSchedules_FlightScheduleScheduleID",
                table: "WaitlistQueues",
                column: "FlightScheduleScheduleID",
                principalTable: "FlightSchedules",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueues_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueues",
                column: "TrainScheduleID",
                principalTable: "TrainSchedules",
                principalColumn: "ScheduleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueues_Bookings_BookingID",
                table: "WaitlistQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueues_FlightSchedules_FlightScheduleScheduleID",
                table: "WaitlistQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueues_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueues");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WaitlistQueues",
                table: "WaitlistQueues");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "WaitlistQueues");

            migrationBuilder.DropColumn(
                name: "QueuedAt",
                table: "WaitlistQueues");

            migrationBuilder.RenameTable(
                name: "WaitlistQueues",
                newName: "WaitlistQueue");

            migrationBuilder.RenameColumn(
                name: "WaitlistId",
                table: "WaitlistQueue",
                newName: "WaitlistID");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "WaitlistQueue",
                newName: "QueuePosition");

            migrationBuilder.RenameColumn(
                name: "FlightScheduleScheduleID",
                table: "WaitlistQueue",
                newName: "FlightScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_TrainScheduleID",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_TrainScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_Position",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_QueuePosition");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_FlightScheduleScheduleID",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_FlightScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_BookingID",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_BookingID");

            migrationBuilder.AlterColumn<int>(
                name: "TrainScheduleID",
                table: "WaitlistQueue",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleType",
                table: "WaitlistQueue",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WaitlistQueue",
                table: "WaitlistQueue",
                column: "WaitlistID");

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueue_Bookings_BookingID",
                table: "WaitlistQueue",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueue_FlightSchedules_FlightScheduleID",
                table: "WaitlistQueue",
                column: "FlightScheduleID",
                principalTable: "FlightSchedules",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_WaitlistQueue_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueue",
                column: "TrainScheduleID",
                principalTable: "TrainSchedules",
                principalColumn: "ScheduleID");
        }
    }
}
