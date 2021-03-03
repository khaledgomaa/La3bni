using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repository;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;

        public PaymentController(IConfiguration _configuration, IUnitOfWork _unitOfWork)
        {
            configuration = _configuration;
            unitOfWork = _unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Charge()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Charge(int bookingId)
        {
            long usdCurrency = (long)(await unitOfWork.BookingRepo.Find(b => b.BookingId == bookingId)).Price;
            var domain = configuration["Domain"];
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                      UnitAmount = 100*usdCurrency,
                      Currency = "usd",
                      ProductData = new SessionLineItemPriceDataProductDataOptions
                      {
                        Name = "Stubborn Attachments",
                      },
                    },
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = domain + "/Payment/OrderSuccess?session_id={CHECKOUT_SESSION_ID}&bookingId=" + bookingId,
                CancelUrl = domain + "/MyBookings/Index",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            return Json(new { id = session.Id });
        }

        [HttpGet]
        public async Task<ActionResult> OrderSuccess([FromQuery] string session_id, int bookingId)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id);

            var customerService = new CustomerService();
            Customer customer = customerService.Get(session.CustomerId);

            var bookingDetails = await unitOfWork.BookingRepo.Find(b => b.BookingId == bookingId);
            bookingDetails.Paid = 1; //paid done
            unitOfWork.BookingRepo.Update(bookingDetails);
            unitOfWork.Save();

            return RedirectToAction("Index", "MyBookings");
        }

        public IActionResult Cancel()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Pay(string stripeEmail, string stripeToken)
        //{
        //    var customers = new CustomerService();
        //    var charges = new ChargeService();
        //    var customer = customers.Create(new CustomerCreateOptions
        //    {
        //        Email = stripeEmail,
        //        Source = stripeToken
        //    });
        //    var charge = charges.Create(new ChargeCreateOptions
        //    {
        //        Amount = 500,
        //        Currency = "usd",
        //        Customer = customer.Id,
        //        ReceiptEmail = stripeEmail
        //    });

        //    if (charge.Status == "succeeded")
        //    {
        //        string transId = charge.BalanceTransactionId;
        //        return View();
        //    }

        //    return View();
        //}
    }
}