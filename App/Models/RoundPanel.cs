using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Document_Frequency_Counter.App.Models
{
    public class RoundPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BorderRadius { get; set; } = 0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor { get; set; } = Color.Black;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BorderWidth { get; set; } = 2;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color FillColor { get; set; } = Color.White;

        public RoundPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // WS_EX_COMPOSITED - reduces flicker!
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (BorderRadius > 10)
                g.SmoothingMode = SmoothingMode.AntiAlias;
            else
                g.SmoothingMode = SmoothingMode.HighSpeed;

            int offset = BorderWidth / 2;

            Rectangle rectangle = new Rectangle(
                ClientRectangle.X + offset,
                ClientRectangle.Y + offset,
                ClientRectangle.Width - BorderWidth,
                ClientRectangle.Height - BorderWidth
            );

            using (GraphicsPath path = CreateRoundPath(rectangle, BorderRadius))
            using (Brush brush = new SolidBrush(FillColor))
            using (Pen pen = new(BorderColor, BorderWidth))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreateRoundPath(Rectangle r, int radius)
        {
            if (radius > r.Width / 2) radius = r.Width / 2;
            if (radius > r.Height / 2) radius = r.Height / 2;

            int x = r.X;
            int y = r.Y;
            int w = r.Width;
            int h = r.Height;
            int d = radius * 2; //Diameter

            GraphicsPath path = new();

            //Top-Left arc
            path.AddArc(x, y, d, d, 180, 90);
            //Top edge
            path.AddLine(x + radius, y, x + w - radius, y);
            //Top-Right arc
            path.AddArc(x + w - d, y, d, d, 270, 90);
            //Right edge
            path.AddLine(x + w, y + radius, x + w, y + h - radius);
            //Bottom-Right arc
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            //Bottom edg
            path.AddLine(x + w - radius, y + h, x + radius, y + h);
            //Bottom-Lef arc
            path.AddArc(x, y + h - d, d, d, 90, 90);
            //Left edge
            path.AddLine(x, y + h - radius, x, y + radius);

            return path;
        }
    }
}