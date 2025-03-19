using System.ComponentModel.DataAnnotations.Schema;

namespace expenseTracker.Models
{
    [NotMapped] // <-- This tells EF to ignore this class as an entity
    public class MonthlyHistoryViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
