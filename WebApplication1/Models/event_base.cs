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
        public DbSet<UserEvent> UserEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEvent>()
                .HasKey(ue => new { ue.UserId, ue.EventId }); // klucz złożony

            modelBuilder.Entity<UserEvent>()
                .HasOne(ue => ue.User)
                .WithMany(u => u.UserEvents)
                .HasForeignKey(ue => ue.UserId);

            modelBuilder.Entity<UserEvent>()
                .HasOne(ue => ue.Event)
                .WithMany(e => e.UserEvents)
                .HasForeignKey(ue => ue.EventId);
        }

    }
}
