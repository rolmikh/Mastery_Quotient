namespace Mastery_Quotient.Models
{
    public class StudentMaterialModel
    {
        public List<Material> Materials { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public List<TypeMaterial> TypeMaterials { get; set; }

        public StudyGroup StudyGroups { get; set; }

        public List<DisciplineOfTheStudyGroup> DisciplineOfTheStudyGroups { get; set; }

        public List<Employee> Employees { get; set; }

        public List<DisciplineEmployee> DisciplineOfTheEmployees { get;set; }

        public Student Students { get; set; }

        public StudentMaterialModel(List<Material> materials, List<Discipline> disciplines, List<TypeMaterial> typeMaterials, StudyGroup studyGroups, List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups, List<Employee> employees, List<DisciplineEmployee> disciplineOfTheEmployees, Student students)
        {
            Materials = materials;
            Disciplines = disciplines;
            TypeMaterials = typeMaterials;
            StudyGroups = studyGroups;
            DisciplineOfTheStudyGroups = disciplineOfTheStudyGroups;
            Employees = employees;
            DisciplineOfTheEmployees = disciplineOfTheEmployees;
            Students = students;
        }
    }
}
