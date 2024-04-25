namespace API_Mastery_Quotient.Models
{
    public class PasswordReset
    {
        public int IdPasswordReset { get; set; }
        public int EmployeeId { get; set; }
        public int StudentId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
