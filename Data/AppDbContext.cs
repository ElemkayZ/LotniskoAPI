using LotniskoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace LotniskoAPI.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Status> FlightStatuses { get; set; }
        public DbSet<Plane> Planes { get; set; }
        public DbSet<PlaneStaff> PlaneStaffs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketTransaction> TicketTransactions { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRoles> UserRoless { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Status)
                .WithMany()
                .HasForeignKey(f => f.StatusId);
            modelBuilder.Entity<Plane>()
            .HasMany(f => f.Flights)
            .WithOne(f => f.Plane)
            .HasForeignKey(f => f.CurrentPlaneId);

            modelBuilder.Entity<PlaneStaff>()
            .HasOne(ps => ps.Plane)
            .WithMany(p => p.PlaneStaffs)
            .HasForeignKey(ps => ps.PlaneId);
            modelBuilder.Entity<PlaneStaff>()
            .HasOne(ps => ps.User)
            .WithMany(p => p.PlaneStaff)
            .HasForeignKey(ps => ps.WorkerId);

            modelBuilder.Entity<Ticket>()
            .HasOne(t => t.User)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.UserId);
            modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Flight)
            .WithMany(f => f.Tickets)
            .HasForeignKey(t => t.OrderFlightId);
            modelBuilder.Entity<Ticket>()
            .HasOne(t => t.TicketTransaction)
            .WithMany()
            .HasForeignKey(t => t.TransactionId);

            modelBuilder.Entity<TicketTransaction>()
            .HasOne(tr => tr.User)
            .WithMany(c => c.Transactions)
            .HasForeignKey(tr => tr.TransactionUserId);

            modelBuilder.Entity<UserRoles>()
            .HasOne(ps => ps.Role)
            .WithMany(p => p.UserRoles)
            .HasForeignKey(ps => ps.RoleId);
            modelBuilder.Entity<UserRoles>()
            .HasOne(ps => ps.User)
            .WithMany(ps => ps.UserRoles)
            .HasForeignKey(ps => ps.UserRoleId);
    
            modelBuilder.Entity<User>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
