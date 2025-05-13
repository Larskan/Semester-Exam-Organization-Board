using Xunit.Abstractions;

namespace OrganizationBoard.Tests.ControllerTests;
using OrganizationBoard.Controller;

public class BoardControllerTests
{
    private readonly ITestOutputHelper _outputHelper;

    public BoardControllerTests(ITestOutputHelper testoutputHelper)
    {
        _outputHelper = testoutputHelper;

    }

    //TODO: Add tests for BoardController
    [Fact]
    public void Test1()
    {
        //Arrange

        //Act

        //Assert
        _outputHelper.WriteLine("");
    }
}
