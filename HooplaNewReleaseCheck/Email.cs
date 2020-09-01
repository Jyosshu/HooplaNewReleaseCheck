using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    public class Email : IEmail
    {
        private readonly IConfiguration _config;
        private readonly ILogger<Email> _log;

        public Email(IConfiguration config, ILogger<Email> log)
        {
            _config = config;
            _log = log;
        }
        public Task SendEmailAsync(List<DigitalBook> newBooksToRead)
        {
            string message = BuildMessageString(newBooksToRead);

            return Execute(AppSettings.SendGridApiKey,
                _config.GetValue<string>("DefaultToEmail"),
                AppSettings.Subject,
                message);
        }

        private Task Execute(string apiKey, string toEmail, string subject, string message)
        {
            var client = new SendGridClient(apiKey);

            EmailAddress from = new EmailAddress(_config.GetValue<string>("DefaultFromEmail"), "HooplaNewReleaseCheck");
            EmailAddress to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            return client.SendEmailAsync(msg);
        }

        private string BuildMessageString(List<DigitalBook> newBooksToRead)
        {
            StringBuilder message = new StringBuilder(ReadHtmlToString());

            if (newBooksToRead.Count > 0)
            {
                string emailIntro = $"<p>Hello Josh,</p><p>There were { newBooksToRead.Count } matches to the current criteria.</p>";
                message.Replace("@ReplaceIntro", emailIntro);

                StringBuilder emailBody = new StringBuilder();

                foreach (DigitalBook book in newBooksToRead)
                {
                    try
                    {
                        Uri tempUri = new Uri($"{ _config.GetValue<string>("TitleBaseUri") }/title/{ book.TitleId }");
                        Uri imageUri = new Uri($"{ _config.GetValue<string>("HooplaImageBaseUrl") }/{ book.ArtKey }_270.jpeg");
         
                        emailBody.AppendLine($"<tr> <td> <a href=\"{ tempUri }\" target=\"_blank\"><strong>{ book.Title }</strong> </a> <div> <div class=\"inlineBlock\"> <ul> <li> Artist: { book.ArtistName } </li> <li> Release Date: { book.ReleaseDateFormatted } </li> </ul> </div> <div class=\"inlineBlock\"> <img src=\"{ imageUri }\" > </ div > </ div > </ td > </ tr > ");
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, ex.Message);
                    }
                }

                message.Replace("@ReplaceBody", emailBody.ToString());
            }

            return message.ToString();
        }

        private string ReadHtmlToString()
        {
            try
            {
                string file = _config.GetValue<string>("EmailTemplate");
                if (File.Exists(file))
                    return File.ReadAllText(file);
                else
                    throw new FileNotFoundException("The email template file: EmailMessage.html was not found.");
            }
            catch (IOException ie)
            {
                _log.LogError(ie, ie.Message);
                return null;
            }
        }
    }
}
