using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    public class AccountController : Controller
    {
        public static string USERID = "";
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ImageManager imageManager;

        private readonly IUnitOfWork unitOfWork;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ImageManager _imageManager, IUnitOfWork _unitOfwork)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            imageManager = _imageManager;
            this.unitOfWork = _unitOfwork;
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Email_Unique(string email)
        {
            var created = await userManager.FindByEmailAsync(email);
            if (created is null)
            {
                return Json(true);
            }
            else return Json(false);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Name_Unique(string Username)
        {
            var created = await userManager.FindByNameAsync(Username);
            if (created is null)
            {
                return Json(true);
            }
            else return Json(false);
        }

        public IActionResult NotifactionRead(int id)
        {
            var toUnread = unitOfWork.NotificationRepo.Find(n => n.NotificationId == id).Result;
            toUnread.Seen = 0;
            unitOfWork.NotificationRepo.Update(toUnread);
            unitOfWork.Save();

            return RedirectToAction("Notification");
        }

        public IActionResult NotifactionDelete(int id)
        {
            var toUnread = unitOfWork.NotificationRepo.Find(n => n.NotificationId == id).Result;

            unitOfWork.NotificationRepo.Delete(toUnread);
            unitOfWork.Save();

            return RedirectToAction("Notification");
        }

        public async Task<IActionResult> Notification()
        {
            var user = await userManager.GetUserAsync(User);
            var res = unitOfWork.NotificationRepo.GetAll().Result;
            var n = res.FindAll(n => n.ApplicationUserId == user.Id);

            return View(n);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                var Appuser = new ApplicationUser
                {
                    UserName = user.Username,
                    Email = user.Email,
                    gender = user.Gender,
                    city = user.City,
                    PhoneNumber = user.PhoneNumber,
                    ImagePath = "",
                    //Type = user.UserType
                };

                string P = (imageManager.UploadFile(user.ImageFile, "AppImages"));

                Appuser.ImagePath = P;
                var created = await userManager.CreateAsync(Appuser, user.Password);
                if (created.Succeeded)
                {
                    await signInManager.SignInAsync(Appuser, isPersistent: false);

                    USERID = Appuser.Id;
                    return RedirectToAction("myProfile");
                }
                foreach (var err in created.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> login(LogIN user)
        {
            if (ModelState.IsValid)
            {
                var Appuser = new ApplicationUser();

                if (user.Email.Contains("@"))
                    Appuser = await userManager.FindByEmailAsync(user.Email);
                else
                    Appuser = await userManager.FindByNameAsync(user.Email);

                if (!(Appuser is null))
                {
                    var result = await signInManager.PasswordSignInAsync(Appuser.UserName, user.Password, user.rememberMe, false);

                    if (result.Succeeded)
                    {
                        USERID = Appuser.Id;
                        return RedirectToAction("myProfile", Appuser);
                    }

                    ModelState.AddModelError("", "Not correct data");
                }
                else
                {
                    ModelState.AddModelError("", "Not correct data");
                    return View(user);
                }
            }
            return View(user);
        }

        public async Task<IActionResult> myProfile(ApplicationUser current)
        {
            var user = await userManager.GetUserAsync(User);
            USERID = user.Id;
            return View(user);
        }

        public IActionResult Profile_Playgrounds(List<Playground> pgs)
        {
            return View(pgs);
        }

        public async Task<IActionResult> logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}