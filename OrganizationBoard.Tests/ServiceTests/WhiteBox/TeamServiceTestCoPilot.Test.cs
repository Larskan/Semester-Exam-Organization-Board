using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using OrganizationBoard.Service;
using OrganizationBoard.DTO;
using EFramework.Data;
using EFrameWork.Model;

namespace OrganizationBoard.Service.Tests
{
    public class TeamServiceTestCoPilot
    {
        private DbContextOptions<OBDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<OBDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private OBDbContext GetDbContextWithData()
        {
            var options = GetInMemoryOptions();
            var context = new OBDbContext(options);

            // Seed roles: 1=Admin, 2=Leader, 3=Member
            context.UserTables.AddRange(
                new User { UserID = 1, RoleID = 1, Email = "admin@test.com" }, // Admin
                new User { UserID = 2, RoleID = 2, Email = "leader@test.com" }, // Leader
                new User { UserID = 3, RoleID = 3, Email = "member@test.com" }  // Member
            );
            context.TeamTables.Add(new Team { TeamID = 1, TeamName = "Alpha", Users = new List<User>() });
            context.SaveChanges();
            return context;
        }
        //TODO: Compare this to my own tests.

        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns403_IfNotAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);
            var teamDto = new TeamDto { TeamName = "NewTeam" };

            var result = await service.CreateTeam(teamDto, 2); // Leader, not admin

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_ReturnsSuccess_IfAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);
            var teamDto = new TeamDto { TeamName = "NewTeam" };

            var result = await service.CreateTeam(teamDto, 1); // Admin

            Assert.True(result.IsSuccess);
            Assert.Equal("Team created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("NewTeam", result.Data.TeamName);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTeam_Returns500_OnException()
        {
            var mockContext = new Mock<OBDbContext>(GetInMemoryOptions());
            mockContext.Setup(c => c.UserTables).Throws(new Exception("DB Error"));
            var service = new TeamService(mockContext.Object);

            var result = await service.CreateTeam(new TeamDto { TeamName = "X" }, 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTeamName_Returns403_IfNotLeader()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);
            var teamDto = new TeamDto { TeamID = 1, TeamName = "Beta" };

            var result = await service.UpdateTeamName(teamDto, 1); // Admin, not leader

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTeamName_Returns404_IfTeamNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);
            var teamDto = new TeamDto { TeamID = 999, TeamName = "Beta" };

            var result = await service.UpdateTeamName(teamDto, 2); // Leader

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTeamName_ReturnsSuccess_IfLeaderAndTeamExists()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);
            var teamDto = new TeamDto { TeamID = 1, TeamName = "Beta" };

            var result = await service.UpdateTeamName(teamDto, 2); // Leader

            Assert.True(result.IsSuccess);
            Assert.Equal("Team updated successfully.", result.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTeamName_Returns500_OnException()
        {
            var mockContext = new Mock<OBDbContext>(GetInMemoryOptions());
            mockContext.Setup(c => c.UserTables.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
                .ReturnsAsync(new User { UserID = 2, RoleID = 2 });
            mockContext.Setup(c => c.TeamTables).Throws(new Exception("DB Error"));
            var service = new TeamService(mockContext.Object);

            var result = await service.UpdateTeamName(new TeamDto { TeamID = 1, TeamName = "Beta" }, 2);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns403_IfNotAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.DeleteTeam(1, 2); // Leader

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns404_IfTeamNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.DeleteTeam(999, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_ReturnsSuccess_IfAdminAndTeamExists()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.DeleteTeam(1, 1); // Admin

            Assert.True(result.IsSuccess);
            Assert.Equal("Team deleted successfully.", result.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTeam_Returns500_OnException()
        {
            var mockContext = new Mock<OBDbContext>(GetInMemoryOptions());
            mockContext.Setup(c => c.UserTables.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
                .ReturnsAsync(new User { UserID = 1, RoleID = 1 });
            mockContext.Setup(c => c.TeamTables).Throws(new Exception("DB Error"));
            var service = new TeamService(mockContext.Object);

            var result = await service.DeleteTeam(1, 1);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns403_IfNotLeaderOrMemberOrAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.GetTeamMembers(1, 999); // Unknown user

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_Returns404_IfNoMembers()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.GetTeamMembers(1, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTeamMembers_ReturnsSuccess_IfMembersExist()
        {
            var context = GetDbContextWithData();
            var team = context.TeamTables.First();
            var user = context.UserTables.First(u => u.UserID == 3);
            user.TeamID = team.TeamID;
            context.SaveChanges();

            var service = new TeamService(context);

            var result = await service.GetTeamMembers(team.TeamID, 1); // Admin

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(user.Email, result.Data[0].Email);
        }

        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns403_IfNotAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.AssignUserToTeam(1, 3, 2); // Leader

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns404_IfTeamNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.AssignUserToTeam(999, 3, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_Returns404_IfUserNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.AssignUserToTeam(1, 999, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task AssignUserToTeam_ReturnsSuccess_IfAdminAndValid()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.AssignUserToTeam(1, 3, 1); // Admin

            Assert.True(result.IsSuccess);
            Assert.Equal("User assigned to team successfully.", result.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns403_IfNotAdmin()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.RemoveUserFromTeam(1, 3, 2); // Leader

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns404_IfTeamNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.RemoveUserFromTeam(999, 3, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_Returns404_IfUserNotFound()
        {
            var context = GetDbContextWithData();
            var service = new TeamService(context);

            var result = await service.RemoveUserFromTeam(1, 999, 1); // Admin

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveUserFromTeam_ReturnsSuccess_IfAdminAndValid()
        {
            var context = GetDbContextWithData();
            var team = context.TeamTables.Include(t => t.Users).First();
            var user = context.UserTables.First(u => u.UserID == 3);
            team.Users.Add(user);
            context.SaveChanges();

            var service = new TeamService(context);

            var result = await service.RemoveUserFromTeam(team.TeamID, user.UserID, 1); // Admin

            Assert.True(result.IsSuccess);
            Assert.Equal("User assigned to team successfully.", result.Message);
        }
    }
}