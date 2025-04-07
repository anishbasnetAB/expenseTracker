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
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Total monthly expenses for the current user
            var totalMonthlyExpense = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                .SumAsync(e => e.Amount);
            ViewBag.TotalMonthlyExpense = totalMonthlyExpense;

            // Bar chart data: Monthly expenses for the current user
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Year == currentYear)
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    Total = group.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
            ViewBag.MonthlyExpenses = monthlyExpenses;

            // Pie chart data: Category-wise expenses for the current user
            var categoryExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Year == currentYear)
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
