using System;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using System.Linq;
using LightCMS.Components.Main.Models;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components;

namespace Components.Main {
    public class MainComponentController : Controller, IComponent {
        public void Bootstrap(string sqlString)
        {
            createRoles(sqlString);
            createSuperUser(sqlString);


            //find or create pattern
            using (var db = CMSContextFactory.Create(sqlString))
            {
                //add language if not exist
                var lang = db.Language.SingleOrDefault(laganguage => laganguage.Name == "English");
                if (lang == null)
                {
                    lang = new Language();
                    lang.Name = "English";
                    lang.Id = 1;
                    lang.orientation = "ltr";
                    db.Add(lang);
                }

                lang = db.Language.SingleOrDefault(laganguage => laganguage.Name == "Arabic");
                if (lang == null)
                {
                    lang = new Language();
                    lang.Name = "Arabic";
                    lang.Id = 2;
                    lang.orientation = "rtl";
                    db.Add(lang);
                }

                //init category
                var mainCategory = db.Categories.SingleOrDefault(cat => cat.Id == 1);
                if (mainCategory == null)
                {
                    mainCategory = new Category()
                    {
                        Id = 1,
                        Role = db.Roles.SingleOrDefault(role => role.Name.Equals("Public"))
                    };
                    db.Add(mainCategory);
                    Category_Language category_language = new Category_Language()
                    {
                        Id = 1,
                        Description = "Main Category",
                        LanguageId = 1,
                        CategoryId=1

                    };
                    db.Add(category_language);
                   
                     category_language = new Category_Language()
                    {
                        Id = 2,
                        Description = "الرئيسية",
                        LanguageId = 2,
                        CategoryId=1
                    };
                    db.Add(category_language);

                }

                //init menu
                var mainMenu = db.Menus.SingleOrDefault(menu => menu.Id == 1);
                if (mainMenu == null)
                {
                    mainMenu = new Menu()
                    {
                        Id = 1,
                        CategoryId = 1, // main category
                        //Description = "Main Menu - Displayed at header"
                    };
                    db.Add(mainMenu);
                    Menu_Language mainMenu_Language = new Menu_Language()
                    {
                        Id = 1,
                        LanguageId=1,
                        MenuId=1,
                        Description = "Main Menu - Displayed at header"
                    };
                    db.Add(mainMenu_Language);
                   
                     mainMenu_Language = new Menu_Language()
                    {
                        Id = 2,
                        LanguageId=2,
                        MenuId=1,
                        Description = "اللائحة الرئيسية"
                    };
                    db.Add(mainMenu_Language);
                }

                db.SaveChanges();
            }

        }

        //TODO: remove @connectioString param
        public IActionResult GetMenuItemView(string link, string connectionString, int langId, IBundle bundle)
        {
            //first fetch menu item
            using (var db = CMSContextFactory.Create(connectionString))
            {
                if (link == null)
                { // requesting index page
                    var indexLink = db.MenuItems.SingleOrDefault(_item => _item.IsIndexPage).Link;
                    return this.GetMenuItemView(indexLink, connectionString, langId, bundle); //invoke self with the new link
                }

                var menuItem = db.MenuItems
                                 .Include(item => item.MenuItemType)
                                 .ThenInclude(menuItemType => menuItemType.Extension)
                                 .Include(item => item.Role)
                                 .SingleOrDefault(item => item.Link == link);



                if (menuItem == null)
                {
                    return new StatusCodeResult(404);
                }

                Type componentType = Type.GetType("LightCMS." + menuItem.MenuItemType.Extension.Namespace);
                IMenuItemTypeComponent component = Activator.CreateInstance(componentType) as IMenuItemTypeComponent;
               
                return component.Render(menuItem, connectionString, langId, bundle);
            }
        }

        private void createRoles(string connectionString)
        {
            using (var db = CMSContextFactory.Create(connectionString))
            {
                var publicRole = db.Roles.SingleOrDefault(role => role.Name.Equals("Public"));
                if(publicRole == null)
                {
                    publicRole = new Role()
                    {
                        Name = "Public"
                    };

                    db.Add(publicRole);
                    
                }

                var adminRole = db.Roles.SingleOrDefault(role => role.Name.Equals("Admin"));
                if(adminRole == null)
                {
                    adminRole = new Role()
                    {
                        Name = "Admin"
                    };

                    db.Add(adminRole);
                }

                var registeredRole = db.Roles.SingleOrDefault(role => role.Name.Equals("Registered"));
                if(registeredRole == null)
                {
                    registeredRole = new Role()
                    {
                        Name = "Registered"
                    };
                    db.Add(registeredRole);
                }

                db.SaveChanges();
            }
        }

        private void createSuperUser(string connectionString)
        {
            using (var db = CMSContextFactory.Create(connectionString))
            {

                //findOrCreate pattern:
                var user = db.Users.SingleOrDefault(_user => _user.Username.Equals("Admin"));

                if(user == null)
                {                    
                    var role = db.Roles.SingleOrDefault(_role => _role.Name.Equals("Admin"));


                    User superUser = new User()
                    {
                        Username = "Admin",
                        FirstName = "Admin",
                        LastName = "Admin",
                        Email = "Admin@Admin.com",
                        Password = "admin",
                        InsertedAt = DateTime.Now,
                        IsApproved = true
                    };

                    UserRole userRole = new UserRole()
                    {
                        Role = role,
                        User = superUser
                    };

                    superUser.UserRoles.Add(userRole);
                    role.UserRoles.Add(userRole);

                    
                    db.Add(userRole);
                    db.Add(superUser);

                    db.SaveChanges();
                }
                
            }
        }
    }
}
