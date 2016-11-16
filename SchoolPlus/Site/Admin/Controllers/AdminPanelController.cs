using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SchoolPlus.Config;
using Microsoft.EntityFrameworkCore;
using SchoolPlus.Components.Main.Models;
using Microsoft.AspNetCore.Http;

namespace SchoolPlus.Controllers
{
    public class AdminPanelController : Controller
    {
        private Settings Settings { get; set; }

        public AdminPanelController(IOptions<Settings> settings){
            Settings = settings.Value;
        }

        public IActionResult Index(string page)
        {
            return View("~/Site/Admin/Views/Dashboard/Index.cshtml");            
        }

        [Route("admin/items")]
        public IActionResult ListItems()
        {
            using(var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.items = db.Items
                                        .Include(item => item.Category )
                                        .ToList();

                return View("~/Site/Admin/Views/Item/Index.cshtml");
            }            
        }

        [HttpGet]
        [Route("admin/items/create")]
        public IActionResult GetCreateItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {                
                //categories
                ViewBag.cats = db.Categories
                                        .ToList();

                return View("~/Site/Admin/Views/Item/Create.cshtml");
            }            
        }

        [HttpPost]
        [Route("admin/items/create")]
        public IActionResult CreateItem(Item item)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(item);
                db.SaveChanges();                
            }
            return Redirect("/admin/items");
        }

        [HttpGet]
        [Route("admin/categories")]
        public IActionResult ListCategories()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Categories                                        
                                        .ToList();

                return View("~/Site/Admin/Views/Category/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("admin/categories/create")]
        public IActionResult GetCreateCategory()
        {
            return View("~/Site/Admin/Views/Category/Create.cshtml");
        }

        [HttpPost]
        [Route("admin/categories/create")]
        public IActionResult CreateCategory(Category category)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(category);
                db.SaveChanges();
            }
            return Redirect("/admin/categories");
        }             

        [HttpGet]
        [Route("admin/menus")]
        public IActionResult ListMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menus = db.Menus
                                        .Include(menu => menu.Category)
                                        .ToList();

                return View("~/Site/Admin/Views/Menu/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("admin/menus/create")]
        public IActionResult GetCreateMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Categories                                        
                                        .ToList();

                return View("~/Site/Admin/Views/Menu/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("admin/menus/create")]
        public IActionResult CreateMenu(Menu menu)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(menu);
                db.SaveChanges();

                return Redirect("/admin/menus");
            }
        }

        [HttpGet]
        [Route("admin/menu-items")]
        public IActionResult ListMenuItems()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItems = db.MenuItems
                                        .Include(menu => menu.MenuItemType)
                                        .ToList();

                return View("~/Site/Admin/Views/MenuItem/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("admin/menu-items/Create")]
        public IActionResult GetCreateMenuItem()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItemType = db.MenuItemTypes
                                        .ToList();

                ViewBag.menus = db.Menus.ToList();

                ViewBag.items = db.Items.ToList();

                ViewBag.cats = db.Categories.ToList();
                
                return View("~/Site/Admin/Views/MenuItem/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("admin/menu-items/Create")]
        public IActionResult CreateMenuItem(MenuItem menuItem, FormCollection form)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //TODO: bind checkbox with menuItem.IsIndexPage & menuItem.IsMenu
                menuItem.IsMenu = this.Request.Form["IsMenu"].ToString().Equals("") ? false : true;
                menuItem.IsIndexPage = this.Request.Form["IsIndexPage"].ToString().Equals("") ? false : true;

                if (menuItem.IsIndexPage)
                    //user choose to set as index page
                {
                    //prev item that was set as an index page
                    var _menuItem = db.MenuItems.SingleOrDefault(item => item.IsIndexPage);
                    _menuItem.IsIndexPage = false;
                    db.Entry(_menuItem).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (!menuItem.IsMenu)
                {
                    if (menuItem.MenuItemTypeId == 1)
                    { // Single Article
                        //TODO: use FormCollection
                        menuItem.Params = "{ItemId : " + this.Request.Form["ItemId"].ToString() + "}";
                    }
                    else if(menuItem.MenuItemTypeId == 2)
                    {
                        //Category List
                        //TODO: use FormCollection
                        menuItem.Params = "{CategoryId : " + this.Request.Form["CategoryId"].ToString() + "}";
                    }
                }

                db.Add(menuItem);
                db.SaveChanges();
            }
            return Redirect("/admin/menu-items");
        }
    }
}
