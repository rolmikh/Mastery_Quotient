namespace Mastery_Quotient.Models
{
    public partial class StudentAnswer
    {
        public int? IdStudentAnswer {  get; set; }
        public int? StudentTestId { get; set; }
        public int? QuestionAnswerOptionsId { get; set; }
        public int? QuestionId { get; set; }
        public string? ContentAnswer { get; set; }
        public int? IsDeleted { get; set; }
    }
}
