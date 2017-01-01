using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LightCMS.Components.Main.Controllers
{
    public class ItemBackendController : Controller
    {
        private Settings Settings { get; set; }
        public ItemBackendController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [Route("backend/items")]
        public IActionResult ListItems()
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.items = db.Item_Language
                                   .Include(_item_language => _item_language.Item)
                                   .ThenInclude(item => item.Category)
                                   .Include(_item_language => _item_language.Language)
                                   .OrderBy(_item_language => _item_language.ItemId)
                                   .ToList();
                return View("~/Components/Main/Item/Views/Backend/Index.cshtml");
            }
        }

        [HttpGet]
        [Route("backend/items/create")]
        public IActionResult GetCreateItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1).ToList();

                return View("~/Components/Main/Item/Views/Backend/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("backend/items/create")]
        public IActionResult CreateItem(Item item, Item_Language item_language)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {

                //get chosen category, to parse custom fields if existed...
                var category = db.Categories.SingleOrDefault(cat => cat.Id == item.CategoryId);

                string customValuesBuilder = null;

                if(category.CustomFields != null)
                {
                    var customFields = JsonConvert.DeserializeObject<List<CustomField>>(category.CustomFields);

                    customValuesBuilder = "{";
                    foreach (var customField in customFields)
                    {
                        var nameOfField = customField.Name;
                        var valueOfField = this.Request.Form[nameOfField];
                        customValuesBuilder += "\"" + nameOfField + "\":" + valueOfField;
                    }

                    customValuesBuilder += "}";
                }

                item.CustomValues = customValuesBuilder;
                db.Add(item);

                item_language.LanguageId = 1;//default language              
                item_language.Item = item;
                db.Add(item_language);

                db.SaveChanges();
            }
            return Redirect("/backend/items");
        }

        //edit existed item
        [HttpGet]
        [Route("backend/items/edit/{item_language_id}")]
        public IActionResult GetEditItem(int item_language_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Item_Language itemLanguage = db.Item_Language
                                                    .Include(_itemLanguage => _itemLanguage.Item)
                                                    .ThenInclude(item => item.Category)
                                                    .SingleOrDefault(_itemLanguage => _itemLanguage.Id == item_language_id);

                ViewBag.itemLanguage = itemLanguage;

                if (itemLanguage.Item.Category.CustomFields != null)
                {
                    ViewBag.catCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(itemLanguage.Item.Category.CustomFields);
                    ViewBag.itemCustomValue = JsonConvert.DeserializeObject<dynamic>(itemLanguage.Item.CustomValues) as JObject;
                }
                else
                    ViewBag.catCustomFields = new List<CustomField>();

                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == itemLanguage.LanguageId).ToList();
                
                return View("~/Components/Main/Item/Views/Backend/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("backend/items/edit")]
        public IActionResult EditItem(Item item, Item_Language itemLanguage)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //get ids
                item.Id = int.Parse(this.Request.Form["ItemId"]);
                itemLanguage.Id = int.Parse(this.Request.Form["Item_LanguageId"]);

                //get chosen category
                var category = db.Categories.SingleOrDefault(cat => cat.Id == item.CategoryId);
                var customFields = JsonConvert.DeserializeObject<List<CustomField>>(category.CustomFields);

                string customValuesBuilder = "{";
                foreach (var customField in customFields)
                {
                    var nameOfField = customField.Name;
                    var valueOfField = this.Request.Form[nameOfField];
                    customValuesBuilder += "\"" + nameOfField + "\":" + valueOfField;
                }

                customValuesBuilder += "}";

                item.CustomValues = customValuesBuilder;
                db.Entry(item).State = EntityState.Modified;
                              
                itemLanguage.Item = item;
                db.Entry(itemLanguage).State = EntityState.Modified;

                db.SaveChanges();                
                return Redirect("/backend/items");
            }
        }


        [HttpGet]
        [Route("backend/items/add")]
        public IActionResult GetAddItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.Where(lang => lang.Id != 1).ToList();
                ViewBag.items = db.Item_Language.Where(cat_lang => cat_lang.LanguageId == 1)//show items within the default language..the English one
                .ToList();
                return View("~/Components/Main/Item/Views/Backend/add.cshtml");
            }
        }



        [HttpPost]
        [Route("backend/items/add")]
        public IActionResult AddItem(Item_Language item_language)
        {

            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(item_language);
                db.SaveChanges();
            }
            return Redirect("/backend/items");
        }
        
        [HttpGet]
        [Route("backend/items/delete/{item_id}")]
        public IActionResult DeleteItem(int item_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                var itemToRemove = db.Item_Language.SingleOrDefault(x => x.Id == item_id); 
          
                if (itemToRemove != null)
                {
                    db.Item_Language.Remove(itemToRemove);

                    //delete the default item language will delete other items languages and the origin item 
                    if (itemToRemove.LanguageId == 1)
                    {
                        var OtherItemsToRemove = db.Item_Language.Where(x => x.ItemId == itemToRemove.ItemId).ToList();
                        foreach (var v in OtherItemsToRemove)
                        {
                            db.Item_Language.Remove(v);

                        }

                        var item = db.Items.SingleOrDefault(x => x.Id == itemToRemove.ItemId);
                        db.Items.Remove(item);
                    }

                }

                db.SaveChanges();
                return Redirect("/backend/items");
            }
        }



    }
}
