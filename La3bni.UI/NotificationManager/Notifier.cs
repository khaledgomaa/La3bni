using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.NotificationManager
{
    public class Notifier : INotifier
    {
        private readonly IUnitOfWork _unitOfWork;

        public Notifier(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void SendNotification(string userId, string title, string body)
        {
            _unitOfWork.NotificationRepo.Add(new Notification
            {
                ApplicationUserId = userId,
                Title = title,
                Body = body
            });
        }
    }
}
