using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Controllers
{
    [Area("Owner")]
    [Route("Playgrounds")]
    public class PlaygroundsController : Controller
    {
        private IUnitOfWork unitOfWork;
        private readonly ImageManager imageManager;
        private readonly UserManager<ApplicationUser> userManager;

        public PlaygroundsController(IUnitOfWork unitOfWork, ImageManager imageManager, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.imageManager = imageManager;
            this.userManager = userManager;
        }

        // GET: playgroundsController
        [Route("Index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            //for testing right now
            ApplicationUser user = await userManager.GetUserAsync(User);
            string userId = user?.Id ?? "";
            if (userManager.GetRolesAsync(user).Result?.ElementAt(0) == "Owner")
            {
                return View(unitOfWork.PlayGroundRepo.GetAll().Where(p => p.ApplicationUserId == userId));
            }

            return View(unitOfWork.PlayGroundRepo.GetAll());
        }

        [Route("Details")]
        // GET: playgroundsController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var stadium = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == id);

            if (stadium == null)
            {
                return NotFound();
            }

            ViewData["PlaygroundTimes"] = unitOfWork.PlaygroundTimesRepo.GetAll().Where(p => p.PlaygroundId == id).ToList();

            return View(stadium);
        }

        [Route("Create")]

        // GET: playgroundsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: playgroundsController/Create

        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(string playgroundtimesinfo, string Playground, string image)
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            string userID = user?.Id ?? "";
            try
            {
                Playground playground1 = JsonConvert.DeserializeObject<Playground>(Playground);
                if (ModelState.IsValid)
                {
                    playground1.ApplicationUser = user;
                    playground1.ApplicationUserId = userID;//user.Id;
                    playground1.ImagePath = imageManager.UploadFile(image, "images");
                    unitOfWork.PlayGroundRepo.Add(playground1);
                    //unitOfWork.Save();
                }

                if (!string.IsNullOrEmpty(playgroundtimesinfo))
                {
                    List<PlaygroundTimes> playgroundTimes = JsonConvert.DeserializeObject<List<PlaygroundTimes>>(playgroundtimesinfo);
                    foreach (var stadiumTimes in playgroundTimes)
                    {
                        stadiumTimes.Playground = playground1;
                        //stadiumTimes.PlaygroundId = playground1.PlaygroundId;
                        unitOfWork.PlaygroundTimesRepo.Add(stadiumTimes);
                    }
                    unitOfWork.Save();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [Route("DeletePlaygroundTimes")]

        // ajax request method for deleting record for specific playground
        public async Task<IActionResult> DeletePlaygroundTimes(string pid)
        {
            if (!string.IsNullOrEmpty(pid))
            {
                //pid is a Json object 1- deseralize to int
                // 2- use this id to get the current playground and remove it from database

                int playgroundId = JsonConvert.DeserializeObject<int>(pid);
                var playgroundTimes = unitOfWork.PlaygroundTimesRepo.Find(t => t.PlaygroundTimesId == playgroundId);
                unitOfWork.PlaygroundTimesRepo.Delete(await playgroundTimes);
                unitOfWork.Save();

                return Json(pid);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("UpdatePlayGroundTimes")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePlayGroundTimes(string playgroundtimesinfo)
        {
            if (!string.IsNullOrEmpty(playgroundtimesinfo))
            {
                // second object because during update the attributes in the recieved object contain values
                // others set to null so we get the get object from database and update the required attributes only

                PlaygroundTimes playgroundTimes = JsonConvert.DeserializeObject<PlaygroundTimes>(playgroundtimesinfo);
                PlaygroundTimes playgroundTimes1 = await unitOfWork.PlaygroundTimesRepo.Find(t => t.PlaygroundTimesId == playgroundTimes.PlaygroundTimesId);
                playgroundTimes1.From = playgroundTimes.From;
                playgroundTimes1.To = playgroundTimes.To;
                unitOfWork.PlaygroundTimesRepo.Update(playgroundTimes1);
                unitOfWork.Save();
                return Json(playgroundtimesinfo);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Edit")]
        // GET: playgroundsController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stadium = await unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
            ViewData["PlaygroundTimes"] = unitOfWork.PlaygroundTimesRepo.GetAll().Where(p => p.PlaygroundId == id).ToList();

            if (stadium == null)
            {
                return NotFound();
            }
            return View(stadium);
        }

        // POST: playgroundsController/Edit/5
        [Route("Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(string playgroundtimesinfo, string Playground)
        {
            try
            {
                Playground playground2 = JsonConvert.DeserializeObject<Playground>(Playground);
                Playground playground1 = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == playground2.PlaygroundId);
                if (ModelState.IsValid)
                {
                    playground1.Name = playground2.Name;
                    playground1.City = playground2.City;
                    playground1.StadiumArea = playground2.StadiumArea;
                    playground1.AmPrice = playground2.PmPrice;
                    playground1.PmPrice = playground2.PmPrice;
                    playground1.Services = playground2.Services;
                    playground1.PlaygroundStatus = playground2.PlaygroundStatus;
                    playground1.IsOffered = playground2.IsOffered;
                    playground1.CreatedOn = playground2.CreatedOn;
                    unitOfWork.PlayGroundRepo.Update(playground1);
                    unitOfWork.Save();
                }

                if (!string.IsNullOrEmpty(playgroundtimesinfo))
                {
                    List<PlaygroundTimes> playgroundTimes = JsonConvert.DeserializeObject<List<PlaygroundTimes>>(playgroundtimesinfo);
                    foreach (var stadiumTimes in playgroundTimes)
                    {
                        stadiumTimes.PlaygroundId = playground1.PlaygroundId;
                        stadiumTimes.Playground = playground1;

                        unitOfWork.PlaygroundTimesRepo.Add(stadiumTimes);
                    }
                    unitOfWork.Save();
                }
            }
            catch
            {
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("Delete")]
        // GET: playgroundsController/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                // return NotFound();
            }
            var stadium = unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
            if (stadium == null)
            {
                // return NotFound();
            }
            return View(await stadium);
        }

        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Playground collection)
        {
            Playground stadium = await unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == collection.PlaygroundId);
            imageManager.DeleteFile("images", stadium.ImagePath);
            unitOfWork.PlayGroundRepo.Delete(stadium);
            var stadiumTimes = unitOfWork.PlaygroundTimesRepo.GetAll().ToList();
            stadiumTimes = stadiumTimes.Where(t => t.PlaygroundId == stadium.PlaygroundId).ToList();
            foreach (var obj in stadiumTimes)
            {
                unitOfWork.PlaygroundTimesRepo.Delete(obj);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}