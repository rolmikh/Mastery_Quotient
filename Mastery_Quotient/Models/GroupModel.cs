namespace Mastery_Quotient.Models
{
    public class GroupModel
    {

        public List<StudyGroup> StudyGroups { get; set; }

        public List<Course> Courses { get; set; }

        public GroupModel(List<StudyGroup> studyGroups, List<Course> courses)
        {
            StudyGroups = studyGroups;
            Courses = courses;
        }
    }
}
