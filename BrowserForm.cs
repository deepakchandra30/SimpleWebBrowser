using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleWebBrowser
{
    public class BrowserForm : Form
    {
        private TextBox addressBar = new TextBox { Dock = DockStyle.Top };
        private Button goButton = new Button { Text = "Go" };
        private Button reloadButton = new Button { Text = "Reload" };
        private Button homeButton = new Button { Text = "Home" };
        private TextBox displayBox = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        private Label titleBar = new Label { Dock = DockStyle.Top, Height = 25 };
        private ListBox linkList = new ListBox { Dock = DockStyle.Right, Width = 250 };

        private SimpleBrowserCore browser = new SimpleBrowserCore();
        private string homeUrl;
        private List<string> history = new List<string>();

        public BrowserForm()
        {
            Text = "Simple C# Web Browser";
            homeUrl = BrowserStorage.LoadHome();

            Controls.Add(displayBox);
            Controls.Add(titleBar);
            Controls.Add(addressBar);
            Controls.Add(goButton);
            Controls.Add(reloadButton);
            Controls.Add(homeButton);
            Controls.Add(linkList);

            goButton.Click += async (_, __) => await Navigate(addressBar.Text);
            reloadButton.Click += async (_, __) => await Navigate(browser.CurrentUrl);
            homeButton.Click += async (_, __) => await Navigate(homeUrl);
            linkList.DoubleClick += async (_, __) =>
            {
                if (linkList.SelectedItem != null)
                    await Navigate(linkList.SelectedItem.ToString());
            };

            Load += async (_, __) => await Navigate(homeUrl);
        }

        private async Task Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            await browser.FetchPageAsync(url);
            titleBar.Text = browser.StatusCode;
            displayBox.Text = browser.HtmlContent;
            addressBar.Text = url;

            history.Add(url);
            BrowserStorage.SaveHistory(history);

            var links = browser.ExtractTopFiveLinks();
            linkList.Items.Clear();
            foreach (var l in links) linkList.Items.Add(l);
        }
    }
}
