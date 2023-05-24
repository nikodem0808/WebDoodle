using Microsoft.EntityFrameworkCore.Migrations;
using WebDoodle.DataModels;

#nullable disable

namespace WebDoodle.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDataTable",
                columns: table => new
                {
                    uid = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(nullable: false),
                    Passwords = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", userData => userData.uid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserDataTable");
        }
    }
}
