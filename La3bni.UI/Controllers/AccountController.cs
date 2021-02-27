using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    public class AccountController : Controller
    {
        public  string USERID = "";
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ImageManager imageManager;

        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailRepository emailRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ImageManager _imageManager, IUnitOfWork _unitOfwork,IEmailRepository emailRepository)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            imageManager = _imageManager;
            this.unitOfWork = _unitOfwork;
            this.emailRepository = emailRepository;
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
                    SecurityStamp = new Guid().ToString(),
                    //Type = user.UserType
                };

                string P = (imageManager.UploadFile(user.ImageFile, "AppImages"));

                Appuser.ImagePath = P;
                var created = await userManager.CreateAsync(Appuser, user.Password);
                if (created.Succeeded)
                {
                    await signInManager.SignInAsync(Appuser, isPersistent: false);
                    var confirmationToken =await userManager.GenerateEmailConfirmationTokenAsync(Appuser);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = Appuser.Id, token = confirmationToken }, Request.Scheme);
                    emailRepository.sendEmail("La3bniKoora Email Confirmation",
                        $"Please confirm your Email Address. click the link below\n{confirmationLink}", new List<string> { Appuser.Email });

                    USERID = Appuser.Id;
                    return RedirectToAction("myProfile",user);
                }
                foreach (var err in created.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(user);
        }

        public async Task<IActionResult> ConfirmEmail(string UserId, string token)
        {
            if (UserId == null || token == null)
            {
                return View("Error");
            }
            else
            {
                var user =await userManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    ViewBag.ErrorTitle = "User Not found!";
                    ViewBag.ErrorMessage = $"User Id: {UserId} Not found! " +
                        $"Please register to be able to use our services.";
                    return View("Error");
                }
                else
                {
                    IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        user.EmailConfirmed = true;

                        await userManager.UpdateAsync(user);

                        if (!signInManager.IsSignedIn(User))
                            await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("myProfile", user);

                    }
                    else
                    {
                        ViewBag.ErrorTitle = "Email Confirmation Failed!";
                        ViewBag.ErrorMessage = "Email Confirmation Failed, Please sign in";
                        return View("Error");

                    }
                }
            }

        }


        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectURL = Url.Action("ExternalLoginCallBack", "Account");
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectURL);

            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallBackAsync(string remoteError = null)
        {
            if(remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from your provider: {remoteError}");
                return View("login");
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                ModelState.AddModelError(string.Empty, "Error during loading you information. Please call your provider");
                return View("login");
            }
            var signInResult =await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if(signInResult.Succeeded)
            {
                var existUser =await userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                return RedirectToAction("myProfile",existUser);
            }
            else //There is no corresponding row in asp userlogins table
            { //Check if he has a local account in our system, then link both external and the local account together
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if(email != null)//we need his Email to sign him in our system
                {
                    var user =await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        var usrGender = (info.Principal.FindFirstValue(ClaimTypes.Gender) == "Male") ? 1 : 0;
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            SecurityStamp = new Guid().ToString(),
                            gender = (Gender)usrGender,
                            ImagePath = "man41.png",
                            city = City.Alexandria,
                            EmailConfirmed = true
                        };

                        var created =await userManager.CreateAsync(user);
                        if (created.Succeeded)
                        {
                            var insertingNewLoginResult = await userManager.AddLoginAsync(user, info);
                            if (insertingNewLoginResult.Succeeded)
                            {
                                await signInManager.SignInAsync(user, isPersistent: false);
                                return RedirectToAction("myProfile");
                            }
                        }
                        else
                        {

                            ViewBag.ErrorTitle = $"Please sign in with  {info.LoginProvider}" +
                                $"Sorry we can't sign you in using your {info.LoginProvider} account";
                            return View("Error");
                        }
                    }
                    else
                    {
                        var insertingNewLoginResult =await userManager.AddLoginAsync(user, info);
                        if (insertingNewLoginResult.Succeeded)
                        {
                            await signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction("myProfile");
                        }
                    }
                }//If Email is null then we can't register him
                ViewBag.ErrorTitle = $"Email wan't found!";
                ViewBag.ErrorMessage = $"Your Email Wasn't received from {info.LoginProvider}" +
                    $"Sorry we can't sign you in using your {info.LoginProvider} account";
                return View("Error");
            } 
        }

        [HttpGet]
        public IActionResult login()
        {
            ViewBag.ExternalLogins = signInManager.GetExternalAuthenticationSchemesAsync().Result.ToList();
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
                    var result = await signInManager.PasswordSignInAsync(Appuser.UserName, user.Password, user.rememberMe, false);

                    if (result.Succeeded)
                    {
                        USERID = Appuser.Id;
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect("/" + returnUrl);
                        }
                        return RedirectToAction(nameof(Index), "Home");
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