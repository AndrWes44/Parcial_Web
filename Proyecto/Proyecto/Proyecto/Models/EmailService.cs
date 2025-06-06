using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Proyecto.Models
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public void EnviarCorreo(string destinatario, string asunto, string mensaje)
        {
            var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.SenderEmail, _smtp.SenderName),
                Subject = asunto,
                Body = mensaje,
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);
            client.Send(mail);
        }
    }
}
