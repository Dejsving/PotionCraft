using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddHerbFormulaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DamageType",
                table: "Herbs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormulaDiceCount",
                table: "Herbs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormulaDiceType",
                table: "Herbs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FormulaIncludesAlchemyMod",
                table: "Herbs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplacementDamageTypes",
                table: "Herbs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDmJudgement",
                table: "Herbs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "FormulaDiceCount",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "FormulaDiceType",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "FormulaIncludesAlchemyMod",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "ReplacementDamageTypes",
                table: "Herbs");

            migrationBuilder.DropColumn(
                name: "RequiresDmJudgement",
                table: "Herbs");
        }
    }
}
