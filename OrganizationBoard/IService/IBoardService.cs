<<<<<<< HEAD
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
        Task<OperationResponse<EFrameWork.Model.Task>> CreateTask(EFrameWork.Model.Task task, int boardId, int userId); // Only Team Leader
        Task<OperationResponse<EFrameWork.Model.Task>> GetTask(int taskId, int userId);  //Team Leader and Members
        Task<OperationResponse<EFrameWork.Model.Task>> UpdateTask(EFrameWork.Model.Task task, int userId); // Team Leader can update all, members can only update assigned to
        Task<OperationResponse<bool>> DeleteTask(int taskId, int userId); // Only Team Leader
        Task<OperationResponse<List<EFrameWork.Model.Task>>> GetBoardTasks(int boardId, int userId); // All in team can see.
        Task<OperationResponse<bool>> AssignTask(int taskId, int userId, int assignedToUserId); // Team Leader and Members
        Task<OperationResponse<bool>> MarkTaskAsComplete(int taskId, int userId); // Member
        Task<OperationResponse<bool>> ConfirmTaskCompletion(int taskId, int userId); // Team Leader

        // Board Management
        Task<OperationResponse<Board>> CreateBoard(Board board, int userId); //Team leader
        Task<OperationResponse<Board>> UpdateBoard(Board board, int userId); //Team leader
        Task<OperationResponse<bool>> DeleteBoard(int boardId, int userId); //Team leader
        Task<OperationResponse<Board>> GetBoard(int boardId, int userId); // All in team can see
        Task<OperationResponse<List<Board>>> GetTeamBoards(int teamId, int userId); // All in team can see

    }
}
=======
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using OrganizationBoard.DTO;
// using 

// namespace OrganizationBoard.IService
// {
//     public interface IBoardService
//     {
//         // Task Management
//         Task<OperationResponse<CardTask>> CreateTask(CardTask task, int userId); // Only Team Leader
//         Task<OperationResponse<CardTask>> GetTask(int taskId);  //All
//         Task<OperationResponse<CardTask>> UpdateTask(CardTask task, int userId); // Team Leader can update all, members can only update assigned to
//         Task<OperationResponse<bool>> DeleteTask(int taskId, int userId); // Only Team Leader
//         Task<OperationResponse<List<CardTask>>> GetTeamTasks(int teamId, int userId); // All in team can see.

//         Task<OperationResponse<bool>> AssignTask(int taskId, int userId, int assignedToUserId); // Team Leader and Members
//         Task<OperationResponse<bool>> MarkTaskAsComplete(int taskId, int userId); // Member
//         Task<OperationResponse<bool>> ConfirmTaskCompletion(int taskId, int userId); // Team Leader

//         // Board/Team Management
//         Task<OperationResponse<Team>> CreateTeam(Team team, int userId); //Team leader
//         Task<OperationResponse<Team>> UpdateTeam(Team team, int userId); //Team leader
//         Task<OperationResponse<bool>> DeleteTeam(int teamId, int userId);  //Team leader
//         Task<OperationResponse<List<User>>> GetTeamMembers(int teamId, int userId); //All in team can see
//         Task<OperationResponse<bool>> AssignUserToTeam(int teamId, int userIdToAssign, int requestingLeaderId); // Team Leader
//     }
// }
>>>>>>> 0b5e9ead5066def673f81cd42c143fb250a4f860
