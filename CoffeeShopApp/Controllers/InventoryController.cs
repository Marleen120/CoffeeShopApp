using System;
using System.Linq;
using System.Web.Mvc;
using CoffeeShopApp.Models;

namespace CoffeeShopApp.Controllers
{
    public class InventoryController : Controller
    {
        CoffeeShopDbEntities3 db = new CoffeeShopDbEntities3();

        public ActionResult Index()
        {
            return View(db.Inventories.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Inventory item)
        {
            if (ModelState.IsValid)
            {
               
                bool exists = db.Inventories
                                .Any(p => p.ProductName.ToLower() == item.ProductName.ToLower());

                if (exists)
                {
                    ModelState.AddModelError("ProductName", "This product already exists in the inventory.");
                    return View(item);
                }

                try
                {
                    item.LastUpdated = DateTime.Now;
                    db.Inventories.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException )
                {
                    // UNIQUE constraint
                    ModelState.AddModelError("ProductName", "Duplicate product not allowed. Please update the existing item.");
                    return View(item);
                }
            }

            return View(item);
        }


        public ActionResult Edit(int id)
        {
            var item = db.Inventories.Find(id);
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Inventory item)
        {
            if (ModelState.IsValid)
            {
                item.LastUpdated = DateTime.Now;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public ActionResult Delete(int id)
        {
            var item = db.Inventories.Find(id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.Inventories.Find(id);
            db.Inventories.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
