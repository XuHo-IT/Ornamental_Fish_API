using Fish_Manage.Repository.IRepository;
using System.Net;
using System.Net.Mail;

namespace Fish_Manage.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("deskit1xuho@gmail.com", "cjpfkzrcjygfmonv")
            };

            return client.SendMailAsync(
                new MailMessage(from: "deskit1xuho@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
