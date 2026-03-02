namespace Kap_TestLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestDataAttribute : Attribute
    {
        public object[] data { get; }

        public TestDataAttribute(params object[] o)
        {
            this.data = o;
        }
    }   
}
