using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickChart.API.Migrations
{
    /// <inheritdoc />
    public partial class UserTable_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "District",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Upazila",
                table: "AspNetUsers",
                newName: "ProfileImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "AspNetUsers",
                newName: "CoverImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "AspNetUsers",
                newName: "Upazila");

            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                table: "AspNetUsers",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
