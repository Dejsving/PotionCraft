using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsSelectedWithSelectedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "PlayerCharacters");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedBy",
                table: "PlayerCharacters",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedBy",
                table: "PlayerCharacters");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
