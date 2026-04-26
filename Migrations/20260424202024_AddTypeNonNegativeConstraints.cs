using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carzi.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeNonNegativeConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_VignetteTypes_Price_NonNegative",
                table: "VignetteTypes",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VignetteTypes_ValidityDays_Positive",
                table: "VignetteTypes",
                sql: "\"ValidityDays\" >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FuelTypes_PricePerLiter_NonNegative",
                table: "FuelTypes",
                sql: "\"PricePerLiter\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AnnualInspectionTypes_Price_NonNegative",
                table: "AnnualInspectionTypes",
                sql: "\"Price\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VignetteTypes_Price_NonNegative",
                table: "VignetteTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VignetteTypes_ValidityDays_Positive",
                table: "VignetteTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FuelTypes_PricePerLiter_NonNegative",
                table: "FuelTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AnnualInspectionTypes_Price_NonNegative",
                table: "AnnualInspectionTypes");
        }
    }
}
