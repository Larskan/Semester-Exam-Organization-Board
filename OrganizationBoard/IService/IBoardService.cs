using System;
using System.Collections.Generic;
using System.Linq;
using OrganizationBoard.DTO;
using EFrameWork.Model;

namespace OrganizationBoard.IService
{
    public interface IBoardService
    {
        // Task Management
        Task<OperationResponse<TaskDto>> CreateTask(TaskDto task, int boardId, int userId); // Only Team Leader
        Task<OperationResponse<TaskReadDto>> GetTask(int taskId, int userId);  //Team Leader and Members
        Task<OperationResponse<TaskReadDto>> UpdateTask(TaskReadDto task, int userId); // Team Leader can update all, members can only update assigned to
        Task<OperationResponse<bool>> DeleteTask(int taskId, int userId); // Only Team Leader
        Task<OperationResponse<List<TaskReadDto>>> GetBoardTasks(int boardId, int userId); // All in team can see.
        Task<OperationResponse<bool>> AssignTask(int taskId, int userId, int assignedToUserId); // Team Leader and Members
        Task<OperationResponse<bool>> MarkTaskAsComplete(int taskId, int userId); // Member
        Task<OperationResponse<bool>> ConfirmTaskCompletion(int taskId, int userId); // Team Leader

        // Board Management
        Task<OperationResponse<BoardDto>> CreateBoard(BoardDto board, int userId); //Team leader
        Task<OperationResponse<BoardReadDto>> UpdateBoard(BoardReadDto board, int userId); //Team leader
        Task<OperationResponse<bool>> DeleteBoard(int boardId, int userId); //Team leader
        Task<OperationResponse<BoardReadDto>> GetBoard(int boardId, int userId); // All in team can see
        Task<OperationResponse<List<BoardReadDto>>> GetTeamBoards(int teamId, int userId); // All in team can see

    }
}
