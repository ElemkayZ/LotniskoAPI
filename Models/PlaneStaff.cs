
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LotniskoAPI.Models
{
    [Table("FlightStaff")]
    public class PlaneStaff
    {
        [Key]
        public int Id { get; set; }
        public int? PlaneId { get; set; }
        public int? WorkerId { get; set; }

        public User? User { get; set; }
        public Plane? Plane { get; set; }
    }
}
