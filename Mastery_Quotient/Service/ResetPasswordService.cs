using System.Text;

namespace Mastery_Quotient.Service
{
    public class ResetPasswordService
    {
        public string GeneratePasswordResetToken(string email)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{DateTime.UtcNow}"));
            return token;
        }


        public bool IsPasswordResetTokenValid(string token, string email)
        {
            var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var tokenParts = tokenData.Split(':');
            var tokenEmail = tokenParts[0];
            var tokenDate = DateTime.Parse(tokenParts[1]);

            if (tokenEmail != email || tokenDate < DateTime.UtcNow.AddHours(-1))
            {
                return false;
            }

            return true;
        }

    }
}
