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

        public void NewEmail(EmailOptions emailOpts, string emailSubject) {
            var fromAddress = new MailAddress("ericnjuki@gmail.com", "Wekezapp Admin");
            var toAddress = new MailAddress(emailOpts.Receipient, "Chama Member");
            string fromPassword = Environment.GetEnvironmentVariable("SENDER_PW");
            string subject = emailSubject;
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

        public async void SendGridMail(string recipient, string subject, string bodyText, string bodyHtml) {
            // if you get a Forbidden response,
            // go to https://app.sendgrid.com/settings/sender_auth.
            // In the middle of the page you'll see "Verify Single Sender".

            if (string.IsNullOrEmpty(recipient))
                recipient = "ericnjuki@gmail.com";

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("njukieric@gmail.com", "WekezApp");
            var to = new EmailAddress(recipient, "Eric Njuki");
            var plainTextContent = bodyText;
            var htmlContent = bodyHtml;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }
}
