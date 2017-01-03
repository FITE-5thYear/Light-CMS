using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    [Table("user_roles")]
    public class UserRole
    {
        public int Id { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
