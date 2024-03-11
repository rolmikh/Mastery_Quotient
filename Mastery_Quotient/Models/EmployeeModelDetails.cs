namespace Mastery_Quotient.Models
{
    public class EmployeeModelDetails
    {

        public Employee employee { get; set; }

        public Role role { get; set; }

        public List<StudyGroup> studyGroups { get; set; }

        public List<EmployeeStudyGroup> employeeStudyGroups { get; set; }

        public EmployeeModelDetails(Employee employee, Role role, List<StudyGroup> studyGroups, List<EmployeeStudyGroup> employeeStudyGroups)
        {
            this.employee = employee;
            this.role = role;
            this.studyGroups = studyGroups;
            this.employeeStudyGroups = employeeStudyGroups;
        }
    }
}
