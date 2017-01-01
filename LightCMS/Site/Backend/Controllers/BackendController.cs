using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LightCMS.Controllers
{
    public class BackendController : Controller
    {
        private Settings Settings { get; set; }

        public BackendController(IOptions<Settings> settings){
            Settings = settings.Value;
        }

        [Route("backend")]
        public IActionResult Index(string page)
        {
            return View("~/Site/Backend/Views/Dashboard/Index.cshtml");            
        }

        
    }
}
