using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.Main.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        //public string Label { get; set; }
        public string Link { get; set; }
        //JSON params
        public string Params { get; set; }
        public bool IsIndexPage { get; set; }

        public bool IsMenu { get; set; }        
        public int ChildMenuId { get; set; }
        public virtual Menu ChildMenu { get; set; }

        // one MenuItemType
        public int? MenuItemTypeId { get; set; }
        public virtual MenuItemType MenuItemType { get; set; }

        // one Menu
        [ForeignKey("Menu")]
        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }
    }
}
