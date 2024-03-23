namespace Mastery_Quotient.Models
{
    public class TestViewTeacher
    {

        public Employee Employees { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<TestParameter> TestParameters { get; set; }

        public List<Test> Test { get; set; }

        public TestViewTeacher(Employee employees, List<Discipline> disciplines, List<DisciplineEmployee> disciplineEmployees, List<TestParameter> testParameters, List<Test> test)
        {
            Employees = employees;
            Disciplines = disciplines;
            DisciplineEmployees = disciplineEmployees;
            TestParameters = testParameters;
            Test = test;
        }
    }
}
