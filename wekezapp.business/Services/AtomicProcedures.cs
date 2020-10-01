using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wekezapp.business.Contracts;
using wekezapp.data.Entities;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class AtomicProcedures : IAtomicProcedures {

        private readonly WekezappContext _ctx;
        private Chama chama;
        public AtomicProcedures(WekezappContext ctx) {
            _ctx = ctx;
            chama = _ctx.Chamas.First();
        }

        public void OutOfChama(float amount) {
            if (chama.Balance >= amount) {
                chama.Balance -= amount;
                _ctx.SaveChanges();
            } else {
                throw new Exception("Insufficient balance in chama account, please top up");
            }
        }

        public void IntoChama(float amount) {
            chama.Balance += amount;
            _ctx.SaveChanges();
        }

        public void OutOfPersonal(float amount, int userId) {
            var user = _ctx.Users.Find(userId);

            if (user.Balance >= amount) {
                user.Balance -= amount;
                _ctx.SaveChanges();
            } else {
                throw new Exception("Insufficient balance in your account, please top up");
            }
        }

        public void IntoPersonal(float amount, int userId) {
            var user = _ctx.Users.Find(userId);
            user.Balance += amount;
            _ctx.SaveChanges();
        }

        public void ChamaToPersonal(float amount, int userId) {
            if (chama.Balance < amount) {
                throw new Exception("Insufficient balance in Chama please top up");
            } else {
                var user = _ctx.Users.Find(userId);
                chama.Balance -= amount;
                user.Balance += amount;
                _ctx.SaveChanges();
            }
        }

        public void PersonalToChama(float amount, int userId) {
            var user = _ctx.Users.Find(userId);
            if (user.Balance < amount) {
                throw new Exception("Insufficient balance in your account please top up");
            } else {
                user.Balance -= amount;
                chama.Balance += amount;
                _ctx.SaveChanges();
            }
        }
    }
}
