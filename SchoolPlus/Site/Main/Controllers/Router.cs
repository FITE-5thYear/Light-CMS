using Components.Main;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SchoolPlus.Controllers
{
    /*
     *  This is the main site router
     */
    public class RouterController : Controller
    {
        private Settings Settings { get; set; }

        public RouterController(IOptions<Settings> settings){
            Settings = settings.Value;
        }

        public IActionResult Index(string link)
        {            
            return (new MainComponentController()).GetMenuItemView(link, Settings.MySqlConnectionString);            
        }

    }
}
