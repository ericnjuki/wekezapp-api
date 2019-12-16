using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using wekezapp.data.Entities;

namespace wekezapp.data.Persistence {
    public class WekezappContext: DbContext {
        public WekezappContext(DbContextOptions opts) : base(opts) {
            //Database.SetInitializer(new CreateDatabaseIfNotExists<ShopAssist2Context>());
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ChamaUser>()
                .HasKey(c => new { c.ChamaId, c.UserId });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
