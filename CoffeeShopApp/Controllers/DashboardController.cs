using System;
using System.Linq;
using System.Web.Mvc;
using CoffeeShopApp.Models;
using System.Data.Entity;

namespace CoffeeShopApp.Controllers
{
    public class DashboardController : Controller
    {
        CoffeeShopDbEntities3 db = new CoffeeShopDbEntities3();

        public ActionResult Insights()
        {
            var today = DateTime.Today;

            var todayOrders = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == today)
                .Include(o => o.OrderItems) 
                .ToList();

            //Total Orders
            ViewBag.TotalOrders = todayOrders.Count();

            // Total Sales 
            ViewBag.TotalSales = todayOrders
                .SelectMany(o => o.OrderItems)
                .Sum(oi => (decimal?)oi.Quantity * oi.UnitPrice) ?? 0;

            // Top Products Sold Today
            var model = todayOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductName)
                .Select(g => new ProductStatsViewModel
                {
                    Product = g.Key,
                    Total =(int) g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            return View(model);
        }

        public JsonResult MonthlySalesChart()
        {
            var monthlySales = db.Orders
               .Where(o => o.OrderDate != default(DateTime))

                .Include(o => o.OrderItems)
                .ToList()
                .GroupBy(o => o.OrderDate.Month)

                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.SelectMany(o => o.OrderItems)
                             .Sum(i => (decimal?)i.Quantity * i.UnitPrice) ?? 0
                })
                .OrderBy(x => x.Month)
                .Select(x => new
                {
                    Month = new DateTime(2025, x.Month, 1).ToString("MMM"),
                    x.Total
                });

            return Json(monthlySales, JsonRequestBehavior.AllowGet);
        }
    }
}
