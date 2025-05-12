using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;
using Microsoft.EntityFrameworkCore;
using Model;

namespace EFramework.Data
{
    public class AGWDbContext : DbContext
    {
        public AGWDbContext(DbContextOptions<AGWDbContext> options) : base(options) { }
        public AGWDbContext() { }

        public virtual DbSet<Role>? RoleTables { get; set; }
        public virtual DbSet<Status>? StatusTables { get; set; }
        public virtual DbSet<EFrameWork.Model.Task>? TaskTables { get; set; }
        public virtual DbSet<Team>? TeamTables { get; set; }
        public virtual DbSet<User>? UserTables { get; set; }
        public virtual DbSet<UserToTask>? UserToTaskTables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>().Property(r => r.RoleName).HasColumnName("Role");
            modelBuilder.Entity<Status>().Property(s => s.StatusOption).HasColumnName("Status");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TeamID);

            modelBuilder.Entity<EFrameWork.Model.Task>()
                .HasOne(t => t.Team)
                .WithMany(team => team.Tasks)
                .HasForeignKey(t => t.TeamID);

            modelBuilder.Entity<EFrameWork.Model.Task>()
                .HasOne(t => t.Status)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.StatusID);

            modelBuilder.Entity<UserToTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ut => ut.UserID) 
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserToTask>()
                .HasOne(ut => ut.Task)
                .WithMany(t => t.UserAssignments)
                .HasForeignKey(ut => ut.TaskID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}