using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Question
    {
       
        public int? IdQuestion { get; set; }
        public string? NameQuestion { get; set; } 
        public int? TypeQuestionId { get; set; }

        public int? NumberQuestion { get; set; }

    }
}
