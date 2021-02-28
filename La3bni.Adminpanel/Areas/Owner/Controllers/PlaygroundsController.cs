using Microsoft.AspNetCore.Http;
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

        public PlaygroundsController(IUnitOfWork unitOfWork, ImageManager imageManager)
        {
            this.unitOfWork = unitOfWork;
            this.imageManager = imageManager;
        }

        // GET: playgroundsController
        [Route("Index")]
        [Route("")]
        public IActionResult Index()
        {
            var x = unitOfWork.PlayGroundRepo.GetAll().ToList();
            return View(x);
        }

        [Route("Details")]
        // GET: playgroundsController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var stadiums = await unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);

            if (stadiums == null)
            {
                return NotFound();
            }
            var times = unitOfWork.PlaygroundTimesRepo.GetAll().ToList();
            ViewData["PlaygroundTimes"] = times.Where(p => p.PlaygroundId == id).ToList();
            return View(stadiums);
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
        //[ValidateAntiForgeryToken]
        public IActionResult Create(string playgroundtimesinfo, string Playground, string image)
        {
            List<PlaygroundTimes> playgroundTimes = JsonConvert.DeserializeObject<List<PlaygroundTimes>>(playgroundtimesinfo);
            Playground playground = JsonConvert.DeserializeObject<Playground>(Playground);
            playground.ImagePath = imageManager.UploadFile(image, "images");

            /* if (playground.Services == null)
             {
                 playground.Services = 0;
             }
             else
             {*/
            // playground.Services = (Services)Enum.Parse(typeof(Services), services);
            //}
            //playground.ImagePath = imageManager.UploadFile(playground.ImageFile, "images");
            unitOfWork.PlayGroundRepo.Add(playground);
            unitOfWork.Save();

            foreach (var obj in playgroundTimes)
            {
                obj.PlaygroundId = playground.PlaygroundId;
                obj.Playground = playground;

                unitOfWork.PlaygroundTimesRepo.Add(obj);
            }
            unitOfWork.Save();
            // try
            //{
            //  if (ModelState.IsValid)
            //  {
            //    if (services == null)
            //    {
            // playground.Services = 0;  // Depending whether you allow neither day to be selected
            // you can handle this differently
            // }
            //  else
            //  {
            //playground.Services = (Services)Enum.Parse(typeof(Services), services);
            //  }
            //playground.ImagePath = imageManager.UploadFile(playground.ImageFile, "images");
            //unitOfWork.PlayGroundRepo.Add(playground);
            //   unitOfWork.Save();
            // }
            // return RedirectToAction(nameof(Index));
            // }
            //catch
            // {
            return RedirectToAction(nameof(Index));
            //}
        }

        [Route("Edit")]
        // GET: playgroundsController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var stadiums = await unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
            var times = unitOfWork.PlaygroundTimesRepo.GetAll().ToList();
            ViewData["PlaygroundTimes"] = times.Where(p => p.PlaygroundId == id).ToList();
            if (stadiums == null)
            {
                return NotFound();
            }
            return View(stadiums);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlaygroundTimes(string pid)
        {
            int playgroundId = JsonConvert.DeserializeObject<int>(pid);
            var playgroundTimes = unitOfWork.PlaygroundTimesRepo.Find(t => t.PlaygroundTimesId == playgroundId);
            unitOfWork.PlaygroundTimesRepo.Delete(await playgroundTimes);
            unitOfWork.Save();

            return Json(pid);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePlayGroundTimesAsync(string playgroundtimesinfo)
        {
            PlaygroundTimes playgroundTimes = JsonConvert.DeserializeObject<PlaygroundTimes>(playgroundtimesinfo);
            PlaygroundTimes playgroundTimes1 = await unitOfWork.PlaygroundTimesRepo.Find(t => t.PlaygroundTimesId == playgroundTimes.PlaygroundTimesId);
            playgroundTimes1.From = playgroundTimes.From;
            playgroundTimes1.To = playgroundTimes.To;
            unitOfWork.PlaygroundTimesRepo.Update(playgroundTimes1);
            unitOfWork.Save();
            return Json(playgroundtimesinfo);
        }

        // POST: playgroundsController/Edit/5
        [Route("Edit")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string playgroundtimesinfo, string Playground)
        {
            Playground playground2 = JsonConvert.DeserializeObject<Playground>(Playground);

            Playground playground1 = await unitOfWork.PlayGroundRepo.Find(p => p.PlaygroundId == playground2.PlaygroundId);

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
            //unitOfWork.Save();
            if (playgroundtimesinfo != null && playgroundtimesinfo != "")
            {
                List<PlaygroundTimes> playgroundTimes = JsonConvert.DeserializeObject<List<PlaygroundTimes>>(playgroundtimesinfo);
                foreach (var obj in playgroundTimes)
                {
                    obj.PlaygroundId = playground1.PlaygroundId;
                    obj.Playground = playground1;

                    unitOfWork.PlaygroundTimesRepo.Add(obj);
                }
                unitOfWork.Save();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //playground.ImagePath = imageManager.UploadFile(playground.ImageFile, "images");
                    // unitOfWork.PlayGroundRepo.Update(playground);
                    //unitOfWork.Save();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [Route("Delete")]
        // GET: playgroundsController/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var stadiums = unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
            if (stadiums == null)
            {
                return NotFound();
            }
            return View(await stadiums);
        }

        // POST: playgroundsController/Delete/5
        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            Playground stadiums = await unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
            imageManager.DeleteFile("images", stadiums.ImagePath);
            unitOfWork.PlayGroundRepo.Delete(stadiums);
            var stadiumTimes = unitOfWork.PlaygroundTimesRepo.GetAll().ToList();
            stadiumTimes = stadiumTimes.Where(t => t.PlaygroundId == stadiums.PlaygroundId).ToList();
            foreach (var obj in stadiumTimes)
            {
                unitOfWork.PlaygroundTimesRepo.Delete(obj);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
            try
            {
                // imageManager.DeleteFile("images");
                // var stadiums = unitOfWork.PlayGroundRepo.Find(s => s.PlaygroundId == id);
                //unitOfWork.PlayGroundRepo.Delete(await stadiums);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}