using Microsoft.AspNetCore.Mvc;
using SchoolPlus.Components.Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolPlus.Components
{
    public interface IMenuItemTypeComponent
    {
        IActionResult Render(MenuItem menuItem, string connectionString);
    }
}
