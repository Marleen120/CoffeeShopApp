using System.Data.Entity;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using CoffeeShopApp.Models;

public class HomeController : Controller //inherit kare ga controller base class
{
    CoffeeShopDbEntities3 db = new CoffeeShopDbEntities3();

    public ActionResult Index()
    {
        if (Session["UserId"] == null)
            //if no user id is stored in session it redirects to login 
            return RedirectToAction("Login", "Account");

        return View(); 
    }

    public ActionResult Reports() => View();
    //go to views/home/reports.cshtml

    public ActionResult SalesChart()
    {
        var rawData = db.OrderItems
            .Where(o => o.ProductName != null)
            .GroupBy(o => o.ProductName.Trim().ToLower())
            .Select(g => new
            {
                ProductName = g.Key,
                TotalSold = g.Sum(x => x.Quantity)
            })
            .ToList(); 

        var data = rawData.Select(g => new
        {
            ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.ProductName),
            TotalSold = g.TotalSold
        }).ToList(); 

        return Json(data, JsonRequestBehavior.AllowGet);
        //Send that list as JSON to the browser
    }

   
    public ActionResult InventoryChartData()
    {
        var rawData = db.Inventories
            .Where(i => i.ProductName != null)
            .Select(i => new
            {
                ProductName = i.ProductName.Trim(),
                Quantity = i.Quantity  
            })
            .ToList(); 

        var data = rawData.Select(i => new
        {
            ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(i.ProductName),
            Quantity = i.Quantity
        }).ToList(); 

        return Json(data, JsonRequestBehavior.AllowGet);
    }
    public ActionResult TodaySalesChart()
    {
        DateTime today = DateTime.Today;

        var todayOrders = db.Orders
            .Where(o => o.OrderDate != null && DbFunctions.TruncateTime(o.OrderDate) == today)
            .Include(o => o.OrderItems) //loads related OrderItems
            .ToList(); 

        var groupedData = todayOrders
            .SelectMany(o => o.OrderItems)
            .Where(i => i.ProductName != null)
            .GroupBy(i => i.ProductName.Trim().ToLower())
            .Select(g => new
            {
                ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.Key),
                TotalSold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5) //first 5
            .ToList();

        return Json(groupedData, JsonRequestBehavior.AllowGet);
    }


}
