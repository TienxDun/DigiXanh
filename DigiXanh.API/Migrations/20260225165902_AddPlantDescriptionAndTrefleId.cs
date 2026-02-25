using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiXanh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantDescriptionAndTrefleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrefleId",
                table: "Plants",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "TrefleId",
                table: "Plants");
        }
    }
}
