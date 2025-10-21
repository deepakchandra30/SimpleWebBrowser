using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SimpleWebBrowser
{
    public class SimpleBrowserCore
    {
        private readonly HttpClient client = new HttpClient();
        public string HtmlContent { get; private set; }
        public string StatusCode { get; private set; }
        public string CurrentUrl { get; private set; }

        public async Task FetchPageAsync(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                HtmlContent = await response.Content.ReadAsStringAsync();
                StatusCode = $"{(int)response.StatusCode} {response.ReasonPhrase}";
                CurrentUrl = url;
            }
            catch (HttpRequestException e)
            {
                HtmlContent = e.Message;
                StatusCode = "Request failed";
            }
        }

        public List<string> ExtractTopFiveLinks()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlContent);
            var links = new List<string>();

            foreach (var node in doc.DocumentNode.SelectNodes("//a[@href]") ?? [])
            {
                var href = node.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                    links.Add(href);
                if (links.Count >= 5) break;
            }
            return links;
        }
    }
}
