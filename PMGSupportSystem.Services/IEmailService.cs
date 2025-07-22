using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMGSupportSystem.Services
{
    public interface IEmailService
    {
        Task SendMailAsync(string toEmail, string subject, string body);
    }
}
