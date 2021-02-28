using Microsoft.EntityFrameworkCore.Migrations;

namespace Repository.Migrations
{
    public partial class AddedOverViewColumnInPlayground : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "gender",
                table: "AspNetUsers",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "AspNetUsers",
                newName: "City");

            migrationBuilder.AddColumn<string>(
                name: "OverView",
                table: "Playgrounds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverView",
                table: "Playgrounds");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "AspNetUsers",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "AspNetUsers",
                newName: "city");
        }
    }
}
