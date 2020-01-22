using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    public class HooplaResponse
    {
        private List<DigitalBook> digitalBooks;
        private List<DigitalBook> newBooksToRead;


        public void GetBooksFromJson(string jsonResponse)
        {
            try
            {
                digitalBooks = JsonConvert.DeserializeObject<List<DigitalBook>>(jsonResponse);
            }
            catch (JsonException je)
            {
                Console.WriteLine("An exception was caught while deserializing the json!  {0}", je.Message);
            }

            newBooksToRead = new List<DigitalBook>();

            foreach (DigitalBook db in digitalBooks)
            {
                try
                {
                    if (AppSettings.Authors.Any(db.ArtistName.Contains))
                    {
                        newBooksToRead.Add(db);
                        Console.WriteLine("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                    else if (AppSettings.Titles.Any(db.Title.Contains))
                    {
                        newBooksToRead.Add(db);
                        Console.WriteLine("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception was caught!  {0}", ex.Message);
                }
            }
        }

        public Task SendEmailAsync()
        {
            string message = BuildMessageString();
 
            return Execute(AppSettings.SendGridApiKey,
                AppSettings.DefaultEmail.ToEmail,
                AppSettings.DefaultEmail.Subject,
                message);
        }

        private Task Execute(string apiKey, string toEmail, string subject, string message)
        {
            var client = new SendGridClient(apiKey);

            EmailAddress from = new EmailAddress(AppSettings.DefaultEmail.FromEmail, "HooplaNewReleaseCheck");
            EmailAddress to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);
            

            return client.SendEmailAsync(msg);
        }

        private string BuildMessageString()
        {
            StringBuilder message = new StringBuilder();

            if (newBooksToRead.Count > 0)
            {
                message.Append($"Hello Josh,\n\nThere were {newBooksToRead.Count} matches to the current criteria.\n\n");
                //message.Append(String.Format("{0:-30} {1:50} {2:20}\n\n", "Title", "Author", "Release Date"));

                foreach (DigitalBook book in newBooksToRead)
                {
                    try
                    {
                        Uri tempUri = new Uri(AppSettings.TitleBaseUri, $"title/{book.TitleId}");

                        message.AppendLine(String.Format("Title: {0}\nArtist: {1}\nRelease Date: {2}\n{3}\n\n", book.Title, book.ArtistName, book.ReleaseDateFormatted, tempUri));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An exception was caught!  {0}", ex.Message);
                    }
                }
            }

            return message.ToString();
        }
    }
}
