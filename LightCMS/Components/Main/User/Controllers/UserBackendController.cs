using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;

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

        [HttpGet,Route("backend/users/edit/{userId}"),Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult GetEdit(int userId)
        {
            ViewBag.roles = db.Roles.ToList();

            ViewBag.user = db.Users
                                   .Include(user => user.UserRoles)
                                   .ThenInclude(userRole => userRole.Role)
                                   .SingleOrDefault(user => user.Id == userId);
                                       
            

            return View("~/Components/Main/User/Views/Backend/edit.cshtml");
        }

        [HttpPost, Route("backend/users/edit"), Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult Edit(int[] roles, int userId)
        {
            var userRoles = db.UserRoles.Where(userRole => userRole.User.Id == userId);
            db.UserRoles.RemoveRange(userRoles);

            var user = db.Users.SingleOrDefault(_user => _user.Id == userId);

            var newRoles = new List<UserRole>();
            foreach(var role in roles)
            {
                newRoles.Add(new Models.UserRole()
                {
                    Role = db.Roles.SingleOrDefault(_role => _role.Id == role),
                    User = user
                });
            }

            db.UserRoles.AddRange(newRoles);
            db.SaveChanges();

            return Redirect("/backend/users");
        }

        [HttpGet, Route("backend/users/pending"), Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult GetPendingUsers()
        {
            ViewBag.users = db.Users.Where(user => !user.IsApproved);

            return View("~/Components/Main/User/Views/Backend/pending_list.cshtml");
        }       

        [HttpGet, Route("backend/users/approve/{userId}"), Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
        public IActionResult ApproveUser(int userId)
        {
            var user = db.Users.SingleOrDefault(_user => _user.Id == userId);

            user.IsApproved = true;

            db.Entry(user).State = EntityState.Modified;

            db.SaveChanges();

            return Redirect("/backend/users/pending");
        }
    }
}
