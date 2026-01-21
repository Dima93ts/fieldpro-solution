using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobProjectField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "Jobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Project",
                table: "Jobs");
        }
    }
}
