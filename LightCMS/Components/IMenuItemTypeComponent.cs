using Microsoft.AspNetCore.Mvc;
using LightCMS.Components.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components
{
    public interface IMenuItemTypeComponent : IComponent
    {
        IActionResult Render(MenuItem menuItem, string connectionString);
    }
}
