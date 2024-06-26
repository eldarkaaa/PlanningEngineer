using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningEngineerApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeviationFor4Walls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WallDeviation",
                table: "Measurements",
                newName: "Wall4Deviation");

            migrationBuilder.AddColumn<double>(
                name: "Wall1Deviation",
                table: "Measurements",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Wall2Deviation",
                table: "Measurements",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Wall3Deviation",
                table: "Measurements",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Wall1Deviation",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Wall2Deviation",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Wall3Deviation",
                table: "Measurements");

            migrationBuilder.RenameColumn(
                name: "Wall4Deviation",
                table: "Measurements",
                newName: "WallDeviation");
        }
    }
}
