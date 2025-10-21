using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleWebBrowser
{
    public static class BrowserStorage
    {
        public static string LoadHome() => File.Exists("home.txt") ? File.ReadAllText("home.txt") : "https://hw.ac.uk";
        public static void SaveHome(string url) => File.WriteAllText("home.txt", url);

        public static void SaveBookmarks(Dictionary<string, string> bookmarks)
        {
            File.WriteAllLines("bookmarks.txt", bookmarks.Select(kv => $"{kv.Key}|{kv.Value}"));
        }

        public static Dictionary<string, string> LoadBookmarks()
        {
            var dict = new Dictionary<string, string>();
            if (File.Exists("bookmarks.txt"))
            {
                foreach (var line in File.ReadAllLines("bookmarks.txt"))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2) dict[parts[0]] = parts[1];
                }
            }
            return dict;
        }

        public static void SaveHistory(List<string> history)
        {
            File.WriteAllLines("history.txt", history);
        }

        public static List<string> LoadHistory() =>
            File.Exists("history.txt") ? File.ReadAllLines("history.txt").ToList() : new List<string>();
    }
}
