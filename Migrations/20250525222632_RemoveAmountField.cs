using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrift.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAmountField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Budgets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Budgets",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
