using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System;

namespace ClassLibrary
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DataAccess> _log;

        public DataAccess(IConfiguration config, ILogger<DataAccess> log)
        {
            _config = config;
            _log = log;
        }

        public List<string> GetAuthorsToCheckFromDb()
        {
            try
            {
                string connstr = _config.GetConnectionString("SQLCONNSTR_DIGITALBOOK");
                using (IDbConnection connection = new SqlConnection(connstr))
                {
                    var results = connection.Query<string>("SELECT ArtistName FROM DigitalItem GROUP BY ArtistName HAVING (COUNT(*)) > 1").ToList();
                    return results;
                }
            }
            catch (DbException de)
            {
                _log.LogError(de, de.Message);
                return null;
            }
        }

        public List<long> GetTitleIdsFromDb()
        {
            try
            {
                string connstr = _config.GetConnectionString("SQLCONNSTR_DIGITALBOOK");
                using (IDbConnection connection = new SqlConnection(connstr))
                {
                    var results = connection.Query<long>("SELECT titleId FROM DigitalItem").ToList();
                    return results;
                }
            }
            catch (DbException de)
            {
                _log.LogError(de, de.Message);
                return null;
            }
        }

        public List<string> GetTitlesToCheckFromDb()
        {
            List<string> output = new List<string>();
            List<string> results;

            try
            {
                string connstr = _config.GetConnectionString("SQLCONNSTR_DIGITALBOOK");
                using (IDbConnection connection = new SqlConnection(connstr))
                {
                    results = connection.Query<string>("SELECT Title FROM DigitalItem ORDER BY Title ASC").ToList();
                }

                if (results.Count < 1)
                {
                    return null;
                }
                else
                {
                    string previousTitle = "NotStarted";

                    foreach (string s in results)
                    {
                        string tempTitle;

                        if (s.Contains(previousTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (s.Contains("Vol.", StringComparison.OrdinalIgnoreCase))
                        {
                            int index = s.IndexOf("Vol.") - 1;
                            tempTitle = s.Substring(0, index);
                        }
                        else if (s.Contains(":", StringComparison.OrdinalIgnoreCase))
                        {
                            int index = s.IndexOf(":");
                            tempTitle = s.Substring(0, index);
                        }
                        else
                        {
                            tempTitle = s;
                        }

                        output.Add(tempTitle);
                        previousTitle = tempTitle;
                    }
                }

                return output;
            }
            catch (DbException de)
            {
                _log.LogError(de, de.Message);
                return null;
            }
        }
    }
}
