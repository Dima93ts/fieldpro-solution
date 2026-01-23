using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Technicians",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Technicians");
        }
    }
}
