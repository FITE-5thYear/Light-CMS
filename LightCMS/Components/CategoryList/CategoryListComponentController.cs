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

        public IActionResult Render(MenuItem menuItem, string connectionString, int langId, IBundle bundle)
        {

            if (!AuthorizationHelper.IsAuthorized(bundle.UserRole, menuItem.Role))
                return Redirect("/forbidden");

            var menuItemParams = JsonConvert.DeserializeObject<Params>(menuItem.Params);

            using (var db = Config.CMSContextFactory.Create(connectionString))
            {
                var category = db.Category_Language.Where(cat => cat.CategoryId == menuItemParams.CategoryId).ToList();
                                
                ViewBag.Link = menuItem.Link;

                //render result:
                if (category == null)
                {
                    //TODO: throw ResourceNotFoundException
                    ViewBag.Title = "Category Not Found";
                    return View("~/Themes/MainTheme/Layouts/404.cshtml");
                }
                var lags = db.Language.ToList();
                ViewBag.Languages = lags;
                ViewBag.items = db.Item_Language.Include(x => x.Item).Where(item_lang => item_lang.Item.CategoryId == menuItemParams.CategoryId && item_lang.LanguageId==langId).ToList();

                ViewBag.Bundle = bundle;

                return View("~/Components/CategoryList/Views/list.cshtml");
            }
        }
    }
}
