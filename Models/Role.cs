using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace LotniskoAPI.Models
{
    [Table("Role")]
    public class Role : IdentityRole<int>
    {
        [Key]
        new public int Id { get; set; }
        override public string? Name { get; set; }

        public ICollection<UserRoles>? UserRoles { get; set; }

    }
}
