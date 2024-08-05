using Hutch.Models;
using Hutch.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Hutch.Controllers
{
    public class HomeController : Controller
    {
        HutchEntities db = new HutchEntities();

        /* Home Page */
        public ActionResult Index()
        {
            IndexViewModel model = new IndexViewModel();
            var categories = db.Categories.ToList();
            var topSellingProducts = db.Products.Where(p => p.p_arrival == "Top").ToList();
            var newArrivals = db.Products.Where(p => p.p_arrival == "New").ToList();
            model.Categories = categories;
            model.TopSellingProducts = topSellingProducts;
            model.NewArrivals = newArrivals;
            return View(model);
        }

        public ActionResult FilterByCategory(string category)
        {
            if (category != null)
            {
                Session["p_category"] = category;

                // Delete the sessions which have been created during searching or search by brand products 
                Session.Remove("searchTerm");
                Session.Remove("brand");
            }
            return RedirectToAction("DisplayProduct", "Product");
        }

        public ActionResult FilterByBrand(string brand) {
            if(brand != null) {
                Session["brand"] = brand;

                // Delete the sessions which have been created during searching or search by category products
                Session.Remove("p_category");
                Session.Remove("searchTerm");
            }
            return RedirectToAction("DisplayProduct", "Product");
        }

        /* I could have used the session based storage however that will make overhead
         * to the server side memory management and moreover that would not fetch me 
         fresh (latest) data on each db request. Instead I am going with fetching data
        from db on each req*/
    }
}