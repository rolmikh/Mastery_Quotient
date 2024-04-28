namespace Mastery_Quotient.Models
{
    public class MyTestAnswerModelView
    {
        public Test Test { get; set; }

        public List<StudentAnswer> Answers { get; set; }

        public List<StudentTest> StudentTests { get; set; }

        public Student Student { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<TestQuestion> TestQuestions { get; set; }

        public List<Question> Questions { get; set; }

        public List<AnswerOption> AnswerOptions { get; set; }
         
        public List<QuestionAnswerOption> QuestionAnswerOptions { get; set; }

        public MyTestAnswerModelView(Test test, List<StudentAnswer> answers, List<StudentTest> studentTests, Student student, List<Discipline> disciplines, List<TestQuestion> testQuestions, List<Question> questions, List<AnswerOption> answerOptions, List<QuestionAnswerOption> questionAnswerOptions)
        {
            Test = test;
            Answers = answers;
            StudentTests = studentTests;
            Student = student;
            Disciplines = disciplines;
            TestQuestions = testQuestions;
            Questions = questions;
            AnswerOptions = answerOptions;
            QuestionAnswerOptions = questionAnswerOptions;
        }
    }
}
