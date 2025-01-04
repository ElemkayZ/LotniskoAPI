
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LotniskoAPI.Models
{
    [Table("Plane")]
    public class Plane
    {
        [Key]
        public int Id { get; set; }
        public string? Model { get; set; }
        public int Capacity { get; set; }
        public string? Airline { get; set; }


        public ICollection<Flight>? Flights { get; set; }
        public ICollection<PlaneStaff>? PlaneStaffs { get; set; }
    }
}
