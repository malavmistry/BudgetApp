using System;
using BudgetApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetApp.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260822123000_AddRecurringItemTemplates")]
    public partial class AddRecurringItemTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurringItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ItemNameId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringItems_ItemNames_ItemNameId",
                        column: x => x.ItemNameId,
                        principalTable: "ItemNames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "RecurringItemId",
                table: "BudgetItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_RecurringItemId",
                table: "BudgetItems",
                column: "RecurringItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringItems_CategoryId",
                table: "RecurringItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringItems_ItemNameId",
                table: "RecurringItems",
                column: "ItemNameId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringItems_UserId_IsActive",
                table: "RecurringItems",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.Sql(@"
                DECLARE @RecurringSource TABLE
                (
                    BudgetItemId INT NOT NULL PRIMARY KEY,
                    UserId INT NOT NULL,
                    Type INT NOT NULL,
                    ItemNameId INT NOT NULL,
                    CategoryId INT NOT NULL,
                    Amount DECIMAL(18,2) NOT NULL,
                    Note NVARCHAR(500) NULL,
                    DayOfMonth INT NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    UpdatedAt DATETIME2 NOT NULL
                );

                INSERT INTO @RecurringSource
                    (BudgetItemId, UserId, Type, ItemNameId, CategoryId, Amount, Note, DayOfMonth, CreatedAt, UpdatedAt)
                SELECT
                    bi.Id,
                    b.UserId,
                    bi.Type,
                    bi.ItemNameId,
                    bi.CategoryId,
                    bi.Amount,
                    bi.Note,
                    DATEPART(DAY, bi.TransactionDateUtc),
                    bi.CreatedAt,
                    bi.UpdatedAt
                FROM BudgetItems bi
                INNER JOIN Budgets b ON b.Id = bi.BudgetId
                WHERE bi.IsRecurring = 1;

                INSERT INTO RecurringItems (UserId, Type, ItemNameId, CategoryId, Amount, Note, DayOfMonth, IsActive, CreatedAt, UpdatedAt)
                SELECT
                    rs.UserId,
                    rs.Type,
                    rs.ItemNameId,
                    rs.CategoryId,
                    rs.Amount,
                    rs.Note,
                    rs.DayOfMonth,
                    CAST(1 AS bit),
                    rs.CreatedAt,
                    rs.UpdatedAt
                FROM @RecurringSource rs;

                ;WITH NumberedSource AS
                (
                    SELECT
                        rs.BudgetItemId,
                        ROW_NUMBER() OVER (ORDER BY rs.BudgetItemId) AS RowNum
                    FROM @RecurringSource rs
                ),
                NumberedRecurring AS
                (
                    SELECT
                        ri.Id,
                        ROW_NUMBER() OVER (ORDER BY ri.Id) AS RowNum
                    FROM RecurringItems ri
                    INNER JOIN @RecurringSource rs
                        ON rs.UserId = ri.UserId
                       AND rs.Type = ri.Type
                       AND rs.ItemNameId = ri.ItemNameId
                       AND rs.CategoryId = ri.CategoryId
                       AND rs.Amount = ri.Amount
                       AND ISNULL(rs.Note, N'') = ISNULL(ri.Note, N'')
                       AND rs.DayOfMonth = ri.DayOfMonth
                       AND rs.CreatedAt = ri.CreatedAt
                       AND rs.UpdatedAt = ri.UpdatedAt
                )
                UPDATE bi
                SET bi.RecurringItemId = nr.Id
                FROM BudgetItems bi
                INNER JOIN NumberedSource ns ON ns.BudgetItemId = bi.Id
                INNER JOIN NumberedRecurring nr ON nr.RowNum = ns.RowNum;
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_RecurringItems_RecurringItemId",
                table: "BudgetItems",
                column: "RecurringItemId",
                principalTable: "RecurringItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "BudgetItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "BudgetItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE BudgetItems
                SET IsRecurring = CASE WHEN RecurringItemId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_RecurringItems_RecurringItemId",
                table: "BudgetItems");

            migrationBuilder.DropTable(
                name: "RecurringItems");

            migrationBuilder.DropIndex(
                name: "IX_BudgetItems_RecurringItemId",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "RecurringItemId",
                table: "BudgetItems");
        }
    }
}