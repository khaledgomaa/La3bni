using Microsoft.AspNetCore.Mvc;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FeedBackController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public FeedBackController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        public IActionResult Index()
        {
            return View(unitOfWork.FeedBackRepo.GetAll());
        }
    }
}