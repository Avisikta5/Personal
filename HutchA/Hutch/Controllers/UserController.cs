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
    public class UserController : Controller
    {
        // GET: User
        HutchEntities db = new HutchEntities();

        /* Profile */
        public ActionResult EditProfile()
        {
            if (Session["u_email"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string userEmail = Session["u_email"].ToString();
            var user = db.Users.FirstOrDefault(u => u.email == userEmail);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile([Bind(Include ="email, password, name, address, phone_no")] User user)
        {
            if (ModelState.IsValid)
            {
                Session["u_name"] = user.name;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return View();
        } 

        /* User Orders */

        public ActionResult Order()
        {
            if (Session["u_email"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string userEmail = Session["u_email"].ToString();
            var orders = db.OrderItems.Where(item => item.email == userEmail).ToList();
            orders = orders.OrderByDescending(item => item.order_date).ToList();

            var userOrders = new OrderViewModel();
            userOrders.pending = orders.Where(item => item.order_status.ToLower() == "pending").ToList();
            userOrders.outForDelivery = orders.Where(item => item.order_status.ToLower() == "out for delivery").ToList();
            userOrders.delivered = orders.Where(item => item.order_status.ToLower() == "delivered").ToList();
            
            return View(userOrders);
        }

        public ActionResult CancelOrder(int oid, long invoice_id, string email)
        {
            var cancelOrderItem = db.OrderItems.Where(item => item.order_item_id == oid && item.invoice_id == invoice_id && item.email == email).FirstOrDefault();
            db.OrderItems.Remove(cancelOrderItem);
            db.SaveChanges();

            var user = db.Users.Find(email);
            string subject = "Order Cancellation Notification";
            string body = "Dear " + user.name + "," + "\r\n\r\n" + "your order for " + cancelOrderItem.p_name + " has been canceled." + "\r\n" + "If you have any questions or need further assistance, please contact us." + "\r\n\r\n" + "Team Hutch";
            SendEmail(email, subject, body);

            return RedirectToAction("Order", "User");
        }

        /* User Wishlist */
        public ActionResult AllWishlist()
        {
            WishlistViewModel model = new WishlistViewModel();
            if (Session["u_email"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                string userEmail = Session["u_email"].ToString();
                var wishlistItems = db.Wishlists.Where(item => item.email == userEmail).ToList();
                int totalItemCount = db.Wishlists.Count(item => item.email == userEmail);

                model.wishlists = wishlistItems;
                model.totalItems = totalItemCount;
            }

            return View(model);
        }

        /* User Cart */

        public ActionResult Cart()
        {
            if (Session["u_email"] != null)
            {
                ViewBag.message = TempData["ErrorMessage"]; // TempData["ErrorMessage"] is now removed
                ViewBag.userErrorMessage = TempData["incorrect_user_details"]; // TempData["incorrect_user_details"] is now removed
                string userEmail = Session["u_email"].ToString();
                int totalItems = 0;
                int totalAmount = 0;
                var cartItems = db.Carts.Where(item => item.email == userEmail).ToList();
                foreach (var item in cartItems)
                {
                    totalAmount += (item.p_quantity * item.p_price);
                    totalItems++;
                }

                CartViewModel model = new CartViewModel();
                model.Carts = cartItems;
                model.totalItem = totalItems;
                model.totalAmount = totalAmount;
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }


        /* Authentication */
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassword(User user)
        {
            var isUserExists = db.Users.Where(u => u.email == user.email).FirstOrDefault();
            if (isUserExists != null) {
                Session["u_verification_mail"] = user.email;
                string otp = GenerateOTP();
                Session["u_otp"] = otp;
                string userEmail = user.email.ToString();

                string emailSubject = "Password Reset Request";
                string emailBody = "Dear User,\r\n\r\n"
                  + "We have received a request to reset your password for Your Hutch Profile. "
                  + "To proceed with the password reset, please use the following code: " + otp + "\r\n\r\n"
                  + "Please note that this code is valid for a limited time period and can only be used once. "
                  + "If you did not request a password reset, please ignore this email or contact our support team immediately.\r\n\r\n"
                  + "Thank you for using Hutch.\r\n\r\n"
                  + "Best Regards,\r\n"
                  + "Hutch it";

                SendEmail(userEmail, emailSubject, emailBody);
                return RedirectToAction("CheckOTP");
            }
            else
            {
                ViewBag.Message = "No User Exist With That Email";
            }
            return View();
        }

        public ActionResult CheckOTP()
        {
            OTPModel model = new OTPModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOTP(OTPModel model)
        {
            if (ModelState.IsValid)
            {
                if (Session["u_otp"].ToString() == model.OTP)
                {
                    Session.Remove("u_otp");
                    return RedirectToAction("SetNewPassword");
                }
                else
                {
                    ViewBag.Message = "Wrong OTP";
                }
            }
            
            return View();
        }

        public ActionResult SetNewPassword()
        {
            NewPasswordModel model = new NewPasswordModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetNewPassword(NewPasswordModel model)
        {

            var userEmail = Session["u_verification_mail"].ToString();

            if (ModelState.IsValid)
            {
                User user = db.Users.Find(userEmail);
                if (user != null)
                {
                    user.password = model.NewPassword;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Session.Remove("u_verification_mail");
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.Message = "user doesn't exist";
                }
            }
            return View();
        }

        /* Services */
        public static void SendEmail(string userEmail, string emailSubject, string emailBody) {
            MailMessage mailMesagge = new MailMessage("updateshutch@gmail.com", userEmail);
            mailMesagge.Subject = emailSubject;
            mailMesagge.Body = emailBody;
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "updateshutch@gmail.com",
                Password = "ghbu kamb wazn wbir"
            };
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMesagge);
        }

        public string GenerateOTP()
        {
            Random random = new Random();
            List<string> listChar = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
            string otp = "";
            for (int i = 0; i < 6; i++)
            {
                int randIndex = (int)Math.Floor(random.NextDouble() * 10);
                otp = otp + listChar[randIndex];
            }
            return otp;
        }
    }
}