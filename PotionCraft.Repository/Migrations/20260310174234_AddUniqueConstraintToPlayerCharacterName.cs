using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToPlayerCharacterName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalRule",
                table: "Herbs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Herbs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Habitats",
                table: "Herbs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCharacters_Name",
                table: "PlayerCharacters",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerCharacters_Name",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "AdditionalRule",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "Habitats",
                table: "Herbs");
        }
    }
}
