namespace Mastery_Quotient.Models
{
    public class MaterialsViewTeacher
    {

        public Employee Employees { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<DisciplineEmployee> DisciplineEmployees { get; set; }

        public List<TypeMaterial> TypeMaterials { get; set; }

        public List<Material> Materials { get; set; }

        public MaterialsViewTeacher(Employee employees, List<Discipline> disciplines, List<DisciplineEmployee> disciplineEmployees, List<TypeMaterial> typeMaterials, List<Material> materials)
        {
            Employees = employees;
            Disciplines = disciplines;
            DisciplineEmployees = disciplineEmployees;
            TypeMaterials = typeMaterials;
            Materials = materials;
        }
    }
}
