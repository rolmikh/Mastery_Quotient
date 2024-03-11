using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Test
    {
        
        public int? IdTest { get; set; }
        public string? NameTest { get; set; } 
        public DateTime? DateCreatedTest { get; set; }
        public DateTime? Deadline { get; set; }
        public int? DisciplineId { get; set; }
        public int? EmployeeId { get; set; }
        public int? TestParameterId { get; set; }

    }
}
