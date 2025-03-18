using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using expenseTracker.Data;

namespace expenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Monthly total expenses
            var totalMonthlyExpense = await _context.Expenses
                .Where(e => e.ExpenseDate.Month == currentMonth && e.ExpenseDate.Year == currentYear)
                .SumAsync(e => e.Amount);

            ViewBag.TotalMonthlyExpense = totalMonthlyExpense;

            // Bar chart data (monthly expenses)
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate.Year == currentYear)
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    Total = group.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            ViewBag.MonthlyExpenses = monthlyExpenses;

            // Pie chart data (category-wise expenses)
            var categoryExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate.Year == currentYear)
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
