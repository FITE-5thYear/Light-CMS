using System;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using System.Linq;
using LightCMS.Components.Main.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LightCMS.Components;

namespace Components.Main {
    public class MainComponentController : Controller {
        public static void Bootstrap(string sqlString)
        {
            //find or create pattern
            using (var db = CMSContextFactory.Create(sqlString))
            {
                //init category
                var mainCategory = db.Categories.SingleOrDefault(cat => cat.Id == 1);
                if(mainCategory == null)
                {
                    mainCategory = new Category()
                    {
                        Id = 1,
                        Description = "Main Category"
                    };
                    db.Add(mainCategory);
                }

                //init menu
                var mainMenu = db.Menus.SingleOrDefault(menu => menu.Id == 1);
                if(mainMenu == null)
                {
                    mainMenu = new Menu()
                    {
                        Id = 1,
                        CategoryId = 1, // main category
                        Description = "Main Menu - Displayed at header"
                    };
                    db.Add(mainMenu);
                }

                db.SaveChanges();
            }
        }

        //TODO: remove @connectioString param
        public IActionResult GetMenuItemView(string link, string connectionString)
        {
            //first fetch menu item
            using (var db = CMSContextFactory.Create(connectionString))
            {
                if (link == null)
                { // requesting index page
                    var indexLink = db.MenuItems.SingleOrDefault(_item => _item.IsIndexPage).Link;
                    return this.GetMenuItemView(indexLink, connectionString); //invoke self with the new link
                }

                var menuItem = db.MenuItems
                                 .Include(item => item.MenuItemType)
                                 .ThenInclude(menuItemType => menuItemType.Extension)
                                 .SingleOrDefault(item => item.Link == link);



                if (menuItem == null) {
                    //TODO: remove main-menu rendering from here
                    //prepare mainmenu
                    ViewBag.MenuItems = db.MenuItems
                                                .Where(_item => _item.MenuId == 1 ) // just main-menu
                                                .Include(_item => _item.ChildMenu)
                                                .ThenInclude(menu => menu.MenuItems)
                                                .ToList()
                    ;

                    return new StatusCodeResult(404);
                }

                Type componentType = Type.GetType("LightCMS." + menuItem.MenuItemType.Extension.Namespace);
                IMenuItemTypeComponent component = Activator.CreateInstance(componentType) as IMenuItemTypeComponent;

                return component.Render(menuItem, connectionString);
            }
        }
    }
}
