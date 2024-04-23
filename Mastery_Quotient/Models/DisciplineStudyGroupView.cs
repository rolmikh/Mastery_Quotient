namespace Mastery_Quotient.Models
{
    public class DisciplineStudyGroupView
    {
        public List<StudyGroup> StudyGroups { get; set; }

        public List<DisciplineOfTheStudyGroup> DisciplineOfTheStudyGroups { get; set; }

        public List<Course> Courses { get; set; }

        public List<Discipline> Disciplines { get; set; }

        public DisciplineStudyGroupView(List<StudyGroup> studyGroups, List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups, List<Course> courses, List<Discipline> disciplines)
        {
            StudyGroups = studyGroups;
            DisciplineOfTheStudyGroups = disciplineOfTheStudyGroups;
            Courses = courses;
            Disciplines = disciplines;
        }
    }
}
