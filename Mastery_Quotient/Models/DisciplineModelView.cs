namespace Mastery_Quotient.Models
{
    public class DisciplineModelView
    {
        public List<Discipline> Disciplines { get; set; }

        public List<Course> Courses { get; set; }

        public DisciplineModelView(List<Discipline> disciplines, List<Course> courses)
        {
            Disciplines = disciplines;
            Courses = courses;
        }
    }
}
