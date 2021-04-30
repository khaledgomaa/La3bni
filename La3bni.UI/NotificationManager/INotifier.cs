using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace La3bni.UI.NotificationManager
{
    public interface INotifier
    {
        void SendNotification(string userId,string title,string body);
    }
}
