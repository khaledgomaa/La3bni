using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.Adminpanel.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmailController : Controller
    {
        private readonly IUnitOfWork context;
        private readonly IEmailRepository emailRepository;

        public EmailController(IUnitOfWork context, IEmailRepository emailRepository)
        {
            this.context = context;
            this.emailRepository = emailRepository;
        }

        [Route("Emails")]
        public IActionResult Index()
        {
            return View(emailRepository.getAllMessages().OrderByDescending(d => d.Date));
        }

        [Route("SendEmail")]
        public IActionResult SendEmail()
        {
            ViewBag.Emails = new SelectList(context.SubscriberRepo.GetAll().Select(e => e.Email));
            return View();
        }

        [Route("SendEmail")]
        [HttpPost]
        public IActionResult SendEmail(FeedBack emails)
        {
            emailRepository.sendEmail(emails.Name, emails.Message, emails.SelectedEmails);
            return RedirectToAction("Index");
        }
    }
}