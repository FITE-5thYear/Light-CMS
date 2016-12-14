using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;

namespace LightCMS.Site.Admin.Controllers
{
    public class MenuAdminController : Controller
    {
        private Settings Settings { get; set; }
        public MenuAdminController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [HttpGet]
        [Route("admin/menus")]
        public IActionResult ListMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menus = db.Menu_Language
                                        .Include(menu_lang => menu_lang.Language)
                                        .Include(menu_lang => menu_lang.Menu)
                                           .ThenInclude(menu_cat => menu_cat.Category)
                                        .OrderBy(menu_lan => menu_lan.MenuId)
                                        .ToList();

                

                return View("~/Site/Admin/Views/Menu/Index.cshtml");
            }
        }


        // add to an exist category with different language
        [HttpGet]
        [Route("admin/menus/add")]
        public IActionResult GetAddMenu()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.Where(lang => lang.Id != 1).ToList();
                ViewBag.menus = db.Menu_Language.Where(menu_lang => menu_lang.LanguageId == 1)//show the menu description within the default language..the English one                             
                 .ToList();

                return View("~/Site/Admin/Views/Menu/add.cshtml");
            }
        }

        [HttpPost]
        [Route("admin/menus/add")]
        public IActionResult AddMenu(Menu_Language menu_language)
        {

            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                db.Add(menu_language);

                db.SaveChanges();

            }
            return Redirect("/admin/menus");
        }


        [HttpGet]
        [Route("admin/menus/create")]
        public IActionResult GetCreateMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1).ToList();

                return View("~/Site/Admin/Views/Menu/Create.cshtml");
            }

        }

        [HttpPost]
        [Route("admin/menus/create")]
        public IActionResult CreateMenu(int CategoryId, string Description)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Menu menu = new Menu();
                menu.CategoryId = CategoryId;
                db.Add(menu);

                Menu_Language menu_language = new Menu_Language();
                menu_language.Description = Description;
                menu_language.LanguageId = 1;//default laguage 
                menu_language.Menu = menu;
                db.Add(menu_language);

                db.SaveChanges();
                return Redirect("/admin/menus");
            }
        }


        //edit exist menu
        [HttpGet]
        [Route("admin/menus/edit/{menu_id}")]
        public IActionResult GetEditItem(int menu_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Menu_Language ml = db.Menu_Language.SingleOrDefault(_menu_lang => _menu_lang.Id == menu_id);
                ViewBag.menu = db.Menu_Language.Where(_menu => _menu.Id == menu_id)
                                        .Include(_item => _item.Menu)
                                        .SingleOrDefault();

                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == ml.LanguageId).ToList();
                return View("~/Site/Admin/Views/Menu/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("admin/menus/edit")]
        public IActionResult EditItem(int MenuId, int Menu_LanguageId, int LanguageId, int CategoryId, string Description)
        {
            Menu m = new Menu() { Id = MenuId, CategoryId = CategoryId };
            Menu_Language il = new Menu_Language() { Id = Menu_LanguageId, LanguageId = LanguageId,  Description= Description, MenuId=MenuId};

            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                
                db.Entry(m).State = EntityState.Modified;

                db.Entry(il).State = EntityState.Modified;
                db.SaveChanges();

                return Redirect("/admin/menus");
            }
        }


        [HttpGet]
        [Route("admin/menus/delete/{menu_id}")]
        public IActionResult DeleteItem(int menu_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var itemToRemove = db.Menu_Language.SingleOrDefault(x => x.Id == menu_id);

                if (itemToRemove != null)
                {
                    db.Menu_Language.Remove(itemToRemove);

                    //delete the default item language will delete other items languages and the origin item 
                    if (itemToRemove.LanguageId == 1)
                    {
                        var OtherItemsToRemove = db.Menu_Language.Where(x => x.MenuId == itemToRemove.MenuId).ToList();
                        foreach (var v in OtherItemsToRemove)
                        {
                            db.Menu_Language.Remove(v);
                        }

                        var item = db.Menus.SingleOrDefault(x => x.Id == itemToRemove.MenuId);
                        db.Menus.Remove(item);
                    }

                }

                db.SaveChanges();
                return Redirect("/admin/menus");
            }
        }


    }
}
