using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
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
            StringBuilder message = new StringBuilder();

            if (newBooksToRead.Count > 0)
            {
                message.Append($"Hello Josh,{Environment.NewLine}There were { newBooksToRead.Count } matches to the current criteria." + Environment.NewLine + Environment.NewLine);

                foreach (DigitalBook book in newBooksToRead)
                {
                    try
                    {
                        Uri tempUri = new Uri($"{ _config.GetValue<string>("TitleBaseUri") }/title/{ book.TitleId }");

                        message.AppendLine(string.Format("Title: {0}{4}Artist: {1}{4}Release Date: {2}{4}{3}{4}{4}", book.Title, book.ArtistName, book.ReleaseDateFormatted, tempUri, Environment.NewLine));
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, ex.Message);
                    }
                }
            }

            return message.ToString();
        }
    }
}
