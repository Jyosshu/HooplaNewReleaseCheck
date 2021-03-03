using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<Email> _log;
        private readonly IOptions<AppSettings> _appSettings;

        public Email(IOptions<AppSettings> appSettings, ILogger<Email> log)
        {
            _log = log;
            _appSettings = appSettings;
        }
        public Task SendEmailAsync(List<DigitalBook> newBooksToRead)
        {
            string message = BuildMessageString(newBooksToRead);
            var sendGridApiKey = AppSettings.SendGridApiKey;

            return Execute(sendGridApiKey,
                _appSettings.Value.DefaultToEmail,
                AppSettings.Subject,
                message);
        }

        private Task Execute(string apiKey, string toEmail, string subject, string message)
        {
            var client = new SendGridClient(apiKey);

            EmailAddress from = new EmailAddress(_appSettings.Value.DefaultFromEmail, "HooplaNewReleaseCheck");
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
                        Uri tempUri = new Uri($"{ _appSettings.Value.TitleBaseUrl }/title/{ book.TitleId }");
                        Uri imageUri = new Uri($"{ _appSettings.Value.HooplaImageBaseUrl }/{ book.ArtKey }_270.jpeg");
         
                        emailBody.AppendLine($"<tr> <td> <a href=\"{ tempUri }\" target=\"_blank\"><strong>{ book.Title }</strong> </a> <div> <div class=\"inlineBlock\"> <ul> <li> Artist: { book.ArtistName } </li> <li> Release Date: { book.ReleaseDateFormatted } </li> </ul> </div> <div class=\"inlineBlock\"> <a href=\"{ tempUri }\" target=\"_blank\"><img src=\"{ imageUri }\" > </a> </ div > </ div > </ td > </ tr > ");
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
                if (File.Exists(_appSettings.Value.EmailTemplate))
                    return File.ReadAllText(_appSettings.Value.EmailTemplate);
                else
                    throw new FileNotFoundException($"The email template file: { _appSettings.Value.EmailTemplate } was not found.");
            }
            catch (IOException ie)
            {
                _log.LogError(ie, ie.Message);
                return null;
            }
        }
    }
}
