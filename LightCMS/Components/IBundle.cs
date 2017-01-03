using LightCMS.Components.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components
{
    public interface IBundle
    {
        ICollection<MenuItem_Language> MainMenuItems { get; set; }

        Role UserRole { get; set; }
    }
}
