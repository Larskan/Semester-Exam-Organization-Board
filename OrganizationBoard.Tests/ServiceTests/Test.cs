using Xunit.Abstractions;

namespace OrganizationBoard.Tests;

public class Test
{
    private readonly ITestOutputHelper _outputHelper;
    
    public Test(ITestOutputHelper testoutputHelper)
    {
        _outputHelper = testoutputHelper;

    }

    [Fact]
    public void Test1()
    {
        //Arrange
        int a = 1;
        int b = 2;
        //Act
        int c = a+b;
        //Assert
        _outputHelper.WriteLine(""+c);
        Assert.Equal(3,c);
    }
}
