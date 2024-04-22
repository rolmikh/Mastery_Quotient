namespace Mastery_Quotient.Models
{
    public class StudentPersonalAccountModel
    {
        public Student Student { get; set; }

        public StudyGroup StudyGroup { get; set; }

        public StudentPersonalAccountModel(Student student, StudyGroup studyGroup)
        {
            Student = student;
            StudyGroup = studyGroup;
        }
    }
}
