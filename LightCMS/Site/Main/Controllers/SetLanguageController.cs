using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace LightCMS.Site.Main.Controllers
{
    public class SetLanguageController : Controller
    {
        
        //TODO: redirect to refresh the page 
        public IActionResult Index(int language_id)
        {
            HttpContext.Session.SetInt32("language", language_id);
            return RedirectToAction("Router", "Index");
        }
    }
}
