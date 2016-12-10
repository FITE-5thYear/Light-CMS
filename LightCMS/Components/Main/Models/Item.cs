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
        
        //one Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
