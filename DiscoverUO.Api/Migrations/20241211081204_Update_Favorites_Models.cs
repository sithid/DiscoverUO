using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscoverUO.Api.Migrations
{
    /// <inheritdoc />
    public partial class Update_Favorites_Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "UserFavoritesListItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "UserFavoritesListItems");
        }
    }
}
