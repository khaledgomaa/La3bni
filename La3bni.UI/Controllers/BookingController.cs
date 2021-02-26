using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Newtonsoft.Json;
using Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public BookingController(IUnitOfWork _unitOfWork,
                                UserManager<ApplicationUser> _userManager,
                                SignInManager<ApplicationUser> _signInManager)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
            signInManager = _signInManager;
        }

        [Route("Booking/{id}")]
        [Route("Booking/Index/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            Playground playGround = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == id);

            return View(playGround);
        }

        public List<PlayGroundTimesViewModel> GetTimes(int id)
        {
            IEnumerable<PlaygroundTimes> times = unitOfWork.PlaygroundTimesRepo.GetAllIQueryable()
                                        .Where(t => t.PlaygroundId == id)
                                        .ToList();

            var newTimesFormat = times.Select(a => new PlayGroundTimesViewModel
            {
                Time = $"{a.From:HH:mm} - {a.To:HH:mm} {a.State}",
                Id = a.PlaygroundTimesId
            }).ToList();

            return newTimesFormat;
        }

        public async Task<BookingViewModel> GetBookings(int playGroundId, string date, string timeId)
        {
            try
            {
                string userId = (await GetCurrentUser())?.Id ?? "";

                if (!string.IsNullOrEmpty(userId))
                {
                    if (await CheckPlaygroundStatus(playGroundId) == Status.Available)
                    {
                        //GetTimes(playGroundId);
                        int.TryParse(timeId, out int bookingTimeId);
                        int bookingId = await CheckBookingNotExist(playGroundId, bookingTimeId, userId, date);
                        if (bookingId == 0) //0 means no booking found to this user for this parameters
                        {
                            var bookings = (await unitOfWork.BookingRepo.Find(
                                b => b.PlaygroundId == playGroundId
                                && b.BookedDate.Date >= DateTime.Now.Date
                                && b.BookedDate.Date == Convert.ToDateTime(date).Date
                                && b.PlaygroundTimesId == int.Parse(timeId)));

                            if (!string.IsNullOrEmpty(bookings?.ApplicationUserId)
                                && await CheckJoinedTeam(bookings.BookingId) != 0)
                                return new BookingViewModel
                                {
                                    BookingId = bookings.BookingId,
                                    BookingOwner = false
                                };

                            return (new BookingViewModel
                            {
                                NumOfPlayers = unitOfWork.BookingTeamRepo.GetAllIQueryable()
                                                      .Count(b => b.BookingId == bookings.BookingId),
                                BookingExist = bookings?.BookingId != 0,
                                BookingStatus = bookings?.BookingStatus ?? BookingStatus.Public,
                                MaxNumOfPlayers = bookings?.MaxNumOfPlayers ?? 0,
                                BookingId = bookings?.BookingId ?? 0
                            });
                        }

                        return new BookingViewModel
                        {
                            BookingId = bookingId,
                            BookingOwner = true
                        };
                    }
                    return new BookingViewModel
                    {
                        PlaygroundStatus = Status.Busy
                    };
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return new BookingViewModel();
        }

        public async Task<int> CheckBookingNotExist(int playgroundId, int timeId, string userId, string date)
        {
            return (await unitOfWork.BookingRepo.Find(b => b.ApplicationUserId == userId
                                            && b.PlaygroundId == playgroundId
                                            && b.BookedDate.Date >= DateTime.Now.Date
                                            && b.BookedDate.Date == Convert.ToDateTime(date).Date
                                            && b.PlaygroundTimesId == timeId))?.BookingId ?? 0;
        }

        private async Task<int> CheckJoinedTeam(int bookingId)
        {
            string userId = (await GetCurrentUser())?.Id;
            return (await unitOfWork.BookingTeamRepo.Find(b => b.BookingId == bookingId && b.ApplicationUserId == userId))?.BookingId ?? 0;
        }

        [Authorize]
        public async Task<IActionResult> CreateBooking(string period, int PlaygroundId, string selectedDate, string numOfPlayers)
        {
            if (int.TryParse(period, out int timeId) && int.TryParse(numOfPlayers, out int playersNo))
            {
                var playgroundTimesDetails = await unitOfWork.PlaygroundTimesRepo.FindWithInclude(p => p.PlaygroundTimesId == timeId);

                State state = playgroundTimesDetails?.State ?? State.AM;
                float price = state == State.AM ?
                    playgroundTimesDetails?.Playground?.AmPrice ?? 0
                    : playgroundTimesDetails?.Playground?.PmPrice ?? 0;

                string userId = (await GetCurrentUser())?.Id;

                var newBooking = new Booking
                {
                    ApplicationUserId = userId,
                    BookingStatus = BookingStatus.Public,
                    BookedDate = Convert.ToDateTime(selectedDate),
                    PlaygroundTimesId = timeId,
                    PlaygroundId = PlaygroundId,
                    MaxNumOfPlayers = playersNo,
                    Price = price
                };

                try
                {
                    unitOfWork.BookingRepo.Add(newBooking);

                    unitOfWork.NotificationRepo.Add(new Notification
                    {
                        ApplicationUserId = userId,
                        Title = "Booking has been created",
                        Body = $"Playground : {playgroundTimesDetails?.Playground.Name ?? "NA"} on {newBooking?.BookedDate:d} - {playgroundTimesDetails}"
                    });

                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return Json(new { error = "No bookings available please reload your page" });
                }
            }
            return Json(new { redirectToUrl = Url.Action("", "Home") });
        }

        [Authorize]
        public async Task<IActionResult> JoinTeam(string bookingId)
        {
            if (int.TryParse(bookingId, out int bookId))
            {
                var curUser = await GetCurrentUser();
                var bookingDetails = await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == bookId);

                try
                {
                    unitOfWork.BookingTeamRepo.Add(new BookingTeam
                    {
                        BookingId = bookId,
                        ApplicationUserId = curUser?.Id
                    });

                    var bookingOwnerDetails = await userManager.FindByIdAsync(bookingDetails.ApplicationUserId);
                    unitOfWork.NotificationRepo.Add(new Notification
                    {
                        ApplicationUserId = bookingOwnerDetails?.Id,
                        Title = "New player joined your team",
                        Body = $"Phone : {curUser?.PhoneNumber} , Playground : {bookingDetails.Playground?.Name ?? "NA"} on {bookingDetails?.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                    });

                    unitOfWork.NotificationRepo.Add(new Notification
                    {
                        ApplicationUserId = (await GetCurrentUser())?.Id,
                        Title = "You have joined team",
                        Body = $"Playground : {bookingDetails.Playground?.Name ?? "NA"} on {bookingDetails?.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                    });

                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return Json(new { error = "No teams available please reload your page" });
                }
            }

            return Json(new { redirectToUrl = Url.Action("", "Home") });
        }

        [Authorize]
        public async Task<IActionResult> CancelBooking(string bookingId)
        {
            if (int.TryParse(bookingId, out int bookId))
            {
                var bookingDetails = await unitOfWork.BookingRepo.FindWithInclude(b => b.BookingId == bookId);

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
                    return Json(new { error = "booking already canceled please reload your page" });
                }
            }
            return Json(new { redirectToUrl = Url.Action("", "Home") });
        }

        [Authorize]
        public async Task<IActionResult> LeaveTeam(string bookingId)
        {
            if (int.TryParse(bookingId, out int bookId))
            {
                var userDetails = await GetCurrentUser();
                var bookingDetails = await unitOfWork.BookingTeamRepo.FindWithInclude(b => b.BookingId == bookId && b.ApplicationUserId == userDetails.Id);

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
                    return Json(new { error = "Team leader already canceled booking please reload your page" });
                }
            }
            return Json(new { redirectToUrl = Url.Action("", "Home") });
        }

        public async Task UpdateRate(string playgroundId, float rate)
        {
            string userId = (await GetCurrentUser())?.Id;

            if (int.TryParse(playgroundId, out int id))
            {
                var checkRatedBefore = await CheckRateBefore(id);
                if (checkRatedBefore.PlaygroundId != 0)
                {
                    checkRatedBefore.Rate = rate;
                    unitOfWork.PlaygroundRateRepo.Update(checkRatedBefore);
                }
                else
                {
                    unitOfWork.PlaygroundRateRepo.Add(new PlaygroundRate
                    {
                        ApplicationUserId = userId,
                        Rate = rate,
                        PlaygroundId = id
                    });
                    unitOfWork.Save();
                }

                float avgRate = (await unitOfWork.PlaygroundRateRepo.GetAll()).Where(r => r.PlaygroundId == id)?.Average(r => r.Rate) ?? 0;

                var playground = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == id);
                playground.Rate = avgRate;
                unitOfWork.PlayGroundRepo.Update(playground);
                unitOfWork.Save();
            }
        }

        public async Task<PlaygroundRate> CheckRateBefore(int playGroundId)
        {
            string userId = (await GetCurrentUser())?.Id;

            return (await unitOfWork.PlaygroundRateRepo
                                  .Find(b => b.ApplicationUserId == userId && b.PlaygroundId == playGroundId)) ?? new PlaygroundRate();
        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            return await userManager.FindByNameAsync("khaledgomaa");
        }

        private async Task<Status> CheckPlaygroundStatus(int playgroundId)
        {
            return (await unitOfWork.PlayGroundRepo.Find(b => b.PlaygroundId == playgroundId))?.PlaygroundStatus ?? Status.Busy;
        }
    }
}