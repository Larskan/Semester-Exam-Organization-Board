using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrganizationBoard.Service;
using Reqnroll;
using EFramework.Data;
using Reqnroll.Assist;

namespace OrganizationBoard.Tests.StepDefinitions
{
    [Binding]
    public class BoardServiceStepsTest
    {
        private Mock<OBDbContext> _mockDbContext;
        private BoardService _boardService;
        private int _userId;
        private int _teamId;
        private int _boardId;
        private bool _result;
        private List<User> _users;
        private List<Board> _boards;

        [Given(@"a mocked database context")]
        public void GivenAMockedDatabaseContext()
        {
            _mockDbContext = new Mock<OBDbContext>();
            _users = new List<User>();
            _boards = new List<Board>();

            var mockUserDbSet = MockDbSet(_users);
            var mockBoardDbSet = MockDbSet(_boards);

            _mockDbContext.Setup(c => c.UserTables).Returns(mockUserDbSet.Object);
            _mockDbContext.Setup(c => c.BoardTables).Returns(mockBoardDbSet.Object);
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class, new()
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                   .ReturnsAsync((object[] ids) => data.FirstOrDefault(o => o.GetType().GetProperty("UserID")?.GetValue(o).Equals(ids[0]) ?? false));
            mockSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<T, bool>>>(), default))
                   .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<T, bool>> predicate, System.Threading.CancellationToken _) => queryable.FirstOrDefault(predicate));
            return mockSet;
        }

        [Given(@"the following users exist:")]
        public void GivenTheFollowingUsersExist(Table table)
        {
            _users.AddRange(table.CreateSet<User>().ToList());
        }

        [Given(@"the following boards exist:")]
        public void GivenTheFollowingBoardsExist(Table table)
        {
            _boards.AddRange(table.CreateSet<Board>().ToList());
        }

        [Given(@"a user ID of (.*)")]
        public void GivenAUserIdOf(int userId)
        {
            _userId = userId;
        }

        [Given(@"a board ID of (.*)")]
        public void GivenABoardIdOf(int boardId)
        {
            _boardId = boardId;
        }

        [When(@"I check if the user is a team member of the board")]
        public async System.Threading.Tasks.Task WhenICheckIfTheUserIsATeamMemberOfTheBoard()
        {
            _boardService = new BoardService(_mockDbContext.Object);

            // Use reflection to call the private method
            var method = typeof(BoardService).GetMethod("IsUserTeamMember", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (System.Threading.Tasks.Task<bool>)method.Invoke(_boardService, new object[] { _userId, _boardId });
            _result = await task;
        }

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(bool expectedResult)
        {
            if (_result != expectedResult)
                throw new System.Exception($"Expected {expectedResult} but got {_result}");
        }
    }
}