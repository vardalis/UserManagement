using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace UserManagement.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private string _smtpServer;
        private int _port;
        private string _username;
        private string _password;

        public EmailSender(string smtpServer, int port, string username, string password)
        {
            _smtpServer = smtpServer;
            _port = port;
            _username = username;
            _password = password;
        }
        public Task SendEmailAsync(string emailAddress, string subject, string text)
        {
            SmtpClient client = new SmtpClient(_smtpServer, _port);
            // SmtpClient client = new SmtpClient("smtp.gmail.com", 587); // TLS
            client.EnableSsl = false; // This is TLS. True SSL is not supported by SmtpClient
            // client.Credentials = new System.Net.NetworkCredential("vardalis@gmail.com", "");
            client.Credentials = new System.Net.NetworkCredential(_username, _password);

            MailMessage message = new MailMessage
            {
                // From = new MailAddress("e-applications@esdi.gr", "Esdi e-applications"),
                From = new MailAddress(_username, "Esdi e-applications"),
                Subject = subject,
                Body = text                
            };

            message.To.Add(new MailAddress(emailAddress));

            return client.SendMailAsync(message);
        }
    }
}
