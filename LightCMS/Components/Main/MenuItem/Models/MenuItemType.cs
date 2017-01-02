using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class MenuItemType
    {
        public MenuItemType()
        {
            MenuItems = new List<MenuItem>();
        }
        public int Id { get; set; }

        public string Name { get; set; }

        //many MenuItems
        public virtual ICollection<MenuItem> MenuItems { get; set; }

        //one Extension
        public int ExtensionId { get; set; }
        public Extension Extension { get; set; }
    }
}
