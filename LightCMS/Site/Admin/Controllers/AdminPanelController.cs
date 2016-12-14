using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;
using System.Collections;


namespace LightCMS.Controllers
{
    public class AdminPanelController : Controller
    {
        private Settings Settings { get; set; }

        public AdminPanelController(IOptions<Settings> settings){
            Settings = settings.Value;                                           
        }

        public IActionResult Index(string page)
        {
            return View("~/Site/Admin/Views/Dashboard/Index.cshtml");            
        }
  

        /////////////////////

        [Route("admin/language")]
        public IActionResult ListLanguages()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                
                ViewBag.languages = db.Language .ToList();

                return View("~/Site/Admin/Views/Language/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("admin/language/create")]
        public IActionResult GetCreateLanguage()
        {
                return View("~/Site/Admin/Views/Language/Create.cshtml");
        }

        [HttpPost]
        [Route("admin/language/create")]
        public IActionResult CreateLanguage(Language language)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(language);
                db.SaveChanges();
            }
            return Redirect("/admin/language");
        }


    }
    
}
