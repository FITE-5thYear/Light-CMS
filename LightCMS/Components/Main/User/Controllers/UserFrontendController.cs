using System;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using System.Linq;
using System.Security.Claims;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace LightCMS.Components.Main.User.Controllers
{
    public class UserFrontendController : Controller
    {
        private CMSDBContext db;

        public UserFrontendController(CMSDBContext context) { db = context; }

        [HttpGet, Route("signup")]
        public IActionResult GetSignup()
        {
            return View("~/Components/Main/User/Views/Frontend/signup.cshtml");
        }

        [HttpPost, Route("signup")]
        public IActionResult Signup(Models.User user)
        {
            user.IsApproved = false;
            user.InsertedAt = DateTime.Now;

            db.Add(user);
            db.SaveChanges();

            return Redirect("/pending");
        }

        [HttpGet,Route("/pending")]
        public IActionResult GetPendingPage()
        {
            return View("~/Components/Main/User/Views/Frontend/pending.cshtml");
        }

        [HttpGet, Route("signin")]
        public IActionResult GetSignin()
        {
            return View("~/Components/Main/User/Views/Frontend/signin.cshtml");
        }

        [HttpPost, Route("signin")]
        public IActionResult Singin(string email, string password)
        {
            var user = db.Users.SingleOrDefault(_user => _user.Email.Equals(email) && _user.Password.Equals(password));

            if (user == null || !user.IsApproved)
                return BadRequest();

            var id = new ClaimsIdentity("user");
            id.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            id.AddClaim(new Claim(ClaimTypes.Role, "Registered"));

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(id);

            HttpContext.Authentication.SignInAsync("Auth", principal, new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties()
            {
                IsPersistent = true
            }).Wait();

            return Redirect("/");
        }

    }
}
