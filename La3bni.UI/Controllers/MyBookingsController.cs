using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    public class MyBookingsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public MyBookingsController(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Teams()
        {
            string userId = (await GetCurrentUser())?.Id;
            int curHour = DateTime.Now.Hour % 12;
            curHour = curHour != 0 ? curHour : 12;
            var bookingDetails = unitOfWork.BookingTeamRepo.GetAllIQueryableWithInclude()
                                           .Where(a => a.ApplicationUserId == userId
                                           && a.Booking.BookedDate.Date >= DateTime.Now.Date
                                           && a.Booking.PlaygroundTimes.From.Hour > curHour);

            return View(bookingDetails);
        }

        public async Task<IActionResult> LeaveTeam(int bookingId)
        {
            var userDetails = await GetCurrentUser();
            var bookingDetails = await unitOfWork.BookingTeamRepo.FindWithInclude(b => b.BookingId == bookingId && b.ApplicationUserId == userDetails.Id);

            try
            {
                unitOfWork.NotificationRepo.Add(new Notification
                {
                    ApplicationUserId = bookingDetails.Booking.ApplicationUserId,
                    Title = "Player left your team",
                    Body = $"Playground : {bookingDetails.Booking.Playground?.Name ?? "NA"} on {bookingDetails.Booking?.BookedDate:d} - {bookingDetails.Booking.PlaygroundTimes}"
                });

                unitOfWork.NotificationRepo.Add(new Notification
                {
                    ApplicationUserId = userDetails.Id,
                    Title = "You left team",
                    Body = $"Playground : {bookingDetails.Booking.Playground?.Name ?? "NA"} on {bookingDetails.Booking?.BookedDate:d} - {bookingDetails.Booking.PlaygroundTimes}"
                });

                unitOfWork.BookingTeamRepo.Delete(bookingDetails);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return RedirectToAction("Teams", "MyBookings");
            }
            return RedirectToAction("Teams", "MyBookings");
        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            return await userManager.GetUserAsync(User);
        }
    }
}