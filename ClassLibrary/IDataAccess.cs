using System.Collections.Generic;

namespace ClassLibrary
{
    public interface IDataAccess
    {
        List<long> GetTitleIdsFromDb();
        List<string> GetTitlesToCheckFromDb();
        List<string> GetAuthorsToCheckFromDb();
    }
}