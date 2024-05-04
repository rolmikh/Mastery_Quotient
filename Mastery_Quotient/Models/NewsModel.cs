namespace Mastery_Quotient.Models
{
    public class NewsModel
    {
        public List<Employee> Employees { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<TestParameter> TestParameters { get; set; }

        public List<Test> Test { get; set; }

        public Student Student { get; set; }

        public List<DisciplineOfTheStudyGroup> DisciplineOfTheStudyGroup { get; set; }

        public StudyGroup StudyGroup { get; set; }

        public List<Material> Materials { get; set; }

        public List<TypeMaterial> TypeMaterials { get; set; }

        public List<Course> Courses { get; set; }

        public NewsModel(List<Employee> employees, List<Discipline> disciplines, List<DisciplineEmployee> disciplineEmployees, List<TestParameter> testParameters, List<Test> test, Student student, List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroup, StudyGroup studyGroup, List<Material> materials, List<TypeMaterial> typeMaterials, List<Course> courses)
        {
            Employees = employees;
            Disciplines = disciplines;
            DisciplineEmployees = disciplineEmployees;
            TestParameters = testParameters;
            Test = test;
            Student = student;
            DisciplineOfTheStudyGroup = disciplineOfTheStudyGroup;
            StudyGroup = studyGroup;
            Materials = materials;
            TypeMaterials = typeMaterials;
            Courses = courses;
        }
    }
}
