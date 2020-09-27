using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

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

        public List<long> GetTitleIdsFromDb()
        {
            try
            {
                string connstr = _config.GetConnectionString("SQLCONNSTR_DIGITALBOOK");
                using (IDbConnection connection = new SqlConnection(connstr))
                {
                    var results = connection.Query<long>("SELECT titleId FROM digital_item").ToList();
                    return results;
                }
            }
            catch (DbException de)
            {
                _log.LogError(de, de.Message);
                return null;
            }
        }
    }
}
