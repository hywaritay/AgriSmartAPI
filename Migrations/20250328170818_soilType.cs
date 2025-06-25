using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriSmartAPI.Migrations
{
    /// <inheritdoc />
    public partial class soilType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoilDetails");

            migrationBuilder.CreateTable(
                name: "SoilPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoilType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedCrops = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredictionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilPredictions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoilPredictions");

            migrationBuilder.CreateTable(
                name: "SoilDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Texture = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilDetails", x => x.Id);
                });
        }
    }
}
