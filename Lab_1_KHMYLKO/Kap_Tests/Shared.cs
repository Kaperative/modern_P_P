using Kap_TestLib;
using Kap_TestLib.Attributes;

[TestClass]
public class SharedContextDemo
{
    private static List<string> _testResults;
    private int _instanceCounter;

    [BeforeAll]
    public static void BeforeAll()
    {
        _testResults = new List<string>();
    }

    [AfterAll]
    public static void AfterAll()
    {
        
        Assert.IsEqual(3, _testResults.Count);
    }

    [TestSetUp]
    public void SetUp()
    {
        _instanceCounter = 42;
    }

    [TestTearDown]
    public void TearDown()
    {

    }

    [TestMethod]
    public void TestA()
    {
        _testResults.Add("TestA executed");
        Assert.IsEqual(42, _instanceCounter);
    }

    [TestMethod]
    public void TestB()
    {
        _testResults.Add("TestB executed");
        Assert.IsEqual(42, _instanceCounter);
    }

    [TestMethod]
    public void TestC()
    {
        _testResults.Add("TestC executed");
        Assert.IsEqual(42, _instanceCounter);
    }
}