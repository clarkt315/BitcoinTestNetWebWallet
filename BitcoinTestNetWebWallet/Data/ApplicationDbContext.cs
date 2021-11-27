using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BitcoinTestNetWebWallet.Models;

namespace BitcoinTestNetWebWallet.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Withdrawal>()
                .Property(p => p.Amount)
                .HasPrecision(18, 8);
        }
        public DbSet<BitcoinTestNetWebWallet.Models.DepositAddress> DepositAddress { get; set; }
        public DbSet<BitcoinTestNetWebWallet.Models.Withdrawal> Withdrawal { get; set; }
    }
}
