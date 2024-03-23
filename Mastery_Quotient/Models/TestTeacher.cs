namespace Mastery_Quotient.Models
{
    public class TestTeacher
    {

        public Employee Employees { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<TestParameter> TestParameters { get; set; }

        public TestTeacher(Employee employees, List<Discipline> disciplines, List<DisciplineEmployee> disciplineEmployees, List<TestParameter> testParameters)
        {
            Employees = employees;
            Disciplines = disciplines;
            DisciplineEmployees = disciplineEmployees;
            TestParameters = testParameters;
        }
    }
}
