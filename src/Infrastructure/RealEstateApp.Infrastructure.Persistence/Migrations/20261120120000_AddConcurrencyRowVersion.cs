using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercentage",
                table: "SubscriptionPlans",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Offers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "AgentReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClienteId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentReviews_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyAbilityEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MonthlyGrossIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyNetIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyDebtPayments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableDownPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMortgageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxPropertyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMonthlyPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebtToIncomeRatio = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    CreditScoreRating = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstimatedAnnualRate = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    RecommendedTermYears = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EvaluationResult = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyAbilityEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Commissions_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadPipelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeadEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LeadPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Nuevo Lead"),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    LastContactDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadPipelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadPipelines_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyValuations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    EstimatedPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComparableCount = table.Column<int>(type: "int", nullable: false),
                    MinPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StandardDeviation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SearchRadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValuations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyValuations_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_AgentId",
                table: "AgentReviews",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_ClienteId_AgentId_PropertyId",
                table: "AgentReviews",
                columns: new[] { "ClienteId", "AgentId", "PropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_PropertyId",
                table: "AgentReviews",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyAbilityEvaluations_ClientId",
                table: "BuyAbilityEvaluations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_AgentId",
                table: "Commissions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_OfferId",
                table: "Commissions",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_PropertyId",
                table: "Commissions",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_Status",
                table: "Commissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPipelines_AgentId",
                table: "LeadPipelines",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPipelines_PropertyId",
                table: "LeadPipelines",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadPipelines_Stage",
                table: "LeadPipelines",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_PropertyId",
                table: "PropertyDocuments",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValuations_PropertyId",
                table: "PropertyValuations",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentReviews");

            migrationBuilder.DropTable(
                name: "BuyAbilityEvaluations");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "LeadPipelines");

            migrationBuilder.DropTable(
                name: "PropertyDocuments");

            migrationBuilder.DropTable(
                name: "PropertyValuations");

            migrationBuilder.DropColumn(
                name: "CommissionPercentage",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Offers");
        }
    }
}
