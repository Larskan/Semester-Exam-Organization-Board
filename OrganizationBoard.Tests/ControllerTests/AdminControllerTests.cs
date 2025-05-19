using Xunit.Abstractions;

namespace OrganizationBoard.Tests.ControllerTests;
using OrganizationBoard.Controller;

public class AdminControllerTests
{
    private readonly ITestOutputHelper _outputHelper;

    public AdminControllerTests(ITestOutputHelper testoutputHelper)
    {
        _outputHelper = testoutputHelper;

    }

    //TODO: Add tests for AdminController
    [Fact]
    public void Test1()
    {
        //Arrange

        //Act

        //Assert
        _outputHelper.WriteLine("");
    }
}
