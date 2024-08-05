using Stripe.BillingPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Text;
using Stripe.Checkout;
using Hutch.Models;
using System.Net.Mail;

namespace Hutch.Controllers
{
    public class CheckOutController : Controller
    {
        // GET: CheckOut
        HutchEntities db = new HutchEntities();

        public ActionResult Checkout()
        {
            string errorMessage = ValidateUser();
            if (errorMessage == "validated")
            {
                if (ValidateCart())
                {
                    return RedirectToAction("Payment");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please reduce the items quantity"; // It will be deletd once accessed
                    return RedirectToAction("Cart", "User");
                }
            }
            else
            {
                TempData["incorrect_user_details"] = errorMessage;
                return RedirectToAction("Cart", "User");
            }
        }

        public ActionResult Payment()
        {

            string userEmail = Session["u_email"].ToString();
            var domain = "https://localhost:44327/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"CheckOut/OrderConfirm",
                CancelUrl = domain + $"Checkout/Login",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = userEmail,
                BillingAddressCollection = "required",
            };
            var cart = db.Carts.Where(item => item.email.ToLower() == userEmail.ToLower()).ToList();

            foreach (var item in cart)
            {
                var SessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.p_price * 100,
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.p_name.ToString(),
                        }
                    },
                    Quantity = item.p_quantity,
                };
                options.LineItems.Add(SessionListItem);
                
            }
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);
            TempData["SessionToken"] = session.Id;
            return Redirect(session.Url);

        }

        public ActionResult OrderConfirm()
        {
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Get(TempData["SessionToken"].ToString());

            if(session.PaymentStatus == "paid")
            {
                
                string userEmail = Session["u_email"].ToString();

                var user = db.Users.Find(userEmail);
                var cart = db.Carts.Where(item => item.email == userEmail);
                int total_amount = 0;
                foreach(var cartItem in cart)
                {
                    total_amount += (cartItem.p_quantity * cartItem.p_price);
                }

                var paymentIntenId = session.PaymentIntentId.ToString();
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                Invoice invoice = new Invoice();
                invoice.timestamp = timestamp.ToString();
                invoice.email = userEmail;
                invoice.shipping_address = user.address;
                invoice.total_amount = total_amount;
                invoice.payment_method = "Online";
                
                db.Invoices.Add(invoice);
                db.SaveChanges();

                var userInvoices = db.Invoices.Where(item => item.email == userEmail);
                long currentInvoice_id = userInvoices.Max(item => item.invoice_id);
                var userCurrentInvoice = userInvoices.Where(item => item.invoice_id == currentInvoice_id).FirstOrDefault();

                TransactionHistory transactionHistory = new TransactionHistory();
                transactionHistory.trans_id = paymentIntenId.ToString();
                transactionHistory.email = userEmail;
                transactionHistory.invoice_id = userCurrentInvoice.invoice_id;
                transactionHistory.timestamp = timestamp;
                db.TransactionHistories.Add(transactionHistory);
                db.SaveChanges();

                
                foreach(var cartItem in cart)
                {
                    using(var odb = new HutchEntities())
                    {
                        OrderItem orderItem = new OrderItem();
                        orderItem.invoice_id = userCurrentInvoice.invoice_id;
                        orderItem.pid = cartItem.pid;
                        orderItem.email = userEmail;
                        orderItem.p_name = cartItem.p_name;
                        orderItem.p_quantity = cartItem.p_quantity;
                        orderItem.p_price = cartItem.p_price;
                        orderItem.p_image = cartItem.p_image;
                        orderItem.order_status = "Pending";
                        orderItem.order_date = timestamp;
                        odb.OrderItems.Add(orderItem);
                        odb.SaveChanges();
                    }
                    
                }

                // Update database status
                UpdateDB();

                /* Order confirmation mail */
                string emailSubject = "Your Order Confirmation - " + "[Order Number: " + invoice.invoice_id + "]";
                string emailBody = "Dear " + Session["u_name"] + ",\r\n\r\n"
                                    + "Thank you for shopping with us! We're delighted to confirm that your order has been successfully placed. Below are the details of your order:\r\n\r\n"
                                    + "Order Number: " + invoice.invoice_id + "\r\n"
                                    + "Order Date: " + invoice.timestamp.Substring(0, 10) + "\r\n"
                                    + "Shipping Address: " + invoice.shipping_address + "\r\n"
                                    + "Payment Method: " + invoice.payment_method + "\r\n\r\n"
                                    + "Your order is currently being processed and will be shipped to you as soon as possible. We'll send you another email with the tracking information once your order has been dispatched.";

                SendEmail(userEmail, emailSubject, emailBody);

                /* Empty the user cart */
                EmptyCart();
                return RedirectToAction("Success", "CheckOut");
            }
            return RedirectToAction("Failed", "CheckOut");
        }


        public ActionResult Success() {
            return View();
        }

        public ActionResult Failed()
        {
            return View();
        }
        public bool ValidateCart()
        {
            
            string userEmail = Session["u_email"].ToString();
            var cart = db.Carts.Where(item => item.email == userEmail)
                .Select(item => new
                {
                    pid = item.pid,
                     p_quantity = item.p_quantity,
                })
                .ToList();

            var allProducts = db.Products
                .Select(item => new
                {
                    pid = item.pid,
                    p_qty = item.p_qty,
                })
                .ToList();

            foreach (var item in cart)
            {
                var product = allProducts.Where(p => p.pid == item.pid).FirstOrDefault();
                if (product != null)
                {
                    if (item.p_quantity > product.p_qty)
                    {
                        return false;
                    }

                }
            }

            return true;
            
        }

        public string ValidateUser()
        {
            string userEmail = Session["u_email"].ToString();
            var user = db.Users.Find(userEmail);
            if(user.address == null || user.address.Length < 10)
            {
                 return "Please provide legit shipping address";
                
            }
            else if(user.phone_no == null || user.phone_no.Length < 10)
            {
                 return "Please provide legit phone number";
                
            }
            return "validated";
            
        }
       
        public bool UpdateDB()
        {
            if(ValidateCart())
            {
                string userEmail = Session["u_email"].ToString();
                var cart = db.Carts.Where(item => item.email == userEmail);
                
                foreach(var item in cart)
                {
                    using(var pdb = new HutchEntities())
                    {
                        var product = pdb.Products.Where(p => p.pid == item.pid).FirstOrDefault();
                        product.p_qty -= item.p_quantity;
                        pdb.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        pdb.SaveChanges();
                    }
                    
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /* Empty Cart */
        public void EmptyCart()
        {
            string userEmail = Session["u_email"].ToString();
            var cart = db.Carts.Where(item => item.email == userEmail);
            db.Carts.RemoveRange(cart);
            db.SaveChanges();
        }

        /* Services */
        public static void SendEmail(string userEmail, string emailSubject, string emailBody)
        {
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

    }
}