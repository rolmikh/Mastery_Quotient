namespace Mastery_Quotient.Models
{
    public class StudentModelView
    {
        public List<Student> Students { get; set; }

        public List<StudyGroup> StudyGroups { get; set; }

        public StudentModelView(List<Student> students, List<StudyGroup> studyGroups)
        {
            Students = students;
            StudyGroups = studyGroups;
        }
    }
}
