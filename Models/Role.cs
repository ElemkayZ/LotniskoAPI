using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace LotniskoAPI.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
         public int Id { get; set; }
         public string? Name { get; set; }

        public ICollection<UserRoles>? UserRoles { get; set; }

    }
}
