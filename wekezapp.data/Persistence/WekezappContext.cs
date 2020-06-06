using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using wekezapp.data.Entities;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Interfaces;

namespace wekezapp.data.Persistence {
    public class WekezappContext: DbContext {
        public WekezappContext(DbContextOptions opts) : base(opts) {
            //Database.SetInitializer(new CreateDatabaseIfNotExists<ShopAssist2Context>());
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Chama> Chamas { get; set; }

        public DbSet<FlowItem> FlowItems { get; set; }

        public DbSet<Loan> Loans { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<ChamaDeposit> ChamaDeposits { get; set; }

        public DbSet<ChamaWithdrawal> ChamaWithdrawals { get; set; }

        public DbSet<Contribution> Contributions { get; set; }

        public DbSet<ContributionFine> ContributionFines { get; set; }

        public DbSet<PayOut> PayOuts { get; set; }

        public DbSet<MerryGoRound> MerryGoRounds { get; set; }

        public DbSet<PersonalDeposit> PersonalDeposits { get; set; }

        public DbSet<PersonalWithdrawal> PersonalWithdrawals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // compound keys have to be added using fluent api
            //modelBuilder.Entity<ChamaUser>()
            //    .HasKey(c => new { c.ChamaId, c.UserId });

            modelBuilder.Entity<Chama>()
                .Property(e => e.MgrOrder)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<FlowItem>()
                .Property(e => e.CanBeSeenBy)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<FlowItem>()
                .Property(e => e.HasBeenSeenBy)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
