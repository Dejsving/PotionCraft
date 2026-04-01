using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterBag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bag_Herbs",
                table: "PlayerCharacters",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Bag_Id",
                table: "PlayerCharacters",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Bag_Poisons",
                table: "PlayerCharacters",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bag_Potions",
                table: "PlayerCharacters",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bag_Herbs",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "Bag_Id",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "Bag_Poisons",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "Bag_Potions",
                table: "PlayerCharacters");
        }
    }
}
