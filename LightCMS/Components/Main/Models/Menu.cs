using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class Menu
    {
        public Menu()
        {
            MenuItems = new System.Collections.Generic.List<MenuItem>();
        }

        public int Id { get; set; }

      //  public string Description { get; set; }

        // Category
        public int CategoryId { get; set; }

        public Category Category { get; set; }


        // many MenuItems
        [InverseProperty("Menu")]
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }
}
