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
                    Effect = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    Habitats = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalRule = table.Column<string>(type: "TEXT", nullable: false)
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
                    AlchemistTool_Proficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlchemistTool_Expertise = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlchemistTool_Modifier = table.Column<int>(type: "INTEGER", nullable: false),
                    HerbalismTool_Proficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    HerbalismTool_Expertise = table.Column<bool>(type: "INTEGER", nullable: false),
                    HerbalismTool_Modifier = table.Column<int>(type: "INTEGER", nullable: false),
                    PoisonerTool_Proficiency = table.Column<bool>(type: "INTEGER", nullable: false),
                    PoisonerTool_Expertise = table.Column<bool>(type: "INTEGER", nullable: false),
                    PoisonerTool_Modifier = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCharacters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCharacters_Name",
                table: "PlayerCharacters",
                column: "Name",
                unique: true);
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
