using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LotniskoAPI.Models
{
    [Table("Ticket")]
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public DateTime? OrderDate { get; set; }
        public int OrderFlightId { get; set; }
        public int TransactionId { get; set; }
        public bool? Insurence { get; set; }
        public int? SeatingType { get; set; }
        public int? SeatingNumber { get; set; }

        public User? User { get; set; }
        public Flight? Flight { get; set; }
        public TicketTransaction? TicketTransaction { get; set; }
    }
}
