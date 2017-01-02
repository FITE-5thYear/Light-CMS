using LightCMS.Components.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LightCMS.Components
{
    public class MainBundle : IBundle
    {
        public ICollection<MenuItem_Language> MainMenuItems { get; set; }

        public Role UserRole { get; set; }
    }
}
