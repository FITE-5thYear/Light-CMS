using Components.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            return (new MainComponentController()).GetMenuItemView(link, Settings.MySqlConnectionString, lang);            
        }

    }
}
