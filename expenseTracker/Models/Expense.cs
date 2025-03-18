namespace expenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }

        // New property added here
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
