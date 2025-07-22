using Microsoft.Extensions.Options;
using PMGSupportSystem.Repositories.ConfigurationModels;
using System.Net;
using System.Net.Mail;

namespace PMGSupportSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSetting;

        public EmailService(IOptionsMonitor<SmtpSettings> optionsMonitor)
        {
            _smtpSetting = optionsMonitor.CurrentValue;
        }

        public async Task SendMailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpSetting.Host)
            {
                Port = _smtpSetting.Port,
                Credentials = new NetworkCredential(_smtpSetting.UserName, _smtpSetting.Password),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSetting.UserName, _smtpSetting.SenderName),
                Subject = subject,
                Body = Body(subject, "Mr/Ms", body, "PMGSupportSystem", "https://pmg-201c-support.vercel.app/"),
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string Body(string subject, string name, string content, string senderName, string buttonUrl)
        {
            string body = $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>{subject}</title>
        <style>
            body {{
                font-family: 'Poppins', Arial, sans-serif;
                margin: 0;
                padding: 0;
                background-color: #f4f4f4;
            }}
            .container {{
                max-width: 600px;
                margin: 0 auto;
                background-color: #ffffff;
                border: 1px solid #cccccc;
            }}
            .header {{
                background-color: #ff7aac;
                padding: 40px;
                text-align: center;
                color: white;
                font-size: 24px;
            }}
            .body {{
                padding: 40px;
                text-align: left;
                font-size: 16px;
                line-height: 1.6;
            }}
            .cta {{
                padding: 10px 20px;
                border-radius: 5px;
                border: 1px solid black;
                display: inline-block;
                color: black;
                text-decoration: none;
                font-weight: bold;
            }}
            .footer {{
                background-color: #333333;
                padding: 40px;
                text-align: center;
                color: white;
                font-size: 14px;
            }}
        </style>
    </head>
    <body>
        <div class=""container"">
            <div class=""header"">
            </div>
            <div class=""body"">
                <p>Dear {name},</p>
                <p>{content}</p>
                <p>Thank you for using our website.</p>
                <div style=""text-align: center;"">
                    <a href=""{buttonUrl}"" target=""_blank"" class=""cta"">Click the Button</a>
                </div>
            </div>
            <div class=""footer"">
                <p>Copyright &copy; 2024 | {senderName}</p>
            </div>
        </div>
    </body>
    </html>";
            return body;
        }
    }
}
