using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.Service;
using Moq;
using OrganizationBoard.DTO;
using Polly;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class BoardServiceTest
    {
        private readonly Mock<IAsyncPolicy> _mockRetryPolicy;

        public BoardServiceTest()
        {
            _mockRetryPolicy = new Mock<IAsyncPolicy>();

            // Mocks the overload for ExecuteAsync that takes a Func<Context, CancellationToken, Task<T>>, a Context and a Cancellationtoken
            // Used when we need to pass context or cancellation tokens
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
            It.IsAny<Func<Context, CancellationToken, Task<OperationResponse<BoardDto>>>>(),
            It.IsAny<Context>(),
            It.IsAny<CancellationToken>()))
            .Returns<Func<Context, CancellationToken, Task<OperationResponse<BoardDto>>>, Context, CancellationToken>(
            (action, context, token) => action(context, token));

            // For BoardDto Return - Task<OperationResponse<BoardDto>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<BoardDto>>>>()))
                .Returns<Func<Task<OperationResponse<BoardDto>>>>(action => action());

            // For List<TaskReadDto> Return - Task<OperationResponse<List<TaskReadDto>>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<List<TaskReadDto>>>>>()
            )).Returns<Func<Task<OperationResponse<List<TaskReadDto>>>>>(action => action());

            // For List<BoardReadDto> Return - Singular Task<OperationResponse<BoardReadDto>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<BoardReadDto>>>>()))
                .Returns<Func<Task<OperationResponse<BoardReadDto>>>>(action => action());

            // For the bool check in DeleteBoard - Task<OperationResponse<bool>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(It.IsAny<Func<Task<OperationResponse<bool>>>>()))
                .Returns<Func<Task<OperationResponse<bool>>>>(action => action());

            //For the GetTeamBoard - Task<OperationResponse<List<BoardReadDto>>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<List<BoardReadDto>>>>>()
                )).Returns<Func<Task<OperationResponse<List<BoardReadDto>>>>>(action => action());

            // For TaskDto Return - Task<OperationResponse<TaskDto>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<TaskDto>>>>()))
                .Returns<Func<Task<OperationResponse<TaskDto>>>>(action => action());

            // For TaskReadDto return - Task<OperationResponse<TaskReadDto>>
            _mockRetryPolicy.Setup(p => p.ExecuteAsync(
                It.IsAny<Func<Task<OperationResponse<TaskReadDto>>>>()))
                .Returns<Func<Task<OperationResponse<TaskReadDto>>>>(action => action());

        }
        private OBDbContext GetInMemoryDbContext(string dbName = "TeamServiceTests")
        {
            var options = new DbContextOptionsBuilder<OBDbContext>()
                .UseInMemoryDatabase(databaseName: dbName).Options;

            var context = new OBDbContext(options);
            context.TeamTables.AddRange(
                new Team { TeamID = 1, TeamName = "Team 1" },
                new Team { TeamID = 2, TeamName = "Team 2" },
                new Team { TeamID = 3, TeamName = "Team 3" }
            );
            context.UserTables.AddRange(
                new User { UserID = 1, RoleID = 1, Email = "Test1@email.com", Password = "Lars123!", OrganizationID = 1 }, //Admin
                new User { UserID = 2, RoleID = 2, Email = "Test2@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 1 }, // Leader
                new User { UserID = 3, RoleID = 3, Email = "Test3@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 1 }, // Member
                new User { UserID = 4, RoleID = 2, Email = "Test4@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 2 },  // Leader
                new User { UserID = 5, RoleID = 3, Email = "Test5@email.com", Password = "Lars123!", OrganizationID = 1 }, //Member without team
                new User { UserID = 6, RoleID = 3, Email = "Test6@email.com", Password = "Lars123!", OrganizationID = 1, TeamID = 3 } //No boards
            );
            context.BoardTables.AddRange(
                new Board { BoardID = 1, BoardName = "Board1", TeamID = 1 },
                new Board { BoardID = 2, BoardName = "Board2", TeamID = 2 },
                new Board { BoardID = 3, BoardName = "Board3", TeamID = 1 }, // Empty board
                new Board { BoardID = 10, BoardName = "Board10" } // No team
            );
            context.TaskTables.AddRange(
                new EFrameWork.Model.Task { TaskID = 1, Title = "Task 1", BoardID = 1, StatusID = 1 },
                new EFrameWork.Model.Task { TaskID = 2, Title = "Task 2", BoardID = 1, StatusID = 2 },
                new EFrameWork.Model.Task { TaskID = 3, Title = "Task 3", BoardID = 2, StatusID = 3 }
            );
            context.SaveChanges();
            return context;
        }

        #region Tests for CreateBoard
        // Test: Leader as invalid user, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task CreateBoard_Returns403_IfUserNotLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotLeaderCreateBoardTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.CreateBoard(new BoardDto { BoardName = "TestBoard" }, requestingUserId: 3);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Access Denied", result.Message);
        }
        // Test: User as null, return 404
        [Fact]
        public async System.Threading.Tasks.Task CreateBoard_Returns404_IfUserNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("UserNotFoundCreateBoardTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.CreateBoard(new BoardDto { BoardName = "Board1" }, requestingUserId: 999);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("User not found", result.Message);
        }

        // Test: User as valid user, creating new board
        [Fact]
        public async System.Threading.Tasks.Task CreateBoard_SuccessfulCreationOfBoard()
        {
            // Arrange
            var context = GetInMemoryDbContext("BoardCreatedTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.CreateBoard(new BoardDto { BoardName = "Board11" }, requestingUserId: 2);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Board created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("Board11", result.Data.BoardName);
            Assert.NotNull(result.Data.BoardID);
            Assert.Equal(11, result.Data.BoardID);
        }
        // Test: Exception in try/catch = 500
        [Fact]
        public async System.Threading.Tasks.Task CreateBoard_Returns500_IfException()
        {
            // Arrange
            var context = GetInMemoryDbContext("BoardCreatedExceptionTest");
            // Creating an error for the retrypolicy
            var retryPolicyMock = new Mock<IAsyncPolicy>();
            retryPolicyMock.Setup(p => p.ExecuteAsync(It.IsAny<Func<Task<OperationResponse<BoardDto>>>>()))
                .ThrowsAsync(new Exception("Retry policy failed"));
            var service = new BoardService(context, retryPolicyMock.Object);

            // Act
            var result = await service.CreateBoard(new BoardDto { BoardName = "Board4" }, requestingUserId: 2);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
        #endregion Tests for CreateBoard

        #region Tests for GetBoardTasks
        // Test: Is part of this team, set to False = 403.
        [Fact]
        public async System.Threading.Tasks.Task GetBoardTasks_Returns403_IfUserNotTeamMember()
        {
            // Arrange
            var context = GetInMemoryDbContext("NotTeamMemberTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetBoardTasks(boardId: 1, requestingUserId: 5); // User 5 not in any team

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Access Denied", result.Message);
        }
        // Test: Tasks as empty, return 404
        [Fact]
        public async System.Threading.Tasks.Task GetBoardTasks_Returns404_IfNoTasksFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("EmptyTasksTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetBoardTasks(boardId: 3, requestingUserId: 2); // User 2 is leader of Team 1

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No tasks found", result.Message);
        }
        // Test: Success, getting all tasks. 
        [Fact]
        public async System.Threading.Tasks.Task GetBoardTasks_ReturnsTasks_IfValidRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidTasksTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetBoardTasks(boardId: 1, requestingUserId: 2); // User 2 is team leader of board 1

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Tasks retrieved successfully", result.Message);
            Assert.NotEmpty(result.Data);
            Assert.All(result.Data, task => Assert.Equal(1, task.BoardID));
        }
        #endregion Tests for GetBoardTasks

        #region Tests for UpdateBoard
        // Test: Board as null, return 404
        [Fact]
        public async System.Threading.Tasks.Task UpdateBoard_Returns404_IfBoardNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateBoard_BoardNotFound");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.UpdateBoard(new BoardReadDto { BoardID = 999, BoardName = "Board2" }, requestingUserId: 4);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Board not found", result.Message);
        }
        // Test: Successfully updated board
        [Fact]
        public async System.Threading.Tasks.Task UpdateBoard_ReturnsSuccess_IfBoardUpdated()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateBoard_Success");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.UpdateBoard(new BoardReadDto { BoardID = 3, BoardName = "New Name", TeamID = 1 }, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Board updated successfully", result.Message);
            Assert.Equal("New Name", context.BoardTables.First(b => b.BoardID == 3).BoardName);
        }
        #endregion Tests for UpdateBoard

        #region Tests for DeleteBoard
        // Test: Successfully deleted board
        [Fact]
        public async System.Threading.Tasks.Task DeleteBoard_Success_IfBoardDeletedWithoutTasks()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteBoardSuccess");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.DeleteBoard(boardId: 1, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Board deleted successfully", result.Message);
            Assert.DoesNotContain(context.BoardTables, b => b.BoardID == 1); // Confirm deletion
        }
        // Test: Board has tasks, delete tasks first
        [Fact]
        public async System.Threading.Tasks.Task DeleteBoard_RemovesTasks_ThenBoard_IfBoardHasTasks()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteBoardWithTasks");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.DeleteBoard(boardId: 1, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Board deleted successfully", result.Message);
            Assert.Null(context.BoardTables.FirstOrDefault(b => b.BoardID == 1));
            Assert.Empty(context.TaskTables.Where(t => t.BoardID == 1));
        }
        #endregion Tests for DeleteBoard

        #region Tests for GetBoard
        // Test: TeamID as null, return 404
        [Fact]
        public async System.Threading.Tasks.Task GetBoard_Returns404_IfTeamIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext("BoardWithNullTeamId");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetBoard(boardId: 10, requestingUserId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Board not found", result.Message);
        }
        // Test: Successfully retrieved board
        [Fact]
        public async System.Threading.Tasks.Task GetBoard_ReturnsBoard_IfUserIsTeamMember()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidBoardTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetBoard(boardId: 1, requestingUserId: 3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Board retrieved successfully", result.Message);
            Assert.Equal(1, result.Data.BoardID);
            Assert.Equal("Board1", result.Data.BoardName);
            Assert.Equal(1, result.Data.TeamID);
        }
        #endregion Tests for GetBoard

        #region Tests for GetTeamBoards
        // Test: Boards as empty, return 404
        [Fact]
        public async System.Threading.Tasks.Task GetTeamBoards_Returns404_IfNoBoardsExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("NoBoardsTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetTeamBoards(teamId: 3, requestingUserId: 6);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No boards found", result.Message);
        }
        // Test: Success, getting all boards.
        [Fact]
        public async System.Threading.Tasks.Task GetTeamBoards_ReturnsBoards_IfUserInTeam()
        {
            // Arrange
            var context = GetInMemoryDbContext("ValidTeamBoardsTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetTeamBoards(teamId: 1, requestingUserId: 3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, b => b.BoardName == "Board1");
            Assert.DoesNotContain(result.Data, b => b.BoardName == "Board2");
            Assert.Contains(result.Data, b => b.BoardName == "Board3");
        }
        #endregion Tests for GetTeamBoards

        #region Tests for CreateTask
        // Test: Create new task successfully
        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("CreateTaskSuccessTest");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            var taskDto = new TaskDto
            {
                BoardID = 1,
                Title = "Implement feature",
                Description = "Add login logic",
                Estimation = 5,
                NumUser = 1
            };

            // Act
            var result = await service.CreateTask(taskDto, 1, 2);


            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Task created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TaskID > 0); // Confirm task was assigned an ID
        }
        #endregion Tests for CreateTask

        #region Tests for GetTask
        // Test: Task as null, return 404
        [Fact]
        public async System.Threading.Tasks.Task GetTask_Returns404_WhenTaskNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetTask_NotFound");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetTask(taskId: 99, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Task not found", result.Message);
        }
        // Test: Successfully retrieved task
        [Fact]
        public async System.Threading.Tasks.Task GetTask_ReturnsSuccess_WhenTaskExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetTask_Success");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.GetTask(taskId: 1, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Task retrieved successfully", result.Message);
        }
        #endregion Tests for GetTask

        #region Tests for UpdateTask
        // Test: Successfully updated task
        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateTask_Success");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            var updatedTaskDto = new TaskReadDto
            {
                TaskID = 1,
                Title = "New Title",
                Description = "Updated description",
                Estimation = 5,
                NumUser = 2,
                StatusID = 1
            };

            // Act
            var result = await service.UpdateTask(updatedTaskDto, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Task updated successfully", result.Message);
        }
        #endregion Tests for UpdateTask

        #region Tests for DeleteTask
        // Test: Successfully deleted task
        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_Success_WhenTaskExistsAndUserIsTeamLeader()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteTaskSuccess");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.DeleteTask(taskId: 1, requestingUserId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Task deleted successfully", result.Message);
        }
        #endregion Tests for DeleteTask

        #region Tests for AssignTask
        // Test: Successfully assigned task
        [Fact]
        public async System.Threading.Tasks.Task AssignTask_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("AssignTask_Success");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.AssignTask(taskId: 1, requestingUserId: 2, assignedToUserId: 3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal("Task assigned successfully", result.Message);
        }
        #endregion Tests for AssignTask

        #region Tests for MarkTaskAsComplete
        // Test: IsUserInTask fails, return 403
        [Fact]
        public async System.Threading.Tasks.Task MarkTaskAsComplete_Returns403_WhenUserNotInTask()
        {
            // Arrange
            var context = GetInMemoryDbContext("MarkTask_Unauthorized");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.MarkTaskAsComplete(1, 3);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Access Denied", result.Message);
        }
        // Test: Successfully marked task as complete
        [Fact]
        public async System.Threading.Tasks.Task MarkTaskAsComplete_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("MarkTaskSuccess");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            context.TaskTables!.Add(new EFrameWork.Model.Task
            {
                TaskID = 10,
                Title = "Task to complete",
                StatusID = 3,
                UserAssignments = new List<UserToTask>
                    {
                        new UserToTask { UserID = 2, TaskID = 10 }
                    }
            });

            await context.SaveChangesAsync();

            // Act
            var result = await service.MarkTaskAsComplete(10, 2);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal("Task marked as complete", result.Message);
        }
        #endregion Tests for MarkTaskAsComplete

        #region ConfirmTaskCompletion
        // Test: Successfully confirmed task completion
        [Fact]
        public async System.Threading.Tasks.Task ConfirmTaskCompletion_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext("ConfirmTaskSuccess");
            var service = new BoardService(context, _mockRetryPolicy.Object);

            // Act
            var result = await service.ConfirmTaskCompletion(1, requestingUserId: 2); 

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Task completion confirmed", result.Message);
            Assert.True(result.Data);
        }
        #endregion ConfirmTaskCompletion


    }
}