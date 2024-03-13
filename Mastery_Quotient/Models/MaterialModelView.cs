namespace Mastery_Quotient.Models
{
    public class MaterialModelView
    {

        public List<Material> Materials { get; set; }

        public MaterialModelView(List<Material> materials)
        {
            Materials = materials;
        }
    }
}
