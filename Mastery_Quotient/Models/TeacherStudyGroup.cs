namespace Mastery_Quotient.Models
{
    public class TeacherStudyGroup
    {
        public List<StudyGroup> StudyGroups { get; set; }

        public List<Course> Courses { get; set; }

        public List<EmployeeStudyGroup> EmployeeStudyGroups { get; set; }

        public List<Employee> Employees { get; set; }

        public TeacherStudyGroup(List<StudyGroup> studyGroups, List<Course> courses, List<EmployeeStudyGroup> employeeStudyGroups, List<Employee> employees)
        {
            StudyGroups = studyGroups;
            Courses = courses;
            EmployeeStudyGroups = employeeStudyGroups;
            Employees = employees;
        }
    }
}
