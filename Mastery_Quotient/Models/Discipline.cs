using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Discipline
    {
        
        public int? IdDiscipline { get; set; }
        public string? NameDiscipline { get; set; }
        public int? CourseId { get; set; }
        public int? IsDeleted { get; set; }

    }
}
