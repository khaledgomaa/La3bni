using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public interface IEmailRepository
    {
        public List<MimeMessage> getAllMessages();
        public void sendEmail(string Subject, string Body, List<string> Recievers);
    }
}
