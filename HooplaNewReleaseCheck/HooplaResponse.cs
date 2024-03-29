﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using ClassLibrary;
using Microsoft.Extensions.Options;

namespace HooplaNewReleaseCheck
{
    public class HooplaResponse : IHooplaResponse
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<HooplaResponse> _log;
        private readonly IDataAccess _dataAccess;

        static readonly HttpClient client = new HttpClient();

        private List<long> _bookHistory;
        private List<string> _authorList;
        private List<string> _titleList;

        public HooplaResponse(IOptions<AppSettings> appSettings, ILogger<HooplaResponse> log, IDataAccess dataAccess)
        {
            _appSettings = appSettings;
            _log = log;
            _dataAccess = dataAccess;
        }

        public async Task<List<DigitalBook>> Run()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36");
            List<DigitalBook> digitalBooks = GetBooksFromJson(await client.GetStringAsync(_appSettings.Value.HooplaRecentReleasesUrl));

            if (digitalBooks.Count > 0)
            {
                _bookHistory = _dataAccess.GetTitleIdsFromDb();
                _authorList = _dataAccess.GetAuthorsToCheckFromDb();
                _titleList = _dataAccess.GetTitlesToCheckFromDb();

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
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<DigitalBook>>(jsonResponse, jsonOptions);
            }
            catch (JsonException je)
            {
                _log.LogError(je, je.Message);
                return null;
            }
        }

        private List<DigitalBook> FindNewBooksFromList(List<DigitalBook> digitalBooks)
        {
            List<DigitalBook> output = new List<DigitalBook>();

            foreach (DigitalBook db in digitalBooks)
            {
                try
                {
                    // Excluding books that have been previously borrowed and loaded into the database
                    if (_bookHistory.Contains(db.TitleId))
                    {
                        continue;
                    }
                    else
                    {
                        if (_authorList.Any(db.ArtistName.Contains))
                        {
                            output.Add(db);
                            _log.LogInformation("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                        }
                        else if (_titleList.Any(db.Title.Contains))
                        {
                            output.Add(db);
                            _log.LogInformation("Added {0}, by {1} to the list.", db.Title, db.ArtistName);
                        }
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
