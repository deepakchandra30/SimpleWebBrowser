using System.Windows.Forms;
using System.Drawing;

namespace SimpleWebBrowser
{
    public class LoginForm : Form
    {
        private TextBox usernameBox;
        private TextBox passwordBox;
        private Button loginButton;
        private Button registerButton;
        private Label statusLabel;
        private readonly DataManager storage;

        public LoginForm()
        {
            Text = "Login - Simple Web Browser";
            Size = new Size(300, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            storage = DataManager.Instance;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Username
            var userLabel = new Label
            {
                Text = "Username:",
                Location = new Point(20, 20)
            };

            usernameBox = new TextBox
            {
                Location = new Point(20, 40),
                Width = 240
            };

            // Password
            var passLabel = new Label
            {
                Text = "Password:",
                Location = new Point(20, 70)
            };

            passwordBox = new TextBox
            {
                Location = new Point(20, 90),
                Width = 240,
                PasswordChar = '•'
            };

            // Buttons
            loginButton = new Button
            {
                Text = "Login",
                Location = new Point(20, 120),
                Width = 100
            };

            registerButton = new Button
            {
                Text = "Register",
                Location = new Point(160, 120),
                Width = 100
            };

            // Status label
            statusLabel = new Label
            {
                Location = new Point(20, 150),
                Width = 240,
                ForeColor = Color.Red
            };

            // Events
            loginButton.Click += (s, e) => HandleLogin();
            registerButton.Click += (s, e) => HandleRegister();

            // Add controls
            Controls.AddRange(new Control[] {
                userLabel,
                usernameBox,
                passLabel,
                passwordBox,
                loginButton,
                registerButton,
                statusLabel
            });

            AcceptButton = loginButton;
        }

        private void HandleLogin()
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                statusLabel.Text = "Please enter both username and password";
                return;
            }

            if (storage.Login(usernameBox.Text, passwordBox.Text))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                statusLabel.Text = "Invalid username or password";
                passwordBox.Clear();
                passwordBox.Focus();
            }
        }

        private void HandleRegister()
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                statusLabel.Text = "Please enter both username and password";
                return;
            }

            if (storage.Register(usernameBox.Text, passwordBox.Text))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                statusLabel.Text = "Username already exists";
                usernameBox.Focus();
            }
        }
    }
}