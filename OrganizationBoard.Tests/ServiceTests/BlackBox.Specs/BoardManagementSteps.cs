using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFramework.Data;
using EFrameWork.Model;
using Moq;
using OrganizationBoard.DTO;
using OrganizationBoard.Service;
using Polly;
using Reqnroll;
using Xunit;

namespace OrganizationBoard.Tests.ServiceTests.BlackBox.Specs
{
    [Binding]
    public class BoardManagementSteps
    {
        private readonly ScenarioContext _context;
        private OBDbContext _dbContext;
        private BoardService _service;
        private OperationResponse<BoardDto> _boardResponse;
        private OperationResponse<BoardReadDto> _boardReadResponse;
        private OperationResponse<List<BoardReadDto>> _boardListResponse;
        private OperationResponse<List<TaskReadDto>> _taskListResponse;
        private OperationResponse<TaskDto> _taskResponse;
        private OperationResponse<TaskReadDto> _taskReadResponse;
        private OperationResponse<bool> _boolResponse;
        private IAsyncPolicy _policy;
        private User _user;
        private Board _board;
        private EFrameWork.Model.Task _task;


        public BoardManagementSteps(ScenarioContext context)
        {
            _context = context;
            _policy = Policy.NoOpAsync();
            _dbContext = TestDbContextFactory.Create();
            _service = new BoardService(_dbContext, _policy);
        }

        [AfterScenario]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            // Re-initialize for the next scenario if needed, or rely on the constructor for new instance
            _dbContext = TestDbContextFactory.Create();
            _service = new BoardService(_dbContext, _policy);
        }
        #region Given Steps

        [Given(@"I am a ""(.*)"" user with ID (\d+) in team (\d+)")]
        public void GivenIAmAUserWithRole(string role, int userId, int teamId)
        {
            _user = TestDataFactory.CreateUser(userId, teamId, role);
            _dbContext.UserTables.Add(_user);
            _dbContext.SaveChanges();
        }

        [Given(@"a user with ID (\d+) exists in team (\d+)")]
        public void GivenAUserWithIdExistsInTeam(int userId, int teamId)
        {
            var user = TestDataFactory.CreateUser(userId, teamId, "Team Member"); // Assuming default role
            _dbContext.UserTables.Add(user);
            _dbContext.SaveChanges();
        }

        [Given(@"a task with ID (\d+) exists on board (\d+) not assigned to user (\d+)")]
        public void GivenATaskWithIdExistsOnBoardNotAssignedToUser(int taskId, int boardId, int userId)
        {
            var task = TestDataFactory.CreateTask(boardId: boardId, title: $"Task {taskId}", taskId: taskId);
            // No UserAssignments are added, ensuring it's not assigned to any user.
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }

        [Given(@"a board with ID (\d+) exists for team (\d+)")]
        public void GivenABoardWithIdExistsForTeam(int boardId, int teamId)
        {
            _board = TestDataFactory.CreateBoard(boardId, $"Test Board {boardId}", teamId);
            _dbContext.BoardTables.Add(_board);
            _dbContext.SaveChanges();
        }

        [Given(@"the board has tasks")]
        public void GivenTheBoardHasTasks()
        {
            var task = TestDataFactory.CreateTask(boardId: _board.BoardID, title: "Task 1");
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }

        [Given(@"a task with ID (\d+) exists on board (\d+)")]
        public void GivenATaskWithIdExistsOnBoard(int taskId, int boardId)
        {
            var task = TestDataFactory.CreateTask(boardId: boardId, title: $"Task {taskId}", taskId: taskId);
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }

        [Given(@"a task with ID (\d+) exists on board (\d+) assigned to user (\d+)")]
        public void GivenATaskWithIdExistsOnBoardAssignedToUser(int taskId, int boardId, int userId)
        {
            var task = TestDataFactory.CreateTask(boardId: boardId, title: $"Task {taskId}", taskId: taskId);
            task.UserAssignments = new List<UserToTask> { new UserToTask { TaskID = taskId, UserID = userId } };
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }

        [Given(@"a task with ID (\d+) exists on board (\d+) marked as complete")]
        public void GivenATaskWithIdExistsOnBoardMarkedAsComplete(int taskId, int boardId)
        {
            var task = TestDataFactory.CreateTask(boardId: boardId, title: $"Completed Task {taskId}", taskId: taskId, statusId: 2); // Assuming 2 is "Complete"
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }
        #endregion

