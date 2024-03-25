namespace Mastery_Quotient.Models
{
    public class NewModelQuestionOne
    {
        public List<AnswerOptionViewModel> AnswerOptionViewModels { get; set; }
    }

    public class AnswerOptionViewModel
    {
        public string? AnswerOption { get; set; }
        public bool? IsCorrectAnswer { get; set; }
    }
}
