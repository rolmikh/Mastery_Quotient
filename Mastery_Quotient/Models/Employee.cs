using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Employee
    {
       

        public int? IdEmployee { get; set; }
        public string? SurnameEmployee { get; set; } 
        public string? NameEmployee { get; set; } 
        public string? MiddleNameEmployee { get; set; }
        public string? EmailEmployee { get; set; }
        public string? PasswordEmployee { get; set; } 
        public string? SaltEmployee { get; set; }
        public int? IsDeleted { get; set; }
        public int? RoleId { get; set; }

    }
}
