namespace Mastery_Quotient.Models
{
    public class ViewTestModel
    {
        public Test Test { get; set; }

        public List<TypeQuestion> TypeQuestion { get; set; }

        public List<Question> Question { get; set; }

        public List<TestQuestion> TestQuestion { get; set; }

        public List<AnswerOption> AnswerOption { get; set;}

        public List<QuestionAnswerOption> QuestionAnswerOption { get;set; }

        public Employee Employee { get; set; }

        public List<Discipline> Discipline { get; set; }

        public List<DisciplineEmployee> disciplineEmployees { get; set; }

        public ViewTestModel(Test test, List<TypeQuestion> typeQuestion, List<Question> question, List<TestQuestion> testQuestion, List<AnswerOption> answerOption, List<QuestionAnswerOption> questionAnswerOption, Employee employee, List<Discipline> discipline, List<DisciplineEmployee> disciplineEmployees)
        {
            Test = test;
            TypeQuestion = typeQuestion;
            Question = question;
            TestQuestion = testQuestion;
            AnswerOption = answerOption;
            QuestionAnswerOption = questionAnswerOption;
            Employee = employee;
            Discipline = discipline;
            this.disciplineEmployees = disciplineEmployees;
        }
    }
}
