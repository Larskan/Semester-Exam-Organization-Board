using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using OrganizationBoard.IService;
using EFrameWork.Model;

namespace OrganizationBoard.Service
{
    public class BoardService : IBoardService
    {
        private readonly OBDbContext _context;
        public BoardService(OBDbContext context)
        {
            _context = context;
        }

        #region Private check Methods
        // private async Task<bool> IsUserInOrganization(int userId, int organizationId)
        // {
        //     var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
        //     return user != null && user.OrganizationID == organizationId;
        // }
        private async Task<bool> IsUserInTeam(int userId, int teamId)
        {
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.TeamID == teamId;
        }

        private async Task<bool> IsUserTeamLeader(int userId)
        {
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 2;
        }
        private async Task<bool> IsUserTeamMember(int userId, int boardId)
        {
            var board = await _context.BoardTables.FirstOrDefaultAsync(b => b.BoardID == boardId);
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.TeamID == board.TeamID && board != null;
        }

        private async Task<bool> IsUserInTask(int userId, int taskId)
        {
            var task = await _context.TaskTables.FirstOrDefaultAsync(t => t.TaskID == taskId);
            return task != null && task.UserAssignments.Any(ua => ua.UserID == userId);
        }
        #endregion

        #region Board Management
        public async Task<OperationResponse<Board>> CreateBoard(Board board, int requestingUserId)
        {
            // Only Team Leaders can create boards
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<Board>("Access Denied", false, 403);

            try
            {
                var newBoard = new Board
                {
                    BoardName = board.BoardName,
                    TeamID = board.TeamID
                };
                _context.BoardTables!.Add(newBoard);
                await _context.SaveChangesAsync();

                return new OperationResponse<Board>(board, "Board created successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<Board>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<EFrameWork.Model.Task>>> GetBoardTasks(int boardId, int requestingUserId)
        {

            if (!await IsUserTeamMember(requestingUserId, boardId))
                return new OperationResponse<List<EFrameWork.Model.Task>>("Access Denied", false, 403);

            try
            {
                var tasks = _context.TaskTables!.Where(t => t.BoardID == boardId).ToList();
                if (tasks == null || tasks.Count == 0)
                    return new OperationResponse<List<EFrameWork.Model.Task>>("No tasks found", false, 404);

                return new OperationResponse<List<EFrameWork.Model.Task>>(tasks, "Tasks retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<List<EFrameWork.Model.Task>>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<Board>> UpdateBoard(Board board, int requestingUserId)
        {

            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<Board>("Access Denied", false, 403);

            if (!await IsUserTeamMember(requestingUserId, board.BoardID))
                return new OperationResponse<Board>("Access Denied", false, 403);

            try
            {
                var existingBoard = _context.BoardTables!.FirstOrDefault(b => b.BoardID == board.BoardID);
                if (existingBoard == null)
                    return new OperationResponse<Board>("Board not found", false, 404);

                existingBoard.BoardName = board.BoardName;
                _context.SaveChanges();
                return new OperationResponse<Board>(board, "Board updated successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<Board>(ex.Message, false, 500);
            }

        }

        public async Task<OperationResponse<bool>> DeleteBoard(int boardId, int requestingUserId)
        {

            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            if (!await IsUserTeamMember(requestingUserId, boardId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            try
            {
                var board = _context.BoardTables!.FirstOrDefault(b => b.BoardID == boardId);
                if (board == null)
                    return new OperationResponse<bool>("Board not found", false, 404);

                _context.BoardTables.Remove(board);
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Board deleted successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<Board>> GetBoard(int boardId, int requestingUserId)
        {

            if (!await IsUserTeamMember(requestingUserId, boardId))
                return new OperationResponse<Board>("Access Denied", false, 403);

            try
            {
                var board = await _context.BoardTables!.FirstOrDefaultAsync(b => b.BoardID == boardId);
                if (board == null)
                    return new OperationResponse<Board>("Board not found", false, 404);

                return new OperationResponse<Board>(board, "Board retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<Board>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<Board>>> GetTeamBoards(int teamId, int requestingUserId)
        {
            if (!await IsUserInTeam(requestingUserId, teamId))
                return new OperationResponse<List<Board>>("Access Denied", false, 403);

            try
            {
                var boards = await _context.BoardTables!.Where(b => b.TeamID == teamId).ToListAsync();
                if (boards == null || boards.Count == 0)
                    return new OperationResponse<List<Board>>("No boards found", false, 404);

                return new OperationResponse<List<Board>>(boards, "Boards retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<List<Board>>(ex.Message, false, 500);
            }
        }

        #endregion

        #region Task Management
        public async Task<OperationResponse<EFrameWork.Model.Task>> CreateTask(EFrameWork.Model.Task task, int requestingUserId)
        {
            // Only Team Leaders can create tasks
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<EFrameWork.Model.Task>("Access Denied", false, 403);

            try
            {
                var newTask = new EFrameWork.Model.Task
                {
                    Title = task.Title,
                    Description = task.Description,
                    BoardID = task.BoardID,
                    Status = task.Status,
                    Estimation = task.Estimation,
                    NumUser = task.NumUser,
                };
                _context.TaskTables!.Add(newTask);
                await _context.SaveChangesAsync();

                return new OperationResponse<EFrameWork.Model.Task>(task, "Task created successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<EFrameWork.Model.Task>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<EFrameWork.Model.Task>> GetTask(int taskId, int requestingUserId)
        {

            try
            {
                var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<EFrameWork.Model.Task>("Task not found", false, 404);

                return new OperationResponse<EFrameWork.Model.Task>(task, "Task retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<EFrameWork.Model.Task>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<EFrameWork.Model.Task>> UpdateTask(EFrameWork.Model.Task task, int requestingUserId)
        {
            try
            {
                // Only Team Leaders can update tasks
                if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<EFrameWork.Model.Task>("Access Denied", false, 403);

                var existingTask = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == task.TaskID);
                if (existingTask == null)
                    return new OperationResponse<EFrameWork.Model.Task>("Task not found", false, 404);

                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Estimation = task.Estimation;
                existingTask.NumUser = task.NumUser;

                await _context.SaveChangesAsync();

                return new OperationResponse<EFrameWork.Model.Task>(existingTask, "Task updated successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<EFrameWork.Model.Task>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> DeleteTask(int taskId, int requestingUserId)
        {
            try
            {
                // Only Team Leaders can delete tasks
                if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<bool>("Access Denied", false, 403);

                var task = _context.TaskTables!.FirstOrDefault(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<bool>("Task not found", false, 404);

                _context.TaskTables.Remove(task);
                _context.SaveChanges();

                return new OperationResponse<bool>(true, "Task deleted successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> AssignTask(int taskId, int requestingUserId, int assignedToUserId)
        {
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            try
            {
                var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<bool>("Task not found", false, 404);

                var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == assignedToUserId);
                if (user == null)
                    return new OperationResponse<bool>("User not found", false, 404);

                task.UserAssignments.Add(new UserToTask { UserID = assignedToUserId, TaskID = taskId });
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Task assigned successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> MarkTaskAsComplete(int taskId, int requestingUserId)
        {
            try
            {
                // Only Team Members can mark tasks as complete
                if (!await IsUserInTask(requestingUserId, taskId))
                    return new OperationResponse<bool>("Access Denied", false, 403);

                var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<bool>("Task not found", false, 404);

                task.StatusID = 2; // Assuming 2 is the ID for "Done"
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Task marked as complete");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> ConfirmTaskCompletion(int taskId, int requestingUserId)
        {
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<bool>("Access Denied", false, 403);
            try
            {
                var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<bool>("Task not found", false, 404);

                task.StatusID = 3; // Assuming 3 is the ID for "Confirmed"
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Task completion confirmed");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        #endregion

    }
}