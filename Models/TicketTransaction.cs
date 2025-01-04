using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LotniskoAPI.Models
{
    [Table("TicketTransaction")]
    public class TicketTransaction
    {
        [Key]
        public int Id { get; set; }
        public int TransactionUserId { get; set; }
        public DateTime? Date { get; set; }
        public Decimal? Amount { get; set; }
        public string? CardDetail { get; set; }
        public string? Status { get; set; }

        public User? User { get; set; }

    }
}
