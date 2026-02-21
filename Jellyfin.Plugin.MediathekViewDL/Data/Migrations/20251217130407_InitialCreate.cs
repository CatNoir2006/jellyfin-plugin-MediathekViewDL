using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jellyfin.Plugin.MediathekViewDL.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    VideoUrlHash = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadPath = table.Column<string>(type: "TEXT", nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityCacheEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrlHash = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityCacheEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadHistory_SubscriptionId",
                table: "DownloadHistory",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadHistory_VideoUrlHash",
                table: "DownloadHistory",
                column: "VideoUrlHash");

            migrationBuilder.CreateIndex(
                name: "IX_QualityCacheEntries_UrlHash",
                table: "QualityCacheEntries",
                column: "UrlHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadHistory");

            migrationBuilder.DropTable(
                name: "QualityCacheEntries");
        }
    }
}
