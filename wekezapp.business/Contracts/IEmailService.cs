using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Interfaces;

namespace wekezapp.business.Contracts {
    public interface IEmailService {
        void NewEmail(EmailOptions emailOpts);

        void SendGridMail();
    }
}
