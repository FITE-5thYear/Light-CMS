using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace LightCMS.Components.Main.Controllers
{
    [Authorize(Roles = "Admin", ActiveAuthenticationSchemes = "Auth")]
    public class CategoryBackendController : Controller
    {
        private Settings Settings { get; set; }
        public CategoryBackendController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [HttpGet]
        [Route("backend/categories")]
        public IActionResult ListCategories()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats_langs = db.Category_Language
                                        .Include(cat => cat.Language)
                                        .Include(cat => cat.Category)
                                        .ThenInclude(cat => cat.ParentCategory)
                                        .ThenInclude(cat => cat.Category_Language)
                                        .OrderBy(cat => cat.CategoryId)
                                        .ToList();
                return View("~/Components/Main/Category/Views/Backend/Index.cshtml");
            }
        }

        // create new category     
        [HttpGet]
        [Route("backend/categories/create")]
        public IActionResult GetCreateCategory()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.Categories = db.Categories
                                            .Where(cat => cat.Items.Count == 0) // a suitable category to be Parent Cateogry (no items)
                                            .Include(cat => cat.Category_Language)
                                            .ToList();
                return View("~/Components/Main/Category/Views/Backend/Create.cshtml");
            }            
        }

        [HttpPost]
        [Route("backend/categories/create")]
        public IActionResult CreateCategory(string description)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                //TODO: encapsulate in Category Model
                var category = new Category();
                var customFields = this.Request.Form["CustomFields"].ToString();

                //check if it's the default custom fields, discard
                var parsedCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(customFields);
                if(parsedCustomFields.Count == 0 || parsedCustomFields[0].Name.Length == 0)
                {
                    category.CustomFields = null;
                }else
                {
                    category.CustomFields = customFields;
                }                

                db.Add(category);

                Category_Language category_language = new Category_Language() { LanguageId = 1, Category = category, Description = description };
                db.Add(category_language);


                db.SaveChanges();

            }
            return Redirect("/backend/categories");
        }


        //edit existed category
        [HttpGet]
        [Route("backend/categories/edit/{category_id}")]
        public IActionResult GetEditCategory(int category_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                ViewBag.categoryLanguage = db.Category_Language
                                                    .Where(_cat => _cat.Id == category_id)
                                                    .Include(catLang => catLang.Category)
                                                    .SingleOrDefault();

                ViewBag.Categories = db.Categories
                                            .Where(cat => cat.Items.Count == 0) // a suitable category to be Parent Cateogry (no items)
                                            .Include(cat => cat.Category_Language)
                                            .ToList();


                return View("~/Components/Main/Category/Views/Backend/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("backend/categories/edit")]
        public IActionResult EditCategory(Category category, Category_Language categroyLanguage, int categoryId, int category_languageId, int languageId, string customFields)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                category.Id = categoryId;
                categroyLanguage.Id = category_languageId;
                categroyLanguage.Category = category;

                //check if default custom fields provided then discard
                var parsedCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(customFields);
                if (parsedCustomFields.Count == 0 || parsedCustomFields[0].Name.Length == 0)
                {
                    category.CustomFields = null;
                }
                else
                {
                    category.CustomFields = customFields;
                }

                db.Entry(category).State = EntityState.Modified;
                db.Entry(categroyLanguage).State = EntityState.Modified;
                db.SaveChanges();

                return Redirect("/backend/categories");
            }
        }

        [HttpGet]
        [Route("backend/categories/delete/{category_id}")]
        public IActionResult DeleteItem(int category_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var itemToRemove = db.Category_Language
                                            .Include(catLang => catLang.Category)
                                            .ThenInclude(cat => cat.Items)
                                            .SingleOrDefault(x => x.Id == category_id);

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

                        //delete all items that the category has
                        db.Items.RemoveRange(itemToRemove.Category.Items);
                    }
                }

                db.SaveChanges();
                return Redirect("/backend/categories");
            }
        }

        [HttpGet]
        [Route("backend/categories/getCustomFields")]
        public IActionResult GetCustomFields(int catId)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var cat = db.Categories
                             .SingleOrDefault(_cat => _cat.Id == catId);

                if (cat == null)
                    return new StatusCodeResult(404); // not found

                ViewBag.Category = cat;

                if(cat.CustomFields != null)
                {
                    JArray customFields = JsonConvert.DeserializeObject(cat.CustomFields) as JArray;
                    ViewBag.CustomFields = customFields;
                }                
                return PartialView("~/Components/Main/Category/Views/Backend/custom_fields.cshtml");
            }
        }

        // add to an exist category with different language
        [HttpGet]
        [Route("backend/categories/add")]
        public IActionResult GetAddCategory()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.Where(lang => lang.Id != 1).ToList();

                ViewBag.cats = db.Categories
                                    .Where(cat => cat.Category_Language.Count == 1 )//show the categories that doesn't have but one Category_Language which should be in defualt language
                                    .Include(cat => cat.Category_Language)
                                    .ToList();
                return View("~/Components/Main/Category/Views/Backend/add.cshtml");
            }
        }


        [HttpPost]
        [Route("backend/categories/add")]
        public IActionResult AddCategory(Category_Language category_language)
        {
            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(category_language);
                db.SaveChanges();
            }
            return Redirect("/backend/categories");
        }
    }
}
