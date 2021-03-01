using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class MyBookingsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public MyBookingsController(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
        }

        public async Task<IActionResult> Index()
        {
            string userId = (await GetCurrentUser())?.Id;
            int curHour = GetCurrentHour();

            var myBookings = unitOfWork.BookingRepo.GetAllWithInclude()
                                                   .Where(a => a.ApplicationUserId == userId
                                                        && a.BookedDate.Date >= DateTime.Now.Date
                                                        && (a.BookedDate.Date == DateTime.Now.Date ? a.PlaygroundTimes.From.Hour > curHour : true));
            return View(myBookings);
        }

        public async Task<IActionResult> Teams()
        {
            string userId = (await GetCurrentUser())?.Id;
            int curHour = GetCurrentHour();
            var bookingDetails = unitOfWork.BookingTeamRepo.GetAllIQueryableWithInclude()
                                           .Where(a => a.ApplicationUserId == userId
                                           && a.Booking.BookedDate.Date >= DateTime.Now.Date
                                           && (a.Booking.BookedDate.Date == DateTime.Now.Date ? a.Booking.PlaygroundTimes.From.Hour > curHour : true));

            return View(bookingDetails);
        }

        //12 hours format
        private int GetCurrentHour()
        {
            int curHour = DateTime.Now.Hour % 12;
            curHour = curHour != 0 ? curHour : 12;

            return curHour;
        }

        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var bookingDetails = await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == bookingId);

            try
            {
                unitOfWork.NotificationRepo.Add(new Notification
                {
                    ApplicationUserId = (await GetCurrentUser())?.Id,
                    Title = "Booking has been canceled",
                    Body = $"Playground : {bookingDetails.Playground.Name} on {bookingDetails.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                });
                unitOfWork.BookingRepo.Delete(bookingDetails);
                unitOfWork.Save();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return RedirectToAction("Index", "MyBookings");
            }

            return RedirectToAction("Index", "MyBookings");
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