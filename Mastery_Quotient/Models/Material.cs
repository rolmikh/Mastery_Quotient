using System;
using System.Collections.Generic;

namespace Mastery_Quotient.Models
{
    public partial class Material
    {
        public int? IdMaterial { get; set; }
        public string? NameMaterial { get; set; } 
        public DateTime? DateCreatedMaterial { get; set; }
        public string? FileMaterial { get; set; }
        public int? DisciplineId { get; set; }
        public int? TypeMaterialId { get; set; }
        public int? EmployeeId { get; set; }
        public string? PhotoMaterial { get; set; }


    }
}
