using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Student
    {
        public int? IdStudent { get; set; }
        public string? SurnameStudent { get; set; } 
        public string? NameStudent { get; set; } 
        public string? MiddleNameStudent { get; set; }
        public string? EmailStudent { get; set; } 
        public string? PasswordStudent { get; set; }
        public string? SaltStudent { get; set; }
        public int? IsDeleted { get; set; }
        public int? StudyGroupId { get; set; }

        public string? PhotoStudent { get; set; } = null;
    }
}
