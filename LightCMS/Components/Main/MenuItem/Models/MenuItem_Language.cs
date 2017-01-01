using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class MenuItem_Language
    {
        public int Id { get; set; }
        public string Label { get; set; }
        
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

    }
}
