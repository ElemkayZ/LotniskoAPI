using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LotniskoAPI.Models
{
    [Table("FlightStatus")]
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int? Terminal { get; set; }
        public int? Gate { get; set; }
        public DateTime? CheckinTime { get; set; }
        public int? BaggageCarousel { get; set; }


    }

    public class FlightAndStatus
    {
        public Flight? _Flight { get; set; }
        public Status? _FlightStatus { get; set; }
    }
}
