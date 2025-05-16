using System.Collections.Generic;
using System.Threading.Tasks;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.DTO;
using OrganizationBoard.Service;
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
        private OperationResponse<List<TaskReadDto>> _taskListResponse;
        private User _user;

        public BoardManagementSteps(ScenarioContext context)
        {
            _context = context;
            _dbContext = TestDbContextFactory.Create();
            _service = new BoardService(_dbContext);
        }

        [Given(@"I am a ""(.*)"" user with ID (\d+) in team (\d+)")]
        public void GivenIAmAUserWithRole(string role, int userId, int teamId)
        {
            _user = TestDataFactory.CreateUser(userId, teamId, role);
            _dbContext.UserTables.Add(_user);
            _dbContext.SaveChanges();
        }

        [When(@"I create a board named ""(.*)""")]
        public async System.Threading.Tasks.Task WhenICreateABoardNamed(string name)
        {
            var dto = TestDataFactory.CreateBoardDto(name);
            _boardResponse = await _service.CreateBoard(dto, _user.UserID);
        }

        [Then(@"the operation should succeed")]
        public void ThenTheOperationShouldSucceed()
        {
            Assert.True(_boardResponse.IsSuccess);
        }

        [Then(@"the returned board ID should not be 0")]
        public void ThenReturnedBoardIDNotZero()
        {
            Assert.True(_boardResponse.Data.BoardID > 0);
        }

        [Then(@"the operation should fail with status code (\d+)")]
        public void ThenOperationFailsWithStatusCode(int status)
        {
            if (_boardResponse != null)
            {
                Assert.False(_boardResponse.IsSuccess);
                Assert.Equal(status, _boardResponse.StatusCode);
            }
            else if (_taskListResponse != null)
            {
                Assert.False(_taskListResponse.IsSuccess);
                Assert.Equal(status, _taskListResponse.StatusCode);
            }
            else
            {
                Assert.Fail("No operation response was available to assert against.");
            }
        }

        [Given(@"a board with ID (\d+) exists for team (\d+)")]
        public void GivenABoardWithIdExistsForTeam(int boardId, int teamId)
        {
            var board = TestDataFactory.CreateBoard(boardId, $"Test Board {boardId}", teamId);
            _dbContext.BoardTables.Add(board);
            _dbContext.SaveChanges();
        }

        [Given(@"the board has tasks")]
        public void GivenTheBoardHasTasks()
        {
            var task = TestDataFactory.CreateTask(boardId: 1, title: "Task 1");
            _dbContext.TaskTables.Add(task);
            _dbContext.SaveChanges();
        }

        [When(@"I fetch tasks for board (\d+)")]
        public async System.Threading.Tasks.Task WhenIFetchTasksForBoard(int boardId)
        {
            _taskListResponse = await _service.GetBoardTasks(boardId, _user.UserID);
        }

        [Then(@"the task response should contain tasks")]
        public void ThenTaskResponseShouldContainTasks()
        {
            Assert.True(_taskListResponse.IsSuccess);
            Assert.NotEmpty(_taskListResponse.Data);
        }
    }
}
