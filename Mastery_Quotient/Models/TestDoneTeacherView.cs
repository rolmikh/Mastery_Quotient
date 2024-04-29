namespace Mastery_Quotient.Models
{
    public class TestDoneTeacherView
    {
        public List<Student> Students { get; set; }

        public List<StudyGroup> StudyGroups { get; set; }

        public List<Course> Courses { get; set; }

        public List<DisciplineOfTheStudyGroup> DisciplineOfTheStudyGroups { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<StudentTest> StudentsTest { get; set;}

        public Employee Employee { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<Test> Tests { get; set; }

        public List<TestQuestion> TestsQuestion { get; set; }

        public TestDoneTeacherView(List<Student> students, List<StudyGroup> studyGroups, List<Course> courses, List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups, List<Discipline> disciplines, List<StudentTest> studentsTest, Employee employee, List<DisciplineEmployee> disciplineEmployees, List<Test> tests, List<TestQuestion> testsQuestion)
        {
            Students = students;
            StudyGroups = studyGroups;
            Courses = courses;
            DisciplineOfTheStudyGroups = disciplineOfTheStudyGroups;
            Disciplines = disciplines;
            StudentsTest = studentsTest;
            Employee = employee;
            DisciplineEmployees = disciplineEmployees;
            Tests = tests;
            TestsQuestion = testsQuestion;
        }
    }
}
