using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Interfaces;

namespace wekezapp.business.Contracts {
    public interface IEmailService {
        void NewEmail(EmailOptions emailOpts, string emailSubject);

        void SendGridMail(string recipient, string subject, string bodyText, string bodyHtml);
    }
}
