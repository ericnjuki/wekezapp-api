using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.business.Contracts;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class AtomicProcedures : IAtomicProcedures {

        private readonly WekezappContext _ctx;
        public AtomicProcedures(WekezappContext ctx) {
            _ctx = ctx;
        }

        public void OutOfChama(float amount) {

        }

        public void IntoChama(float amount) {

        }

        public void OutOfPersonal(float amount, int userId) {

        }

        public void IntoPersonal(float amount, int userId) {

        }

        public void ChamaToPersonal(float amount, int userId) {

        }

        public void PersonalToChama(float amount, int userId) {

        }
    }
}
