using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thrift.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(85)", maxLength: 85, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Month = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HousingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UtilitiesExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FoodExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HealthcareExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EntertainmentExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsAllocation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebtPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");
        }
    }
}
