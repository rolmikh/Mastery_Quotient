namespace Mastery_Quotient.Models
{
    public class TestParameterView
    {
        public List<TestParameter> testParameters { get; set; }

        public TestParameterView(List<TestParameter> testParameters)
        {
            this.testParameters = testParameters;
        }
    }
}
