using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LotniskoAPI.Models
{
    [Table("Flight")]
    public class Flight
    {
        [Key]
        public int Id { get; set; }
        public int CurrentPlaneId { get; set; }
        public string? Starting { get; set; }
        public string? Destination { get; set; }
        public string? Type { get; set; }
        public string? Direction { get; set; }
        public int? Distance { get; set; }
        public int? AvailableSeats { get; set; }
        public int? StatusId { get; set; }

        public Plane? Plane { get; set; }
        public Status? Status { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
