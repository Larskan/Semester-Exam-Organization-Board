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