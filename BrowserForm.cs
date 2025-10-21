using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleWebBrowser
{
    public class BrowserForm : Form
    {
        private TextBox addressBar;
        private RichTextBox displayBox;
        private Label titleBar;
        private ListBox linkList;
        private ListBox bookmarkList;
        private ListBox historyList;
        private Button goButton, reloadButton, homeButton, backButton, forwardButton;
        private MenuStrip menuStrip;
        private Label statusLabel;

        private SimpleBrowserCore browser = new SimpleBrowserCore();
        private BrowserStorage storage = BrowserStorage.Instance;
        private string homeUrl;
        private Dictionary<string, string> bookmarks = new Dictionary<string, string>();
        private List<string> history = new List<string>();
        private int currentHistoryIndex = -1;

        private readonly Color primaryColor = Color.FromArgb(64, 105, 225);  // Royal blue
        private readonly Color secondaryColor = Color.FromArgb(245, 247, 250);  // Light blue-gray
        private readonly Color accentColor = Color.FromArgb(46, 204, 113);  // Emerald green
        private readonly Color backgroundColor = Color.FromArgb(250, 251, 252);  // Off-white
        private readonly Color gradientStart = Color.FromArgb(64, 105, 225);  // Royal blue
        private readonly Color gradientEnd = Color.FromArgb(72, 126, 238);    // Lighter blue
        private readonly Font defaultFont = new Font("Segoe UI", 9F);
        private readonly Font titleFont = new Font("Segoe UI Semibold", 10F);
        private readonly Font headerFont = new Font("Segoe UI", 11F, FontStyle.Bold);

        public BrowserForm()
        {
            Text = "Simple C# Web Browser";
            Size = new Size(1200, 800);
            BackColor = Color.White;
            Font = defaultFont;
            
            // Show login form first
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    return;
                }
            }

            InitializeComponents();
            LoadSavedData();
            SetupEventHandlers();
            
            // Add form closing handler
            this.FormClosing += (s, e) => {
                // Save current state before closing
                storage.SaveBookmarks(bookmarks);
                storage.SaveHistory(history);
                storage.SaveHome(homeUrl);
            };
        }

        private void InitializeComponents()
        {
            // Main menu
            menuStrip = new MenuStrip
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = defaultFont,
                Padding = new Padding(5, 2, 0, 2),
                Renderer = new CustomMenuRenderer(primaryColor)
            };

            var fileMenu = new ToolStripMenuItem("File");
            var editMenu = new ToolStripMenuItem("Edit");
            var bookmarksMenu = new ToolStripMenuItem("Bookmarks");
            
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Close());
            editMenu.DropDownItems.Add("Set Homepage", null, (s, e) => SetHomePage());
            bookmarksMenu.DropDownItems.Add("Add Bookmark", null, (s, e) => AddBookmark());
            bookmarksMenu.DropDownItems.Add("Edit Bookmark", null, (s, e) => EditBookmark());
            bookmarksMenu.DropDownItems.Add("Delete Bookmark", null, (s, e) => DeleteBookmark());

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(bookmarksMenu);

            // Navigation controls
            var navPanel = new Panel { 
                Height = 45, 
                Dock = DockStyle.Top,
                BackColor = gradientStart,
                Padding = new Padding(8)
            };
            navPanel.Paint += (s, e) => {
                var rect = new Rectangle(0, 0, navPanel.Width, navPanel.Height);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, gradientStart, gradientEnd, System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };

            // Create custom button style
            Action<Button> styleButton = (btn) => {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.FromArgb(255, 255, 255, 40);
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 12);
                btn.Cursor = Cursors.Hand;
                btn.Size = new Size(35, 35);
                btn.Margin = new Padding(3);
                
                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(255, 255, 255, 80);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(255, 255, 255, 40);
            };

            backButton = new Button { Text = "←" };
            forwardButton = new Button { Text = "→" };
            reloadButton = new Button { Text = "⟳" };
            homeButton = new Button { Text = "🏠" };
            styleButton(backButton);
            styleButton(forwardButton);
            styleButton(reloadButton);
            styleButton(homeButton);

            addressBar = new TextBox { 
                Width = 450,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(255, 255, 255, 240),
                ForeColor = Color.White,
                Margin = new Padding(5, 0, 5, 0),
                Height = 28
            };
            var addressPanel = new Panel {
                Padding = new Padding(1),
                BackColor = Color.FromArgb(255, 255, 255, 80),
                Height = 30,
                Width = 452
            };
            addressPanel.Controls.Add(addressBar);

            goButton = new Button { 
                Text = "Go",
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var compileButton = new Button {
                Text = "⚙ Run",
                Width = 70,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 255, 255, 60),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Margin = new Padding(5, 0, 0, 0)
            };
            compileButton.Click += async (s, e) => {
                if (addressBar.Text.EndsWith(".html") || addressBar.Text.EndsWith(".htm"))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(addressBar.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error running the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid HTML file path to run.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            navPanel.Controls.AddRange(new Control[] { 
                backButton, forwardButton, reloadButton, homeButton, addressBar, goButton 
            });

            // Title and status
            titleBar = new Label { Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleLeft };
            statusLabel = new Label { Dock = DockStyle.Bottom, Height = 25 };

            // Main display and side panel container
            var mainContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            displayBox = new RichTextBox { 
                Multiline = true, 
                ScrollBars = RichTextBoxScrollBars.Both,
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                WordWrap = false
            };

            // Side panels
            var rightPanel = new Panel { 
                Dock = DockStyle.Right, 
                Width = 250,
                BackColor = secondaryColor,
                Padding = new Padding(10),
                AutoScroll = true
            };

            // Common style for section headers
            Action<Label> styleHeader = (lbl) => {
                lbl.Font = titleFont;
                lbl.ForeColor = primaryColor;
                lbl.Dock = DockStyle.Top;
                lbl.Height = 25;
            };

            // Common style for list boxes
            Action<ListBox> styleListBox = (lst) => {
                lst.Font = defaultFont;
                lst.BorderStyle = BorderStyle.FixedSingle;
                lst.BackColor = Color.White;
                lst.ForeColor = Color.Black;
                lst.IntegralHeight = false;
                lst.Dock = DockStyle.Fill;
            };

            // Link List Section
            var linkPanel = new Panel { 
                Dock = DockStyle.Top,
                Height = 150,
                Margin = new Padding(0, 0, 0, 10)
            };
            var linkLabel = new Label { Text = "Page Links" };
            styleHeader(linkLabel);
            linkList = new ListBox();
            styleListBox(linkList);
            linkPanel.Controls.AddRange(new Control[] { linkList, linkLabel });

            // Bookmark Section
            var bookmarkPanel = new Panel {
                Dock = DockStyle.Top,
                Height = 200,
                Margin = new Padding(0, 0, 0, 10)
            };
            var bookmarkLabel = new Label { Text = "Bookmarks" };
            styleHeader(bookmarkLabel);
            bookmarkList = new ListBox();
            styleListBox(bookmarkList);
            bookmarkPanel.Controls.AddRange(new Control[] { bookmarkList, bookmarkLabel });

            // History Section
            var historyPanel = new Panel {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };
            var historyLabel = new Label { Text = "History" };
            styleHeader(historyLabel);
            historyList = new ListBox();
            styleListBox(historyList);

            historyPanel.Controls.Add(historyList);

            // Add panels to right panel
            rightPanel.Controls.Add(historyPanel);
            rightPanel.Controls.Add(bookmarkPanel);
            rightPanel.Controls.Add(linkPanel);

            // Navigation panel layout
            var toolStrip = new ToolStrip();
            backButton.Size = new Size(30, 25);
            forwardButton.Size = new Size(30, 25);
            reloadButton.Size = new Size(30, 25);
            homeButton.Size = new Size(30, 25);
            addressBar.Size = new Size(500, 25);
            goButton.Size = new Size(50, 25);

            var navFlow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 2, 5, 2)
            };
            navFlow.Controls.AddRange(new Control[] { 
                backButton, forwardButton, reloadButton, homeButton, addressPanel, goButton, compileButton
            });
            navPanel.Controls.Add(navFlow);

            // Browser display setup
            var displayPanel = new Panel { 
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = secondaryColor
            };

            // Add a border effect
            var borderPanel = new Panel {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = Color.FromArgb(200, 200, 200)
            };

            borderPanel.Controls.Add(displayBox);
            displayPanel.Controls.Add(borderPanel);

            // Add all controls
            mainContainer.Panel1.Controls.Add(displayPanel);
            Controls.Add(mainContainer);
            Controls.Add(rightPanel);
            Controls.Add(statusLabel);
            Controls.Add(titleBar);
            Controls.Add(navPanel);
            Controls.Add(menuStrip);

            MainMenuStrip = menuStrip;
        }

        private void LoadSavedData()
        {
            if (!storage.IsLoggedIn) return;
            
            Text = $"Simple C# Web Browser - {storage.CurrentUsername}";
            homeUrl = storage.LoadHome();
            bookmarks = storage.LoadBookmarks();
            history = storage.LoadHistory();
            
            // Update UI
            UpdateBookmarksList();
            UpdateHistoryList();
        }

        private void SetupEventHandlers()
        {
            goButton.Click += async (_, __) => await Navigate(addressBar.Text);
            reloadButton.Click += async (_, __) => await Navigate(browser.CurrentUrl);
            homeButton.Click += async (_, __) => await Navigate(homeUrl);
            backButton.Click += async (_, __) => await NavigateHistory(-1);
            forwardButton.Click += async (_, __) => await NavigateHistory(1);

            addressBar.KeyPress += async (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    await Navigate(addressBar.Text);
                }
            };

            linkList.DoubleClick += async (_, __) => {
                if (linkList.SelectedItem != null)
                    await Navigate(linkList.SelectedItem.ToString());
            };

            bookmarkList.DoubleClick += async (_, __) => {
                if (bookmarkList.SelectedItem != null)
                    await Navigate(bookmarks[bookmarkList.SelectedItem.ToString()]);
            };

            historyList.DoubleClick += async (_, __) => {
                if (historyList.SelectedItem != null)
                {
                    var idx = history.Count - 1 - historyList.SelectedIndex;
                    if (idx >= 0 && idx < history.Count)
                    {
                        currentHistoryIndex = idx;
                        await Navigate(history[idx], false);
                    }
                }
            };

            Load += async (_, __) => await Navigate(homeUrl);
        }

        private void SetHomePage()
        {
            using var dialog = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Set Homepage",
                StartPosition = FormStartPosition.CenterParent
            };
            var textBox = new TextBox() { Left = 20, Top = 20, Width = 340, Text = homeUrl };
            var button = new Button() { Text = "Save", Left = 150, Width = 100, Top = 50, DialogResult = DialogResult.OK };
            
            dialog.Controls.Add(textBox);
            dialog.Controls.Add(button);
            dialog.AcceptButton = button;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                homeUrl = textBox.Text;
                storage.SaveHome(homeUrl);
            }
        }

        private void AddBookmark()
        {
            using var dialog = new Form()
            {
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Add Bookmark",
                StartPosition = FormStartPosition.CenterParent,
                Padding = new Padding(10)
            };

            var nameLabel = new Label { Text = "Name:", Dock = DockStyle.Top };
            var nameBox = new TextBox { 
                Dock = DockStyle.Top,
                Text = browser.PageTitle // Use page title as default name
            };

            var urlLabel = new Label { 
                Text = "URL:", 
                Dock = DockStyle.Top,
                Padding = new Padding(0, 10, 0, 0)
            };
            var urlBox = new TextBox { 
                Dock = DockStyle.Top,
                Text = browser.CurrentUrl,
                ReadOnly = true
            };

            var buttonPanel = new Panel {
                Height = 40,
                Dock = DockStyle.Bottom
            };

            var button = new Button { 
                Text = "Add Bookmark",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 30
            };
            button.Location = new Point((buttonPanel.Width - button.Width) / 2, 5);
            buttonPanel.Controls.Add(button);

            dialog.Controls.AddRange(new Control[] {
                buttonPanel,
                urlBox,
                urlLabel,
                nameBox,
                nameLabel
            });
            dialog.AcceptButton = button;

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(nameBox.Text))
            {
                bookmarks[nameBox.Text] = browser.CurrentUrl;
                storage.SaveBookmarks(bookmarks);
                UpdateBookmarksList();
                statusLabel.Text = "Bookmark added: " + nameBox.Text;
            }
        }

        private void EditBookmark()
        {
            if (bookmarkList.SelectedItem == null) return;

            var oldName = bookmarkList.SelectedItem.ToString();
            using var dialog = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Edit Bookmark",
                StartPosition = FormStartPosition.CenterParent
            };
            var nameBox = new TextBox() { Left = 20, Top = 20, Width = 340, Text = oldName };
            var urlBox = new TextBox() { Left = 20, Top = 70, Width = 340, Text = bookmarks[oldName] };
            var button = new Button() { Text = "Save", Left = 150, Width = 100, Top = 100, DialogResult = DialogResult.OK };
            
            dialog.Controls.Add(new Label() { Text = "Name:", Left = 20, Top = 3 });
            dialog.Controls.Add(new Label() { Text = "URL:", Left = 20, Top = 53 });
            dialog.Controls.Add(nameBox);
            dialog.Controls.Add(urlBox);
            dialog.Controls.Add(button);
            dialog.AcceptButton = button;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                bookmarks.Remove(oldName);
                bookmarks[nameBox.Text] = urlBox.Text;
                storage.SaveBookmarks(bookmarks);
                UpdateBookmarksList();
            }
        }

        private void DeleteBookmark()
        {
            if (bookmarkList.SelectedItem == null) return;

            if (MessageBox.Show("Delete this bookmark?", "Confirm Delete", 
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                bookmarks.Remove(bookmarkList.SelectedItem.ToString());
                storage.SaveBookmarks(bookmarks);
                UpdateBookmarksList();
            }
        }

        private void UpdateBookmarksList()
        {
            bookmarkList.Items.Clear();
            foreach (var name in bookmarks.Keys)
                bookmarkList.Items.Add(name);
        }

        private void UpdateHistoryList()
        {
            historyList.Items.Clear();
            // Show most recent first, with timestamps
            for (int i = history.Count - 1; i >= 0; i--)
            {
                var url = history[i];
                var current = (i == currentHistoryIndex) ? "→ " : "  ";
                historyList.Items.Add($"{current}{url}");
            }

            backButton.Enabled = currentHistoryIndex > 0;
            forwardButton.Enabled = currentHistoryIndex < history.Count - 1;
            
            // Update status label with navigation info
            if (history.Count > 0)
            {
                statusLabel.Text = $"Page {currentHistoryIndex + 1} of {history.Count}";
            }
        }

        private async Task NavigateHistory(int direction)
        {
            var newIndex = currentHistoryIndex + direction;
            if (newIndex >= 0 && newIndex < history.Count)
            {
                currentHistoryIndex = newIndex;
                await Navigate(history[newIndex], false);
            }
        }

        private async Task Navigate(string url, bool addToHistory = true)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            statusLabel.Text = "Loading...";
            Application.DoEvents();

            try
            {
                await browser.FetchPageAsync(url);
                displayBox.Text = browser.HtmlContent;
                addressBar.Text = url;
                titleBar.Text = $"{browser.StatusCode} - {browser.PageTitle}";
                statusLabel.Text = browser.StatusCode;

                if (addToHistory)
                {
                    // Remove consecutive duplicates
                    if (history.Count == 0 || history[history.Count - 1] != url)
                    {
                        history.Add(url);
                        currentHistoryIndex = history.Count - 1;
                        storage.SaveHistory(history);
                        UpdateHistoryList();
                    }
                }

                var links = browser.ExtractTopFiveLinks();
                linkList.Items.Clear();
                foreach (var link in links)
                    linkList.Items.Add(link);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error: " + ex.Message;
            }
        }
    }
}
