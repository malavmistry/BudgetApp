using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BudgetApp.ViewModels;

namespace BudgetApp.Services
{
    public interface IReportService
    {
        Task<ReportResultViewModel> GenerateReportAsync(ReportViewModel filter, int userId, string userTimeZoneId);

        Task<byte[]> ExportToExcelAsync(ReportResultViewModel result);
    }
}
