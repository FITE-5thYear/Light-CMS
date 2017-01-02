using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Authorization;

namespace LightCMS.Components.Main.Controllers
{
    [Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
    public class Menu_ItemAdminController : Controller
    {
        private Settings Settings { get; set; }

        public Menu_ItemAdminController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }


        [HttpGet]
        [Route("backend/menu-items")]
        public IActionResult ListMenuItems()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItems = db.MenuItem_Language
                                        .Include(menu_item_lang => menu_item_lang.MenuItem)
                                            .ThenInclude(menu_item=>menu_item.MenuItemType)
                                        .ToList();

                ViewBag.menus = db.Menu_Language.ToList();

                return View("~/Components/Main/MenuItem/Views/Backend/Index.cshtml");
            }
        }

        //add to existed 
        [HttpGet]
        [Route("backend/menu-items/add")]
        public IActionResult GetAddMenuItem()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.Where(lags => lags.Id != 1).ToList();
                ViewBag.menuItems = db.MenuItem_Language.Where(menu_item_lag=>menu_item_lag.LanguageId==1)
                                        .ToList();

                return View("~/Components/Main/MenuItem/Views/Backend/add.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/menu-items/add")]
        public IActionResult AddMenuItem(MenuItem_Language menuItem_language)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                
                db.Add(menuItem_language);
                db.SaveChanges();
            }
            return Redirect("/backend/menu-items");
        }

        //create new 
        [HttpGet]
        [Route("backend/menu-items/Create")]
        public IActionResult GetCreateMenuItem()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.menuItemType = db.MenuItemTypes
                                        .ToList();

                ViewBag.menus = db.Menu_Language.Where(menu_lang=> menu_lang.LanguageId==1).Include(menu_lang=>menu_lang.Menu).ToList();

                ViewBag.items = db.Item_Language.Where(item_lang => item_lang.LanguageId == 1).Include(item_lang=>item_lang.Item).ToList();

                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1).Include(cat_lang=>cat_lang.Category).ToList();

                return View("~/Components/Main/MenuItem/Views/Backend/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/menu-items/Create")]
        public IActionResult CreateMenuItem(string  Label, MenuItem menuItem, FormCollection form)
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

                MenuItem_Language menuItem_langaue = new MenuItem_Language() { LanguageId = 1, MenuItem = menuItem, Label = Label};
                db.Add(menuItem_langaue);

                db.SaveChanges();
            }
            return Redirect("/backend/menu-items");
        }



        //edit exist item
        [HttpGet]
        [Route("backend/menu-items/edit/{menu_items_id}")]
        public IActionResult GetEditItem(int menu_items_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
               
                ViewBag.menuItem = db.MenuItem_Language.Where(_item => _item.Id == menu_items_id)
                                        .Include(_item => _item.MenuItem)
                                        .SingleOrDefault();

                ViewBag.menuItemType = db.MenuItemTypes .ToList();

                ViewBag.menus = db.Menu_Language.Where(menu_lang => menu_lang.LanguageId == 1).Include(menu_lang => menu_lang.Menu).ToList();

                ViewBag.items = db.Item_Language.Where(item_lang => item_lang.LanguageId == 1).Include(item_lang => item_lang.Item).ToList();

                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1).Include(cat_lang => cat_lang.Category).ToList();


                return View("~/Components/Main/MenuItem/Views/Backend/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("backend/menu-items/edit")]
        public IActionResult EditItem(MenuItem menu_item,FormCollection form, int menuItem_languageId, int LangaugeId, string Label)
        {

           
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                menu_item.IsMenu = this.Request.Form["IsMenu"].ToString().Equals("") ? false : true;
                menu_item.IsIndexPage = this.Request.Form["IsIndexPage"].ToString().Equals("") ? false : true;

                if (menu_item.IsIndexPage)
                //user choose to set as index page
                {
                    //prev item that was set as an index page
                    var _menuItem = db.MenuItems.SingleOrDefault(item => item.IsIndexPage);
                    _menuItem.IsIndexPage = false;
                    db.Entry(_menuItem).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (!menu_item.IsMenu)
                {
                    if (menu_item.MenuItemTypeId == 1)
                    { // Single Article
                      //TODO: use FormCollection
                        menu_item.Params = "{ItemId : " + this.Request.Form["ItemId"].ToString() + "}";
                    }
                    else if (menu_item.MenuItemTypeId == 2)
                    {
                        //Category List
                        //TODO: use FormCollection
                        menu_item.Params = "{CategoryId : " + this.Request.Form["CategoryId"].ToString() + "}";
                    }
                }

                MenuItem_Language mil = new MenuItem_Language() { Id = menuItem_languageId, LanguageId = LangaugeId, Label = Label, MenuItem = menu_item };

                db.Entry(menu_item).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(mil).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect("/backend/menu-items");
            }
        }


        [HttpGet]
        [Route("backend/menu-items/delete/{menuItem_id}")]
        public IActionResult DeleteItem(int menuItem_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var itemToRemove = db.MenuItem_Language.SingleOrDefault(x => x.Id == menuItem_id);

                if (itemToRemove != null)
                {
                    db.MenuItem_Language.Remove(itemToRemove);

                    //delete the default item language will delete other items languages and the origin item 
                    if (itemToRemove.LanguageId == 1)
                    {
                        var OtherItemsToRemove = db.MenuItem_Language.Where(x => x.MenuItemId == itemToRemove.MenuItemId).ToList();
                        foreach (var v in OtherItemsToRemove)
                        {
                            db.MenuItem_Language.Remove(v);
                        }

                        var item = db.MenuItems.SingleOrDefault(x => x.Id == itemToRemove.MenuItemId);
                        db.MenuItems.Remove(item);
                    }

                }

                db.SaveChanges();
                return Redirect("/backend/menu-items");
            }
        }
    }

}
