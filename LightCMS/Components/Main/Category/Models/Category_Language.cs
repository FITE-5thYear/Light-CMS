using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class Category_Language
    {

        public int Id { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string Description { get; set; }

    }
}
