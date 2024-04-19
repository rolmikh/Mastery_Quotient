using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class StudyGroup
    {
       

        public int? IdStudyGroup { get; set; }
        public string? NameStudyGroup { get; set; } 
        public int? CourseId { get; set; }
        public int? IsDeleted { get; set; }

    }
}
