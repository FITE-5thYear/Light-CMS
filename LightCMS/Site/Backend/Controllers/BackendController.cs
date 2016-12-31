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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightCMS.Controllers
{
    public class BackendController : Controller
    {
        private Settings Settings { get; set; }

        public BackendController(IOptions<Settings> settings){
            Settings = settings.Value;
        }

        public IActionResult Index(string page)
        {
            return View("~/Site/Backend/Views/Dashboard/Index.cshtml");            
        }

        
    }
}
