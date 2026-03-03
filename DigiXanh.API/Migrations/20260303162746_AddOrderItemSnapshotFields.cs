using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiXanh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemSnapshotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Plants_TrefleId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "TrefleId",
                table: "Plants");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "OrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScientificName",
                table: "OrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PlantName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ScientificName",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "TrefleId",
                table: "Plants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plants_TrefleId",
                table: "Plants",
                column: "TrefleId",
                filter: "[TrefleId] IS NOT NULL");
        }
    }
}
