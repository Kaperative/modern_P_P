namespace Kap_TestLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : Attribute
    {
        public string NameOfTesting { get; }
        public TestMethodAttribute(string NameOfTesting = "")
        {
            this.NameOfTesting = NameOfTesting;
        }
    }
}