        #region When Steps

        [When(@"I create a board named ""(.*)""")]
        public async System.Threading.Tasks.Task WhenICreateABoardNamed(string name)
        {
            var dto = TestDataFactory.CreateBoardDto(name);
            _boardResponse = await _service.CreateBoard(dto, _user.UserID);
        }

        [When(@"I update board (\d+) name to ""(.*)""")]
        public async System.Threading.Tasks.Task WhenIUpdateBoardNameTo(int boardId, string newName)
        {
            var existingBoard = await _dbContext.BoardTables.FindAsync(boardId);
            if (existingBoard != null)
            {
                var dto = new BoardReadDto { BoardID = boardId, BoardName = newName, TeamID = existingBoard.TeamID };
                _boardReadResponse = await _service.UpdateBoard(dto, _user.UserID);
            }
            else
            {
                // Create a DTO with the provided ID and name, even if the board doesn't exist
                var dto = new BoardReadDto { BoardID = boardId, BoardName = newName, TeamID = 1 }; // Assuming a default TeamID for the DTO
                _boardReadResponse = await _service.UpdateBoard(dto, _user.UserID);
            }
        }

        [When(@"I delete board (\d+)")]
        public async System.Threading.Tasks.Task WhenIDeleteBoard(int boardId)
        {
            _boolResponse = await _service.DeleteBoard(boardId, _user.UserID);
        }

        [When(@"I get board with ID (\d+)")]
        public async System.Threading.Tasks.Task WhenIGetBoardWithID(int boardId)
        {
            _boardReadResponse = await _service.GetBoard(boardId, _user.UserID);
        }

        [When(@"I get all boards for team (\d+)")]
        public async System.Threading.Tasks.Task WhenIGetAllBoardsForTeam(int teamId)
        {
            _boardListResponse = await _service.GetTeamBoards(teamId, _user.UserID);
        }

        [When(@"I fetch tasks for board (\d+)")]
        public async System.Threading.Tasks.Task WhenIFetchTasksForBoard(int boardId)
        {
            _taskListResponse = await _service.GetBoardTasks(boardId, _user.UserID);
        }

        [When(@"I create a task named ""(.*)"" for board (\d+)")]
        public async System.Threading.Tasks.Task WhenICreateATaskNamedForBoard(string taskName, int boardId)
        {
            var dto = TestDataFactory.CreateTaskDto(taskName);
            _taskResponse = await _service.CreateTask(dto, boardId, _user.UserID);
        }

        [When(@"I get task with ID (\d+)")]
        public async System.Threading.Tasks.Task WhenIGetTaskWithID(int taskId)
        {
            _taskReadResponse = await _service.GetTask(taskId, _user.UserID);
        }

        [When(@"I update task (\d+) title to ""(.*)""")]
        public async System.Threading.Tasks.Task WhenIUpdateTaskTitleTo(int taskId, string newTitle)
        {
            var existingTask = await _dbContext.TaskTables.FindAsync(taskId);
            if (existingTask != null)
            {
                var dto = new TaskReadDto
                {
                    TaskID = taskId,
                    Title = newTitle,
                    Description = existingTask.Description,
                    Estimation = existingTask.Estimation,
                    NumUser = existingTask.NumUser,
                    StatusID = existingTask.StatusID,
                    BoardID = existingTask.BoardID
                };
                _taskReadResponse = await _service.UpdateTask(dto, _user.UserID);
            }
            else
            {
                // Create a DTO with the provided ID and title, even if the task doesn't exist
                var dto = new TaskReadDto
                {
                    TaskID = taskId,
                    Title = newTitle,
                    Description = "Description", // Provide default values
                    Estimation = 3,
                    NumUser = 1,
                    StatusID = 1,
                    BoardID = 1 // Provide a default BoardID
                };
                _taskReadResponse = await _service.UpdateTask(dto, _user.UserID);
            }
        }

        [When(@"I delete task (\d+)")]
        public async System.Threading.Tasks.Task WhenIDeleteTask(int taskId)
        {
            _boolResponse = await _service.DeleteTask(taskId, _user.UserID);
        }

