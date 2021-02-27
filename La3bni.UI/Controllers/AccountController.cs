using Microsoft.AspNetCore.Authorization;
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
        public  string USERID = "";
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ImageManager imageManager;

        private readonly IUnitOfWork unitOfWork;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ImageManager _imageManager, IUnitOfWork _unitOfwork, RoleManager<IdentityRole> _roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            imageManager = _imageManager;
            this.unitOfWork = _unitOfwork;
            this.roleManager = _roleManager;
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
        [Authorize]
        public IActionResult NotifactionRead(int id)
        {
            var toUnread = unitOfWork.NotificationRepo.Find(n => n.NotificationId == id).Result;
            toUnread.Seen = 0;
            unitOfWork.NotificationRepo.Update(toUnread);
            unitOfWork.Save();

            return RedirectToAction("Notification");
        }
        [Authorize]
        public IActionResult NotifactionDelete(int id)
        {
            var toUnread = unitOfWork.NotificationRepo.Find(n => n.NotificationId == id).Result;

            unitOfWork.NotificationRepo.Delete(toUnread);
            unitOfWork.Save();

            return RedirectToAction("Notification");
        }
        [Authorize]
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
                    
                };
                if (user.ImageFile != null)
                {
                    string P = (imageManager.UploadFile(user.ImageFile, "AppImages"));

                    Appuser.ImagePath = P;
                }
                if (user.ImageFile == null)
                {
                   
                    if (user.Gender==Gender.Male)
                    Appuser.ImagePath = "manred.png";
                    if (user.Gender == Gender.Female)
                        Appuser.ImagePath = "woman.png";
                }

                var created = await userManager.CreateAsync(Appuser, user.Password);
                if (created.Succeeded)
                {
                   
                    if (user.UserType == UserType.Player)
                    {
                        

                        await signInManager.SignInAsync(Appuser, isPersistent: false);

                        await userManager.AddToRoleAsync(Appuser, "Player");
                       
                        return RedirectToAction("myProfile");


                    }
                    if (user.UserType == UserType.Owner)
                    {
                       await  userManager.AddToRoleAsync(Appuser, "Owner");
                        await signInManager.SignInAsync(Appuser, isPersistent: false);
                        return RedirectToAction("myProfile");
                    }

                   
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

        public async Task<IActionResult> login(LogIN user, string returnUrl)
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
                    var result = await signInManager.PasswordSignInAsync(Appuser.UserName, user.Password,user.rememberMe, false);

                    if (result.Succeeded)
                    {

                        // var res = userManager.GetRolesAsync(Appuser);


                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
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
       //[Authorize(Roles = "Player")]
        [Authorize]
        public async Task<IActionResult> myProfile(ApplicationUser current)
        {
            var user = await userManager.GetUserAsync(User);
           
            return View(user);
        }


        [Authorize]
        public IActionResult Profile_Playgrounds(List<Playground> pgs)
        {
            return View(pgs);
        }
        [Authorize]
        public async Task<IActionResult> logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
       
        
        [HttpPost]
        public IActionResult EditProfile(Models.User user)
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

                };
                var e = "Yess";
                userManager.UpdateAsync(Appuser);
            }
            return View(user);
        }
       
    }
}