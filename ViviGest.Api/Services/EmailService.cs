using System.Net;
using System.Net.Mail;

namespace ViviGest.Api.Services
{
    public interface IEmailService
    {
        void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml)
        {
            var smtp = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
            {
                Credentials = new NetworkCredential(
                    _config["Email:User"],
                    _config["Email:Password"]
                ),
                EnableSsl = true
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]!, "ViviGest"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            smtp.Send(mensaje);
        }
    }
}
