using MimeKit;
using MailKit.Net.Smtp;

namespace Mastery_Quotient.Service
{
    public class EmailService
    {
        public async Task SendEmail(string email, string subject, string message)
        {
            try
            {
                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("MastQuo", "isip_l.m.rozkovskaiya@mpt.ru"));
                emailMessage.To.Add(MailboxAddress.Parse(email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtp.Authenticate("isip_l.m.rozkovskaiya@mpt.ru", "wbiwkdbxwkpdwaa");
                    smtp.Send(emailMessage);
                    smtp.Disconnect(true);
                }; 
            }
            catch(Exception e)
            {
                
            }
        }
     
    }
}
