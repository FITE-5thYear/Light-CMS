using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightCMS.Components.Main
{
    public class MainComponentBackendController : Controller
    {

        private Settings Settings { get; set; }

        public MainComponentBackendController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [Route("backend/items")]
        public IActionResult ListItems()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.items = db.Items
                                        .Include(item => item.Category)
                                        .ToList();

                return View("~/Components/Main/Views/Backend/Item/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/items/create")]
        public IActionResult GetCreateItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //categories
                ViewBag.cats = db.Categories
                                        .ToList();

                return View("~/Components/Main/Views/Backend/Item/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/items/create")]
        public IActionResult CreateItem(Item item)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //get chosen category
                var category = db.Categories.SingleOrDefault(cat => cat.Id == item.CategoryId);
                var customFields = JsonConvert.DeserializeObject<List<CustomField>>(category.CustomFields);

                string customValuesBuilder = "{";
                foreach (var customField in customFields)
                {
                    var nameOfField = customField.Name;
                    var valueOfField = this.Request.Form[nameOfField];
                    customValuesBuilder += nameOfField + ":" + valueOfField;
                }

                customValuesBuilder += "}";

                item.CustomValues = customValuesBuilder;

                db.Add(item);
                db.SaveChanges();
            }
            return Redirect("/backend/items");
        }

        [HttpGet]
        [Route("backend/categories")]
        public IActionResult ListCategories()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Categories
                                        .Include(cat => cat.ParentCategory)
                                        .ToList();

                return View("~/Components/Main/Views/Backend/Category/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/categories/create")]
        public IActionResult GetCreateCategory()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.Categories = db.Categories
                                            .Where(cat => cat.Items.Count == 0)
                                            .ToList();
                return View("~/Components/Main/Views/Backend/Category/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/categories/create")]
        public IActionResult CreateCategory(Category category, FormCollection form)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //TODO: encapsulate in Category Model
                var customFields = this.Request.Form["CustomFields"].ToString();
                category.CustomFields = customFields;
                db.Add(category);
                db.SaveChanges();
            }
            return Redirect("/backend/categories");
        }

        [HttpGet]
        [Route("backend/categories/getCustomFields")]
        public IActionResult GetCustomFields(int catId)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var cat = db.Categories
                             .SingleOrDefault(_cat => _cat.Id == catId);

                if (cat == null)
                    return new StatusCodeResult(404); // not found

                ViewBag.Category = cat;
                JArray customFields = JsonConvert.DeserializeObject(cat.CustomFields) as JArray;
                ViewBag.CustomFields = customFields;
                return PartialView("~/Components/Main/Views/Backend/Category/custom_fields.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/menus")]
        public IActionResult ListMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menus = db.Menus
                                        .Include(menu => menu.Category)
                                        .ToList();

                return View("~/Components/Main/Views/Backend/Menu/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/menus/create")]
        public IActionResult GetCreateMenus()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Categories
                                        .ToList();

                return View("~/Components/Main/Views/Backend/Menu/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/menus/create")]
        public IActionResult CreateMenu(Menu menu)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(menu);
                db.SaveChanges();

                return Redirect("/backend/menus");
            }
        }

        [HttpGet]
        [Route("backend/menu-items")]
        public IActionResult ListMenuItems()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItems = db.MenuItems
                                        .Include(menu => menu.MenuItemType)
                                        .ToList();

                return View("~/Components/Main/Views/Backend/MenuItem/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/menu-items/create")]
        public IActionResult GetCreateMenuItem()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItemType = db.MenuItemTypes
                                        .ToList();

                ViewBag.menus = db.Menus.ToList();

                ViewBag.items = db.Items.ToList();

                ViewBag.cats = db.Categories.ToList();

                return View("~/Components/Main/Views/Backend/MenuItem/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/menu-items/create")]
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
                    else if (menuItem.MenuItemTypeId == 2)
                    {
                        //Category List
                        //TODO: use FormCollection
                        menuItem.Params = "{CategoryId : " + this.Request.Form["CategoryId"].ToString() + "}";
                    }
                }

                db.Add(menuItem);
                db.SaveChanges();
            }
            return Redirect("/backend/menu-items");
        }
    }
}
