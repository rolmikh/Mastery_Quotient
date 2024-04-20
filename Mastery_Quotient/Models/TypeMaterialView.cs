namespace Mastery_Quotient.Models
{
    public class TypeMaterialView
    {
        public List<TypeMaterial> TypeMaterials { get; set; }

        public TypeMaterialView(List<TypeMaterial> typeMaterials)
        {
            TypeMaterials = typeMaterials;
        }
    }
}
