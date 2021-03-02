using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    [Authorize(Roles = "Owner,Player")]
    public class AccountController : Controller
    {
        public string USERID = "";
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ImageManager imageManager;

        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailRepository emailRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,

           RoleManager<IdentityRole> _roleManager,

            ImageManager _imageManager, IUnitOfWork _unitOfwork, IEmailRepository emailRepository)

        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            imageManager = _imageManager;
            this.unitOfWork = _unitOfwork;

            this.roleManager = _roleManager;

            this.emailRepository = emailRepository;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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
            var res = unitOfWork.NotificationRepo.GetAll().ToList();
            var n = res.FindAll(n => n.ApplicationUserId == user.Id);

            return View(n);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                var Appuser = new ApplicationUser
                {
                    UserName = user.Username,
                    Email = user.Email,
                    Gender = user.Gender,
                    City = user.City,
                    PhoneNumber = user.PhoneNumber,
                    ImagePath = "",

                    SecurityStamp = new Guid().ToString(),
                };
                if (user?.ImageFile != null)
                {
                    string P = (imageManager.UploadFile(user.ImageFile, "AppImages"));

                    Appuser.ImagePath = P;
                }
                if (user?.ImageFile == null)
                {
                    if (user.Gender == Gender.Male)
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
                        if (!await roleManager.RoleExistsAsync("Player"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Player";
                            role.NormalizedName = "Player";
                            await roleManager.CreateAsync(role);
                        }

                        await userManager.AddToRoleAsync(Appuser, "Player");

                        //return RedirectToAction("myProfile");
                    }
                    else if (user.UserType == UserType.Owner)
                    {
                        if (!await roleManager.RoleExistsAsync("Owner"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Owner";
                            role.NormalizedName = "Owner";
                            await roleManager.CreateAsync(role);
                        }
                        await userManager.AddToRoleAsync(Appuser, "Owner");
                        await signInManager.SignInAsync(Appuser, isPersistent: false);
                        //return RedirectToAction("myProfile");
                    }

                    await signInManager.SignInAsync(Appuser, isPersistent: false);
                    var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(Appuser);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = Appuser.Id, token = confirmationToken }, Request.Scheme);
                    emailRepository.sendEmail("La3bniKoora Email Confirmation",
                        $"Please confirm your Email Address. click the link below\n{confirmationLink}", new List<string> { Appuser.Email });

                    USERID = Appuser.Id;
                    return RedirectToAction("myProfile", user);
                }
                //foreach (var err in created.Errors)
                //{
                ModelState.AddModelError("", "your data has something wrong");
                //}
            }
            return View(user);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string token)
        {
            if (UserId == null || token == null)
            {
                ModelState.AddModelError("", "Invalid Token");
                return View();
            }
            else
            {
                var user = await userManager.FindByIdAsync(UserId);
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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectURL = Url.Action("ExternalLoginCallBack", "Account");
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectURL);

            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBackAsync(string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from your provider: {remoteError}");
                return View("login");
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error during loading you information. Please call your provider");
                return View("login");
            }
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var existUser = await userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                return RedirectToAction("myProfile", existUser);
            }
            else //There is no corresponding row in asp userlogins table
            { //Check if he has a local account in our system, then link both external and the local account together
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)//we need his Email to sign him in our system
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        var usrGender = (info.Principal.FindFirstValue(ClaimTypes.Gender) == "Male") ? 1 : 0;
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            SecurityStamp = new Guid().ToString(),
                            Gender = (Gender)usrGender,
                            ImagePath = "man41.png",
                            City = City.Alexandria,
                            EmailConfirmed = true
                        };
                        var created = await userManager.CreateAsync(user);

                        if (created.Succeeded)
                        {
                            var insertingNewLoginResult = await userManager.AddLoginAsync(user, info);
                            if (insertingNewLoginResult.Succeeded)
                            {
                                await signInManager.SignInAsync(user, isPersistent: false);
                                if (!await roleManager.RoleExistsAsync("Player"))
                                {
                                    var role = new IdentityRole();
                                    role.Name = "Player";
                                    role.NormalizedName = "Player";
                                    await roleManager.CreateAsync(role);
                                }
                                await userManager.AddToRoleAsync(user, "Player");

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
                        var insertingNewLoginResult = await userManager.AddLoginAsync(user, info);
                        if (insertingNewLoginResult.Succeeded)
                        {
                            await signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction("myProfile");
                        }
                    }
                }//If Email is null then we can't register him
                ViewBag.ErrorTitle = $"Email wasn't found!";
                ViewBag.ErrorMessage = $"Your Email Wasn't received from {info.LoginProvider}" +
                    $"Sorry we can't sign you in using your {info.LoginProvider} account";
                return View("Error");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult login()
        {
            ViewBag.ExternalLogins = signInManager.GetExternalAuthenticationSchemesAsync().Result.ToList();
            return View();
        }

        [AllowAnonymous]
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
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect("/" + returnUrl);
                        }
                        return RedirectToAction(nameof(Index), "Home");
                        // return RedirectToAction("myProfile", Appuser);
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
        public async Task<IActionResult> myProfile(ApplicationUser current)
        {
            var user = await userManager.GetUserAsync(User);

            return View(user);
        }

        public IActionResult PlayGroundDiaplay(string id)
        {
            var AllBg = unitOfWork.PlayGroundRepo.GetAll().ToList();
            List<Models.Playground> Only_MyBg = AllBg.FindAll(bg => bg.ApplicationUserId == id);

            return View(Only_MyBg);
        }

        [AllowAnonymous]
        public async Task<IActionResult> logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public void UpdateNOtificationStatus(int notificationId)
        {
            Notification notification = unitOfWork.NotificationRepo.Find(n => n.NotificationId == notificationId)?.Result;

            if (notification?.Seen == 0)
            {
                notification.Seen = 1; // to make it seen
                unitOfWork.NotificationRepo.Update(notification);
                unitOfWork.Save();
            }
        }

        public IActionResult SeenNotifactions(List<Notification> id)
        {
            foreach (Notification item in id)
            {
                item.Seen = 1;
                unitOfWork.NotificationRepo.Update(item);
            }

            unitOfWork.Save();

            return RedirectToAction("Home", "Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditUser user)
        {
            if (ModelState.IsValid)
            {
                var Appuser = await userManager.GetUserAsync(User);
                var correct_pass = await userManager.CheckPasswordAsync(Appuser, user.Old_Password);
                if (!correct_pass)
                {
                    ModelState.AddModelError("", "Wrong Password");
                    return View(user);
                }
                if (Appuser.UserName != user.Username)
                {
                    JsonResult NewUserName = (JsonResult)Name_Unique(user.Username).Result;
                    if (NewUserName.Value.ToString() != "true")
                    {
                        ModelState.AddModelError("", "Not valid user name ");
                        return View(user);
                    }
                }

                if (Appuser.Email != user.Email)
                {
                    JsonResult Newemail = (JsonResult)Email_Unique(user.Username).Result;
                    if (Newemail.Value.ToString() != "true")
                    {
                        ModelState.AddModelError("", "this email has an  account already ");
                        return View(user);
                    }
                }
                if (user.ConfirmPassword != null && user.NewPassword != string.Empty && user.NewPassword != null)
                {
                    var password_correct = await userManager.ChangePasswordAsync(Appuser, user.Old_Password, user.NewPassword);
                    if (!password_correct.Succeeded)
                    {
                        ModelState.AddModelError("", "Wrong Password");
                        return View(user);
                    }
                }
                Appuser.UserName = user.Username;
                Appuser.Email = user.Email;
                Appuser.Gender = user.Gender;
                Appuser.City = user.City;
                Appuser.PhoneNumber = user.PhoneNumber;
                // Appuser.ImagePath = user.ImagePath;

                if (user?.ImageFile != null)
                {
                    string P = (imageManager.UploadFile(user.ImageFile, "AppImages"));

                    Appuser.ImagePath = P;
                }

                await userManager.UpdateAsync(Appuser);
                return RedirectToAction("myProfile", user);
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var Appuser = await userManager.GetUserAsync(User);
            var user = new EditUser
            {
                Username = Appuser.UserName,
                Email = Appuser.Email,
                Gender = Appuser.Gender,
                City = Appuser.City,
                PhoneNumber = Appuser.PhoneNumber,
                ImagePath = Appuser.ImagePath,
            };
            return View(user);
        }

        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View("ResetPassword");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPasswordTokenCallBack(string email, string token)
        {
            if (email == null || token == null)
            {
                ModelState.AddModelError("", "Invalid Password Reset Token");
                return View();
            }
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPasswordTokenCallBack(ResetPasswordModel passwordModel)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.FindByEmailAsync(passwordModel.Email).Result;
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid Password Reset Token");
                    return View();
                }
                else
                {
                    IdentityResult result = userManager.ResetPasswordAsync(user, passwordModel.Token, passwordModel.Password).Result;
                    if (result.Succeeded)
                    {
                        //password changed
                        return RedirectToAction("Login");
                    }
                    ModelState.AddModelError("", "Invalid Password Reset Token");
                    return View();
                }
            }
            else
                return View(passwordModel);
        }

        [AllowAnonymous]
        public IActionResult SendResetPasswordEmail(IFormCollection form)
        {
            var email = form["Email"];
            var user = userManager.FindByEmailAsync(email).Result;
            if (user.EmailConfirmed)
            {
                var confirmationToken = userManager.GeneratePasswordResetTokenAsync(user).Result;
                var confirmationLink = Url.Action("ResetPasswordTokenCallBack", "Account", new { email = email, token = confirmationToken }, Request.Scheme);
                emailRepository.sendEmail("Password Reset", $"Click below to change your password\n{confirmationLink}", new List<string> { email });

                return RedirectToAction("Login");
            }
            else
                ModelState.AddModelError("emailValidate", "Please Confirm Your Email first!");
            return View("ResetPassword");
        }
    }
}