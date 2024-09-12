using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace holibz.Migrations.WebComponents_Db
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeveloperGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebComponents_ItemDbModelWebComponents_TagDbModel",
                columns: table => new
                {
                    ItemDbModelsId = table.Column<int>(type: "int", nullable: false),
                    TagDbModelsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebComponents_ItemDbModelWebComponents_TagDbModel", x => new { x.ItemDbModelsId, x.TagDbModelsId });
                    table.ForeignKey(
                        name: "FK_WebComponents_ItemDbModelWebComponents_TagDbModel_Items_ItemDbModelsId",
                        column: x => x.ItemDbModelsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebComponents_ItemDbModelWebComponents_TagDbModel_Tags_TagDbModelsId",
                        column: x => x.TagDbModelsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebComponents_ItemDbModelWebComponents_TagDbModel_TagDbModelsId",
                table: "WebComponents_ItemDbModelWebComponents_TagDbModel",
                column: "TagDbModelsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebComponents_ItemDbModelWebComponents_TagDbModel");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
