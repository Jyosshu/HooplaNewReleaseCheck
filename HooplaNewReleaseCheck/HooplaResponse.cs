using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using HooplaNewReleaseCheck.Models;

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
                        Console.WriteLine("Added the book, {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                    else if (AppSettings.Titles.Any(db.Title.Contains))
                    {
                        newBooksToRead.Add(db);
                        Console.WriteLine("Added the book, {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception was caught!  {0}", ex.Message);
                }
            }
        }

        public void SendResultsByEmail()
        {
            User user = AppSettings.GetUser();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Josh Wygle", "jwygle.dev@gmail.com"));
            message.To.Add(new MailboxAddress("Josh Wygle", "jwygle@gmail.com"));
            message.Subject = $"Hoopla Recent Releases Results - {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}";

            message.Body = new TextPart("plain")
            {
                Text = $@"Hello Josh,
                There were {newBooksToRead.Count} matches to the current criteria.
                {BuildMessageString()}"
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(user.Username, user.Password);

                client.Send(message);
                client.Disconnect(true);
            }
        }

        private string BuildMessageString()
        {
            StringBuilder message = new StringBuilder();

            if (newBooksToRead.Count > 0)
            {
                foreach (DigitalBook book in newBooksToRead)
                {
                    try
                    {
                        Uri tempUri = new Uri(AppSettings.TitleBaseUri, $"title/{book.TitleId}");
                        message.AppendLine($"{book.Title}\t{book.ArtistName}\n{tempUri}\n");
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
