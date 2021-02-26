using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using Repository.IBookingRepository;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Areas.Booking.Controllers
{
    [Area("Booking")]
    public class BookingsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public BookingsController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        // GET: BookingsController
        [Route("")]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            return View(await unitOfWork.BookingRepo.GetAllWithInclude());
        }

        // GET: BookingsController/Details/5
        //[Route("Details/{id}")]
        public async Task<ActionResult> Details(int id)
        {
            return View(await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == id));
        }

        // GET: BookingsController/Delete/5
        //[Route("Delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return View(await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == id));
        }

        // POST: BookingsController/Delete/5
        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
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