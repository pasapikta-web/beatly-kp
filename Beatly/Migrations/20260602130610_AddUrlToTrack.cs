using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beatly.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlToTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Tracks");
        }
    }
}
