using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotionCraft.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPlayerCharacterTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlchemistTool_Expertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AlchemistTool_Modifier",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AlchemistTool_Proficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HerbalismTool_Expertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HerbalismTool_Modifier",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HerbalismTool_Proficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PoisonerTool_Expertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PoisonerTool_Modifier",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PoisonerTool_Proficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE PlayerCharacters
                SET AlchemistTool_Proficiency = HasAlchemistSuppliesProficiency,
                    AlchemistTool_Expertise = HasAlchemistSuppliesExpertise,
                    HerbalismTool_Proficiency = HasHerbalismKitProficiency,
                    HerbalismTool_Expertise = HasHerbalismKitExpertise,
                    PoisonerTool_Proficiency = HasPoisonerSuppliesProficiency,
                    PoisonerTool_Expertise = HasPoisonerSuppliesExpertise
                """);

            migrationBuilder.DropColumn(
                name: "HasAlchemistSuppliesExpertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HasAlchemistSuppliesProficiency",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HasHerbalismKitExpertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HasHerbalismKitProficiency",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HasPoisonerSuppliesExpertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HasPoisonerSuppliesProficiency",
                table: "PlayerCharacters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAlchemistSuppliesExpertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasAlchemistSuppliesProficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHerbalismKitExpertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHerbalismKitProficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPoisonerSuppliesExpertise",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPoisonerSuppliesProficiency",
                table: "PlayerCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE PlayerCharacters
                SET HasAlchemistSuppliesProficiency = AlchemistTool_Proficiency,
                    HasAlchemistSuppliesExpertise = AlchemistTool_Expertise,
                    HasHerbalismKitProficiency = HerbalismTool_Proficiency,
                    HasHerbalismKitExpertise = HerbalismTool_Expertise,
                    HasPoisonerSuppliesProficiency = PoisonerTool_Proficiency,
                    HasPoisonerSuppliesExpertise = PoisonerTool_Expertise
                """);

            migrationBuilder.DropColumn(
                name: "AlchemistTool_Expertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "AlchemistTool_Modifier",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "AlchemistTool_Proficiency",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HerbalismTool_Expertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HerbalismTool_Modifier",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "HerbalismTool_Proficiency",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "PoisonerTool_Expertise",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "PoisonerTool_Modifier",
                table: "PlayerCharacters");

            migrationBuilder.DropColumn(
                name: "PoisonerTool_Proficiency",
                table: "PlayerCharacters");
        }
    }
}
