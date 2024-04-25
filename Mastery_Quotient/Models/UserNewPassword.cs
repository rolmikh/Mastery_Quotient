namespace Mastery_Quotient.Models
{
    public class UserNewPassword
    {
        public int? id { get; set; }

        public string email {  get; set; }
        
        public string newPassword { get; set; }

        public string repeatPassword { get; set; }

        public string token { get; set; }
    }
}
