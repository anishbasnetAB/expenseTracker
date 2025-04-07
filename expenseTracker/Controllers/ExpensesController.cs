using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using expenseTracker.Data;
using expenseTracker.Models;
using System.Security.Claims;

namespace expenseTracker.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            // Get current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Retrieve only the expenses that belong to the current user
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .ToListAsync();
            return View(expenses);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                // Assign the current user's ID to the expense
                expense.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // GET: Expenses/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            // Optionally: Verify that the expense belongs to the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId)
            {
                return Unauthorized();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.Id) return NotFound();

            // Ensure the expense belongs to the logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _context.Update(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // GET: Expenses/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            // Optionally: Check if the expense belongs to the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense.UserId != userId)
            {
                return Unauthorized();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            // Verify the expense belongs to the current user before deleting
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (expense == null || expense.UserId != userId)
            {
                return Unauthorized();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Expenses/History
        public async Task<IActionResult> History(int? year, int? month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .AsQueryable();

            if (year.HasValue && month.HasValue)
            {
                expenses = expenses.Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month);
                ViewBag.SelectedMonth = new DateTime(year.Value, month.Value, 1).ToString("MMMM yyyy");
                return View("HistoryDetails", await expenses.ToListAsync());
            }

            var historyData = await expenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new MonthlyHistoryViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(h => h.Year)
                .ThenByDescending(h => h.Month)
                .ToListAsync();

            return View(historyData);
        }

        // GET: Expenses/Details/{year}/{month}
        [HttpGet]
        public async Task<IActionResult> Details(int year, int month)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
                .Include(e => e.Category)
                .ToListAsync();

            var categoryData = expenses
                .GroupBy(e => e.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .ToList();

            ViewBag.CategoryData = categoryData;
            ViewBag.MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy");
            ViewBag.Year = year;
            ViewBag.Month = month;

            return View(expenses);
        }
    }
}
