using Hutch.Models;
using Hutch.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Hutch.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        HutchEntities db = new HutchEntities();

        /* Products */

        public ActionResult DisplayProduct(string searchInput, List<string> filters)
        {
            
            var products = db.Products.ToList();

            if(searchInput != null)
            {
                Session["searchTerm"] = searchInput;
            }
           

            if (searchInput != null || Session["searchTerm"] != null)
            {
                string search = Session["searchTerm"].ToString();

                /* First check the search input by category */
                var productItem = db.Products.Where(p => p.p_category.ToLower().StartsWith(search.ToLower()));

                /* If not found, where method returns empty list. So count if the list has any items */
                if (productItem.Count() > 0)
                {
                    products = productItem.ToList();
                }

                /* If user does not search by category then check if searched by product type */
                else if (db.Products.Where(p => p.p_type.ToLower().StartsWith(search.ToLower())).Count() > 0)
                {
                    products = db.Products.Where(p => p.p_type.ToLower().StartsWith(search.ToLower())).ToList();  
                }

                /* If user does not search by category and type then check if searched by product brand */
                else if (db.Products.Where(p => p.p_brand.ToLower() == search.ToLower()).ToList().Count > 0)
                {
                    products = db.Products.Where(p => p.p_brand.ToLower() == search.ToLower()).ToList();
                }

                /* If user does not search by category and type and brand then check if searched by product name */
                // If search input does not matches with any product then products list will have empty list of products
                else
                {
                    products = db.Products.Where(p => p.p_name.ToLower().StartsWith(search.ToLower())).ToList();
                }

                /* When search input exists then we do not need the category session from category card */
                Session.Remove("p_category");
                Session.Remove("brand");
            }

            
            /* If category from category card exists then retrieve the products that matches the category and store to the session. */
            else if (Session["p_category"] != null)
            {
                string category = Session["p_category"].ToString();
                products = db.Products.Where(p => p.p_category.ToLower() == category.ToLower()).ToList();
            }

            else if (Session["brand"] != null)
            {
                string brand = Session["brand"].ToString();
                products = db.Products.Where(p => p.p_brand.ToLower() == brand.ToLower()).ToList();
            }


            /* When an user directly go through url */
            if (Session["p_category"] == null && Session["searchTerm"] == null && Session["brand"] == null && searchInput == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            /* Filter the products */
            var filterObj = new FilterLogic();
            products = filterObj.filter(filters, products);

            /* Wishlist Items */
            AllProductsViewModel model = new AllProductsViewModel();
            model.Products = products;

            /* If an user logs in then check which items are already wishlisted */
            if (Session["u_email"] != null)
            {
                string userEmail = Session["u_email"].ToString();
                /* Filtering out only the logged in user's wishlist product ids. We do not need to fetch entire wishlist entity and pass it to the view */
                var wishlist = db.Wishlists.Where(p => p.email == userEmail).ToList();
                List<string> wishlistItemId = new List<string>();
                foreach (var item in wishlist)
                {
                    /* Double check if the wishlisted item belongs to the logged in user */
                    if (item.email == Session["u_email"].ToString())
                    {
                        /* Adding the wishlist items id to the list */
                        wishlistItemId.Add(item.pid);
                    }
                }
                model.WishlistIds = wishlistItemId;
            }

            /* If an user does not log in then do not filter out wishlist items. Assign just a empty list*/
            else
            {
                model.WishlistIds = new List<string>();
            }

            return View(model);

        }

        public ActionResult Details(string pid)
        {
            
            var product = db.Products.Find(pid);
            if(product == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
            }

            CheckoutViewModel model = new CheckoutViewModel();
            model.CheckoutItem = product;

            if (Session["u_email"] != null)
            {
                string userEmail = Session["u_email"].ToString();

                bool isCarted = db.Carts.Any(item => item.pid == pid && item.email == userEmail);
                model.isCarted = isCarted;

                bool isWishlisted = db.Wishlists.Any(item =>item.pid == pid && item.email ==  userEmail);
                model.isWishlisted = isWishlisted;

                bool isBought = db.OrderItems.Any(item => item.email == userEmail && item.pid == pid);
                model.isBought = isBought;
            }
            else
            {
                model.isCarted= false;
                model.isWishlisted= false;
                model.isBought= false;
            }
            
            var feedback = db.Feedbacks.Where(f => f.pid == pid).ToList();
            model.Feedbacks = feedback;

            return View(model);
        }

        public ActionResult Feedback()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Feedback(string email, string pid, string feedback)
        {
            if (Session["u_email"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string userName = Session["u_name"].ToString();
            Feedback model = new Feedback();
            model.email = email;
            model.name = userName;
            model.pid = pid;
            model.f_message = feedback;
            db.Feedbacks.Add(model);
            db.SaveChanges();
            return View();
        }

        public ActionResult ItemNotFound()
        {
            return View();
        }

        /* Wishlist Views */

        [HttpPost]
        
        public ActionResult Wishlist(string productId)
        {
            if (Session["u_email"] == null)
            {
               return Json(new { redirect = Url.Action("Login", "Auth") });
            }
           
            else
            {
                string userEmail = Session["u_email"].ToString();
                /* Any method returns true or false */
                /* Check if the item is already wishlisted or not with two params (id, user email) */
                var isWishlisted = db.Wishlists.Any(item => item.pid == productId && item.email == userEmail);

                /* If not */
                if (!isWishlisted)
                {
                    var product = db.Products.Find(productId);
                    Wishlist model = new Wishlist();
                    model.email = Session["u_email"].ToString();
                    model.pid = productId;
                    model.p_price = product.p_price;
                    model.p_name = product.p_name;
                    
                    if (product != null)
                    {
                        model.p_image = product.p_image;
                        db.Wishlists.Add(model);
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.wishlistMessage = "Sorry this product could not be wishlisted";
                    }
                }
                /* If already in the wishlist */
                else
                {
                    /* Search with two params (id, user email) */
                    var wishlistProduct = db.Wishlists.FirstOrDefault(item => item.pid == productId && item.email == userEmail);
                    db.Wishlists.Remove(wishlistProduct);
                    db.SaveChanges();
                }
            return Json(new { });
            }
        }

        public ActionResult HandleSingleWishlist(string pid)
        {
            if (Session["u_email"] == null)
            {
                return Json(new { redirect = Url.Action("Login", "Auth") });
            }

            else
            {
                string userEmail = Session["u_email"].ToString();
                /* Any method returns true or false */
                /* Check if the item is already wishlisted or not with two params (id, user email) */
                var isWishlisted = db.Wishlists.Any(item => item.pid == pid && item.email == userEmail);

                /* If not */
                if (!isWishlisted)
                {
                    var product = db.Products.Find(pid);
                    Wishlist model = new Wishlist();
                    model.email = Session["u_email"].ToString();
                    model.pid = pid;
                    model.p_price = product.p_price;
                    model.p_name = product.p_name;

                    if (product != null)
                    {
                        model.p_image = product.p_image;
                        db.Wishlists.Add(model);
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.wishlistMessage = "Sorry this product could not be wishlisted";
                    }
                }
                /* If already in the wishlist */
                else
                {
                    /* Search with two params (id, user email) */
                    var wishlistProduct = db.Wishlists.FirstOrDefault(item => item.pid == pid && item.email == userEmail);
                    db.Wishlists.Remove(wishlistProduct);
                    db.SaveChanges();
                }
                return Json(new { });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleWishlist(string pid, string email) {
            var wishlistItem = db.Wishlists.FirstOrDefault(item => item.pid == pid && item.email == email);
            if(wishlistItem == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
            }
            db.Wishlists.Remove(wishlistItem);
            db.SaveChanges();
            return RedirectToAction("AllWishlist", "User");
        }

        /* Cart */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleCart(string pid, string email)
        {
            
            bool isCarted = db.Carts.Any(item => item.pid == pid && item.email == email);

            if (!isCarted) {
                var product = db.Products.Find(pid);
                if (product != null)
                {
                    Cart item = new Cart();
                    item.email = Session["u_email"].ToString();
                    item.pid = pid;
                    item.p_name = product.p_name;
                    item.p_price = product.p_price;
                    item.p_image = product.p_image;
                    item.p_quantity = 1;
                    item.p_quantity_available = 1;
                    db.Carts.Add(item);
                    db.SaveChanges();
                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
                }
            }
            else
            {
                var product = db.Carts.FirstOrDefault(item => item.pid == pid && item.email == email);
                if (product != null)
                {
                    db.Carts.Remove(product);
                    db.SaveChanges();
                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
                }
            }
            
            return RedirectToAction("Details", "Product", new {pid = pid});
        }

        public ActionResult RemoveFromCart(string pid)
        {
            if (Session["u_email"] != null)
            {
                string userEmail = Session["u_email"].ToString();
                var product = db.Carts.FirstOrDefault(item => item.pid == pid && item.email == userEmail);
                db.Carts.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Cart", "User");
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
            }
        }

        public ActionResult IncreaseQuantity(string pid)
        {
            if (Session["u_email"] != null)
            {
                string userEmail = Session["u_email"].ToString();
                var cartProduct = db.Carts.FirstOrDefault(item => item.pid == pid && item.email == userEmail);
                var product = db.Products.Find(pid);
                if (cartProduct != null)
                {
                    cartProduct.p_quantity++;

                    if (cartProduct.p_quantity > product.p_qty)
                    {
                        cartProduct.p_quantity_available = 0;
                    }

                    db.Entry(cartProduct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Cart", "User");
                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound); 
                }
                
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        public ActionResult DecreaseQuantity(string pid)
        {
            if (Session["u_email"] != null)
            {
                string userEmail = Session["u_email"].ToString();
                var cartProduct = db.Carts.FirstOrDefault(item => item.pid == pid && item.email == userEmail);
                var product = db.Products.Find(pid);
                if (cartProduct != null)
                {
                    if(cartProduct.p_quantity > 1) {
                        cartProduct.p_quantity--;
                    }
                    else
                    {
                        cartProduct.p_quantity = 1;
                    }

                    if(cartProduct.p_quantity <= product.p_qty)
                    {
                        cartProduct.p_quantity_available = 1;
                    }
                    db.Entry(cartProduct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Cart", "User");
                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotFound);
                }

            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        /* Search */
        public JsonResult AutoComplete(string input)
        {
            var suggestion = db.Products
                .Where(item => item.p_name.StartsWith(input))
                .Select(item => item.p_name).Take(5).ToList();
            if (suggestion.Count > 0)
            {

                return Json(suggestion, JsonRequestBehavior.AllowGet);
            }
            else
            {
                 suggestion = db.Categories
                .Where(item => item.c_name.StartsWith(input))
                .Select(item => item.c_name).Take(5).ToList();
            }
            return Json(suggestion, JsonRequestBehavior.AllowGet);
        }
    }
}