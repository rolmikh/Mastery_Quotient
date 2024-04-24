namespace Mastery_Quotient.Models
{
    public class UserAnswerViewModel
    {
            public int QuestionId { get; set; }
            public int QuestionTypeId { get; set; }
            public string AnswerOption { get; set; }
            public int SelectedAnswerId { get; set; }
            public List<int> SelectedAnswerIds { get; set; }
    }
    
}
