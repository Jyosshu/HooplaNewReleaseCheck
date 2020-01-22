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
            // TODO: Create a list of titles and or authors that I am interested in checking up on.  This list could live in a database, json, Google sheet???? or hardcoded.  

            try
            {
                Uri uri = new Uri(AppSettings.HooplaRecentReleasesUrl);

                // Without setting Default Request Headers you will receive 500 Error
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36");
                string responseBody = await client.GetStringAsync(uri);

                HooplaResponse hooplaResponse = new HooplaResponse();
                hooplaResponse.GetBooksFromJson(responseBody);
                await hooplaResponse.SendEmailAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }
    }
}
