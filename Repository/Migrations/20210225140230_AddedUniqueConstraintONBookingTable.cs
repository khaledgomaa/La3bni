using Microsoft.EntityFrameworkCore.Migrations;

namespace Repository.Migrations
{
    public partial class AddedUniqueConstraintONBookingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_PlaygroundId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PlaygroundId_BookedDate_PlaygroundTimesId",
                table: "Bookings",
                columns: new[] { "PlaygroundId", "BookedDate", "PlaygroundTimesId" },
                unique: true,
                filter: "[PlaygroundTimesId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_PlaygroundId_BookedDate_PlaygroundTimesId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PlaygroundId",
                table: "Bookings",
                column: "PlaygroundId");
        }
    }
}
