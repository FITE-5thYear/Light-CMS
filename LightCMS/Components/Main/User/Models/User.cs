using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.User.Models
{
    public class User
    {
        public User()
        {
            this.UserRoles = new List<UserRole>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime InsertedAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public bool hasRole(string roleName)
        {
            foreach(var userRole in UserRoles)
            {
                if (userRole.Role.Name.Equals(roleName))
                    return true;
            }

            return false;
        }
    }
}
