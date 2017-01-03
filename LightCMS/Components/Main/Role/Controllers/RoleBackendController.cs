using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace LightCMS.Components.Main.Role.Controllers
{
    [Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
    public class RoleBackendController : Controller
    {
        private CMSDBContext db;

        public RoleBackendController(CMSDBContext context) { db = context; }

        [Route("backend/roles")]
        public IActionResult Index()
        {
            ViewBag.roles = db.Roles.ToList();
            return View("~/Components/Main/Role/Views/Backend/list.cshtml");
        }

        [HttpGet, Route("backend/roles/create")]
        public IActionResult GetCreate()
        {
            return View("~/Components/Main/Role/Views/Backend/create.cshtml");
        }

        [HttpPost, Route("backend/roles/create")]
        public IActionResult Create(Models.Role role)
        {
            db.Add(role);
            db.SaveChanges();

            return Redirect("/backend/roles");
        }

        [HttpGet, Route("backend/roles/edit/{roleId}")]
        public IActionResult GetEdit(int roleId)
        {
            ViewBag.role = db.Roles.SingleOrDefault(role => role.Id == roleId);
            return View("~/Components/Main/Role/Views/Backend/edit.cshtml");
        }

        [HttpPost, Route("backend/roles/edit")]
        public IActionResult Edit(Models.Role role)
        {
            db.Entry(role).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect("/backend/roles");
        }

        [HttpGet, Route("backend/roles/delete/{roleId}")]
        public IActionResult Delete(int roleId)
        {
            if (roleId == 1 || roleId == 2 || roleId == 3) //Public & Admin roles & registered
                return Redirect("/backend/roles"); // prevent deletion

            var role = db.Roles.SingleOrDefault(_role => _role.Id == roleId);
            db.Roles.Remove(role);
            db.SaveChanges();

            return Redirect("/backend/roles");
        }
    }
}
