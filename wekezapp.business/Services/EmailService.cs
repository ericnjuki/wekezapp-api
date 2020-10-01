using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using wekezapp.business.Contracts;
using wekezapp.data.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace wekezapp.business.Services {
    public class EmailService : IEmailService {
        public EmailService() {

        }

        public void NewEmail(EmailOptions emailOpts) {
            var fromAddress = new MailAddress("njukieric@gmail.com", "Wekezapp Admin");
            var toAddress = new MailAddress(emailOpts.Receipient, "Chama Member");
            const string fromPassword = "greyskelton";
            string subject = "Chama Login Credentials";
            string body = emailOpts.Message;

            var smtp = new SmtpClient {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress) {
                Subject = subject,
                Body = body
            }) {
                smtp.Send(message);
            }
        }

        public async void SendGridMail() {
            // if you get a Forbidden response,
            // go to https://app.sendgrid.com/settings/sender_auth.
            // In the middle of the page you'll see "Verify Single Sender".

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("njukieric@gmail.com", "WekezApp");
            var subject = "WekezApp notification";
            var to = new EmailAddress("ericnjuki@gmail.com", "Eric Njuki");
            var plainTextContent = "This is a WekezApp notification.";
            var htmlContent = "<strong>Thank you for being with us.</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }
}
