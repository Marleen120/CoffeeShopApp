using CoffeeShopApp.Models;
using System.Web.Mvc;
using System.Linq;


public class AccountController : Controller
{
    private CoffeeShopDbEntities3 db = new CoffeeShopDbEntities3(); 

    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    //jab user submit krega login form then this is called
    public ActionResult Login(Auth model)
    {
        if (ModelState.IsValid)
        {
            var user = db.Auths
                         .FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user != null)
            {
                Session["UserId"] = user.Id;
                Session["Username"] = user.Username;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
        }

        return View(model);
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Login");
    }
}
