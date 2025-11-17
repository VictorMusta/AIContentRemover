using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIContentRemover.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TweetTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TweetId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AiVotes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    NotAiVotes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TweetTagId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserIdentifier = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsAiVote = table.Column<bool>(type: "INTEGER", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_TweetTags_TweetTagId",
                        column: x => x.TweetTagId,
                        principalTable: "TweetTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetTags_TweetId",
                table: "TweetTags",
                column: "TweetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TweetTags_UpdatedAt",
                table: "TweetTags",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TweetTags_Votes",
                table: "TweetTags",
                columns: new[] { "AiVotes", "NotAiVotes" });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_IpAddress",
                table: "Votes",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_TweetTag_User",
                table: "Votes",
                columns: new[] { "TweetTagId", "UserIdentifier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "TweetTags");
        }
    }
}
