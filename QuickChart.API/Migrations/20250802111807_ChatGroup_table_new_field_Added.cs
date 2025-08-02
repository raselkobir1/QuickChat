using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickChart.API.Migrations
{
    /// <inheritdoc />
    public partial class ChatGroup_table_new_field_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ChatGroups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChatGroups");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ChatGroups");
        }
    }
}
