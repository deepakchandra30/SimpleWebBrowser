using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SimpleWebBrowser.Models;

namespace SimpleWebBrowser
{
    public class DataManager
    {
        private static DataManager _instance;
        private readonly BrowserDbContext _context;
        private User _currentUser;

        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataManager();
                }
                return _instance;
            }
        }

        // Private constructor for singleton pattern
        private DataManager()
        {
            try
            {
                _context = new BrowserDbContext();
                _context.Database.EnsureCreated();
            }
            catch
            {
                // Silently handle database initialization errors
            }
        }

        public bool Login(string username, string password)
        {
            try
            {
                var passwordHash = HashPassword(password);
                _currentUser = _context.Users
                    .Include(u => u.Bookmarks)
                    .Include(u => u.History)
                    .FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);

                return _currentUser != null;
            }
            catch
            {
                return false;
            }
        }

        public bool Register(string username, string password)
        {
            try
            {
                if (_context.Users.Any(u => u.Username == username))
                {
                    return false;
                }

                _currentUser = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    HomePage = "",
                    Bookmarks = new List<Bookmark>(),
                    History = new List<HistoryEntry>()
                };

                _context.Users.Add(_currentUser);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public string LoadHome()
        {
            return _currentUser?.HomePage ?? "";
        }

        public void SaveHome(string url)
        {
            if (_currentUser != null)
            {
                _currentUser.HomePage = url?.Trim() ?? "";
                _context.SaveChanges();
            }
        }

        public Dictionary<string, string> LoadBookmarks()
        {
            if (_currentUser == null)
                return new Dictionary<string, string>();

            return _context.Bookmarks
                .Where(b => b.UserId == _currentUser.Id)
                .ToDictionary(b => b.Name, b => b.Url);
        }

        public void SaveBookmarks(Dictionary<string, string> bookmarks)
        {
            try 
            {
                if (_currentUser == null) return;

                // Remove old bookmarks
                var oldBookmarks = _context.Bookmarks.Where(b => b.UserId == _currentUser.Id);
                _context.Bookmarks.RemoveRange(oldBookmarks);

                // Add new bookmarks
                foreach (var bookmark in bookmarks)
                {
                    _context.Bookmarks.Add(new Bookmark
                    {
                        UserId = _currentUser.Id,
                        Name = bookmark.Key,
                        Url = bookmark.Value
                    });
                }

                _context.SaveChanges();
            }
            catch
            {
                // Silently handle bookmark saving errors
            }
        }

        public List<string> LoadHistory()
        {
            if (_currentUser == null)
                return new List<string>();

            return _context.History
                .Where(h => h.UserId == _currentUser.Id)
                .OrderBy(h => h.VisitedAt)
                .Select(h => h.Url)
                .ToList();
        }

        public void SaveHistory(List<string> history)
        {
            if (_currentUser == null) return;

            // Remove old history
            var oldHistory = _context.History.Where(h => h.UserId == _currentUser.Id);
            _context.History.RemoveRange(oldHistory);

            // Add new history
            foreach (var url in history)
            {
                _context.History.Add(new HistoryEntry
                {
                    UserId = _currentUser.Id,
                    Url = url,
                    VisitedAt = DateTime.Now
                });
            }

            _context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool IsLoggedIn => _currentUser != null;
        public string CurrentUsername => _currentUser?.Username;
    }
}
