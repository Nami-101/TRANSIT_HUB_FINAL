using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitHub.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_WaitlistQueues",
                table: "WaitlistQueues");

            migrationBuilder.DropIndex(
                name: "IX_WaitlistQueues_FlightScheduleScheduleID",
                table: "WaitlistQueues");

            migrationBuilder.DropColumn(
                name: "FlightScheduleScheduleID",
                table: "WaitlistQueues");

            migrationBuilder.RenameTable(
                name: "WaitlistQueues",
                newName: "WaitlistQueue");

            migrationBuilder.RenameColumn(
                name: "WaitlistId",
                table: "WaitlistQueue",
                newName: "WaitlistID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_TrainScheduleID",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_TrainScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_Position",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_Position");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueues_BookingID",
                table: "WaitlistQueue",
                newName: "IX_WaitlistQueue_BookingID");

            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainClass",
                table: "WaitlistQueue",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

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
                name: "FK_WaitlistQueue_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueue",
                column: "TrainScheduleID",
                principalTable: "TrainSchedules",
                principalColumn: "ScheduleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueue_Bookings_BookingID",
                table: "WaitlistQueue");

            migrationBuilder.DropForeignKey(
                name: "FK_WaitlistQueue_TrainSchedules_TrainScheduleID",
                table: "WaitlistQueue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WaitlistQueue",
                table: "WaitlistQueue");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrainClass",
                table: "WaitlistQueue");

            migrationBuilder.RenameTable(
                name: "WaitlistQueue",
                newName: "WaitlistQueues");

            migrationBuilder.RenameColumn(
                name: "WaitlistID",
                table: "WaitlistQueues",
                newName: "WaitlistId");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_TrainScheduleID",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_TrainScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_Position",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_Position");

            migrationBuilder.RenameIndex(
                name: "IX_WaitlistQueue_BookingID",
                table: "WaitlistQueues",
                newName: "IX_WaitlistQueues_BookingID");

            migrationBuilder.AddColumn<int>(
                name: "FlightScheduleScheduleID",
                table: "WaitlistQueues",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WaitlistQueues",
                table: "WaitlistQueues",
                column: "WaitlistId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistQueues_FlightScheduleScheduleID",
                table: "WaitlistQueues",
                column: "FlightScheduleScheduleID");

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
    }
}
