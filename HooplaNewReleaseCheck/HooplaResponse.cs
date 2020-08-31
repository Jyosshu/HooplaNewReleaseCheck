using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace HooplaNewReleaseCheck
{
    public class HooplaResponse : IHooplaResponse
    {
        private readonly IConfiguration _config;
        private readonly ILogger<HooplaResponse> _log;

        static readonly HttpClient client = new HttpClient();

        public HooplaResponse(IConfiguration config, ILogger<HooplaResponse> log)
        {
            _config = config;
            _log = log;
        }

        public async Task<List<DigitalBook>> Run()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36");
            List<DigitalBook> digitalBooks = GetBooksFromJson(await client.GetStringAsync(_config.GetValue<string>("HooplaRecentReleasesUrl")));

            if (digitalBooks.Count > 0)
            {
               return FindNewBooksFromList(digitalBooks);

            }
            else
            {
                _log.LogInformation("There were no new books from Hoopla.");
                return null;
            }
        }

        private List<DigitalBook> GetBooksFromJson(string jsonResponse)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<DigitalBook>>(jsonResponse);
            }
            catch (JsonException je)
            {
                _log.LogError(je, je.Message);
                return null;
            }
        }

        private List<DigitalBook> FindNewBooksFromList(List<DigitalBook> digitalBooks)
        {
            string[] authors = _config.GetSection("Authors").Get<string[]>();
            string[] titles = _config.GetSection("Titles").Get<string[]>();
            List<DigitalBook> output = new List<DigitalBook>();

            foreach (DigitalBook db in digitalBooks)
            {
                try
                {
                    if (authors.Any(db.ArtistName.Contains))
                    {
                        output.Add(db);
                        _log.LogInformation("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                    else if (titles.Any(db.Title.Contains))
                    {
                        output.Add(db);
                        _log.LogInformation("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, ex.Message);
                }
            }

            return output;
        }
    }
}
