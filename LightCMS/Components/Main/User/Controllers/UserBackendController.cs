using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace LightCMS.Components.Main.User.Controllers
{
    public class UserBackendController : Controller
    {

        private CMSDBContext db; 

        public UserBackendController(CMSDBContext context) { db = context; }

        [Route("backend/login")]
        [HttpGet]
        public IActionResult GetLogin()
        {
            return View("~/Components/Main/User/Views/Backend/login.html");
        }

        [Route("backend/login")]
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            //TODO: use password hashing:
            var user = db.Users
                            .Include(_user => _user.UserRoles)
                            .ThenInclude(_userRole => _userRole.Role)
                            .SingleOrDefault(_user => _user.Email.Equals(email) && _user.Password.Equals(password));

            if (user == null)
                return BadRequest();//user not found

            if (!user.hasRole("Admin"))//unauthorized
                return Forbid();

            var id = new ClaimsIdentity("user");
            id.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            id.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(id);

            HttpContext.Authentication.SignInAsync("Auth", principal, new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties()
            {
                IsPersistent = true
            }).Wait();

            return Redirect("/backend");
        }

        
        [Route("backend/logout"),HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Authentication.SignOutAsync("Auth").Wait();
            return Redirect("/backend/login");
        }

        [HttpGet,Route("backend/users"),Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult List()
        {
            ViewBag.users = db.Users.ToList();
            return View("~/Components/Main/User/Views/Backend/list.cshtml");
        }

        
        [HttpGet,Route("backend/isloggedin"),Authorize(Roles = "Admin",ActiveAuthenticationSchemes ="Auth")]        
        public IActionResult IsLoggedIn()
        {
            var principal = HttpContext.User as ClaimsPrincipal;

            return new StatusCodeResult(200);
        }




    }
}
