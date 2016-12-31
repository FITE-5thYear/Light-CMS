using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class Category
    {
        public Category()
        {
            this.Items = new List<Item>();
            this.ChildCategories = new List<Category>();
        }
        public int Id { get; set; }

        public string Description { get; set; }

        public string CustomFields { get; set; }

        public Nullable<int> ParentId { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category ParentCategory { get; set; }

        [ForeignKey("ParentId")]
        public virtual ICollection<Category> ChildCategories { get; set; }
    }
}
