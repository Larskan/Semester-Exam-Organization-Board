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

    // Tests Ideas
    // 1. Test for CreateUser
    // 2. Test for UpdateUser
    // 3. Test for DeleteUser
    // 4. Test for GetUser
    // 5. Test for GetAllUsers
    // 6. Test for GetUserById
    // 7. Test for GetUserByEmail
    // 8. Test for GetUserByRole
    // 9. Test for GetUserByOrganization
    // 10. Test for GetUserByTeam
    // 11. Test for GetUserByStatus
    // 12. Test for GetUserByTask
    // 13. Test for GetUserByBoard
    // 14. Test for 
    
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
