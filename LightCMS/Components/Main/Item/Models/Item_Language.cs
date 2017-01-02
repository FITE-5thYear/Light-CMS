using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace LightCMS.Components.Main.Models
{
    public class Item_Language
    {
        public int Id { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public string Title { get; set; }

        public string ShortContent { get; set; }

        [StringLength(500)]
        public string FullContent { get; set; }

        internal static object Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
