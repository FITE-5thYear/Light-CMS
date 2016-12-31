using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LightCMS.Components.Main.Models;
using LightCMS.Components.SingleArticle.Models;
using LightCMS.Config;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LightCMS.Components.SingleArticle
{
    public class SingleArticleComponentController : Controller, IMenuItemTypeComponent
    {
        public void Bootstrap(string connectionString)
        {
            using (var db = CMSContextFactory.Create(connectionString))
            {
                //init extendsion
                var extension = db.Extensions.SingleOrDefault(ext => ext.Namespace == "Components.SingleArticle.SingleArticleComponentController");
                if(extension == null)
                {
                    extension = new Extension()
                    {
                        Namespace = "Components.SingleArticle.SingleArticleComponentController"
                    };
                    db.Add(extension);
                }


                //init 'Single Article' menu type
                var menuItemType = db.MenuItemTypes.SingleOrDefault(type => type.Id == 1);
                if (menuItemType == null)
                {
                    menuItemType = new MenuItemType()
                    {
                        Id = 1,
                        Name = "Single Article",
                        ExtensionId = 1
                    };
                    db.Add(menuItemType);
                }

                //init menu-item
                var menuItem = db.MenuItems.SingleOrDefault(item => item.Id == 1);
                if (menuItem == null)
                {
                    menuItem = new MenuItem()
                    {
                        Id = 1,
                        Label = "Home",
                        Link = "home",
                        MenuItemTypeId = 1,
                        Params = "{ItemId : 1}", // item will be initialized next
                        IsMenu = false,
                        ChildMenuId = 1, //TODO: this should be zero
                        MenuId = 1,
                        IsIndexPage = true
                    };
                    db.Add(menuItem);
                }

                //init an item 
                var _item = db.Items.SingleOrDefault(item => item.Id == 1);
                if (_item == null)
                {
                    _item = new Item()
                    {
                        Id = 1,
                        Title = "Welcome To LightCMS",
                        ShortContent = "Welcome to LightCMS",
                        FullContent = @"Thank you for using LightCMS, you can access the <strong>Admin Panel</strong> by navigating to <strong>/admin</admin><br>.
                                        This is a sample page which was generated automatically by LightCMS.",
                        CategoryId = 1
                    };
                    db.Add(_item);
                }

                db.SaveChanges();
            }
            
        }

        [NonAction]
        public IActionResult Render(MenuItem menuItem, string connectionString)
        {
            var menuItemParams = JsonConvert.DeserializeObject<Params>(menuItem.Params);

            using(var db = Config.CMSContextFactory.Create(connectionString))
            {
                var item = db.Items.SingleOrDefault(_item => _item.Id == menuItemParams.ItemId);



                //TODO: remove main-menu rendering from here
                //prepare mainmenu
                ViewBag.MenuItems = db.MenuItems
                                            .Where(_item => _item.MenuId == 1) // just main-menu
                                            .Include(_item => _item.ChildMenu)
                                            .ThenInclude(menu => menu.MenuItems)
                                            .ToList()
                ;

                //render result:
                if (item == null)
                {
                    //TODO: throw ResourceNotFoundException
                    ViewBag.Title = "Item Not Found";
                    return View("~/Themes/MainTheme/Layouts/404.cshtml");
                }

                ViewBag.Title = item.Title;
                ViewBag.FullContent = item.FullContent;
                ViewBag.Link = menuItem.Link;

                //TODO:
                if (item.CustomValues!= null && !item.CustomValues.Equals("")) { 
                    JObject obj = JsonConvert.DeserializeObject(item.CustomValues) as JObject;
                    ViewBag.CustomFieldValue = obj.Values().ToList()[0];
                }else
                {
                    ViewBag.CustomFieldValue = "";
                }
                return View("~/Components/SingleArticle/Views/article.cshtml");
            }
        }
    }
}
