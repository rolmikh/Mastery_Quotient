namespace Mastery_Quotient.Models
{
    public class MaterialTeacher
    {
        public Employee Employees { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<TypeMaterial> TypeMaterials { get; set; }


        public MaterialTeacher(Employee employees, List<Discipline> disciplines, List<DisciplineEmployee> disciplineEmployees, List<TypeMaterial> typeMaterials)
        {
            Employees = employees;
            Disciplines = disciplines;
            DisciplineEmployees = disciplineEmployees;
            TypeMaterials = typeMaterials;
        }

       
    }
}
