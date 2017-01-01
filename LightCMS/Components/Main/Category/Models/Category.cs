using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LightCMS.Components.Main.Models
{
    public class Category
    {
        public Category()
        {
            this.Items = new List<Item>();
            this.ChildCategories = new List<Category>();
            this.Category_Language = new List<Category_Language>();
        }
        
        public int Id { get; set; }

        public string CustomFields { get; set; }

        public Nullable<int> ParentId { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category ParentCategory { get; set; }

        [ForeignKey("ParentId")]
        public virtual ICollection<Category> ChildCategories { get; set; }
    
        public virtual ICollection<Category_Language> Category_Language { get; set; }

    }
}
