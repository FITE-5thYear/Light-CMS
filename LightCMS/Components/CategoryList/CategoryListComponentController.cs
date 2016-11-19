using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LightCMS.Components.CategoryList.Models;
using LightCMS.Components.Main.Models;
using LightCMS.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightCMS.Components.CategoryList
{
    public class CategoryListComponentController : Controller, IMenuItemTypeComponent
    {
        public void Bootstrap(string connectionString)
        {
            using (var db = CMSContextFactory.Create(connectionString))
            {
                //init extendsion
                var extension = db.Extensions.SingleOrDefault(ext => ext.Namespace == "Components.CategoryList.CategoryListComponentController");
                if (extension == null)
                {
                    extension = new Extension()
                    {
                        Namespace = "Components.CategoryList.CategoryListComponentController"
                    };
                    db.Add(extension);
                }


                //init 'Single Article' menu type
                var menuItemType = db.MenuItemTypes.SingleOrDefault(type => type.Id == 2);
                if (menuItemType == null)
                {
                    menuItemType = new MenuItemType()
                    {
                        Id = 2,
                        Name = "Category List",
                        ExtensionId = 2
                    };
                    db.Add(menuItemType);
                }

                db.SaveChanges();
            }
        }

        public IActionResult Render(MenuItem menuItem, string connectionString)
        {
            var menuItemParams = JsonConvert.DeserializeObject<Params>(menuItem.Params);

            using (var db = Config.CMSContextFactory.Create(connectionString))
            {
                var category = db.Categories
                                            .Include(cat => cat.Items)
                                           .SingleOrDefault(
                                                cat => cat.Id == menuItemParams.CategoryId);

                //TODO: remove main-menu rendering from here
                //prepare mainmenu
                ViewBag.MenuItems = db.MenuItems
                                            .Include(_item => _item.ChildMenu)
                                            .ThenInclude(menu => menu.MenuItems)
                                            .Where(_item => _item.MenuId == 1) // just main-menu
                                            .ToList()
                ;
                ViewBag.Link = menuItem.Link;

                //render result:
                if (category == null)
                {
                    //TODO: throw ResourceNotFoundException
                    ViewBag.Title = "Category Not Found";
                    return View("~/Themes/MainTheme/Layouts/404.cshtml");
                }

                ViewBag.items = category.Items;
                
                return View("~/Components/CategoryList/Views/list.cshtml");
            }
        }
    }
}
