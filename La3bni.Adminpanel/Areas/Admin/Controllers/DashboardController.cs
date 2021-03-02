using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Repository;
using System.Linq;

namespace La3bni.Adminpanel.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    //[Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public DashboardController(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
        }

        public IActionResult Index()
        {
            return View(new StatisticsViewModel
            {
                NumberOfBookings = unitOfWork.BookingRepo.GetAll().Count(),
                NumberOfStadiums = unitOfWork.PlayGroundRepo.GetAll().Count(),
                NumberOfPlayers = userManager.GetUsersInRoleAsync("Player").Result.Count(),
                NumberOfOwners = userManager.GetUsersInRoleAsync("Owner").Result.Count()
            });
        }
    }
}