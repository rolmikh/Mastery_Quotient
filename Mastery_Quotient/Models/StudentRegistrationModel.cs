namespace Mastery_Quotient.Models
{
    public class StudentRegistrationModel
    {

        public static int? IdStudent { get; set; }
        public static string? SurnameStudent { get; set; }
        public static string? NameStudent { get; set; }
        public static string? MiddleNameStudent { get; set; }
        public static string? EmailStudent { get; set; }
        public static string? PasswordStudent { get; set; }
        public static string? SaltStudent { get; set; }
        public static int? IsDeleted { get; set; }
        public static int? StudyGroupId { get; set; }

        public static string? PhotoStudent { get; set; } = null;


        public static string? Key { get; set; }
    }
}
