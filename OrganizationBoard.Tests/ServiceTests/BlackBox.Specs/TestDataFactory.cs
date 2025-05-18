using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;
using OrganizationBoard.DTO;

public static class TestDataFactory
{
    public static User CreateUser(int id, int teamId, string role)
    {
        int roleId = role == "Team Leader" ? 2 : 1;
        return new User
        {
            UserID = id,
            RoleID = roleId,
            TeamID = teamId,
            Email = $"user{id}@example.com",
            Password = "Password123!"
        };
    }

    public static Board CreateBoard(int id, string name, int teamId)
    {
        return new Board
        {
            BoardID = id,
            BoardName = name,
            TeamID = teamId
        };
    }

    public static EFrameWork.Model.Task CreateTask(int boardId, string title = "Task", string desc = "Description", int? taskId = null, int statusId = 1)
    {
        return new EFrameWork.Model.Task
        {
            TaskID = taskId ?? 0, // Allow setting TaskID explicitly
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
}