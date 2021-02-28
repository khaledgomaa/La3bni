using La3bni.Repository;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class La3bniContext : IdentityDbContext<ApplicationUser>
    {
        public La3bniContext(DbContextOptions<La3bniContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Playground> Playgrounds { get; set; }

        public DbSet<BookingTeam> BookingTeams { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<PlaygroundRate> PlaygroundRates { get; set; }

        public DbSet<PlaygroundTimes> PlaygroundTimes { get; set; }

        public DbSet<Subscriber> Subscribers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(EntityConfiguration<>).Assembly);

           // builder.Entity<Booking>().HasOne(b => b.ApplicationUser).WithMany().OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }

    }
}