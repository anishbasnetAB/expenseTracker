using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using expenseTracker.Data;
using System.Security.Claims;

namespace expenseTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            // Total monthly expenses for the current user using a date range filter
            var totalMonthlyExpense = await _context.Expenses
                .Where(e => e.UserId == userId &&
                            e.ExpenseDate >= startOfMonth &&
                            e.ExpenseDate < startOfNextMonth)
                .SumAsync(e => e.Amount);
            ViewBag.TotalMonthlyExpense = totalMonthlyExpense;

            // Bar chart data: Monthly expenses for the current user for the current year
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Year == now.Year)
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    Total = group.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
            ViewBag.MonthlyExpenses = monthlyExpenses;

            // Pie chart data: Category-wise expenses for the current user for the current year
            var categoryExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Year == now.Year)
                .Include(e => e.Category)
                .GroupBy(e => e.Category.Name)
                .Select(group => new
                {
                    CategoryName = group.Key,
                    Total = group.Sum(e => e.Amount)
                })
                .ToListAsync();
            ViewBag.CategoryExpenses = categoryExpenses;

            return View();
        }
    }
}
