using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beatly.Migrations
{
    /// <inheritdoc />
    public partial class SyncSchemaFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEarlyAccess",
                table: "Tracks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LosslessAudioUrl",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FavoriteTracks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEarlyAccess",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "LosslessAudioUrl",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavoriteTracks");
        }
    }
}
