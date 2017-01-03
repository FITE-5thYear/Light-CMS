using System.ComponentModel.DataAnnotations;

namespace LightCMS.Components.Main.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string CustomValues { get; set; }
        
        //one Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        //one Role
        public virtual int? RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
