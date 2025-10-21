using System.Drawing;
using System.Windows.Forms;

namespace SimpleWebBrowser
{
    public class CustomMenuRenderer : ToolStripProfessionalRenderer
    {
        private Color primaryColor;

        public CustomMenuRenderer(Color primaryColor) : base(new CustomColorTable(primaryColor))
        {
            this.primaryColor = primaryColor;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
                base.OnRenderMenuItemBackground(e);
            else
            {
                var rc = new Rectangle(Point.Empty, e.Item.Size);
                using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                    e.Graphics.FillRectangle(brush, rc);
            }
        }
    }

    public class CustomColorTable : ProfessionalColorTable
    {
        private Color primaryColor;

        public CustomColorTable(Color primaryColor)
        {
            this.primaryColor = primaryColor;
        }

        public override Color MenuItemSelected => Color.FromArgb(240, 240, 240);
        public override Color MenuItemBorder => Color.FromArgb(220, 220, 220);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(240, 240, 240);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(240, 240, 240);
        public override Color MenuBorder => Color.FromArgb(220, 220, 220);
    }
}