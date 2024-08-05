using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Hutch.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        HutchEntities db = new HutchEntities();

        /* Authentication */
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(User user)
        {
            if (ModelState.IsValid)
            {

                if (db.Users.Any(u => u.email == user.email))
                {
                    ViewBag.message = "Email already exists please login";
                }

                else
                {
                    Session["new_user"] = user;
                    var otp = GenerateOTP();
                    Session["new_u_otp"] = otp;

                    string emailSubject = "Welcome to Hutch";
                    string emailBody = "Dear " + user.name + ",\r\n\r\n"
                        + "Welcome to Hutch! We're thrilled to have you join our community.\r\n"
                        + "To complete your signup process, please provide the following code to verify your email address and activate your account.\r\n" 
                        + "Verification Code: " + otp + "\r\n" 
                        + "If you didn't initiate this signup process or believe you received this email in error, please disregard it.\r\n\r\n"
                        + "Thank you for choosing Hutch. We look forward to serving you!";

                    SendEmail(user.email, emailSubject, emailBody, otp);
                    return RedirectToAction("OTPVerification");
                }

            }
            return View();
        }

        public ActionResult OTPVerification()
        {
            OTPModel model = new OTPModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OTPVerification(OTPModel model)
        {
            if (ModelState.IsValid)
            {
                if (Session["new_u_otp"].ToString() == model.OTP)
                {
                    var new_user = (User)Session["new_user"];
                    Session.Remove("new_u_otp");
                    Session.Remove("new_user");
                    db.Users.Add(new_user);
                    db.SaveChanges();
                    Session["u_email"] = new_user.email.ToString();
                    Session["u_name"] = new_user.name.ToString();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Wrong OTP";
                }
            }
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {

                var isUserExists = db.Users.Where(u => u.email.Equals(user.email) && u.password.Equals(user.password)).FirstOrDefault();
                if (isUserExists != null)
                {
                    Session["u_email"] = user.email.ToString();
                    Session["u_name"] = db.Users.Find(user.email).name;
                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    ViewBag.error = "Credentials don't match";
                }

            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        /* Services */
        public static void SendEmail(string userEmail, string subject, string emailBody, string otp)
        {
            MailMessage mailMesagge = new MailMessage("updateshutch@gmail.com", userEmail);
            mailMesagge.Subject = subject;
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
            List<string> listChar = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
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