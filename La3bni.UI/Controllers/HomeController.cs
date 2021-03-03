using Microsoft.AspNetCore.Mvc;
using Models;
using Repository;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace La3bni.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageManager imageManager;
        private readonly IUnitOfWork unitOfwork;
        private readonly IConfiguration configuration;
        private readonly IEmailRepository emailRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(
            ImageManager _imageManager,
            IUnitOfWork unitOfwork,
            IConfiguration configuration,
            IEmailRepository emailRepository,
            UserManager<ApplicationUser> userManager)
        {
            imageManager = _imageManager;
            this.unitOfwork = unitOfwork;
            this.configuration = configuration;
            this.emailRepository = emailRepository;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult getPlaygrounds()
        {
            IActionResult ret = null;

            var playgrounds = getPlaygroundsInJsonFormats();

            if (playgrounds != null)
            {
                ret = Ok(playgrounds);
            }
            else
            {
                ret = NotFound();
            }
            return ret;
        }

        [HttpPost]
        public void AddToSubscribers(string email)
        {
            if(!unitOfwork.SubscriberRepo.GetAll().Any(p=>p.Email==email))
            {
                unitOfwork.SubscriberRepo.Add(new Subscriber() { Email =email  });
                if(unitOfwork.Save() >0)
                {
                    emailRepository.sendEmail("La3bniKoora", "Welcome on board, you'r now a subscriber", new List<string>() { email });
                }
                
            }

        }
        public IActionResult GetInTouch(FeedBack feedBack)
        {
            unitOfwork.FeedBackRepo.Add(feedBack);
            //add his email to subscribers if he is not already a subscriber
            if (!unitOfwork.SubscriberRepo.GetAll().Any(d => d.Email == feedBack.Email))
                unitOfwork.SubscriberRepo.Add(new Subscriber() { Email = feedBack.Email });
            unitOfwork.Save();
            emailRepository.sendEmail(
                "La3bniKoora buiseness",
                 feedBack.Message + "\n\nFrom : " + feedBack.Name + "\n" + feedBack.Email,
                 new List<string>() { "mohmedshawky2019@gmail.com" });
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var myNews = await GetNewsAsync();
            dynamic jsonData = JObject.Parse(myNews);
            ViewBag.articles = jsonData.articles;

            ViewBag.Playgrounds = unitOfwork.PlayGroundRepo.GetAll().ToList();
            return View();
        }

        public async Task<string> GetNewsAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://newscatcher.p.rapidapi.com/v1/latest_headlines?topic=sport&country=EG&media=True"),
                Headers =
                        {
                            { "x-rapidapi-key", "ac3cefb84bmsha47f177bda5693ep16f1b6jsn43a49b242cd7" },
                            { "x-rapidapi-host", "newscatcher.p.rapidapi.com" },
                        },
            };

            using (var response = await client.SendAsync(request))
            {
                //response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public JsonResult getPlaygroundsInJsonFormats()
        {
            return Json(unitOfwork.PlayGroundRepo.GetAll().ToList());
        }
    }
}