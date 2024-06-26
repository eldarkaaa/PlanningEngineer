using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningEngineerApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddedBuildingMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Characteristics",
                table: "Buildings");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Buildings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Material",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Material",
                table: "Buildings");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Characteristics",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
