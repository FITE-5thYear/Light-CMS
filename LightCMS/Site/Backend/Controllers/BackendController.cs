using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult Index(string page)
        {
            return View("~/Site/Backend/Views/Dashboard/Index.cshtml");            
        }

        [Route("forbidden")]
        public IActionResult Forbidden()
        {
            return View("~/Site/Backend/Views/403.cshtml");
        }

        
    }
}
