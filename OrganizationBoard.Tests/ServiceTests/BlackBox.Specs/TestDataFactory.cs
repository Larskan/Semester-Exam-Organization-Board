using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;
using Model;
using OrganizationBoard.DTO;

public static class TestDataFactory
{
    private static int _nextUserId = 10; // Start from 10, increment for each call
    private static int _nextOrganizationId = 1;
    private static int _nextRoleId = 1;

    public static User CreateUser(int id = 0, int teamId = 1, string role = "User", string email = null, string password = null)
    {
        // If ID is not provided (or is 0), generate a unique one
        int userId = id == 0 ? Interlocked.Increment(ref _nextUserId) : id;
        int roleId = role == "Admin" ? 1 : (role == "Team Leader" ? 2 : 3); // Keep fixed role IDs if they are static in your app

        return new User
        {
            UserID = userId,
            RoleID = roleId, // Assign role ID from role
            TeamID = teamId,
            Email = email ?? $"user{userId}@example.com",
            Password = password ?? "hashedPassword" // Assuming passwords are hashed when stored
        };
    }

    public static Role CreateRole(string roleName, int id = 0)
    {
        if (roleName == "Admin") return new Role { RoleID = 1, RoleName = "Admin" };
        if (roleName == "Team Leader") return new Role { RoleID = 2, RoleName = "Team Leader" };
        if (roleName == "Team Member") return new Role { RoleID = 3, RoleName = "Team Member" };

        int roleId = id == 0 ? Interlocked.Increment(ref _nextRoleId) : id;

        return new Role
        {
            RoleID = roleId,
            RoleName = roleName
        };
    }

    public static Organization CreateOrganization(string orgName, int orgId = 0)
    {
        int organizationId = orgId == 0 ? Interlocked.Increment(ref _nextOrganizationId) : orgId;
        return new Organization
        {
            OrganizationID = organizationId,
            OrganizationName = orgName
        };
    }

    public static Board CreateBoard(int id = 0, string name = "Test Board", int teamId = 1)
    {
        return new Board
        {
            BoardID = id, // If ID is 0, EF Core In-Memory will auto-generate.
            BoardName = name,
            TeamID = teamId
        };
    }

    public static EFrameWork.Model.Task CreateTask(int boardId, string title = "Task", string desc = "Description", int? taskId = null, int statusId = 1)
    {
        return new EFrameWork.Model.Task
        {
            TaskID = taskId ?? 0, // Allow setting TaskID explicitly, otherwise EF Core in-memory will generate
            BoardID = boardId,
            Title = title,
            Description = desc,
            StatusID = statusId,
            Estimation = 3,
            NumUser = 1
        };
    }

    public static BoardDto CreateBoardDto(string name)
    {
        return new BoardDto
        {
            BoardName = name
        };
    }

    public static TaskDto CreateTaskDto(string title, string description = "Description", int estimation = 3, int numUser = 1, int boardId = 0)
    {
        return new TaskDto
        {
            Title = title,
            Description = description,
            Estimation = estimation,
            NumUser = numUser,
            BoardID = boardId
        };
    }

    public static LoginDto CreateLoginDto(string email, string password)
    {
        return new LoginDto
        {
            Email = email,
            Password = password
        };
    }

    public static AccountAndOrgDto CreateAccountAndOrgDto(string email, string password, string orgName)
    {
        return new AccountAndOrgDto
        {
            Email = email,
            Password = password,
            OrgName = orgName
        };
    }
}