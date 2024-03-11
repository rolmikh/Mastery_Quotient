namespace Mastery_Quotient.Models
{
    public class GroupModel
    {

        public List<StudyGroup> StudyGroups { get; set; }

        public GroupModel(List<StudyGroup> studyGroups)
        {
            StudyGroups = studyGroups;
        }
    }
}
