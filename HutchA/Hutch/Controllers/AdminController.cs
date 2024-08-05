using Hutch.Models;
using Hutch.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Hutch.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        HutchEntities db = new HutchEntities();
        public ActionResult Dashboard()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            int userCount = db.Users.Count();
            int sum = 0;
            var soldProducts = db.OrderItems.Select(item => item.p_price).ToList();
            foreach(var item in soldProducts)
            {
                sum += item;
            }

            model.userCount = userCount;
            model.totalRevenue = sum;

            return View(model);
        }

        /* Authentication */
        public ActionResult Login()
        {
            if (Session["a_email"] != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        [HttpPost]  
        public ActionResult Login(Admin admin) {

            var isAdminExists = db.Admins.Where(a => a.a_email.Equals(admin.a_email) && a.a_password.Equals(admin.a_password)).FirstOrDefault();
            if (isAdminExists != null) {
                Session["a_email"] = admin.a_email;
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.error = "Admin credential does not macth";
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        /* Products */
        public ActionResult Product()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        public ActionResult AddProduct()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            Product model = new Product();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(Product product, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (image != null)
                    {
                        product.p_image = new byte[image.ContentLength];
                        image.InputStream.Read(product.p_image, 0, image.ContentLength);
                    }
                    db.Products.Add(product);
                    db.SaveChanges();
                }
            }
            catch(DbUpdateException ex)
            {
                ViewBag.Message = "A product with same id exists";
            }
            catch(DbEntityValidationException ex)
            {
                ViewBag.Message = "Product image is required";
            }
            return View();
        }

        public ActionResult ViewAllProduct()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }
            return View(db.Products.ToList());
        }

        public ActionResult ViewSingleProduct(string id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.Products.Find(id);
            if(product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult UpdateProduct()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }
            return View(db.Products.ToList());
        }

        public ActionResult EditProduct(string id)
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.Products.Find(id);
            if(product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct([Bind(Include = "pid, p_name, p_price, p_desc, p_type, p_category, p_qty, p_arrival, p_image, p_brand")]Product product, HttpPostedFileBase image, string imagesrc) {

            /* Storing the image outside Model data validation because if the ModelState.IsValid is false
               then it will return to the view without image and it would generate null object error inside
               the cshtml on image container.
           */

            // If product image is changed by the admin
            if (image != null)
            {
                product.p_image = new byte[image.ContentLength];
                image.InputStream.Read(product.p_image, 0, image.ContentLength);
            }

            //If admin does not change the product image, then keep the previous image
            else
            {
                // Here you see I am taking the imagesrc and retrieving the base64 string then converting the base64 to byte
                // It is a one step lengthy procedure. The short cut is in the EditCategory method.

                byte[] imageBytes = null;
                string base64 = imagesrc.Split(',')[1];
                imageBytes = Convert.FromBase64String(base64);
                product.p_image = imageBytes;
            }

            if (ModelState.IsValid)
            {
                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Product");
            }

            // If ModelState.IsValid is false then returning to the same view with the same product object
            return View(product);
        }

        public ActionResult ProductsToDisable()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var products = db.Products.Where(item => item.p_availability != "unavailable").ToList();
            return View(products);
        }


        public ActionResult DisableTheProduct(string pid)
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var product = db.Products.Find(pid);
            if(product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisableTheProduct([Bind(Include = "pid, p_name, p_price, p_desc, p_type, p_category, p_qty, p_arrival, p_image, p_brand, p_availability")]Product product, string imagesrc)
        {
            if(imagesrc != null)
            {
                byte[] imageBytes = null;
                string base64 = imagesrc.Split(',')[1];
                imageBytes = Convert.FromBase64String(base64);
                product.p_image = imageBytes;
                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            
        }

        /* Category */
        public ActionResult Category()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult AddCategory()
        {
            if (Session["a_email"]  == null) {
                return RedirectToAction("Login");
            }

            Category model = new Category();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(Category category, HttpPostedFileBase image) {
            try
            {
                if (ModelState.IsValid)
                {
                    if (image != null)
                    {
                        category.c_image = new byte[image.ContentLength];
                        image.InputStream.Read(category.c_image, 0, image.ContentLength);
                    }
                    db.Categories.Add(category);
                    db.SaveChanges();
                }
            }
            catch(DbUpdateException ex)
            {
                ViewBag.Message = "A category with same id exists";
            }
            catch(DbEntityValidationException ex)
            {
                ViewBag.Message = "Category image is required";
            }
            return View();
        }

        public ActionResult UpdateCategory() {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            return View(db.Categories.ToList());
        }

        public ActionResult EditCategory(string id){
            var category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory([Bind (Include = "cid, c_name, c_image")] Category category, HttpPostedFileBase image, string base64) {

            /* Storing the image outside Model data validation because if the ModelState.IsValid is false
                then it will return to the view without image and it would generate null object error inside
                the cshtml on image container.
            */

            // If category image is changed by the admin
            if (image != null)
            {
                category.c_image = new byte[image.ContentLength];
                image.InputStream.Read(category.c_image, 0, image.ContentLength);
            }
            //If admin does not change the category image, then keep the previous image
            else
            {
                // Here I am directly taking the base64 string(No need to retrive it from imagesrc) and converting it to bytes.
                // Short cut procedure.

                byte[] imageBytes = null;
                imageBytes = Convert.FromBase64String(base64);
                category.c_image = imageBytes;
            }

            if (ModelState.IsValid)
            {
                db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("UpdateCategory");
            }

            // If ModelState.IsValid is false then returning to the same view with the same category object
            return View(category);
        }

        public ActionResult ViewAllCategory()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var categories = db.Categories.ToList();
            return View(categories);
        }


        public ActionResult RemoveCategory() {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var categories = db.Categories.ToList();
            return View(categories);

        }

        public ActionResult DeleteCategory(string id)
        {
            var category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("RemoveCategory");
        }


        /* Users */
        public ActionResult Customer()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var users = db.Users.ToList();
            return View(users);
        }

        /* Feedback */
        public ActionResult FeedbackAllProduct()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var products = db.Products.ToList();
            return View(products);
        }

        public ActionResult ViewProductFeedback(string pid)
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var feedbacks = db.Feedbacks.Where(item => item.pid == pid).ToList();
            return View(feedbacks);
        }

        /* Transactions */
        public ActionResult Transaction()
        {
            if (Session["a_email"] == null)
            {
                return RedirectToAction("Login");
            }

            var transactions = db.TransactionHistories.ToList();
            return View(transactions);
        }

        /* Orders */

        public ActionResult ViewAllOrders()
        {
            var orders = db.OrderItems.Where(item => item.order_status.ToLower() == "pending" || item.order_status.ToLower() == "out for delivery").ToList();

            if (orders == null) {
               return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }

            return View(orders);
        }

        public ActionResult UpdateOrder(int oid, long invoice_id, string email, string order_status)
        {
            var user = db.Users.Where(item => item.email == email).FirstOrDefault();
            var userOrderItem = db.OrderItems.Where(item => item.order_item_id == oid && item.invoice_id == invoice_id && item.email == email).FirstOrDefault();
            userOrderItem.order_status = order_status;
            db.Entry(userOrderItem).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            if(order_status == "Out for delivery")
            {
                string subject = "Your Order is Out for Delivery";
                string body = "Dear " + user.name + "," + "\r\n\r\n" + "Your order " + userOrderItem.p_name + " is now out for delivery. Please be available at the provided address." + "\r\n\r\n" + "Team Hutch";
                SendEmail(email, subject, body);
            }

            if (order_status == "Delivered")
            {
                string subject = "Your Order Has Been Delivered!";
                string body = "Dear " + user.name + "," + "\r\n\r\n" + "We're delighted to inform you that your order " + userOrderItem.p_name + " has been successfully delivered." +"\r\n"+ "We hope you're satisfied with your purchase." +"\r\n"+ "Please take a moment to leave us a review and share your experience with us." + "\r\n\r\n" + "Team Hutch";
                SendEmail(email, subject, body);
            }
            return RedirectToAction("ViewAllOrders");
        }

        public ActionResult CancelOrder(int oid, long invoice_id, string email)
        {
            var user = db.Users.Find(email);
            var userOrderItem = db.OrderItems.Where(item => item.order_item_id == oid && item.invoice_id == invoice_id && item.email == email).FirstOrDefault();
            db.OrderItems.Remove(userOrderItem);
            db.SaveChanges();

            string subject = "Order Cancellation Notification";
            string body = "Dear " + user.name + "," + "\r\n\r\n" + "We regret to inform you that your order " + userOrderItem.p_name + " has been canceled." + "\r\n" + "If you have any questions or need further assistance, please contact us." + "\r\n\r\n" + "Team Hutch";
            SendEmail(email, subject, body);

            return RedirectToAction("ViewAllOrders");
        }

        /* Services */
        public static void SendEmail(string email, string subject, string body)
        {
            MailMessage mailMesagge = new MailMessage("updateshutch@gmail.com", email);
            mailMesagge.Subject = subject;
            mailMesagge.Body = body;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "updateshutch@gmail.com",
                Password = "ghbu kamb wazn wbir"
            };
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMesagge);
        }
    }
}