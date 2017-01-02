using Components.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components;
using Microsoft.AspNetCore.Authorization;

namespace LightCMS.Controllers
{
    /*
     *  This is the main site router
     */
    public class RouterController : Controller
    {
        private Settings Settings { get; set; }

        private readonly IHttpContextAccessor Context;

        private CMSDBContext db;

        public RouterController(IOptions<Settings> settings, IHttpContextAccessor context, CMSDBContext dbContext)
        {
            Settings = settings.Value;
            Context = context;
            db = dbContext;
        }

        [AllowAnonymous]
        [Authorize(ActiveAuthenticationSchemes = "Auth")]
        public IActionResult Index(string link)
        {
            if (Context.HttpContext.Session.GetInt32("language") == null)
                Context.HttpContext.Session.SetInt32("language", 1); // default is english

            int lang = (int) Context.HttpContext.Session.GetInt32("language");
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var selected_language = db.Language.SingleOrDefault(_lang => _lang.Id == lang);
                TempData["orientation"] = selected_language.orientation;
            

                IBundle bundle = cookMainBundle(lang, db);
                return (new MainComponentController()).GetMenuItemView(link, Settings.MySqlConnectionString, lang, bundle);
            }
            
        }


        public IActionResult SetLanguage(int language_id)
        {
            HttpContext.Session.SetInt32("language", language_id);        
            return RedirectToAction( "Index");
        }


        private IBundle cookMainBundle(int langId,CMSDBContext db)
        {
            var role = AuthorizationHelper.GetUserRole(this.User, db);

            if(role == null) //public role
            {
                role = db.Roles.SingleOrDefault(_role => _role.Name.Equals("Public"));
            }

            //TODO: tidy !!!
            var mainMenuItems= db.MenuItem_Language
                                        .Include(_menu_item => _menu_item.MenuItem)
                                            .ThenInclude(_item => _item.ChildMenu)
                                                 .ThenInclude(menu => menu.MenuItems)
                                        .Where(_item => _item.MenuItem.MenuId == 1 && _item.LanguageId == langId) // just main-menu
                                        .ToList();

            //remove menu items which are not public or the current user doesn't have access to
            mainMenuItems.RemoveAll(menuItemLang =>
            {
                var itemRole = db.Roles.SingleOrDefault(_role => _role.Id == menuItemLang.MenuItem.RoleId);
                if (!itemRole.Name.Equals(role.Name) && !itemRole.Name.Equals("Public"))
                    return true;
                return false;
            });

            return new MainBundle()
            {
                MainMenuItems = mainMenuItems,
                UserRole = role
            };
        }

    }
}
