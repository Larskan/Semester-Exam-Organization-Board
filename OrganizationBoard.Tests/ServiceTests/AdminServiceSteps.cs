using OrganizationBoard.IService;
using OrganizationBoard.Model;
using OrganizationBoard.DTO;
using Moq;
using FluentAssertions;
using Reqnroll; // Ein Mocking-Framework für Abhängigkeiten

[Binding]
public class AdminServiceSteps
{
    private readonly ScenarioContext _scenarioContext;
    private Mock<IAdminService> _mockAdminService; // Mock des zu testenden Services
    private User _userToCreate;
    private OperationResponse<User> _creationResult;
    private int _requestingAdminId;

    public AdminServiceSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _mockAdminService = new Mock<IAdminService>();
    }

    [Given(@"I am an administrator with the ID (.*)")]
    public void GivenIAmAnAdministratorWithTheID(int adminId)
    {
        _requestingAdminId = adminId;
    }

    [Given(@"I have the following user data:")]
    public void GivenIHaveTheFollowingUserData(Table table)
    {
        _userToCreate = new User
        {
            Email = table.Rows[0]["Email"],
            Password = table.Rows[0]["Password"] //Maybe engangspassword
        };

        // Mock the CreateUser method
        _mockAdminService.Setup(service => service.CreateUser(_userToCreate, _requestingAdminId))
            .ReturnsAsync(new OperationResponse<User>
            {
                IsSuccess = true,
                Data = new User
                {
                    UserId = 1,
                    Email = _userToCreate.Email,
                    Password = _userToCreate.Password,
                },
                Message = "User created successfully"
            });
    }

    [When(@"I try to create a new user with this data")]
    public async Task WhenITryToCreateANewUserWithThisData()
    {
        // Hier "programmieren wir gegen das Interface", da die Implementierung noch fehlt
        _creationResult = await _mockAdminService.Object.CreateUser(_userToCreate, _requestingAdminId);
    }

    [Then(@"the operation should be successful")]
    public void ThenTheOperationShouldBeSuccessful()
    {
        _creationResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"a new user with the specified data and a unique ID should have been created")]
    public void ThenANewUserWithTheSpecifiedDataAndAUniqueIDShouldHaveBeenCreated()
    {
        _creationResult.Data.Should().NotBeNull();
        _creationResult.Data.UserId.Should().BeGreaterThan(0); // Annahme: ID wird automatisch generiert
        _creationResult.Data.Email.Should().Be(_userToCreate.Email);
        _creationResult.Data.Password.Should().Be(_userToCreate.Password);
    }

    [Then(@"I should receive a success message with the created user")]
    public void ThenIShouldReceiveASuccessMessageWithTheCreatedUser()
    {
        _creationResult.Message.Should().NotBeNullOrEmpty();
        _creationResult.Data.Should().NotBeNull();
    }
}