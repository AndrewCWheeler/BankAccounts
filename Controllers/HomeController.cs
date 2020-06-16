using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Registering")]
        [HttpPost]
        public IActionResult Registering(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("BankPortal");
            }
            else
            {
                return View("Index");
            }
        }

        [Route("Login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("Logging")]
        [HttpPost]
        public IActionResult Logging(LogUser user)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                PasswordHasher<LogUser> Hasher = new PasswordHasher<LogUser>();
                PasswordVerificationResult Result = Hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
                if(Result == 0)
                {
                    ModelState.AddModelError("LogUser", "Invalid Email/Password");
                    return View("Login");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("BankPortal");
            }
            else
            {
                return View("Login");
            }
        }

        [Route("BankPortal")]
        [HttpGet]
        public IActionResult BankPortal()
        {
            int? LoggedUser = HttpContext.Session.GetInt32("UserId");
            if(LoggedUser == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.LoggedUser = dbContext.Users
                .Include(u => u.Transactions)
                .FirstOrDefault(u => u.UserId == (int)LoggedUser);
                
        
        return View("BankPortal");
        }

        [Route("Transacting")]
        [HttpPost]
        public IActionResult Transacting(decimal amount)
        {
            int? LoggedUser = HttpContext.Session.GetInt32("UserId");
            int thisUserId = LoggedUser ?? default(int);
            if(LoggedUser == null)
            {
                return RedirectToAction("Index");
            }
            else 
            {   
                User thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == thisUserId);
                if (thisUser.Balance + amount < 0)
                    {
                        TempData["Error"] = "You can't withdraw what you don't have!";
                        return RedirectToAction ("BankPortal");
                    }
                else
                {
                    Transaction transaction = new Transaction();
                    {
                        transaction.Amount = amount; 
                        transaction.UserId = thisUserId;
                        dbContext.Add(transaction);
                        dbContext.SaveChanges();
                        thisUser.Balance += amount;
                        thisUser.UpdatedAt = DateTime.Now;
                        dbContext.SaveChanges();
                    }
                }
                    return RedirectToAction("BankPortal");
            }
        }

        [Route("Logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            int? LoggedUser = HttpContext.Session.GetInt32("UserId");
            if(LoggedUser == null)
            {
                return RedirectToAction("Index");
            }
            HttpContext.Session.Clear();
            return RedirectToAction("LoggedOut");
        }

        [Route("LoggedOut")]
        [HttpGet]
        public IActionResult LoggedOut()
        {
            return View("LoggedOut");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}