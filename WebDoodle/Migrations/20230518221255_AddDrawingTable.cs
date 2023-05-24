using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDoodle.Migrations
{
    /// <inheritdoc />
    public partial class AddDrawingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("DrawingTable", bld =>
            new {
                id = bld.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                uid = bld.Column<int>(nullable: false),
                data = bld.Column<string>(nullable: false)
            },
            constraints: (table) => {
                table.PrimaryKey("PK_Drawing", x => x.id);
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DrawingTable");
        }
    }
}
