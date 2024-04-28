namespace Mastery_Quotient.Models
{
    public class MyTestModelView
    {
        public Student Student { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<Test> Tests { get; set; }

        public List<StudentTest> StudentTest { get; set; }

       public List<TestQuestion> TestQuestions { get; set; }

        public MyTestModelView(Student student, List<Discipline> disciplines, List<Test> tests, List<StudentTest> studentTest, List<TestQuestion> testQuestions)
        {
            Student = student;
            Disciplines = disciplines;
            Tests = tests;
            StudentTest = studentTest;
            TestQuestions = testQuestions;
        }
    }
}
