using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string pass)
        {
            if (!(string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(pass)))
            {
                var result = await signInManager.PasswordSignInAsync(userName, pass, true, false);

                if (result.Succeeded)
                {
                    ApplicationUser userInDb = await userManager.FindByNameAsync(userName);
                    var userRole = userManager.GetRolesAsync(userInDb).Result.ElementAt(0);

                    if (userRole == "Admin")
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        //return Redirect("Admin/Dashboard/Index");
                    }
                    else if (userRole == "Owner")
                    {
                        return RedirectToAction("Index", "Playgrounds", new { area = "Owner" });
                    }
                }
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}