using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailConfirmationAttempts",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailConfirmationCodeExpiresAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationCodeHash",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailConfirmationCodeSentAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeSentAt",
                table: "AspNetUsers");
        }
    }
}
