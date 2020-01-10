using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main()
        {
            // TODO: Create a list of titles and authors that I am interested in checking up on.  This list could live in a database, json, Google sheet???? or hardcoded.
            // TODO: Maybe save results to Db.  The would have a null Borrowed dateTime.  This could get updated once the book has been borrowed and read.

            // TODO: Use Hoopla API for recently added titles
            // https://hoopla-ws.hoopladigital.com/kinds/10/titles/new?offset=0&limit=200&kindId=10&wwwVersion=4.31.0

            try
            {
                Uri uri = new Uri("https://hoopla-ws.hoopladigital.com/kinds/10/titles/new?offset=0&limit=200&kindId=10&wwwVersion=4.31.0");

                // Without setting Default Request Headers you will receive 500 Error
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36");
                string responseBody = await client.GetStringAsync(uri);

                //Console.WriteLine(responseBody);

                HooplaResponse hooplaResponse = new HooplaResponse();
                hooplaResponse.GetBooksFromJson(responseBody);

                // TODO: Send results in email.
                //hooplaResponse.SendResultsByEmail();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }
    }
}
