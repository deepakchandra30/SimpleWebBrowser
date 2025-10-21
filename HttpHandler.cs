using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleWebBrowser
{
    public class HttpHandler
    {
        private readonly HttpClient client;

        public HttpHandler()
        {
            client = new HttpClient();
        }

        public async Task<(string content, int statusCode)> FetchUrlAsync(string url)
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return (content, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return ($"Error: {ex.Message}", 0);
            }
        }
    }
}