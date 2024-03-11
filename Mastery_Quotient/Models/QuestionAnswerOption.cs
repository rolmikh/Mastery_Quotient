using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class QuestionAnswerOption
    {
        public int? IdQuestionAnswerOptions { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerOptionsId { get; set; }

    }
}
