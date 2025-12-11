using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetCore.Migrations.Library_Db
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultLibraryGuid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultShelfGuid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RelatedVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _integrityVersion = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HasImage = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Libraries_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_OwnerDbModelLibrary_OwnerDbModel",
                columns: table => new
                {
                    FollowersId = table.Column<int>(type: "int", nullable: false),
                    FollowingsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_OwnerDbModelLibrary_OwnerDbModel", x => new { x.FollowersId, x.FollowingsId });
                    table.ForeignKey(
                        name: "FK_Library_OwnerDbModelLibrary_OwnerDbModel_Owners_FollowersId",
                        column: x => x.FollowersId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_OwnerDbModelLibrary_OwnerDbModel_Owners_FollowingsId",
                        column: x => x.FollowingsId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shelves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _integrityVersion = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HasImage = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shelves_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedVersionsId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _integrityVersion = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HasImage = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_RelatedVersions_RelatedVersionsId",
                        column: x => x.RelatedVersionsId,
                        principalTable: "RelatedVersions",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_LibraryDbModelLibrary_OwnerDbModel",
                columns: table => new
                {
                    FavoriteLibrariesId = table.Column<int>(type: "int", nullable: false),
                    InFavorOfId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_LibraryDbModelLibrary_OwnerDbModel", x => new { x.FavoriteLibrariesId, x.InFavorOfId });
                    table.ForeignKey(
                        name: "FK_Library_LibraryDbModelLibrary_OwnerDbModel_Libraries_Favorit~",
                        column: x => x.FavoriteLibrariesId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_LibraryDbModelLibrary_OwnerDbModel_Owners_InFavorOfId",
                        column: x => x.InFavorOfId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_LibraryDbModelLibrary_ShelfDbModel",
                columns: table => new
                {
                    ParentLibrariesId = table.Column<int>(type: "int", nullable: false),
                    ShelvesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_LibraryDbModelLibrary_ShelfDbModel", x => new { x.ParentLibrariesId, x.ShelvesId });
                    table.ForeignKey(
                        name: "FK_Library_LibraryDbModelLibrary_ShelfDbModel_Libraries_ParentL~",
                        column: x => x.ParentLibrariesId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_LibraryDbModelLibrary_ShelfDbModel_Shelves_ShelvesId",
                        column: x => x.ShelvesId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_OwnerDbModelLibrary_ShelfDbModel",
                columns: table => new
                {
                    FavoriteShelvesId = table.Column<int>(type: "int", nullable: false),
                    InFavorOfId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_OwnerDbModelLibrary_ShelfDbModel", x => new { x.FavoriteShelvesId, x.InFavorOfId });
                    table.ForeignKey(
                        name: "FK_Library_OwnerDbModelLibrary_ShelfDbModel_Owners_InFavorOfId",
                        column: x => x.InFavorOfId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_OwnerDbModelLibrary_ShelfDbModel_Shelves_FavoriteShe~",
                        column: x => x.FavoriteShelvesId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Elements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ParentDocumentId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elements_Documents_ParentDocumentId",
                        column: x => x.ParentDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Elements_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_DocumentDbModelLibrary_OwnerDbModel",
                columns: table => new
                {
                    FavoriteDocumentsId = table.Column<int>(type: "int", nullable: false),
                    InFavorOfId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_DocumentDbModelLibrary_OwnerDbModel", x => new { x.FavoriteDocumentsId, x.InFavorOfId });
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_OwnerDbModel_Documents_Favori~",
                        column: x => x.FavoriteDocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_OwnerDbModel_Owners_InFavorOf~",
                        column: x => x.InFavorOfId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_DocumentDbModelLibrary_ShelfDbModel",
                columns: table => new
                {
                    DocumentsId = table.Column<int>(type: "int", nullable: false),
                    ParentShelvesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_DocumentDbModelLibrary_ShelfDbModel", x => new { x.DocumentsId, x.ParentShelvesId });
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_ShelfDbModel_Documents_Docume~",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_ShelfDbModel_Shelves_ParentSh~",
                        column: x => x.ParentShelvesId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Library_DocumentDbModelLibrary_TagDbModel",
                columns: table => new
                {
                    DocumentsId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library_DocumentDbModelLibrary_TagDbModel", x => new { x.DocumentsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_TagDbModel_Documents_Document~",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Library_DocumentDbModelLibrary_TagDbModel_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Guid",
                table: "Documents",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OwnerId",
                table: "Documents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_RelatedVersionsId",
                table: "Documents",
                column: "RelatedVersionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Elements_Guid",
                table: "Elements",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elements_OwnerId",
                table: "Elements",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Elements_ParentDocumentId",
                table: "Elements",
                column: "ParentDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_Guid",
                table: "Libraries",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_OwnerId",
                table: "Libraries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_DocumentDbModelLibrary_OwnerDbModel_InFavorOfId",
                table: "Library_DocumentDbModelLibrary_OwnerDbModel",
                column: "InFavorOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_DocumentDbModelLibrary_ShelfDbModel_ParentShelvesId",
                table: "Library_DocumentDbModelLibrary_ShelfDbModel",
                column: "ParentShelvesId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_DocumentDbModelLibrary_TagDbModel_TagsId",
                table: "Library_DocumentDbModelLibrary_TagDbModel",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_LibraryDbModelLibrary_OwnerDbModel_InFavorOfId",
                table: "Library_LibraryDbModelLibrary_OwnerDbModel",
                column: "InFavorOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_LibraryDbModelLibrary_ShelfDbModel_ShelvesId",
                table: "Library_LibraryDbModelLibrary_ShelfDbModel",
                column: "ShelvesId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_OwnerDbModelLibrary_OwnerDbModel_FollowingsId",
                table: "Library_OwnerDbModelLibrary_OwnerDbModel",
                column: "FollowingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_OwnerDbModelLibrary_ShelfDbModel_InFavorOfId",
                table: "Library_OwnerDbModelLibrary_ShelfDbModel",
                column: "InFavorOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_Guid",
                table: "Owners",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedVersions_Guid",
                table: "RelatedVersions",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_Guid",
                table: "Shelves",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_OwnerId",
                table: "Shelves",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Elements");

            migrationBuilder.DropTable(
                name: "Library_DocumentDbModelLibrary_OwnerDbModel");

            migrationBuilder.DropTable(
                name: "Library_DocumentDbModelLibrary_ShelfDbModel");

            migrationBuilder.DropTable(
                name: "Library_DocumentDbModelLibrary_TagDbModel");

            migrationBuilder.DropTable(
                name: "Library_LibraryDbModelLibrary_OwnerDbModel");

            migrationBuilder.DropTable(
                name: "Library_LibraryDbModelLibrary_ShelfDbModel");

            migrationBuilder.DropTable(
                name: "Library_OwnerDbModelLibrary_OwnerDbModel");

            migrationBuilder.DropTable(
                name: "Library_OwnerDbModelLibrary_ShelfDbModel");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropTable(
                name: "Shelves");

            migrationBuilder.DropTable(
                name: "RelatedVersions");

            migrationBuilder.DropTable(
                name: "Owners");
        }
    }
}
