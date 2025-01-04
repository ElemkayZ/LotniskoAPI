using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace LotniskoAPI.Models
{
    [Table("User")]
    public class User : IdentityUser<int>
    {
        [Key]
        new public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        new public string? UserName { get; set; }
        required public string Password { get; set; }
        public string? Mail { get; set; }
        public string? Phone { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
        public ICollection<UserRoles>? UserRoles { get; set; }
        public ICollection<PlaneStaff>? PlaneStaff { get; set; }
        public ICollection<TicketTransaction>? Transactions { get; set; }

    }
}
