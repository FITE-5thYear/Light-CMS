using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class Category
    {
        public Category()
        {
            this.Items = new List<Item>();
        }
        public int Id { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
