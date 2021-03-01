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
    public class StadiumsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        UserManager<ApplicationUser> userManager;

        public StadiumsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            this.unitOfWork = unitOfWork;
            userManager = _userManager;

        }

        public async Task<IActionResult> Index(string PlaygroundName, int? price, Models.City? city)
        {
           // ApplicationUser user = await userManager.GetUserAsync(User);
            var stadiums = unitOfWork.PlayGroundRepo.GetAll().ToList();

            ViewData["PName"] = PlaygroundName;
            ViewData["Price"] = price;
            ViewData["city"] = city;

            if (!String.IsNullOrEmpty(PlaygroundName))
            {
                stadiums = stadiums.Where(s => s.Name.Contains(PlaygroundName)).ToList();
            }
            if (price.HasValue)
            {
                stadiums = stadiums.Where(s => s.PmPrice <= price || s.AmPrice <= price).ToList();
            }
            if (city.HasValue)
            {
                stadiums = stadiums.Where(s => s.City == city).ToList();
            }
            
            return View(stadiums);
        }
    }
}