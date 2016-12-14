using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            this.Category_Language = new List<Category_Language>();
        }

        //[Key]
       // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public virtual ICollection<Item> Items { get; set; }
    
        public virtual ICollection<Category_Language> Category_Language { get; set; }

    }
}
