using Components.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace LightCMS.Controllers
{
    /*
     *  This is the main site router
     */
    public class RouterController : Controller
    {
        private Settings Settings { get; set; }

        private readonly IHttpContextAccessor Context;

        public RouterController(IOptions<Settings> settings, IHttpContextAccessor context){
            Settings = settings.Value;
            Context = context;
        }

        public IActionResult Index(string link)
        {
            if (Context.HttpContext.Session.GetInt32("language") == null)
                Context.HttpContext.Session.SetInt32("language", 1); // default is english

            int lang = (int) Context.HttpContext.Session.GetInt32("language");
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var selected_language = db.Language.SingleOrDefault(_lang => _lang.Id == lang);
                TempData["orientation"] = selected_language.orientation;
            }

            return (new MainComponentController()).GetMenuItemView(link, Settings.MySqlConnectionString, lang);            
        }


        public IActionResult SetLanguage(int language_id)
        {
            HttpContext.Session.SetInt32("language", language_id);
        
                return RedirectToAction( "Index");
        }

    }
}
