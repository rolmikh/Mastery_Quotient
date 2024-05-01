namespace Mastery_Quotient.Models
{
    public class EmployeeModelDetails
    {

        public Employee employee { get; set; }

        public Role role { get; set; }

        public List<StudyGroup> studyGroups { get; set; }

        public List<EmployeeStudyGroup> employeeStudyGroups { get; set; }

        public List<DisciplineEmployee> disciplineEmployees { get; set; }

        public List<Discipline> disciplines { get; set; }

        public EmployeeModelDetails(Employee employee, Role role, List<StudyGroup> studyGroups, List<EmployeeStudyGroup> employeeStudyGroups, List<DisciplineEmployee> disciplineEmployees, List<Discipline> disciplines)
        {
            this.employee = employee;
            this.role = role;
            this.studyGroups = studyGroups;
            this.employeeStudyGroups = employeeStudyGroups;
            this.disciplineEmployees = disciplineEmployees;
            this.disciplines = disciplines;
        }
    }
}
