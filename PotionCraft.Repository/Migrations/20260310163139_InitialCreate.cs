using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Herbs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    HerbType = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    Effect = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herbs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Intelligence = table.Column<int>(type: "INTEGER", nullable: false),
                    Wisdom = table.Column<int>(type: "INTEGER", nullable: false),
                    ProficiencyBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    HasHerbalismKitProficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasHerbalismKitExpertise = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAlchemistSuppliesProficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAlchemistSuppliesExpertise = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPoisonerSuppliesProficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPoisonerSuppliesExpertise = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCharacters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Herbs");

            migrationBuilder.DropTable(
                name: "PlayerCharacters");
        }
    }
}
