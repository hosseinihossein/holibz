using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetCore.Migrations.Review_Db
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SubjectGuid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentReviewId = table.Column<int>(type: "int", nullable: false),
                    WriterId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReplyToId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ReplyToId",
                        column: x => x.ReplyToId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Reviews_ParentReviewId",
                        column: x => x.ParentReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_WriterId",
                        column: x => x.WriterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Review_ReviewDbModelReview_UserDbModel",
                columns: table => new
                {
                    LikesId = table.Column<int>(type: "int", nullable: false),
                    LikesId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review_ReviewDbModelReview_UserDbModel", x => new { x.LikesId, x.LikesId1 });
                    table.ForeignKey(
                        name: "FK_Review_ReviewDbModelReview_UserDbModel_Reviews_LikesId",
                        column: x => x.LikesId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_ReviewDbModelReview_UserDbModel_Users_LikesId1",
                        column: x => x.LikesId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Review_CommentDbModelReview_UserDbModel",
                columns: table => new
                {
                    ThumbsUpsId = table.Column<int>(type: "int", nullable: false),
                    ThumbsUpsId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review_CommentDbModelReview_UserDbModel", x => new { x.ThumbsUpsId, x.ThumbsUpsId1 });
                    table.ForeignKey(
                        name: "FK_Review_CommentDbModelReview_UserDbModel_Comments_ThumbsUpsId1",
                        column: x => x.ThumbsUpsId1,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_CommentDbModelReview_UserDbModel_Users_ThumbsUpsId",
                        column: x => x.ThumbsUpsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Review_CommentDbModelReview_UserDbModel1",
                columns: table => new
                {
                    ThumbsDownsId = table.Column<int>(type: "int", nullable: false),
                    ThumbsDownsId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review_CommentDbModelReview_UserDbModel1", x => new { x.ThumbsDownsId, x.ThumbsDownsId1 });
                    table.ForeignKey(
                        name: "FK_Review_CommentDbModelReview_UserDbModel1_Comments_ThumbsDown~",
                        column: x => x.ThumbsDownsId1,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_CommentDbModelReview_UserDbModel1_Users_ThumbsDownsId",
                        column: x => x.ThumbsDownsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Guid",
                table: "Comments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentReviewId",
                table: "Comments",
                column: "ParentReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyToId",
                table: "Comments",
                column: "ReplyToId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WriterId",
                table: "Comments",
                column: "WriterId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_CommentDbModelReview_UserDbModel_ThumbsUpsId1",
                table: "Review_CommentDbModelReview_UserDbModel",
                column: "ThumbsUpsId1");

            migrationBuilder.CreateIndex(
                name: "IX_Review_CommentDbModelReview_UserDbModel1_ThumbsDownsId1",
                table: "Review_CommentDbModelReview_UserDbModel1",
                column: "ThumbsDownsId1");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ReviewDbModelReview_UserDbModel_LikesId1",
                table: "Review_ReviewDbModelReview_UserDbModel",
                column: "LikesId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OwnerId",
                table: "Reviews",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SubjectGuid",
                table: "Reviews",
                column: "SubjectGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Guid",
                table: "Users",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Review_CommentDbModelReview_UserDbModel");

            migrationBuilder.DropTable(
                name: "Review_CommentDbModelReview_UserDbModel1");

            migrationBuilder.DropTable(
                name: "Review_ReviewDbModelReview_UserDbModel");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
