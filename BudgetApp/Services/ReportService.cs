using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetApp.Constants;
using BudgetApp.Data;
using BudgetApp.Enums;
using BudgetApp.Models;
using BudgetApp.ViewModels;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetApp.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ReportService> _logger;

        public ReportService(AppDbContext dbContext, ILogger<ReportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ReportResultViewModel> GenerateReportAsync(
            ReportViewModel filter, int userId, string userTimeZoneId)
        {
            var timeZone = GetTimeZone(userTimeZoneId);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var (startUtc, endUtc, periodLabel) = ResolveDateRange(filter, now, timeZone);

            var query = _dbContext.BudgetItems
                .Include(bi => bi.Budget)
                .Include(bi => bi.ItemName)
                .Include(bi => bi.Category)
                .Where(bi => bi.Budget.UserId == userId &&
                             bi.TransactionDateUtc >= startUtc &&
                             bi.TransactionDateUtc < endUtc);

            if (filter.BudgetId.HasValue)
                query = query.Where(bi => bi.BudgetId == filter.BudgetId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(bi => bi.CategoryId == filter.CategoryId.Value);

            if (filter.Type.HasValue)
                query = query.Where(bi => bi.Type == filter.Type.Value);

            var items = await query.OrderBy(bi => bi.TransactionDateUtc).ToListAsync();

            var rows = items.Select(bi => new ReportRowViewModel
            {
                ItemName = bi.ItemName.Name,
                Category = bi.Category.Name,
                Type = bi.Type == TransactionType.Expense ? "Expense" : "Earnings",
                Amount = bi.Amount,
                Date = TimeZoneInfo.ConvertTimeFromUtc(bi.TransactionDateUtc, timeZone)
                                   .ToString(AppConstants.DATE_FORMAT),
                Note = bi.Note,
                Budget = bi.Budget.Name
            }).ToList();

            // When a category filter is active every item shares the same category,
            // so group the pie by Name instead to show meaningful slices.
            var groupByName = filter.CategoryId.HasValue;
            Func<BudgetItem, string> pieKey = groupByName
                ? bi => bi.ItemName.Name
                : bi => bi.Category.Name;

            var expensePie = items
                .Where(bi => bi.Type == TransactionType.Expense)
                .GroupBy(pieKey)
                .Select(g => new PieSliceViewModel
                {
                    Label = g.Key,
                    Value = g.Sum(bi => bi.Amount)
                })
                .OrderByDescending(p => p.Value)
                .ToList();

            var earningsPie = items
                .Where(bi => bi.Type == TransactionType.Earnings)
                .GroupBy(pieKey)
                .Select(g => new PieSliceViewModel
                {
                    Label = g.Key,
                    Value = g.Sum(bi => bi.Amount)
                })
                .OrderByDescending(p => p.Value)
                .ToList();

            return new ReportResultViewModel
            {
                Rows = rows,
                ExpensePie = expensePie,
                EarningsPie = earningsPie,
                TotalEarnings = items.Where(bi => bi.Type == TransactionType.Earnings).Sum(bi => bi.Amount),
                TotalExpense = items.Where(bi => bi.Type == TransactionType.Expense).Sum(bi => bi.Amount),
                PeriodLabel = periodLabel,
                PieGroupLabel = groupByName ? "Name" : "Category"
            };
        }

        public async Task<byte[]> ExportToExcelAsync(ReportResultViewModel result)
        {
            return await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Report");

                var headers = new[] { "Date", "Name", "Category", "Type", "Amount", "Budget", "Note" };
                for (var col = 1; col <= headers.Length; col++)
                {
                    ws.Cell(1, col).Value = headers[col - 1];
                    ws.Cell(1, col).Style.Font.Bold = true;
                }

                var row = 2;
                foreach (var r in result.Rows)
                {
                    ws.Cell(row, 1).Value = r.Date;
                    ws.Cell(row, 2).Value = r.ItemName;
                    ws.Cell(row, 3).Value = r.Category;
                    ws.Cell(row, 4).Value = r.Type;
                    ws.Cell(row, 5).Value = r.Amount;
                    ws.Cell(row, 6).Value = r.Budget;
                    ws.Cell(row, 7).Value = r.Note ?? string.Empty;
                    row++;
                }

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            });
        }

        private static (DateTime startUtc, DateTime endUtc, string label) ResolveDateRange(
            ReportViewModel filter, DateTime now, TimeZoneInfo timeZone)
        {
            return filter.Period switch
            {
                ReportPeriod.ThisMonth => (
                    ToUtc(new DateTime(now.Year, now.Month, 1), timeZone),
                    ToUtc(new DateTime(now.Year, now.Month, 1).AddMonths(1), timeZone),
                    $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month)} {now.Year}"
                ),
                ReportPeriod.YearToDate => (
                    ToUtc(new DateTime(now.Year, 1, 1), timeZone),
                    ToUtc(now.Date.AddDays(1), timeZone),
                    $"Year to Date {now.Year}"
                ),
                ReportPeriod.PreviousYear => (
                    ToUtc(new DateTime(now.Year - 1, 1, 1), timeZone),
                    ToUtc(new DateTime(now.Year, 1, 1), timeZone),
                    $"{now.Year - 1}"
                ),
                ReportPeriod.CustomRange when filter.CustomStart.HasValue && filter.CustomEnd.HasValue => (
                    ToUtc(filter.CustomStart.Value.Date, timeZone),
                    ToUtc(filter.CustomEnd.Value.Date.AddDays(1), timeZone),
                    $"{filter.CustomStart.Value:MM-dd-yyyy} – {filter.CustomEnd.Value:MM-dd-yyyy}"
                ),
                _ => (
                    ToUtc(new DateTime(now.Year, now.Month, 1), timeZone),
                    ToUtc(new DateTime(now.Year, now.Month, 1).AddMonths(1), timeZone),
                    $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month)} {now.Year}"
                )
            };
        }

        private static DateTime ToUtc(DateTime local, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), timeZone);
        }

        private static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
