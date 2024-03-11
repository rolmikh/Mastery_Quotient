namespace Mastery_Quotient.Models
{
    public class StudentModelDetails
    {
        public Student Student { get; set; }

        public StudyGroup StudyGroup { get; set; }

        public Course Course { get; set;}

        public StudentModelDetails(Student student, StudyGroup studyGroup, Course course)
        {
            Student = student;
            StudyGroup = studyGroup;
            Course = course;
        }
    }
}
