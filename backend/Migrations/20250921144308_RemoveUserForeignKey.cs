using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitHub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainBookings_Users_UserId",
                table: "TrainBookings");

            migrationBuilder.DropIndex(
                name: "IX_TrainBookings_UserId",
                table: "TrainBookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrainBookings_UserId",
                table: "TrainBookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainBookings_Users_UserId",
                table: "TrainBookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
