using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string ShortContent { get; set; }

        [StringLength(500)]
        public string FullContent { get; set; }

        public string CustomValues { get; set; }

        //one Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
