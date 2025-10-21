using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;

namespace SimpleWebBrowser
{
    public class SimpleBrowserCore
    {
        private readonly HttpClient client = new HttpClient();
        public string HtmlContent { get; private set; } = "";
        public string StatusCode { get; private set; } = "";
        public string CurrentUrl { get; private set; } = "";
        public string PageTitle { get; private set; } = "";

        public async Task FetchPageAsync(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            try
            {
                var response = await client.GetAsync(url);
                HtmlContent = await response.Content.ReadAsStringAsync();
                StatusCode = $"{(int)response.StatusCode} {response.ReasonPhrase}";
                CurrentUrl = url;

                // Extract page title
                var doc = new HtmlDocument();
                doc.LoadHtml(HtmlContent);
                var titleNode = doc.DocumentNode.SelectSingleNode("//title");
                PageTitle = titleNode?.InnerText?.Trim() ?? "No Title";

                // Handle specific error codes
                if (!response.IsSuccessStatusCode)
                {
                    switch ((int)response.StatusCode)
                    {
                        case 400:
                            HtmlContent = "400 Bad Request: The server could not understand the request.";
                            break;
                        case 403:
                            HtmlContent = "403 Forbidden: You don't have permission to access this resource.";
                            break;
                        case 404:
                            HtmlContent = "404 Not Found: The requested page could not be found.";
                            break;
                        default:
                            HtmlContent = $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";
                            break;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                HtmlContent = $"Request failed: {e.Message}";
                StatusCode = "Error";
                PageTitle = "Error";
            }
            catch (Exception e)
            {
                HtmlContent = $"An unexpected error occurred: {e.Message}";
                StatusCode = "Error";
                PageTitle = "Error";
            }
        }

        public List<string> ExtractTopFiveLinks()
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(HtmlContent);
                var links = new List<string>();

                var nodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var href = node.GetAttributeValue("href", "");
                        if (string.IsNullOrEmpty(href)) continue;

                        // Convert relative URLs to absolute
                        if (Uri.TryCreate(new Uri(CurrentUrl), href, out Uri absoluteUri))
                            href = absoluteUri.ToString();

                        if (!links.Contains(href))
                            links.Add(href);

                        if (links.Count >= 5) break;
                    }
                }
                return links;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
