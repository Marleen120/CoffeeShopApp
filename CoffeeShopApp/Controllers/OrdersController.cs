using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CoffeeShopApp.Models;

namespace CoffeeShopApp.Controllers
{
    public class OrdersController : Controller
    {
        CoffeeShopDbEntities3 db = new CoffeeShopDbEntities3();

        
            public ActionResult Index()
        {
            var orders = db.Orders.Include("OrderItems").ToList();
            return View(orders);
        }

        
        public ActionResult Create()
        { //dropdown
            ViewBag.ProductList = new SelectList(db.Inventories.ToList(), "ProductName", "ProductName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(string customerName, List<string> productNames, List<int> quantities, List<decimal> unitPrices)
        {
            if (string.IsNullOrEmpty(customerName) ||
                productNames == null || quantities == null || unitPrices == null ||
                productNames.Count != quantities.Count || productNames.Count != unitPrices.Count)
            {
                ModelState.AddModelError("", "Form data is incomplete or invalid.");
                ViewBag.ProductList = new SelectList(db.Inventories.ToList(), "ProductName", "ProductName");
                return View();
            }

         
            var order = new Order
            {
                CustomerName = customerName,
                OrderDate = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges(); 

            if (order.Id == 0)
            {
                ModelState.AddModelError("", "Order creation failed.");
                ViewBag.ProductList = new SelectList(db.Inventories.ToList(), "ProductName", "ProductName");
                return View();
            }

           
            for (int i = 0; i < productNames.Count; i++)
            {
                string productName = productNames[i];
                int qty = quantities[i];
                decimal submittedPrice = unitPrices[i];

                var inventoryItem = db.Inventories.FirstOrDefault(p => p.ProductName == productName);

                if (inventoryItem == null || qty <= 0 || inventoryItem.Quantity < qty)
                {
                    ModelState.AddModelError("", $"Product '{productName}' is invalid or out of stock.");
                    ViewBag.ProductList = new SelectList(db.Inventories.ToList(), "ProductName", "ProductName");
                    return View();
                }

             
                if (inventoryItem.Price != submittedPrice)
                {
                    ModelState.AddModelError("", $"Price mismatch for '{productName}'. Order not saved.");
                    ViewBag.ProductList = new SelectList(db.Inventories.ToList(), "ProductName", "ProductName");
                    return View();
                }

                inventoryItem.Quantity -= qty;
                inventoryItem.LastUpdated = DateTime.Now;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductName = productName,
                    Quantity = qty,
                    UnitPrice = submittedPrice
                };

                db.OrderItems.Add(orderItem);
            }

            db.SaveChanges();  

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var order = db.Orders.Include("OrderItems").FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();

          
            ViewBag.ProductNames = db.Inventories.Select(p => p.ProductName).ToList();

            return View(order);
        }

        [HttpPost]
        public ActionResult Edit(Order order, List<string> newProductNames, List<int> newQuantities, List<decimal> newUnitPrices)
        {
            var existingOrder = db.Orders.Include("OrderItems").FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder == null) return HttpNotFound();

            existingOrder.CustomerName = order.CustomerName;

           
            var submittedItemIds = new List<int>();
            //to get existing items and then remove if needed to
            foreach (string key in Request.Form.AllKeys)
            {
                if (key.StartsWith("OrderItems") && key.EndsWith(".Id"))
                {
                    var value = Request.Form[key];
                    if (int.TryParse(value, out int id))
                        submittedItemIds.Add(id);
                }
            }

          
            var itemsToRemove = existingOrder.OrderItems
                .Where(x => !submittedItemIds.Contains(x.Id))
                .ToList();

            foreach (var removedItem in itemsToRemove)
                db.OrderItems.Remove(removedItem);

            
            foreach (var updatedItem in order.OrderItems ?? new List<OrderItem>())
            {
                var existingItem = existingOrder.OrderItems.FirstOrDefault(x => x.Id == updatedItem.Id);
                if (existingItem != null)
                {
                    existingItem.ProductName = updatedItem.ProductName;
                    existingItem.Quantity = updatedItem.Quantity;
                    existingItem.UnitPrice = updatedItem.UnitPrice;
                }
            }

         //new added product
            if (newProductNames != null)
            {
                for (int i = 0; i < newProductNames.Count; i++)
                {
                    var name = newProductNames[i];
                    var qty = newQuantities[i];
                    var price = newUnitPrices[i];

                    var inventoryItem = db.Inventories.FirstOrDefault(p => p.ProductName == name);
                    if (inventoryItem == null || qty <= 0 || inventoryItem.Quantity < qty)
                    {
                        ModelState.AddModelError("", $"Product '{name}' is invalid or out of stock.");
                        ViewBag.ProductNames = db.Inventories.Select(p => p.ProductName).ToList();
                        return View(order);
                    }

                    if ((inventoryItem.Price) != price)

                    {
                        ModelState.AddModelError("", $"Price mismatch for '{name}'.");
                        ViewBag.ProductNames = db.Inventories.Select(p => p.ProductName).ToList();
                        return View(order);
                    }

                    inventoryItem.Quantity -= qty;
                    inventoryItem.LastUpdated = DateTime.Now;

                    db.OrderItems.Add(new OrderItem
                    {
                        OrderId = existingOrder.Id,
                        ProductName = name,
                        Quantity = qty,
                        UnitPrice = price
                    });
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult Delete(int id)
        {
            var order = db.Orders.Find(id);
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var order = db.Orders.Find(id);
            var items = db.OrderItems.Where(i => i.OrderId == id).ToList();

            foreach (var item in items)
                db.OrderItems.Remove(item);

            db.Orders.Remove(order);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public JsonResult GetPrice(string productName)
        {
            var price = db.Inventories
                          .Where(p => p.ProductName == productName)
                          .Select(p => p.Price)
                          .FirstOrDefault();

            return Json(price, JsonRequestBehavior.AllowGet);
        }
    }
}