        [When(@"I assign task (\d+) to user (\d+)")]
        public async System.Threading.Tasks.Task WhenIAssignTaskToUser(int taskId, int assignedUserId)
        {
            _boolResponse = await _service.AssignTask(taskId, _user.UserID, assignedUserId);
        }

        [When(@"I mark task (\d+) as complete")]
        public async System.Threading.Tasks.Task WhenIMarkTaskAsComplete(int taskId)
        {
            _boolResponse = await _service.MarkTaskAsComplete(taskId, _user.UserID);
        }

        [When(@"I confirm task (\d+) completion")]
        public async System.Threading.Tasks.Task WhenIConfirmTaskCompletion(int taskId)
        {
            _boolResponse = await _service.ConfirmTaskCompletion(taskId, _user.UserID);
        }

        #endregion

        #region Then Steps

        [Then(@"the operation should succeed")]
        public void ThenTheOperationShouldSucceed()
        {
            if (_boardResponse != null) Assert.True(_boardResponse.IsSuccess);
            if (_boardReadResponse != null) Assert.True(_boardReadResponse.IsSuccess);
            if (_boardListResponse != null) Assert.True(_boardListResponse.IsSuccess);
            if (_taskListResponse != null) Assert.True(_taskListResponse.IsSuccess);
            if (_taskResponse != null) Assert.True(_taskResponse.IsSuccess);
            if (_taskReadResponse != null) Assert.True(_taskReadResponse.IsSuccess);
            if (_boolResponse != null) Assert.True(_boolResponse.IsSuccess);
        }

        [Then(@"the returned board ID should not be 0")]
        public void ThenReturnedBoardIDNotZero()
        {
            Assert.True(_boardResponse.Data.BoardID > 0);
        }

        [Then(@"the returned board ID should be (\d+)")]
        public void ThenTheReturnedBoardIdShouldBe(int boardId)
        {
            Assert.Equal(boardId, _boardReadResponse.Data.BoardID);
        }

        [Then(@"the returned board list should contain at least (\d+) boards")]
        public void ThenTheReturnedBoardListShouldContainAtLeastBoards(int count)
        {
            Assert.True(_boardListResponse.Data.Count >= count);
        }

        [Then(@"the task response should contain tasks")]
        public void ThenTaskResponseShouldContainTasks()
        {
            Assert.True(_taskListResponse.IsSuccess);
            Assert.NotEmpty(_taskListResponse.Data);
        }

        [Then(@"the returned task ID should not be 0")]
        public void ThenTheReturnedTaskIDShouldNotBe0()
        {
            Assert.True(_taskResponse.Data.TaskID > 0);
        }

        [Then(@"the returned task ID should be (\d+)")]
        public void ThenTheReturnedTaskIDShouldBe(int taskId)
        {
            Assert.Equal(taskId, _taskReadResponse.Data.TaskID);
        }

        [Then(@"the operation should fail with status code (\d+)")]
        public void ThenOperationFailsWithStatusCode(int status)
        {
            if (_boardResponse != null)
            {
                Assert.False(_boardResponse.IsSuccess);
                Assert.Equal(status, _boardResponse.StatusCode);
            }
            else if (_boardReadResponse != null)
            {
                Assert.False(_boardReadResponse.IsSuccess);
                Assert.Equal(status, _boardReadResponse.StatusCode);
            }
            else if (_boardListResponse != null)
            {
                Assert.False(_boardListResponse.IsSuccess);
                Assert.Equal(status, _boardListResponse.StatusCode);
            }
            else if (_taskListResponse != null)
            {
                Assert.False(_taskListResponse.IsSuccess);
                Assert.Equal(status, _taskListResponse.StatusCode);
            }
            else if (_taskResponse != null)
            {
                Assert.False(_taskResponse.IsSuccess);
                Assert.Equal(status, _taskResponse.StatusCode);
            }
            else if (_taskReadResponse != null)
            {
                Assert.False(_taskReadResponse.IsSuccess);
                Assert.Equal(status, _taskReadResponse.StatusCode);
            }
            else if (_boolResponse != null)
            {
                Assert.False(_boolResponse.IsSuccess);
                Assert.Equal(status, _boolResponse.StatusCode);
            }
            else
            {
                Assert.Fail("No operation response was available to assert against.");
            }
        }

        #endregion
    }
}