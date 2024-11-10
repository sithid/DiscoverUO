using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscoverUO.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    DailyVotesRemaining = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerPort = table.Column<int>(type: "int", nullable: false),
                    ServerEra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PvPEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Votes = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servers_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoritesLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoritesLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoritesLists_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserBiography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAvatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoritesListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerPort = table.Column<int>(type: "int", nullable: false),
                    ServerEra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PvPEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FavoritesListId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoritesListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoritesListItems_UserFavoritesLists_FavoritesListId",
                        column: x => x.FavoritesListId,
                        principalTable: "UserFavoritesLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoritesListItems_FavoritesListId",
                table: "UserFavoritesListItems",
                column: "FavoritesListId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoritesLists_OwnerId",
                table: "UserFavoritesLists",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_OwnerId",
                table: "UserProfiles",
                column: "OwnerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "UserFavoritesListItems");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserFavoritesLists");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
