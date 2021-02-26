using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Pop3;
using MimeKit;
using System.Net.Mail;
using System.Net;

namespace Repository
{
    public class EmailRepository : IEmailRepository
    {
        public List<MimeMessage> getAllMessages() {
		        using (var client = new Pop3Client()) {
				client.Connect("pop.gmail.com", 995, true);

				client.Authenticate("recent:"+Resources.Email, Resources.Password);
				int d = client.GetMessageCount();
				List<MimeMessage> messages = new List<MimeMessage>();
				for (int i = 0; i<client.Count; i++) {
					messages.Add(  client.GetMessage(i));
				}
				client.Disconnect(true);


                return messages;
			}
		}
        public void sendEmail(string Subject, string Body, List<string> Recievers)
        {
            foreach (var item in Recievers)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(item);
                mail.From = new MailAddress(Resources.Email);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.UseDefaultCredentials = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(Resources.Email, Resources.Password);
                smtp.Send(mail);

            }
        }    
    }

}
