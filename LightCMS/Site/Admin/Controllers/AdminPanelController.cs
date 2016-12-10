using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;
using System.Collections;

namespace LightCMS.Controllers
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
                 ViewBag.items = db.Item_Language
                                    .Include(_item_language => _item_language.Item)
                                    .ThenInclude(item=>item.Category)
                                    .Include(_item_language=> _item_language.Language)
                                    .OrderBy(_item_language => _item_language.ItemId)
                                    .ToList();
              return View("~/Site/Admin/Views/Item/Index.cshtml");
            }            
        }



        [HttpGet]
        [Route("admin/items/add")]
        public IActionResult GetAddItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.ToList();
                ViewBag.items = db.Item_Language.Where(cat_lang => cat_lang.LanguageId == 1)//show the items within the default language..the English one
                .ToList();
                return View("~/Site/Admin/Views/Item/add.cshtml");
            }
        }


        [HttpPost]
        [Route("admin/items/add")]
        public IActionResult AddItem(Item_Language item_language)
        {

            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(item_language);
                db.SaveChanges();
            }
            return Redirect("/admin/items");
        }


        [HttpGet]
        [Route("admin/items/create")]
        public IActionResult GetCreateItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Category_Language.Where(cat_lang=>cat_lang.LanguageId==1).ToList();
          
                return View("~/Site/Admin/Views/Item/Create.cshtml");
            }            
        }

        [HttpPost]
        [Route("admin/items/create")]
        public IActionResult CreateItem(int CategoryId, string FullContent, string ShortContent, string Title)
        {
            
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Item item = new Item();
                item.CategoryId = CategoryId;
                db.Add(item);
                
                Item_Language item_language = new Item_Language();
                item_language.FullContent = FullContent;
                item_language.LanguageId = 1;//default laguage 
                item_language.ShortContent = ShortContent;
                item_language.Title = Title;
                item_language.Item = item;
                db.Add(item_language);
                
                db.SaveChanges();                
            }
            return Redirect("/admin/items");
        }
        //////////
       
        [HttpGet]
        [Route("admin/categories")]
        public IActionResult ListCategories()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats_langs = db.Category_Language                                        
                                        .Include(cat=>cat.Language)
                                        .OrderBy(cat=>cat.CategoryId)
                                        .ToList();
                return View("~/Site/Admin/Views/Category/Index.cshtml");
            }
        }
        // add to an exist category with different language
        [HttpGet]
        [Route("admin/categories/add")]
        public IActionResult GetAddCategory()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.ToList();
                ViewBag.cats = db.Category_Language.Where(cat_lang=>cat_lang.LanguageId==1)//show the category within the default language..the English one
                .ToList();
                return View("~/Site/Admin/Views/Category/add.cshtml");
            }
        }

        [HttpPost]
        [Route("admin/categories/add")]
        public IActionResult AddCategory(Category_Language category_language)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(category_language);
                db.SaveChanges();
            }
            return Redirect("/admin/categories");
        }

        // create new category     
        [HttpGet]
        [Route("admin/categories/create")]
        public IActionResult GetCreateCategory()
        {
            return View("~/Site/Admin/Views/Category/create.cshtml");

        }

        [HttpPost]
        [Route("admin/categories/create")]
        public IActionResult CreateCategory( string Description)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Category category = new Category();
                db.Add(category);
              //  db.SaveChanges();

                Category_Language category_language = new Category_Language();
                category_language.Description = Description;
                category_language.LanguageId = 1;
                category_language.Category = category;
                db.Add(category_language);

                db.SaveChanges();
            }
            return Redirect("/admin/categories");
        }



        /////////////
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
                                        .OrderBy(menu_lan=>menu_lan.MenuId)
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
                ViewBag.languages = db.Language.ToList();
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




        /////////////


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


        /////////////////////

        [Route("admin/language")]
        public IActionResult ListLanguages()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                
                ViewBag.languages = db.Language .ToList();

                return View("~/Site/Admin/Views/Language/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("admin/language/create")]
        public IActionResult GetCreateLanguage()
        {
                return View("~/Site/Admin/Views/Language/Create.cshtml");
        }

        [HttpPost]
        [Route("admin/language/create")]
        public IActionResult CreateLanguage(Language language)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(language);
                db.SaveChanges();
            }
            return Redirect("/admin/language");
        }


    }
}
