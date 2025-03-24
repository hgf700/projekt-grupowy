using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class event_base : DbContext
    {
        public event_base(DbContextOptions<event_base> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the one-to-many relationship between User and Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)              // Each Event has one User
                .WithMany(u => u.Events)          // Each User has many Events
                .HasForeignKey(e => e.UserId)     // The foreign key in Event is UserId
                .OnDelete(DeleteBehavior.Cascade); // Optional: set delete behavior if necessary (e.g., cascade delete)
        }
    }
}
