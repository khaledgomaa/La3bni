using La3bni.UI.NotificationManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly INotifier notifier;

        public BookingController(IUnitOfWork _unitOfWork,
                                UserManager<ApplicationUser> _userManager
                               , INotifier _notifier)
        {
            unitOfWork = _unitOfWork;
            userManager = _userManager;
            notifier = _notifier;
        }

        [Route("Booking/{id}")]
        [Route("Booking/Index/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Index(int id)
        {
            Playground playGround = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == id);

            return View(playGround);
        }

        [AllowAnonymous]
        public List<PlayGroundTimesViewModel> GetTimes(int id)
        {
            IEnumerable<PlaygroundTimes> times = GetPlaygroundTimesById(id);

            var newTimesFormat = times.Select(a => new PlayGroundTimesViewModel
            {
                Time = $"{a.From:HH:mm} - {a.To:HH:mm} {a.State}",
                Id = a.PlaygroundTimesId
            }).ToList();

            return newTimesFormat;
        }

        private List<PlaygroundTimes> GetPlaygroundTimesById(int playGroundId) =>
            unitOfWork.PlaygroundTimesRepo.GetAllIQueryable()
                                        .Where(t => t.PlaygroundId == playGroundId)
                                        .ToList();
        private async Task<bool> IsUserSignedIn()
        {
            string userId = (await GetCurrentUser())?.Id ?? "";
            return !string.IsNullOrEmpty(userId);
        }

        private bool IsPlayerCanJoinTeam(int bookingId, int maxNumberOfPlayers) =>
            unitOfWork.BookingTeamRepo.GetAllIQueryable()
                                                  .Count(b => b.BookingId == bookingId) < maxNumberOfPlayers;

        private async Task<bool> IsBookingNotExist(int playGroundId, int timeId, DateTime bookedDate)
        {
            var booking = await GetBookingByPlayGroundIdWithDateTime(playGroundId, timeId, bookedDate);
            return string.IsNullOrEmpty(booking?.ApplicationUserId);
        }

        private async Task<Booking> GetBookingByPlayGroundIdWithDateTime(int playGroundId, int timeId, DateTime bookedDate)
            => await unitOfWork.BookingRepo.Find(b => b.PlaygroundId == playGroundId
                                                               && b.PlaygroundTimesId == timeId
                                                               && b.BookedDate == bookedDate) ?? new Booking();

        public async Task<PlayerBookingStatus> GetPlayerBookingStatus(int playGroundId, string date, string timeId)
        {
            if (!await IsUserSignedIn())
            {
                return PlayerBookingStatus.NotFound;
            }
            else
            {
                int.TryParse(timeId, out int bookingTimeId);
                string userId = (await GetCurrentUser()).Id;
                DateTime bookedDate = Convert.ToDateTime(date);
                var booking = await SearchBookingByPlayGroundIdForUserIdWithDateTime(playGroundId, bookingTimeId, userId, bookedDate);
                if (booking.BookingId == (int)Result.NotFound)
                {
                    if (await IsBookingNotExist(playGroundId, bookingTimeId, bookedDate))
                        return PlayerBookingStatus.BookTeam;

                    var bookingInDb = await GetBookingByPlayGroundIdWithDateTime(playGroundId, bookingTimeId, bookedDate);

                    if (await CheckUserJoinedTeam(userId, bookingInDb.BookingId))
                        return PlayerBookingStatus.LeaveTeam;

                    if (IsPlayerCanJoinTeam(bookingInDb.BookingId, bookingInDb.MaxNumOfPlayers))
                        return PlayerBookingStatus.CanJoinTeam;

                    return PlayerBookingStatus.CanNotJoinTeam;
                }
                return booking.Paid == (int)PaymentStatus.Paid ? PlayerBookingStatus.Paid : PlayerBookingStatus.CancelBook;
            }
        }

        public async Task<Booking> SearchBookingByPlayGroundIdForUserIdWithDateTime(int playGroundId, int timeId, string userId, DateTime date)
        {
            return (await unitOfWork.BookingRepo.Find(b => b.ApplicationUserId == userId
                                        && b.PlaygroundId == playGroundId
                                        && b.BookedDate.Date >= DateTime.Now.Date
                                        && b.BookedDate.Date == date.Date
                                        && b.PlaygroundTimesId == timeId)) ?? new Booking();

        }

        private async Task<bool> CheckUserJoinedTeam(string userId, int bookingId)
        {
            var bookingDetails = await unitOfWork.BookingTeamRepo.Find(b => b.BookingId == bookingId && b.ApplicationUserId == userId);
            return !string.IsNullOrEmpty(bookingDetails?.ApplicationUserId);
        }

        private async Task<Booking> GetBookingDetails(string period, int playgroundId, string selectedDate)
        {
            int.TryParse(period, out int timeId);
            DateTime bookedDate = Convert.ToDateTime(selectedDate);
            return await GetBookingByPlayGroundIdWithDateTime(playgroundId, timeId, bookedDate);
        }

        public async Task<IActionResult> CreateBooking(string period, int playgroundId, string selectedDate, string numOfPlayers)
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
                    PlaygroundId = playgroundId,
                    MaxNumOfPlayers = playersNo,
                    Price = price
                };

                try
                {
                    unitOfWork.BookingRepo.Add(newBooking);

                    notifier.SendNotification(userId, "Booking has been created",
                        $"Playground : {playgroundTimesDetails?.Playground.Name ?? "NA"} on {newBooking?.BookedDate:d} - {playgroundTimesDetails}");

                    unitOfWork.Save();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return Json(new { error = "No bookings available please reload your page" });
                }
            }
            return Json(new { redirectToUrl = Url.Action("Index", "MyBookings") });
        }

        public async Task<IActionResult> JoinTeam(string period, int playgroundId, string selectedDate)
        {
            try
            {
                var bookingDetails = await GetBookingDetails(period, playgroundId, selectedDate);
                var curUser = await GetCurrentUser();
                unitOfWork.BookingTeamRepo.Add(new BookingTeam
                {
                    BookingId = bookingDetails.BookingId,
                    ApplicationUserId = curUser?.Id
                });

                var bookingOwnerDetails = await userManager.FindByIdAsync(bookingDetails.ApplicationUserId);

                notifier.SendNotification(bookingOwnerDetails?.Id, "New player joined your team",
                    $"Phone : {curUser?.PhoneNumber} , Playground : {bookingDetails.Playground?.Name ?? "NA"} on {bookingDetails?.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                    );

                notifier.SendNotification((await GetCurrentUser())?.Id, "You have joined team",
                    $"Playground : {bookingDetails.Playground?.Name ?? "NA"} on {bookingDetails?.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                    );

                unitOfWork.Save();
                return Json(new { redirectToUrl = Url.Action("Teams", "MyBookings") });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Json(new { error = "No teams available please reload your page" });
            }

        }

        public async Task<IActionResult> CancelBooking(string period, int playgroundId, string selectedDate)
        {
            var bookingDetails = await GetBookingDetails(period, playgroundId, selectedDate);

            try
            {
                notifier.SendNotification((await GetCurrentUser())?.Id, "Booking has been canceled",
                    $"Playground : {bookingDetails.Playground.Name} on {bookingDetails.BookedDate:d} - {bookingDetails.PlaygroundTimes}"
                    );

                unitOfWork.BookingRepo.Delete(bookingDetails);
                unitOfWork.Save();
                return Json(new { redirectToUrl = Url.Action("Index", "MyBookings") });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Json(new { error = "booking already canceled please reload your page" });
            }
        }

        public async Task<IActionResult> LeaveTeam(string period, int playgroundId, string selectedDate)
        {
            try
            {
                var userDetails = await GetCurrentUser();
                int bookingId = (await GetBookingDetails(period, playgroundId, selectedDate))?.BookingId ?? 0;
                var bookingTeamDetails = await unitOfWork.BookingTeamRepo.FindWithInclude(b => b.BookingId == bookingId);

                notifier.SendNotification(bookingTeamDetails?.Booking?.ApplicationUserId,
                    "Player left your team",
                    $"Playground : {bookingTeamDetails.Booking.Playground?.Name ?? "NA"} on {bookingTeamDetails.Booking?.BookedDate:d} - {bookingTeamDetails.Booking.PlaygroundTimes}"
                    );

                notifier.SendNotification(userDetails?.Id,
                    "You left team",
                    $"Playground : {bookingTeamDetails.Booking.Playground?.Name ?? "NA"} on {bookingTeamDetails.Booking?.BookedDate:d} - {bookingTeamDetails.Booking.PlaygroundTimes}"
                    );

                unitOfWork.BookingTeamRepo.Delete(bookingTeamDetails);

                unitOfWork.Save();

                return Json(new { redirectToUrl = Url.Action("", "Home") });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Json(new { error = "Team leader already canceled booking please reload your page" });
            }

        }

        private void AddNewRatingForUserToPlayground(string userId,int playGroundId,float rate)
        {
            unitOfWork.PlaygroundRateRepo.Add(new PlaygroundRate
            {
                ApplicationUserId = userId,
                Rate = rate,
                PlaygroundId = playGroundId
            });
        }

        private float GetPlaygroundAvgRate(int playGroundId)
        {
            return unitOfWork.PlaygroundRateRepo.GetAll()
                                                .Where(r => r.PlaygroundId == playGroundId)
                                                .Average(r => r.Rate);
        }

        private async Task UpdatePlayGroundRateById(int playGroundId,float rate)
        {
            var playground = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == playGroundId);
            playground.Rate = rate;
            unitOfWork.PlayGroundRepo.Update(playground);
        }

        public async Task UpdateRate(string playgroundId, float rate)
        {
            string userId = (await GetCurrentUser())?.Id;

            if (int.TryParse(playgroundId, out int playGroundId))
            {
                if (await CheckRatedBefore(playGroundId) > 0)
                {
                    var ratingDetails = await GetUserPlayGroundRatingDetails(userId, playGroundId);
                    ratingDetails.Rate = rate;
                    unitOfWork.PlaygroundRateRepo.Update(ratingDetails);
                }
                else
                {
                    AddNewRatingForUserToPlayground(userId, playGroundId, rate);
                    unitOfWork.Save();
                }

                float playGroundAvgRate = GetPlaygroundAvgRate(playGroundId);
                await UpdatePlayGroundRateById(playGroundId, playGroundAvgRate);

                unitOfWork.Save();
            }
        }

        private async Task<PlaygroundRate> GetUserPlayGroundRatingDetails(string userId, int playGroundId)
            => await unitOfWork.PlaygroundRateRepo.Find(b => b.ApplicationUserId == userId && b.PlaygroundId == playGroundId)
            ?? new PlaygroundRate();

        public async Task<float> CheckRatedBefore(int playGroundId)
        {
            string userId = (await GetCurrentUser()).Id;
            var ratingDetails = await GetUserPlayGroundRatingDetails(userId, playGroundId);
            return ratingDetails?.Rate ?? 0;
        }

        [AllowAnonymous]
        public async Task<ApplicationUser> GetCurrentUser() => await userManager.GetUserAsync(User);

    }

    public enum PlayerBookingStatus
    {
        BookTeam,
        CancelBook,
        CanJoinTeam,
        CanNotJoinTeam,
        LeaveTeam,
        Paid,
        NotFound
    }

    public enum Result
    {
        NotFound,
        Found
    }

    public enum PaymentStatus
    {
        NotPaid,
        Paid,
    }
}