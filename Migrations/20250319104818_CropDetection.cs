using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriSmartAPI.Migrations
{
    /// <inheritdoc />
    public partial class CropDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Crops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlantingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CareSchedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HarvestSchedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crops_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PestDiagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentRecommendation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PestDiagnoses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoilDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Texture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoilDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Crops_UserId",
                table: "Crops",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Crops");

            migrationBuilder.DropTable(
                name: "PestDiagnoses");

            migrationBuilder.DropTable(
                name: "SoilDetails");
        }
    }
}
