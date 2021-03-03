using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using Repository.IBookingRepository;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Areas.Booking.Controllers
{
    [Area("Booking")]
    public class BookingsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public BookingsController(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
        }

        // GET: BookingsController
        [Route("index")]
        public ActionResult Index()
        {
            var currentUser = userManager.GetUserAsync(User).Result;
            if (userManager.GetRolesAsync(currentUser).Result?.ElementAt(0) == "Owner")
            {
                return View(unitOfWork.BookingRepo.GetAllWithInclude().Where(b => b.Playground.ApplicationUserId == currentUser.Id));
            }

            return View(unitOfWork.BookingRepo.GetAllWithInclude());
        }

        // GET: BookingsController/Details/5
        //[Route("Details/{id}")]
        [Route("Details")]
        public async Task<ActionResult> Details(int id)
        {
            return View(await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == id));
        }

        // GET: BookingsController/Delete/5
        //[Route("Delete/{id}")]
        [Route("Delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return View(await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == id));
        }

        // POST: BookingsController/Delete/5
        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        [Route("Delete/{id}")]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var booking = await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == id);
                unitOfWork.BookingRepo.Delete(booking);
                unitOfWork.NotificationRepo.Add(new Notification
                {
                    ApplicationUserId = booking.ApplicationUserId,
                    Title = "Booking has been canceled",
                    Body = $"Playground : {booking.Playground.Name} on {booking.BookedDate:d} - {booking.PlaygroundTimes}"
                });
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}