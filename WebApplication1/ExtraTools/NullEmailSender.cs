using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication1.ExtraTools
{
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Pusty implementacja, która nic nie robi
            return Task.CompletedTask;
        }
    }
}
