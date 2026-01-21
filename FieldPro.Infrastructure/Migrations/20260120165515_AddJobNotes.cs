using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Jobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Jobs");
        }
    }
}
