using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using wekezapp.business.Contracts;
using wekezapp.data.Interfaces;

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
    }
}
