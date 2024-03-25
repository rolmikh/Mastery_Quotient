using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class AnswerOption
    {
        

        public int? IdAnswerOptions { get; set; }
        public int? NumberAnswer { get; set; }
        public string? ContentAnswer { get; set; }
        public int? CorrectnessAnswer { get; set; }
    }
}
