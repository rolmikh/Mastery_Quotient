namespace Mastery_Quotient.Models
{
    public class MaterialModelView
    {

        public List<Material> Materials { get; set; }

        public List<Discipline> Disciplines { get; set; } 

        public List<TypeMaterial> TypeMaterials { get; set; }

        public MaterialModelView(List<Material> materials, List<Discipline> disciplines, List<TypeMaterial> typeMaterials)
        {
            Materials = materials;
            Disciplines = disciplines;
            TypeMaterials = typeMaterials;
        }
    }
}
