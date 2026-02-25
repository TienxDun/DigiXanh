using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiXanh.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlantsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Plants",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "ImageUrl", "IsDeleted", "Name", "Price", "ScientificName" },
                values: new object[,]
                {
                    { 1001, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1459156212016-c812468e2115", false, "Trầu Bà Lá Xẻ", 320000m, "Monstera deliciosa" },
                    { 1002, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1485955900006-10f4d324d411", false, "Lưỡi Hổ", 180000m, "Dracaena trifasciata" },
                    { 1003, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1512428813834-c702c7702b78", false, "Kim Tiền", 250000m, "Zamioculcas zamiifolia" },
                    { 1004, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1501004318641-b39e6451bec6", false, "Cau Tiểu Trâm", 210000m, "Chamaedorea elegans" },
                    { 1005, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1463320726281-696a485928c7", false, "Phát Tài Núi", 290000m, "Dracaena fragrans" },
                    { 1006, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1466692476868-aef1dfb1e735", false, "Sen Đá Nâu", 90000m, "Echeveria affinis" },
                    { 1007, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1463936575829-25148e1db1b8", false, "Xương Rồng Bát Tiên", 120000m, "Euphorbia milii" },
                    { 1008, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1519331379826-f10be5486c6f", false, "Ngọc Ngân", 195000m, "Aglaonema commutatum" },
                    { 1009, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1455656678494-4d1b5f3e7ad5", false, "Bàng Singapore", 410000m, "Ficus lyrata" },
                    { 1010, null, new DateTime(2026, 2, 25, 16, 30, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1490750967868-88aa4486c946", false, "Đa Búp Đỏ", 360000m, "Ficus elastica" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plants",
                keyColumn: "Id",
                keyValues: new object[] { 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010 });
        }
    }
}
