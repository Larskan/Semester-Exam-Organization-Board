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
            var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.TeamID == teamId;
        }

        private async Task<bool> IsUserTeamLeader(int userId)
        {
            var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 2;
        }
        private async Task<bool> IsUserTeamMember(int userId, int boardId)
        {
            var board = await _context.BoardTables!.FirstOrDefaultAsync(b => b.BoardID == boardId);
            var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == userId);

            if (board == null || user == null)
                return false;

            return user.TeamID == board.TeamID;
        }

        private async Task<bool> IsUserInTask(int userId, int taskId)
        {
            var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
            return task != null && task.UserAssignments!.Any(ua => ua.UserID == userId);
        }
        #endregion

        #region Board Management
        public async Task<OperationResponse<BoardDto>> CreateBoard(BoardDto board, int requestingUserId)
        {
            // Only Team Leaders can create boards
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<BoardDto>("Access Denied", false, 403);

            var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == requestingUserId);
            if (user == null)
                return new OperationResponse<BoardDto>("User not found", false, 404);

            try
            {
                var newBoard = new Board
                {
                    BoardName = board.BoardName,
                    TeamID = user.TeamID,
                };
                _context.BoardTables!.Add(newBoard);
                await _context.SaveChangesAsync();

                board.BoardID = newBoard.BoardID;

                return new OperationResponse<BoardDto>(board, "Board created successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<BoardDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<TaskReadDto>>> GetBoardTasks(int boardId, int requestingUserId)
        {

            if (!await IsUserTeamMember(requestingUserId, boardId))
                return new OperationResponse<List<TaskReadDto>>("Access Denied", false, 403);

            try
            {
                var tasks = _context.TaskTables!.Where(t => t.BoardID == boardId).ToList();
                if (tasks == null || tasks.Count == 0)
                    return new OperationResponse<List<TaskReadDto>>("No tasks found", false, 404);

                var taskDtos = tasks.Select(t => new TaskReadDto
                {
                    TaskID = t.TaskID,
                    Title = t.Title,
                    Description = t.Description,
                    Estimation = t.Estimation,
                    NumUser = t.NumUser,
                    StatusID = t.StatusID,
                    BoardID = t.BoardID,
                }).ToList();

                return new OperationResponse<List<TaskReadDto>>(taskDtos, "Tasks retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<List<TaskReadDto>>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<BoardReadDto>> UpdateBoard(BoardReadDto board, int requestingUserId)
        {

            try
            {
                var existingBoard = _context.BoardTables!.FirstOrDefault(b => b.BoardID == board.BoardID);
                if (existingBoard == null)
                    return new OperationResponse<BoardReadDto>("Board not found", false, 404);

                if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<BoardReadDto>("Access Denied", false, 403);

                if (!await IsUserTeamMember(requestingUserId, board.BoardID))
                    return new OperationResponse<BoardReadDto>("Access Denied", false, 403);


                existingBoard.BoardName = board.BoardName;
                _context.SaveChanges();
                return new OperationResponse<BoardReadDto>(board, "Board updated successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<BoardReadDto>(ex.Message, false, 500);
            }

        }

        public async Task<OperationResponse<bool>> DeleteBoard(int boardId, int requestingUserId)
        {

            try
            {
                var board = _context.BoardTables!.FirstOrDefault(b => b.BoardID == boardId);
                if (board == null)
                    return new OperationResponse<bool>("Board not found", false, 404);

                if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<bool>("Access Denied", false, 403);

                if (!await IsUserTeamMember(requestingUserId, boardId))
                    return new OperationResponse<bool>("Access Denied", false, 403);

                // Check if the board has any tasks
                if (_context.TaskTables!.Any(t => t.BoardID == boardId))
                    _context.TaskTables!.RemoveRange(_context.TaskTables.Where(t => t.BoardID == boardId));

                _context.BoardTables!.Remove(board);
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Board deleted successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<BoardReadDto>> GetBoard(int boardId, int requestingUserId)
        {


            try
            {
                var board = await _context.BoardTables!.FirstOrDefaultAsync(b => b.BoardID == boardId);
                if (board == null || board.TeamID == null)
                    return new OperationResponse<BoardReadDto>("Board not found", false, 404);

                if (!await IsUserTeamMember(requestingUserId, boardId))
                    return new OperationResponse<BoardReadDto>("Access Denied", false, 403);

                var boardDto = new BoardReadDto
                {
                    BoardID = board.BoardID,
                    BoardName = board.BoardName,
                    TeamID = board.TeamID
                };

                return new OperationResponse<BoardReadDto>(boardDto, "Board retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<BoardReadDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<BoardReadDto>>> GetTeamBoards(int teamId, int requestingUserId)
        {
            if (!await IsUserInTeam(requestingUserId, teamId))
                return new OperationResponse<List<BoardReadDto>>("Access Denied", false, 403);

            try
            {
                var boards = await _context.BoardTables!.Where(b => b.TeamID == teamId).ToListAsync();
                if (boards == null || boards.Count == 0)
                    return new OperationResponse<List<BoardReadDto>>("No boards found", false, 404);

                var boardDtos = boards.Select(b => new BoardReadDto
                {
                    BoardID = b.BoardID,
                    BoardName = b.BoardName,
                    TeamID = b.TeamID
                }).ToList();

                return new OperationResponse<List<BoardReadDto>>(boardDtos);
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<List<BoardReadDto>>(ex.Message, false, 500);
            }
        }

        #endregion

        #region Task Management
        public async Task<OperationResponse<TaskDto>> CreateTask(TaskDto task, int boardId, int requestingUserId)
        {
            // Only Team Leaders can create tasks
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<TaskDto>("Access Denied", false, 403);

            try
            {
                var newTask = new EFrameWork.Model.Task
                {
                    Title = task.Title,
                    Description = task.Description,
                    BoardID = boardId,
                    StatusID = 1,
                    Estimation = task.Estimation,
                    NumUser = task.NumUser,
                };
                _context.TaskTables!.Add(newTask);
                await _context.SaveChangesAsync();

                task.TaskID = newTask.TaskID;

                return new OperationResponse<TaskDto>(task, "Task created successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<TaskDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<TaskReadDto>> GetTask(int taskId, int requestingUserId)
        {

            try
            {
                var task = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == taskId);
                if (task == null)
                    return new OperationResponse<TaskReadDto>("Task not found", false, 404);

                var taskDto = new TaskReadDto
                {
                    TaskID = task.TaskID,
                    Title = task.Title,
                    Description = task.Description,
                    Estimation = task.Estimation,
                    NumUser = task.NumUser,
                    StatusID = task.StatusID,
                };

                return new OperationResponse<TaskReadDto>(taskDto, "Task retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<TaskReadDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<TaskReadDto>> UpdateTask(TaskReadDto task, int requestingUserId)
        {
            try
            {
                // Only Team Leaders can update tasks
                if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<TaskReadDto>("Access Denied", false, 403);

                var existingTask = await _context.TaskTables!.FirstOrDefaultAsync(t => t.TaskID == task.TaskID);
                if (existingTask == null)
                    return new OperationResponse<TaskReadDto>("Task not found", false, 404);


                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Estimation = task.Estimation;
                existingTask.NumUser = task.NumUser;

                await _context.SaveChangesAsync();

                var updatedTaskDto = new TaskReadDto
                {
                    TaskID = existingTask.TaskID,
                    Title = existingTask.Title,
                    Description = existingTask.Description,
                    Estimation = existingTask.Estimation,
                    NumUser = existingTask.NumUser,
                    StatusID = existingTask.StatusID,
                };

                return new OperationResponse<TaskReadDto>(updatedTaskDto, "Task updated successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<TaskReadDto>(ex.Message, false, 500);
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

                _context.TaskTables!.Remove(task);
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

                var newAssignment = new UserToTask
                {
                    UserID = assignedToUserId,
                    TaskID = taskId
                };

                task.UserAssignments!.Add(newAssignment);
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Task assigned successfully");
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        //TODO: Check if the user is a member of the team not working correctly
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