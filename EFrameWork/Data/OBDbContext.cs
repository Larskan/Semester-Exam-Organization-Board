using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EFrameWork.Model;
using Microsoft.EntityFrameworkCore;
using Model;

namespace EFramework.Data
{
    public class OBDbContext : DbContext
    {
        public OBDbContext(DbContextOptions<OBDbContext> options) : base(options) { }
        public OBDbContext() { }

        public virtual DbSet<Role>? RoleTables { get; set; }
        public virtual DbSet<Status>? StatusTables { get; set; }
        public virtual DbSet<EFrameWork.Model.Task>? TaskTables { get; set; }
        public virtual DbSet<Team>? TeamTables { get; set; }
        public virtual DbSet<User>? UserTables { get; set; }
        public virtual DbSet<UserToTask>? UserToTaskTables { get; set; }
        public virtual DbSet<Organization>? OrganizationTables { get; set; }
        public virtual DbSet<Board>? BoardTables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>().Property(r => r.RoleName).HasColumnName("Role");
            modelBuilder.Entity<Status>().Property(s => s.TaskStatus).HasColumnName("Status");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.OrganizationID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TeamID);

            modelBuilder.Entity<Board>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Boards)
                .HasForeignKey(u => u.TeamID);

            modelBuilder.Entity<EFrameWork.Model.Task>()
                .HasOne(t => t.Board)
                .WithMany(b => b.Tasks)
                .HasForeignKey(t => t.BoardID);

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

            modelBuilder.Entity<Team>().HasData(
                new Team { TeamID = 1, TeamName = "team 1" },
                new Team { TeamID = 2, TeamName = "team 2" }
            );

            modelBuilder.Entity<Organization>().HasData(
                new Organization { OrganizationID = 1, OrganizationName = "Lars" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Team Leader" },
                new Role { RoleID = 3, RoleName = "Team Member" }
            );

            modelBuilder.Entity<Status>().HasData(
                new Status { StatusID = 1, TaskStatus = "To Do" },
                new Status { StatusID = 2, TaskStatus = "In Progress" },
                new Status { StatusID = 3, TaskStatus = "Done" },
                new Status { StatusID = 4, TaskStatus = "Confirmed" }
            );

            // Pass = 1234
            modelBuilder.Entity<User>().HasData(
                new User {UserID = 1, Email = "Mail1", Password = "$2a$11$zSZaqcPjjtI3tWf0hHEVbey9fBLldqw/6OoCGvia5jCSLLDUkW.NW", RoleID = 1, OrganizationID = 1},
                new User {UserID = 2, Email = "Mail2", Password = "$2a$11$zSZaqcPjjtI3tWf0hHEVbey9fBLldqw/6OoCGvia5jCSLLDUkW.NW", TeamID = 2, RoleID = 2, OrganizationID = 1},
                new User {UserID = 3, Email = "Mail3", Password = "$2a$11$zSZaqcPjjtI3tWf0hHEVbey9fBLldqw/6OoCGvia5jCSLLDUkW.NW", TeamID = 1, RoleID = 3, OrganizationID = 1}
            );
        }
    }
}