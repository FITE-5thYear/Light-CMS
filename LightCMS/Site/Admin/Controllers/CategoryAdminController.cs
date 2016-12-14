using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;


namespace LightCMS.Site.Admin.Controllers
{
    public class CategoryAdminController : Controller
    {
        private Settings Settings { get; set; }
        public CategoryAdminController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [HttpGet]
        [Route("admin/categories")]
        public IActionResult ListCategories()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats_langs = db.Category_Language
                                        .Include(cat => cat.Language)
                                        .OrderBy(cat => cat.CategoryId)
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
                ViewBag.languages = db.Language.Where(lang => lang.Id != 1).ToList();
                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1)//show the category within the default language..the English one
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
                var temp=db.Categories.Last();
                Category category = new Category();
                category.Id = temp.Id + 1;
                db.Add(category);

                Category_Language category_language = new Category_Language() { LanguageId = 1, Category = category, Description = Description };
                db.Add(category_language);

                db.SaveChanges();

            }
            return Redirect("/admin/categories");
        }


        //edit exsist category
        [HttpGet]
        [Route("admin/categories/edit/{category_id}")]
        public IActionResult GetEditCategory(int category_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                ViewBag.cat = db.Category_Language.Where(_cat => _cat.Id == category_id)
                                        .SingleOrDefault();

                return View("~/Site/Admin/Views/category/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("admin/categories/edit")]
        public IActionResult EditCategory(int CategoryId, int Category_LanguageId, int LanguageId, string Description)
        {
            Category c = new Category() { Id = CategoryId };
            Category_Language cl = new Category_Language() { Id = Category_LanguageId, LanguageId = LanguageId, Description = Description, Category = c, CategoryId = CategoryId };
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                db.Entry(cl).State = EntityState.Modified;
                db.SaveChanges();

                return Redirect("/admin/categories");
            }
        }

        [HttpGet]
        [Route("admin/categories/delete/{category_id}")]
        public IActionResult DeleteItem(int category_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var itemToRemove = db.Category_Language.SingleOrDefault(x => x.Id == category_id);

                if (itemToRemove != null)
                {
                    db.Category_Language.Remove(itemToRemove);

                    //delete the default item language will delete other items languages and the origin item 
                    if (itemToRemove.LanguageId == 1)
                    {
                        var OtherItemsToRemove = db.Category_Language.Where(x => x.CategoryId == itemToRemove.CategoryId).ToList();
                        foreach (var v in OtherItemsToRemove)
                        {
                            db.Category_Language.Remove(v);
                        }

                        var item = db.Categories.SingleOrDefault(x => x.Id == itemToRemove.CategoryId);
                        db.Categories.Remove(item);
                    }

                }

                db.SaveChanges();
                return Redirect("/admin/categories");
            }
        }
    }
}
