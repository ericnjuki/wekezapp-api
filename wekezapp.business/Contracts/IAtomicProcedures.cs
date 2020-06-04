using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.business.Contracts
{
    public interface IAtomicProcedures
    {
        void OutOfChama(float amount);

        void IntoChama(float amount);

        void OutOfPersonal(float amount, int userId);

        void IntoPersonal(float amount, int userId);

        void ChamaToPersonal(float amount, int userId);

        void PersonalToChama(float amount, int userId);
    }
}
