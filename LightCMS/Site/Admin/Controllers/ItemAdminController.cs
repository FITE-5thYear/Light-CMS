using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using LightCMS.Config;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;
using System.Collections;


namespace LightCMS.Site.Admin.Controllers
{
    public class ItemAdminController : Controller
    {
        private Settings Settings { get; set; }
        public ItemAdminController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }

        [Route("admin/items")]
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
                return View("~/Site/Admin/Views/Item/Index.cshtml");
            }
        }


        //edit exist item
        [HttpGet]
        [Route("admin/items/edit/{item_id}")]
        public IActionResult GetEditItem(int item_id)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Item_Language il = db.Item_Language.SingleOrDefault(_tem_lang => _tem_lang.Id == item_id);
                ViewBag.item = db.Item_Language.Where(_item => _item.Id == item_id)
                                        .Include(_item => _item.Item)
                                        .SingleOrDefault();
                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == il.LanguageId).ToList();
                return View("~/Site/Admin/Views/Item/edit.cshtml");
            }
        }


        [HttpPost]
        [Route("admin/items/edit")]
        public IActionResult EditItem(int ItemId, int Item_LanguageId, int LanguageId, int CategoryId, string FullContent, string ShortContent, string Title)
        {
            Item i = new Item() { Id = ItemId, CategoryId = CategoryId };
            Item_Language il = new Item_Language() { Id = Item_LanguageId, LanguageId = LanguageId, FullContent = FullContent, Title = Title, ShortContent = ShortContent, Item = i, ItemId = i.Id };

            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                //     db.Update(i);
                db.Entry(i).State = EntityState.Modified;
                db.SaveChanges();

                //    db.Update(il);
                db.Entry(il).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect("/admin/items");
            }
        }


        [HttpGet]
        [Route("admin/items/add")]
        public IActionResult GetAddItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.languages = db.Language.Where(lang => lang.Id != 1).ToList();
                ViewBag.items = db.Item_Language.Where(cat_lang => cat_lang.LanguageId == 1)//show items within the default language..the English one
                .ToList();
                return View("~/Site/Admin/Views/Item/add.cshtml");
            }
        }



        [HttpPost]
        [Route("admin/items/add")]
        public IActionResult AddItem(Item_Language item_language)
        {

            //TODO: authorize, validate...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                db.Add(item_language);
                db.SaveChanges();
            }
            return Redirect("/admin/items");
        }


        [HttpGet]
        [Route("admin/items/create")]
        public IActionResult GetCreateItem()
        {
            //TODO: authorize...
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                ViewBag.cats = db.Category_Language.Where(cat_lang => cat_lang.LanguageId == 1).ToList();

                return View("~/Site/Admin/Views/Item/Create.cshtml");
            }
        }

        [HttpPost]
        [Route("admin/items/create")]
        public IActionResult CreateItem(int CategoryId, string FullContent, string ShortContent, string Title)
        {
            using (var db = CMSContextFactory.Create(Settings.MySqlConnectionString))
            {
                Item item = new Item();
                item.CategoryId = CategoryId;
                db.Add(item);

                Item_Language item_language = new Item_Language();
                item_language.FullContent = FullContent;
                item_language.LanguageId = 1;//default laguage 
                item_language.ShortContent = ShortContent;
                item_language.Title = Title;
                item_language.Item = item;
                db.Add(item_language);

                db.SaveChanges();
            }
            return Redirect("/admin/items");
        }

        [HttpGet]
        [Route("admin/items/delete/{item_id}")]
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
                return Redirect("/admin/items");
            }
        }



    }
}
