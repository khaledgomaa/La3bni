using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

using Models.ViewModels;
using Newtonsoft.Json;

using System.Diagnostics;

namespace La3bni.Adminpanel.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Role="Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, IUnitOfWork _unitOfWork)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            unitOfWork = _unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("CreateRole")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole(CreateRole model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [Route("ListRoles")]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        [Route("EditRole")]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMsg = $"Role with Id = {id} Cannot found";
                return View("NotFound");
            }

            var model = new EditRole
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Route("EditRole")]
        public async Task<IActionResult> EditRole(EditRole model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMsg = $"Role with Id = {model.Id} Cannot found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        [Route("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMsg = $"Role with Id = {roleId} Cannot found";
                return View("NotFound");
            }

            var model = new List<UserRole>();

            foreach (var user in userManager.Users)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRole.IsSelected = true;
                }
                else
                {
                    userRole.IsSelected = false;
                }
                model.Add(userRole);
            }

            return View(model);
        }

        [HttpPost]
        [Route("EditUsersInRole")]
        public async Task<IActionResult> EditUsersInRole(List<UserRole> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMsg = $"Role with Id = {roleId} Cannot found";
                return View("NotFound");
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult res = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    res = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!(model[i].IsSelected) && (await userManager.IsInRoleAsync(user, role.Name)))
                {
                    res = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (res.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        [HttpGet]
        [Route("ListUsers")]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpGet]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMsg = $"User with Id = {id} Cannot be found";
                return View("NotFound");
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);
            var model = new EditUserAdminPanel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser(EditUserAdminPanel umodel)
        {
            var user = await userManager.FindByIdAsync(umodel.Id);

            if (user == null)
            {
                ViewBag.ErrorMsg = $"User with Id = {umodel.Id} Cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = umodel.Email;
                user.UserName = umodel.UserName;
                user.City = umodel.City;

                var res = await userManager.UpdateAsync(user);

                if (res.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in res.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(umodel);
        }

        [HttpPost]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);

            List<Models.Booking> All_bookings = unitOfWork.BookingRepo.GetAllWithInclude().ToList();

            List<Models.Playground> All_playGrounds = unitOfWork.PlayGroundRepo.GetAll().ToList();
            if (All_bookings != null)
            {
                foreach (var book in All_bookings)
                {
                    if (book.ApplicationUserId == id)
                    {
                        unitOfWork.BookingRepo.Delete(book);
                    }
                }
                unitOfWork.Save();
            }

            if (All_playGrounds != null)
            {
                foreach (var P in All_playGrounds)
                {
                    if (P.ApplicationUserId == id)
                    {
                        unitOfWork.PlayGroundRepo.Delete(P);
                    }
                }
                unitOfWork.Save();
            }

            if (user == null)
            {
                ViewBag.ErrorMsg = $"User with Id = {id} Cannot be found";
                return View("NotFound");
            }
            else
            {
                var res = await userManager.DeleteAsync(user);

                if (res.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in res.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("ListUsers");
        }

        [HttpPost]
        [Route("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMsg = $"Role with Id = {id} Cannot be found";
                return View("NotFound");
            }
            else
            {
                var res = await roleManager.DeleteAsync(role);

                if (res.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in res.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("ListRoles");
        }
    }
}