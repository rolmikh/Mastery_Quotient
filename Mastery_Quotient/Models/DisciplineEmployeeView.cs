namespace Mastery_Quotient.Models
{
    public class DisciplineEmployeeView
    {
        public List<Discipline> Disciplines { get; set; }

        public List<Employee> Employees { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<Course> Courses { get; set; }

        public DisciplineEmployeeView(List<Discipline> disciplines, List<Employee> employees, List<DisciplineEmployee> disciplineEmployees, List<Course> courses)
        {
            Disciplines = disciplines;
            Employees = employees;
            DisciplineEmployees = disciplineEmployees;
            Courses = courses;
        }
    }
}
