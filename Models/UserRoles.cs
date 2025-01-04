using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LotniskoAPI.Models
{
    [Table("UserRoles")]
    public class UserRoles
    {
        [Key]
        public int Id { get; set; }
        public int UserRoleId { get; set; }
        public int RoleId { get; set; }
        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
