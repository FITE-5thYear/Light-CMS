using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LightCMS.Components.Main.Models;
using LightCMS.Components.SingleArticle.Models;
using LightCMS.Config;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
                if (extension == null)
                {
                    extension = new Extension()
                    {
                        Id = 1,
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
                        Link = "home",
                        MenuItemTypeId = 1,
                        Params = "{ItemId : 1}", // item will be initialized next
                        IsMenu = false,
                        ChildMenuId = 1, //TODO: this should be zero
                        MenuId = 1,
                        IsIndexPage = true,
                        Role = db.Roles.SingleOrDefault(role => role.Name.Equals("Public"))
                    };
                    db.Add(menuItem);

                    MenuItem_Language menuItem_laguage = new MenuItem_Language()
                    {
                        Id = 1,
                        LanguageId = 1,
                        Label = "Home",
                        MenuItemId = 1
                    };
                    db.Add(menuItem_laguage);

                    menuItem_laguage = new MenuItem_Language()
                    {
                        Id = 2,
                        LanguageId = 2,
                        Label = "الرئيسية",
                        MenuItemId = 1
                    };
                    db.Add(menuItem_laguage);


                }

                //init an item 
                var _item = db.Items.SingleOrDefault(item => item.Id == 1);
                if (_item == null)
                {
                    _item = new Item()
                    {
                        Id = 1,
                        CategoryId = 1,
                        Role = db.Roles.SingleOrDefault(role => role.Name.Equals("Public"))
                    };
                    db.Add(_item);

                    Item_Language _item_language = new Item_Language()
                    {
                        ItemId = 1,
                        LanguageId = 1,
                        Title = "Welcome To LightCMS",
                        ShortContent = "Welcome to LightCMS",
                        FullContent = @"Thank you for using LightCMS, you can access the <strong>Admin Panel</strong> by navigating to <strong>/admin</admin><br>.
                                       This is a sample page which was generated automatically by LightCMS."
                    };
                    db.Add(_item_language);
                    _item_language = new Item_Language()
                    {
                        ItemId = 1,
                        LanguageId = 2,
                        Title = "LightCMS أهلاً بك في ",
                        ShortContent = "LightCMS أهلاً بك في",
                        FullContent = @"نشكر ثقتك بنا.. يمكنك إضافة المزيد  باستخدام admin"
                    };
                    db.Add(_item_language);
                }
                 
                db.SaveChanges();
            }

        }

        [NonAction]
        public IActionResult Render(MenuItem menuItem, string connectionString, int language_id, IBundle bundle)
        {

            if (!AuthorizationHelper.IsAuthorized(bundle.UserRole, menuItem.Role))
                 return Redirect("/forbidden");
            

            var menuItemParams = JsonConvert.DeserializeObject<Params>(menuItem.Params);

            using (var db = Config.CMSContextFactory.Create(connectionString))
            {
                var item = db.Items.SingleOrDefault(_item => _item.Id == menuItemParams.ItemId);
                
                //render result:
                if (item == null)
                {
                    //TODO: throw ResourceNotFoundException
                    ViewBag.Title = "Item Not Found";
                    return View("~/Themes/MainTheme/Layouts/404.cshtml");
                }
                
                //TODO:
                if (item.CustomValues!= null && !item.CustomValues.Equals("")) { 
                    JObject obj = JsonConvert.DeserializeObject(item.CustomValues) as JObject;
                    ViewBag.CustomFieldValue = obj.Values().ToList()[0];
                }else
                {
                    ViewBag.CustomFieldValue = "";
                }

                var lags = db.Language.ToList();
                ViewBag.Languages = lags;

                var temp = db.Item_Language.SingleOrDefault(_Item_Language => _Item_Language.LanguageId == language_id && _Item_Language.ItemId == item.Id);
               
                ViewBag.Title = temp.Title;
                ViewBag.FullContent = temp.FullContent;
                ViewBag.Link = menuItem.Link;

                ViewBag.Bundle = bundle;
                return View("~/Components/SingleArticle/Views/article.cshtml");
            }
        }
    }
}
