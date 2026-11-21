using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AIBlockerInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }

    internal enum InstallStepState
    {
        Pending,
        Active,
        Done,
        Failed
    }

    internal enum StatusKind
    {
        Idle,
        Installing,
        Error,
        Success
    }

    internal sealed class ThemePalette
    {
        public bool IsDark;
        public Color Canvas;
        public Color Rail;
        public Color Surface;
        public Color SurfaceHover;
        public Color InputBg;
        public Color BorderSubtle;
        public Color BorderStrong;
        public Color InkPrimary;
        public Color InkSecondary;
        public Color InkMuted;
        public Color PrimaryBg;
        public Color PrimaryHover;
        public Color PrimaryPressed;
        public Color PrimaryInk;
        public Color SecondaryBg;
        public Color SecondaryHover;
        public Color SecondaryPressed;
        public Color SecondaryInk;
        public Color Accent;
        public Color AccentSoft;
        public Color AccentInk;
        public Color AccentBorder;
        public Color Danger;
        public Color DangerSoft;
        public Color DangerInk;
        public Color DangerBorder;
        public Color Warning;
        public Color WarningSoft;
        public Color WarningInk;
        public Color WarningBorder;
        public Color BadgeBg;
        public Color BadgeStroke;
        public Color TrackBg;
        public Color Divider;

        public static readonly ThemePalette Light = CreateLight();
        public static readonly ThemePalette Dark = CreateDark();

        private static ThemePalette CreateLight()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = false;
            p.Canvas = Color.FromArgb(244, 242, 236);
            p.Rail = Color.FromArgb(236, 232, 223);
            p.Surface = Color.FromArgb(255, 255, 255);
            p.SurfaceHover = Color.FromArgb(250, 249, 245);
            p.InputBg = Color.FromArgb(255, 255, 255);
            p.BorderSubtle = Color.FromArgb(221, 215, 202);
            p.BorderStrong = Color.FromArgb(194, 187, 174);
            p.InkPrimary = Color.FromArgb(24, 26, 31);
            p.InkSecondary = Color.FromArgb(67, 72, 82);
            p.InkMuted = Color.FromArgb(82, 88, 100);
            p.PrimaryBg = Color.FromArgb(24, 26, 31);
            p.PrimaryHover = Color.FromArgb(44, 48, 58);
            p.PrimaryPressed = Color.FromArgb(17, 19, 22);
            p.PrimaryInk = Color.FromArgb(255, 255, 255);
            p.SecondaryBg = Color.FromArgb(255, 255, 255);
            p.SecondaryHover = Color.FromArgb(236, 232, 223);
            p.SecondaryPressed = Color.FromArgb(229, 224, 213);
            p.SecondaryInk = Color.FromArgb(24, 26, 31);
            p.Accent = Color.FromArgb(14, 98, 69);
            p.AccentSoft = Color.FromArgb(230, 243, 238);
            p.AccentInk = Color.FromArgb(10, 77, 54);
            p.AccentBorder = Color.FromArgb(179, 219, 199);
            p.Danger = Color.FromArgb(180, 35, 42);
            p.DangerSoft = Color.FromArgb(252, 236, 238);
            p.DangerInk = Color.FromArgb(142, 26, 32);
            p.DangerBorder = Color.FromArgb(240, 189, 192);
            p.Warning = Color.FromArgb(154, 91, 6);
            p.WarningSoft = Color.FromArgb(254, 245, 228);
            p.WarningInk = Color.FromArgb(122, 70, 3);
            p.WarningBorder = Color.FromArgb(234, 205, 151);
            p.BadgeBg = Color.FromArgb(24, 26, 31);
            p.BadgeStroke = Color.FromArgb(255, 255, 255);
            p.TrackBg = Color.FromArgb(229, 224, 213);
            p.Divider = Color.FromArgb(230, 225, 215);
            return p;
        }

        private static ThemePalette CreateDark()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = true;
            p.Canvas = Color.FromArgb(17, 19, 22);
            p.Rail = Color.FromArgb(24, 27, 32);
            p.Surface = Color.FromArgb(24, 27, 32);
            p.SurfaceHover = Color.FromArgb(33, 37, 44);
            p.InputBg = Color.FromArgb(29, 33, 40);
            p.BorderSubtle = Color.FromArgb(44, 50, 59);
            p.BorderStrong = Color.FromArgb(64, 72, 86);
            p.InkPrimary = Color.FromArgb(242, 240, 234);
            p.InkSecondary = Color.FromArgb(197, 201, 210);
            p.InkMuted = Color.FromArgb(156, 163, 175);
            p.PrimaryBg = Color.FromArgb(242, 240, 234);
            p.PrimaryHover = Color.FromArgb(223, 220, 211);
            p.PrimaryPressed = Color.FromArgb(207, 202, 192);
            p.PrimaryInk = Color.FromArgb(17, 19, 22);
            p.SecondaryBg = Color.FromArgb(29, 33, 40);
            p.SecondaryHover = Color.FromArgb(38, 43, 52);
            p.SecondaryPressed = Color.FromArgb(45, 51, 62);
            p.SecondaryInk = Color.FromArgb(242, 240, 234);
            p.Accent = Color.FromArgb(22, 122, 86);
            p.AccentSoft = Color.FromArgb(21, 43, 36);
            p.AccentInk = Color.FromArgb(115, 227, 183);
            p.AccentBorder = Color.FromArgb(37, 91, 71);
            p.Danger = Color.FromArgb(224, 82, 89);
            p.DangerSoft = Color.FromArgb(46, 22, 24);
            p.DangerInk = Color.FromArgb(255, 156, 159);
            p.DangerBorder = Color.FromArgb(107, 39, 43);
            p.Warning = Color.FromArgb(217, 138, 26);
            p.WarningSoft = Color.FromArgb(46, 35, 18);
            p.WarningInk = Color.FromArgb(248, 198, 112);
            p.WarningBorder = Color.FromArgb(106, 75, 28);
            p.BadgeBg = Color.FromArgb(242, 240, 234);
            p.BadgeStroke = Color.FromArgb(17, 19, 22);
            p.TrackBg = Color.FromArgb(38, 43, 52);
            p.Divider = Color.FromArgb(38, 43, 52);
            return p;
        }
    }

    internal static class UiDrawing
    {
        public static FontFamily ResolveFontFamily(string[] preferred, FontFamily fallback)
        {
            FontFamily[] installed = FontFamily.Families;
            for (int i = 0; i < preferred.Length; i++)
            {
                string name = preferred[i];
                for (int j = 0; j < installed.Length; j++)
                {
                    if (string.Equals(installed[j].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return installed[j];
                    }
                }
            }
            return fallback;
        }

        public static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0f || rect.Width <= 0f || rect.Height <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }
            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawShieldMark(Graphics g, Rectangle bounds, Color badgeBg, Color strokeColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF plateRect = new RectangleF(bounds.X, bounds.Y, bounds.Width - 1f, bounds.Height - 1f);
            float r = Math.Max(4f, bounds.Width * 0.22f);
            using (GraphicsPath plate = CreateRoundedRectPath(plateRect, r))
            using (SolidBrush bgBrush = new SolidBrush(badgeBg))
            {
                g.FillPath(bgBrush, plate);
            }

            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            float sx = bounds.Width * 0.25f;
            float topY = cy - bounds.Height * 0.27f;
            float midY = cy - bounds.Height * 0.16f;
            float lowY = cy + bounds.Height * 0.04f;
            float tipY = cy + bounds.Height * 0.29f;

            using (GraphicsPath shield = new GraphicsPath())
            {
                shield.StartFigure();
                shield.AddLine(cx, topY, cx + sx, midY);
                shield.AddLine(cx + sx, midY, cx + sx, lowY);
                shield.AddBezier(
                    cx + sx, lowY,
                    cx + sx, cy + bounds.Height * 0.19f,
                    cx + sx * 0.35f, cy + bounds.Height * 0.26f,
                    cx, tipY
                );
                shield.AddBezier(
                    cx, tipY,
                    cx - sx * 0.35f, cy + bounds.Height * 0.26f,
                    cx - sx, cy + bounds.Height * 0.19f,
                    cx - sx, lowY
                );
                shield.AddLine(cx - sx, lowY, cx - sx, midY);
                shield.CloseFigure();

                float strokeW = Math.Max(1.4f, bounds.Width * 0.052f);
                using (Pen pen = new Pen(strokeColor, strokeW))
                {
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, shield);
                }
            }
        }

        public static void DrawCheckMark(Graphics g, float x, float y, float size, Color color, float thickness)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, thickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                PointF p1 = new PointF(x + size * 0.22f, y + size * 0.53f);
                PointF p2 = new PointF(x + size * 0.43f, y + size * 0.74f);
                PointF p3 = new PointF(x + size * 0.79f, y + size * 0.29f);
                g.DrawLines(pen, new PointF[] { p1, p2, p3 });
            }
        }

        public static void DrawMoonOrSunGlyph(Graphics g, float cx, float cy, bool isDarkTheme, Color color, Color bgFill)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (!isDarkTheme)
            {
                // Anti-aliased moon crescent for "Mode Gelap"
                using (SolidBrush fgBrush = new SolidBrush(color))
                using (SolidBrush cutBrush = new SolidBrush(bgFill))
                {
                    g.FillEllipse(fgBrush, cx - 5.2f, cy - 5.2f, 10.4f, 10.4f);
                    g.FillEllipse(cutBrush, cx - 1.5f, cy - 6.8f, 9.2f, 9.2f);
                }
            }
            else
            {
                // Sun outline + rays for "Mode Terang"
                using (Pen p = new Pen(color, 1.4f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawEllipse(p, cx - 2.6f, cy - 2.6f, 5.2f, 5.2f);
                    for (int i = 0; i < 8; i++)
                    {
                        double a = (i * Math.PI) / 4.0;
                        float x1 = cx + (float)(Math.Cos(a) * 4.3);
                        float y1 = cy + (float)(Math.Sin(a) * 4.3);
                        float x2 = cx + (float)(Math.Cos(a) * 5.8);
                        float y2 = cy + (float)(Math.Sin(a) * 5.8);
                        g.DrawLine(p, x1, y1, x2, y2);
                    }
                }
            }
        }
    }

    internal sealed class EditorialButton : Button
    {
        public bool IsPrimary;
        public bool IsThemeToggle;
        public ThemePalette Theme;
        private bool isHover;
        private bool isPressed;

        public EditorialButton()
        {
            Theme = ThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled)
            {
                isHover = false;
                isPressed = false;
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled)
            {
                isHover = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (Enabled && mevent.Button == MouseButtons.Left)
            {
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent != null ? Parent.BackColor : Theme.Canvas;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            Color fill;
            Color fg;
            Color border = Color.Transparent;

            if (IsPrimary)
            {
                if (!Enabled)
                {
                    fill = Theme.IsDark ? Color.FromArgb(66, 72, 84) : Color.FromArgb(148, 154, 166);
                    fg = Theme.IsDark ? Color.FromArgb(180, 185, 195) : Color.FromArgb(250, 249, 245);
                }
                else if (isPressed)
                {
                    fill = Theme.PrimaryPressed;
                    fg = Theme.PrimaryInk;
                }
                else if (isHover)
                {
                    fill = Theme.PrimaryHover;
                    fg = Theme.PrimaryInk;
                }
                else
                {
                    fill = Theme.PrimaryBg;
                    fg = Theme.PrimaryInk;
                }
            }
            else
            {
                if (!Enabled)
                {
                    fill = Theme.SurfaceHover;
                    fg = Theme.InkMuted;
                    border = Theme.BorderSubtle;
                }
                else if (isPressed)
                {
                    fill = Theme.SecondaryPressed;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
                else if (isHover)
                {
                    fill = Theme.SecondaryHover;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
                else
                {
                    fill = Theme.SecondaryBg;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
            }

            RectangleF rect = new RectangleF(1f, 1f, Width - 3f, Height - 3f);
            using (GraphicsPath path = UiDrawing.CreateRoundedRectPath(rect, 7f))
            using (SolidBrush bgBrush = new SolidBrush(fill))
            {
                g.FillPath(bgBrush, path);
                if (!IsPrimary && border != Color.Transparent)
                {
                    using (Pen pen = new Pen(border, 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            if (Focused && ShowFocusCues)
            {
                RectangleF focusRect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
                Color focusColor = Theme.IsDark ? Theme.AccentInk : Theme.Accent;
                using (GraphicsPath focusPath = UiDrawing.CreateRoundedRectPath(focusRect, 8f))
                using (Pen focusPen = new Pen(focusColor, 1.8f))
                {
                    g.DrawPath(focusPen, focusPath);
                }
            }

            if (IsThemeToggle)
            {
                float glyphX = 18f;
                float glyphY = Height / 2f;
                UiDrawing.DrawMoonOrSunGlyph(g, glyphX, glyphY, Theme.IsDark, fg, fill);
                Rectangle textRect = new Rectangle(28, 0, Width - 34, Height);
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
                );
            }
            else
            {
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    ClientRectangle,
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
                );
            }
        }
    }

    internal sealed class EditorialCheckBox : CheckBox
    {
        public string Subtitle;
        public bool DrawBottomDivider;
        public ThemePalette Theme;
        public Font SubtitleFont;
        private bool isHover;

        public EditorialCheckBox()
        {
            Theme = ThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            AutoSize = false;
            Height = 52;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled) isHover = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled)
            {
                isHover = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color bg = (isHover && Enabled) ? Theme.SurfaceHover : Theme.Surface;
            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            RectangleF boxRect = new RectangleF(14f, 17f, 18f, 18f);
            using (GraphicsPath boxPath = UiDrawing.CreateRoundedRectPath(boxRect, 4.5f))
            {
                if (Checked)
                {
                    Color fill = Enabled
                        ? Theme.Accent
                        : (Theme.IsDark ? Color.FromArgb(42, 82, 66) : Color.FromArgb(148, 184, 170));
                    using (SolidBrush boxBrush = new SolidBrush(fill))
                    {
                        g.FillPath(boxBrush, boxPath);
                    }
                    UiDrawing.DrawCheckMark(g, 14f, 17f, 18f, Color.White, 2f);
                }
                else
                {
                    using (SolidBrush boxBrush = new SolidBrush(Theme.InputBg))
                    {
                        g.FillPath(boxBrush, boxPath);
                    }
                    Color boxBorder = Enabled ? Theme.BorderStrong : Theme.BorderSubtle;
                    using (Pen boxPen = new Pen(boxBorder, 1.3f))
                    {
                        g.DrawPath(boxPen, boxPath);
                    }
                }
            }

            Color titleColor = Enabled ? Theme.InkPrimary : Theme.InkMuted;
            Color subColor = Theme.InkMuted;

            Rectangle titleRect = new Rectangle(44, 8, Width - 56, 19);
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                titleRect,
                titleColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            if (!string.IsNullOrEmpty(Subtitle))
            {
                Font subFont = SubtitleFont != null ? SubtitleFont : Font;
                Rectangle subRect = new Rectangle(44, 27, Width - 56, 17);
                TextRenderer.DrawText(
                    g,
                    Subtitle,
                    subFont,
                    subRect,
                    subColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }

            if (DrawBottomDivider)
            {
                using (Pen divPen = new Pen(Theme.Divider, 1f))
                {
                    g.DrawLine(divPen, 14, Height - 1, Width - 14, Height - 1);
                }
            }

            if (Focused && ShowFocusCues)
            {
                RectangleF focusRect = new RectangleF(2f, 2f, Width - 5f, Height - 5f);
                Color focusColor = Theme.IsDark ? Theme.AccentInk : Theme.Accent;
                using (GraphicsPath focusPath = UiDrawing.CreateRoundedRectPath(focusRect, 5f))
                using (Pen focusPen = new Pen(focusColor, 1.4f))
                {
                    focusPen.DashStyle = DashStyle.Solid;
                    g.DrawPath(focusPen, focusPath);
                }
            }
        }
    }

    internal sealed class EditorialCardPanel : Panel
    {
        public float CornerRadius;
        public Color FillColor;
        public Color BorderColor;
        public Color FocusedBorderColor;
        public bool IsFocusedBorder;

        public EditorialCardPanel()
        {
            CornerRadius = 10f;
            FillColor = Color.White;
            BorderColor = Color.LightGray;
            FocusedBorderColor = Color.SeaGreen;
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBg = Parent != null ? Parent.BackColor : FillColor;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath path = UiDrawing.CreateRoundedRectPath(rect, CornerRadius))
            using (SolidBrush bgBrush = new SolidBrush(FillColor))
            {
                g.FillPath(bgBrush, path);
                Color bCol = IsFocusedBorder ? FocusedBorderColor : BorderColor;
                float bW = IsFocusedBorder ? 1.5f : 1f;
                using (Pen pen = new Pen(bCol, bW))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }

    internal sealed class EditorialStatusPanel : Control
    {
        public ThemePalette Theme;
        public StatusKind Kind;
        public string Title;
        public string Detail;
        public int Percent;
        public Font TitleFont;
        public Font DetailFont;
        public Font MonoFont;

        public EditorialStatusPanel()
        {
            Theme = ThemePalette.Light;
            Kind = StatusKind.Idle;
            Title = string.Empty;
            Detail = string.Empty;
            Percent = 0;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent != null ? Parent.BackColor : Theme.Canvas;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            Color fill = Theme.Surface;
            Color border = Theme.BorderSubtle;
            if (Kind == StatusKind.Error)
            {
                fill = Theme.DangerSoft;
                border = Theme.DangerBorder;
            }
            else if (Kind == StatusKind.Success)
            {
                fill = Theme.AccentSoft;
                border = Theme.AccentBorder;
            }

            RectangleF cardRect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath cardPath = UiDrawing.CreateRoundedRectPath(cardRect, 9f))
            using (SolidBrush bgBrush = new SolidBrush(fill))
            using (Pen borderPen = new Pen(border, 1f))
            {
                g.FillPath(bgBrush, cardPath);
                g.DrawPath(borderPen, cardPath);
            }

            Font tf = TitleFont != null ? TitleFont : Font;
            Font df = DetailFont != null ? DetailFont : Font;
            Font mf = MonoFont != null ? MonoFont : Font;

            if (Kind == StatusKind.Idle)
            {
                using (SolidBrush dotBrush = new SolidBrush(Theme.Accent))
                {
                    g.FillEllipse(dotBrush, 16f, 17f, 8f, 8f);
                }
                TextRenderer.DrawText(
                    g,
                    Title,
                    tf,
                    new Rectangle(32, 11, Width - 46, 20),
                    Theme.InkPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    Detail,
                    df,
                    new Rectangle(32, 32, Width - 46, 24),
                    Theme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
            else if (Kind == StatusKind.Installing)
            {
                string pctText = string.Format("{0}%", Math.Max(0, Math.Min(100, Percent)));
                TextRenderer.DrawText(
                    g,
                    Title,
                    tf,
                    new Rectangle(16, 11, Width - 84, 20),
                    Theme.InkPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    pctText,
                    mf,
                    new Rectangle(Width - 68, 11, 52, 20),
                    Theme.AccentInk,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                );

                RectangleF trackRect = new RectangleF(16f, 39f, Width - 32f, 7f);
                using (GraphicsPath trackPath = UiDrawing.CreateRoundedRectPath(trackRect, 3.5f))
                using (SolidBrush trackBrush = new SolidBrush(Theme.TrackBg))
                {
                    g.FillPath(trackBrush, trackPath);
                }

                float ratio = Math.Max(0.02f, Math.Min(1f, Percent / 100f));
                float fillW = Math.Max(7f, (Width - 32f) * ratio);
                RectangleF barRect = new RectangleF(16f, 39f, fillW, 7f);
                using (GraphicsPath barPath = UiDrawing.CreateRoundedRectPath(barRect, 3.5f))
                using (SolidBrush barBrush = new SolidBrush(Theme.Accent))
                {
                    g.FillPath(barBrush, barPath);
                }
            }
            else if (Kind == StatusKind.Error)
            {
                using (SolidBrush badgeBrush = new SolidBrush(Theme.Danger))
                {
                    g.FillEllipse(badgeBrush, 15f, 13f, 18f, 18f);
                }
                using (Pen exPen = new Pen(Color.White, 2f))
                {
                    exPen.StartCap = LineCap.Round;
                    exPen.EndCap = LineCap.Round;
                    g.DrawLine(exPen, 24f, 17f, 24f, 23f);
                    using (SolidBrush dotBrush = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(dotBrush, 22.8f, 25.3f, 2.4f, 2.4f);
                    }
                }
                TextRenderer.DrawText(
                    g,
                    Title,
                    tf,
                    new Rectangle(42, 11, Width - 56, 20),
                    Theme.DangerInk,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    Detail,
                    df,
                    new Rectangle(42, 31, Width - 56, 30),
                    Theme.DangerInk,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
            else if (Kind == StatusKind.Success)
            {
                using (SolidBrush badgeBrush = new SolidBrush(Theme.Accent))
                {
                    g.FillEllipse(badgeBrush, 15f, 13f, 18f, 18f);
                }
                UiDrawing.DrawCheckMark(g, 15f, 13f, 18f, Color.White, 2f);
                TextRenderer.DrawText(
                    g,
                    Title,
                    tf,
                    new Rectangle(42, 11, Width - 56, 20),
                    Theme.AccentInk,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    Detail,
                    df,
                    new Rectangle(42, 31, Width - 56, 30),
                    Theme.AccentInk,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
        }
    }

    public class SetupForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private ThemePalette currentTheme = ThemePalette.Light;

        private FontFamily uiFamily;
        private FontFamily semiboldFamily;
        private FontFamily monoFamily;

        private Font fBrandTitle;
        private Font fHeading;
        private Font fButton;
        private Font fSectionLabel;
        private Font fLabel;
        private Font fSubtle;
        private Font fMicro;
        private Font fMono;
        private Font fMonoSmall;
        private Icon appIcon;

        private Panel pnlRail;
        private Panel pnlMain;
        private Panel pnlFooter;

        private Label lblHeading;
        private Label lblSubtitle;
        private EditorialButton btnThemeToggle;

        private Label lblDir;
        private EditorialCardPanel pnlDirBox;
        private TextBox txtInstallDir;
        private EditorialButton btnBrowse;

        private Label lblOptions;
        private EditorialCardPanel pnlOptionsCard;
        private EditorialCheckBox chkAutoStart;
        private EditorialCheckBox chkDesktopShortcut;
        private EditorialCheckBox chkLaunchAfter;

        private EditorialStatusPanel statusPanel;
        private EditorialButton btnCancel;
        private EditorialButton btnInstall;
        private ToolTip statusTip;

        private bool isInstalling;
        private bool isCompleted;
        private int activeStepIndex = -1;
        private readonly InstallStepState[] stepStates = new InstallStepState[]
        {
            InstallStepState.Pending,
            InstallStepState.Pending,
            InstallStepState.Pending,
            InstallStepState.Pending
        };

        private string[] componentSizes;
        private bool isAdministrator;
        private string machineName;

        private static readonly string[] ComponentNames = new string[]
        {
            "AIBlockerService.exe",
            "AIBlockerGuard.exe",
            "Restore-Normal.bat"
        };

        private static readonly string[] ComponentDescs = new string[]
        {
            "Layanan Windows (LocalSystem)",
            "Agen Monitoring Endpoint",
            "Pemulihan Darurat Lokal"
        };

        private static readonly string[] StepLabels = new string[]
        {
            "Hentikan proses & layanan lama",
            "Ekstrak biner & izin sistem",
            "Daftarkan Windows Service",
            "Atur startup & pintasan"
        };

        public SetupForm()
        {
            InitializeMetadata();
            InitializeFontsAndIcon();
            InitializeLayout();
            ApplyTheme(ThemePalette.Light);
            SetIdleStatus();
        }

        private void InitializeMetadata()
        {
            isAdministrator = CheckIsAdministrator();
            machineName = Environment.MachineName;

            Assembly asm = Assembly.GetExecutingAssembly();
            string svcSize = ReadResourceSizeLabel(asm, "AIBlockerService.exe");
            string guardSize = ReadResourceSizeLabel(asm, "AIBlockerGuard.exe");
            componentSizes = new string[] { svcSize, guardSize, "Skrip" };
        }

        private static bool CheckIsAdministrator()
        {
            try
            {
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(id);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string ReadResourceSizeLabel(Assembly asm, string resourceName)
        {
            try
            {
                using (Stream s = asm.GetManifestResourceStream(resourceName))
                {
                    if (s == null || s.Length <= 0)
                    {
                        return "Kosong";
                    }
                    long kb = Math.Max(1L, (s.Length + 1023L) / 1024L);
                    return string.Format("{0} KB", kb);
                }
            }
            catch
            {
                return "Kosong";
            }
        }

        private void InitializeFontsAndIcon()
        {
            uiFamily = UiDrawing.ResolveFontFamily(new string[] { "Segoe UI", "Microsoft Sans Serif" }, FontFamily.GenericSansSerif);
            semiboldFamily = UiDrawing.ResolveFontFamily(new string[] { "Segoe UI Semibold", "Segoe UI" }, uiFamily);
            monoFamily = UiDrawing.ResolveFontFamily(new string[] { "Consolas", "Cascadia Mono", "Courier New" }, FontFamily.GenericMonospace);

            fBrandTitle = new Font(uiFamily, 12.2f, FontStyle.Bold);
            fHeading = new Font(uiFamily, 15f, FontStyle.Bold);
            fButton = new Font(semiboldFamily, 9.2f, FontStyle.Bold);
            fSectionLabel = new Font(semiboldFamily, 7.6f, FontStyle.Bold);
            fLabel = new Font(semiboldFamily, 8.8f, FontStyle.Bold);
            fSubtle = new Font(uiFamily, 8.2f, FontStyle.Regular);
            fMicro = new Font(uiFamily, 7.6f, FontStyle.Regular);
            fMono = new Font(monoFamily, 9.5f, FontStyle.Regular);
            fMonoSmall = new Font(monoFamily, 8f, FontStyle.Regular);

            appIcon = CreateGuardShieldIcon();
            if (appIcon != null)
            {
                this.Icon = appIcon;
            }
        }

        private static Icon CreateGuardShieldIcon()
        {
            Bitmap bmp = null;
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    UiDrawing.DrawShieldMark(g, new Rectangle(1, 1, 30, 30), Color.FromArgb(24, 26, 31), Color.White);
                }
                hIcon = bmp.GetHicon();
                using (Icon temp = Icon.FromHandle(hIcon))
                {
                    return (Icon)temp.Clone();
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                {
                    DestroyIcon(hIcon);
                }
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }
        }

        private void InitializeLayout()
        {
            this.Name = "SetupForm";
            this.Text = "GuardAgent v1.6.0 - Setup Instalasi Perangkat";
            this.ClientSize = new Size(760, 520);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.AutoScaleMode = AutoScaleMode.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Font = new Font(uiFamily, 9f, FontStyle.Regular);

            statusTip = new ToolTip();

            // Left Institutional Rail
            pnlRail = new DoubleBufferedPanel();
            pnlRail.Name = "pnlRail";
            pnlRail.Location = new Point(0, 0);
            pnlRail.Size = new Size(244, 520);
            pnlRail.Paint += PnlRail_Paint;
            this.Controls.Add(pnlRail);

            // Right Configuration Workspace
            pnlMain = new Panel();
            pnlMain.Name = "pnlMain";
            pnlMain.Location = new Point(244, 0);
            pnlMain.Size = new Size(516, 520);
            this.Controls.Add(pnlMain);

            // Header
            lblHeading = new Label();
            lblHeading.Name = "lblHeading";
            lblHeading.Text = "Pasang GuardAgent";
            lblHeading.Font = fHeading;
            lblHeading.Location = new Point(28, 20);
            lblHeading.Size = new Size(340, 28);
            pnlMain.Controls.Add(lblHeading);

            lblSubtitle = new Label();
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Text = "Atur direktori dan opsi agen sebelum pemasangan dijalankan.";
            lblSubtitle.Font = fSubtle;
            lblSubtitle.Location = new Point(28, 50);
            lblSubtitle.Size = new Size(340, 20);
            pnlMain.Controls.Add(lblSubtitle);

            btnThemeToggle = new EditorialButton();
            btnThemeToggle.Name = "btnThemeToggle";
            btnThemeToggle.IsThemeToggle = true;
            btnThemeToggle.Text = "Mode Gelap";
            btnThemeToggle.Font = new Font(semiboldFamily, 8.2f, FontStyle.Bold);
            btnThemeToggle.Location = new Point(376, 24);
            btnThemeToggle.Size = new Size(112, 34);
            btnThemeToggle.TabIndex = 7;
            btnThemeToggle.AccessibleName = "Ganti tema terang atau gelap";
            btnThemeToggle.Click += BtnThemeToggle_Click;
            pnlMain.Controls.Add(btnThemeToggle);

            // Directory Section
            lblDir = new Label();
            lblDir.Name = "lblDir";
            lblDir.Text = "Direktori Instalasi";
            lblDir.Font = fLabel;
            lblDir.Location = new Point(28, 90);
            lblDir.Size = new Size(260, 18);
            pnlMain.Controls.Add(lblDir);

            pnlDirBox = new EditorialCardPanel();
            pnlDirBox.Name = "pnlDirBox";
            pnlDirBox.CornerRadius = 7f;
            pnlDirBox.Location = new Point(28, 111);
            pnlDirBox.Size = new Size(358, 38);
            pnlDirBox.Click += delegate(object s, EventArgs e)
            {
                if (txtInstallDir.Enabled) txtInstallDir.Focus();
            };
            pnlMain.Controls.Add(pnlDirBox);

            txtInstallDir = new TextBox();
            txtInstallDir.Name = "txtInstallDir";
            txtInstallDir.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AIBlockerGuard");
            txtInstallDir.BorderStyle = BorderStyle.None;
            txtInstallDir.Font = fMono;
            int dirTextHeight = Math.Max(20, txtInstallDir.PreferredHeight);
            txtInstallDir.Location = new Point(12, Math.Max(4, (pnlDirBox.Height - dirTextHeight) / 2));
            txtInstallDir.Size = new Size(334, dirTextHeight);
            txtInstallDir.TabIndex = 0;
            txtInstallDir.AccessibleName = "Direktori Instalasi";
            txtInstallDir.Enter += delegate(object s, EventArgs e)
            {
                pnlDirBox.IsFocusedBorder = true;
                pnlDirBox.Invalidate();
            };
            txtInstallDir.Leave += delegate(object s, EventArgs e)
            {
                pnlDirBox.IsFocusedBorder = false;
                pnlDirBox.Invalidate();
            };
            txtInstallDir.TextChanged += delegate(object s, EventArgs e)
            {
                if (!isInstalling && !isCompleted && statusPanel.Kind == StatusKind.Error)
                {
                    SetIdleStatus();
                }
            };
            pnlDirBox.Controls.Add(txtInstallDir);

            btnBrowse = new EditorialButton();
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Text = "Pilih...";
            btnBrowse.Font = fButton;
            btnBrowse.Location = new Point(394, 111);
            btnBrowse.Size = new Size(94, 38);
            btnBrowse.TabIndex = 1;
            btnBrowse.AccessibleName = "Pilih direktori instalasi";
            btnBrowse.Click += BtnBrowse_Click;
            pnlMain.Controls.Add(btnBrowse);

            // Options Section
            lblOptions = new Label();
            lblOptions.Name = "lblOptions";
            lblOptions.Text = "Opsi Konfigurasi";
            lblOptions.Font = fLabel;
            lblOptions.Location = new Point(28, 162);
            lblOptions.Size = new Size(260, 18);
            pnlMain.Controls.Add(lblOptions);

            pnlOptionsCard = new EditorialCardPanel();
            pnlOptionsCard.Name = "pnlOptionsCard";
            pnlOptionsCard.CornerRadius = 10f;
            pnlOptionsCard.Location = new Point(28, 184);
            pnlOptionsCard.Size = new Size(460, 168);
            pnlMain.Controls.Add(pnlOptionsCard);

            chkAutoStart = new EditorialCheckBox();
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Text = "Aktif otomatis saat Windows menyala";
            chkAutoStart.Subtitle = "Pasang entri Registry Run dan folder Startup untuk semua pengguna";
            chkAutoStart.Checked = true;
            chkAutoStart.DrawBottomDivider = true;
            chkAutoStart.Font = fLabel;
            chkAutoStart.SubtitleFont = fMicro;
            chkAutoStart.Location = new Point(3, 6);
            chkAutoStart.Size = new Size(454, 52);
            chkAutoStart.TabIndex = 2;
            chkAutoStart.AccessibleName = chkAutoStart.Text;
            pnlOptionsCard.Controls.Add(chkAutoStart);

            chkDesktopShortcut = new EditorialCheckBox();
            chkDesktopShortcut.Name = "chkDesktopShortcut";
            chkDesktopShortcut.Text = "Buat pintasan pada layar Desktop";
            chkDesktopShortcut.Subtitle = "Tambahkan GuardAgent.lnk untuk akses cepat pengawas";
            chkDesktopShortcut.Checked = true;
            chkDesktopShortcut.DrawBottomDivider = true;
            chkDesktopShortcut.Font = fLabel;
            chkDesktopShortcut.SubtitleFont = fMicro;
            chkDesktopShortcut.Location = new Point(3, 58);
            chkDesktopShortcut.Size = new Size(454, 52);
            chkDesktopShortcut.TabIndex = 3;
            chkDesktopShortcut.AccessibleName = chkDesktopShortcut.Text;
            pnlOptionsCard.Controls.Add(chkDesktopShortcut);

            chkLaunchAfter = new EditorialCheckBox();
            chkLaunchAfter.Name = "chkLaunchAfter";
            chkLaunchAfter.Text = "Jalankan agen setelah instalasi selesai";
            chkLaunchAfter.Subtitle = "Buka AIBlockerGuard.exe segera setelah pemasangan berhasil";
            chkLaunchAfter.Checked = true;
            chkLaunchAfter.DrawBottomDivider = false;
            chkLaunchAfter.Font = fLabel;
            chkLaunchAfter.SubtitleFont = fMicro;
            chkLaunchAfter.Location = new Point(3, 110);
            chkLaunchAfter.Size = new Size(454, 52);
            chkLaunchAfter.TabIndex = 4;
            chkLaunchAfter.AccessibleName = chkLaunchAfter.Text;
            pnlOptionsCard.Controls.Add(chkLaunchAfter);

            // Status Panel
            statusPanel = new EditorialStatusPanel();
            statusPanel.Name = "statusPanel";
            statusPanel.Location = new Point(28, 368);
            statusPanel.Size = new Size(460, 68);
            statusPanel.TitleFont = fLabel;
            statusPanel.DetailFont = fMicro;
            statusPanel.MonoFont = fMonoSmall;
            pnlMain.Controls.Add(statusPanel);

            // Footer Bar
            pnlFooter = new DoubleBufferedPanel();
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Location = new Point(0, 452);
            pnlFooter.Size = new Size(516, 68);
            pnlFooter.Paint += PnlFooter_Paint;
            pnlMain.Controls.Add(pnlFooter);

            btnCancel = new EditorialButton();
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "Batal";
            btnCancel.Font = fButton;
            btnCancel.Location = new Point(28, 15);
            btnCancel.Size = new Size(104, 38);
            btnCancel.TabIndex = 6;
            btnCancel.AccessibleName = "Batalkan dan tutup setup";
            btnCancel.Click += BtnCancel_Click;
            pnlFooter.Controls.Add(btnCancel);

            btnInstall = new EditorialButton();
            btnInstall.Name = "btnInstall";
            btnInstall.IsPrimary = true;
            btnInstall.Text = "Pasang Agen";
            btnInstall.Font = fButton;
            btnInstall.Location = new Point(324, 15);
            btnInstall.Size = new Size(164, 38);
            btnInstall.TabIndex = 5;
            btnInstall.AccessibleName = "Pasang Agen GuardAgent";
            btnInstall.Click += BtnInstall_Click;
            pnlFooter.Controls.Add(btnInstall);

            this.AcceptButton = btnInstall;
            this.CancelButton = btnCancel;
        }

        public void SetTheme(bool dark)
        {
            ApplyTheme(dark ? ThemePalette.Dark : ThemePalette.Light);
        }

        private void ApplyTheme(ThemePalette theme)
        {
            currentTheme = theme;

            this.BackColor = theme.Canvas;
            this.ForeColor = theme.InkPrimary;

            pnlRail.BackColor = theme.Rail;
            pnlMain.BackColor = theme.Canvas;
            pnlFooter.BackColor = theme.Surface;

            lblHeading.ForeColor = theme.InkPrimary;
            lblHeading.BackColor = theme.Canvas;
            lblSubtitle.ForeColor = theme.InkMuted;
            lblSubtitle.BackColor = theme.Canvas;

            btnThemeToggle.Theme = theme;
            btnThemeToggle.Text = theme.IsDark ? "Mode Terang" : "Mode Gelap";
            btnThemeToggle.Invalidate();

            lblDir.ForeColor = theme.InkSecondary;
            lblDir.BackColor = theme.Canvas;

            pnlDirBox.FillColor = theme.InputBg;
            pnlDirBox.BorderColor = theme.BorderStrong;
            pnlDirBox.FocusedBorderColor = theme.Accent;
            pnlDirBox.Invalidate();

            txtInstallDir.BackColor = theme.InputBg;
            txtInstallDir.ForeColor = theme.InkPrimary;

            btnBrowse.Theme = theme;
            btnBrowse.Invalidate();

            lblOptions.ForeColor = theme.InkSecondary;
            lblOptions.BackColor = theme.Canvas;

            pnlOptionsCard.FillColor = theme.Surface;
            pnlOptionsCard.BorderColor = theme.BorderSubtle;
            pnlOptionsCard.Invalidate();

            chkAutoStart.Theme = theme;
            chkAutoStart.Invalidate();
            chkDesktopShortcut.Theme = theme;
            chkDesktopShortcut.Invalidate();
            chkLaunchAfter.Theme = theme;
            chkLaunchAfter.Invalidate();

            statusPanel.Theme = theme;
            statusPanel.Invalidate();

            btnCancel.Theme = theme;
            btnCancel.Invalidate();
            btnInstall.Theme = theme;
            btnInstall.Invalidate();

            pnlRail.Invalidate();
            pnlFooter.Invalidate();
        }

        private void PnlFooter_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(currentTheme.BorderSubtle, 1f))
            {
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            }
        }

        private void PnlRail_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            ThemePalette t = currentTheme;

            // Vertical divider hairline on right edge
            using (Pen dividerPen = new Pen(t.BorderSubtle, 1f))
            {
                g.DrawLine(dividerPen, pnlRail.Width - 1, 0, pnlRail.Width - 1, pnlRail.Height);
            }

            // Brand Shield + Title + Version Tag
            UiDrawing.DrawShieldMark(g, new Rectangle(22, 22, 38, 38), t.BadgeBg, t.BadgeStroke);

            string brandTitle = "GuardAgent";
            Size measuredTitle = TextRenderer.MeasureText(
                g,
                brandTitle,
                fBrandTitle,
                new Size(140, 30),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );

            TextRenderer.DrawText(
                g,
                brandTitle,
                fBrandTitle,
                new Rectangle(70, 20, 120, 24),
                t.InkPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );

            string verText = "v1.6.0";
            int badgeX = 70 + measuredTitle.Width + 6;
            RectangleF verRect = new RectangleF(badgeX, 23f, 44f, 18f);
            if (verRect.Right <= 226f)
            {
                using (GraphicsPath verPath = UiDrawing.CreateRoundedRectPath(verRect, 4f))
                using (SolidBrush verBg = new SolidBrush(t.Surface))
                using (Pen verBorder = new Pen(t.BorderStrong, 1f))
                {
                    g.FillPath(verBg, verPath);
                    g.DrawPath(verBorder, verPath);
                }
                TextRenderer.DrawText(
                    g,
                    verText,
                    fMonoSmall,
                    new Rectangle((int)verRect.X, (int)verRect.Y, (int)verRect.Width, (int)verRect.Height),
                    t.InkSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
                );
            }

            TextRenderer.DrawText(
                g,
                "Installer Perangkat Ujian",
                fSubtle,
                new Rectangle(70, 44, 156, 18),
                t.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            // Section Divider 1
            using (Pen hr = new Pen(t.BorderSubtle, 1f))
            {
                g.DrawLine(hr, 22, 78, 222, 78);
            }

            // Section: KOMPONEN TERPAKET
            TextRenderer.DrawText(
                g,
                "KOMPONEN TERPAKET",
                fSectionLabel,
                new Rectangle(22, 90, 200, 16),
                t.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );

            for (int i = 0; i < ComponentNames.Length; i++)
            {
                int rowY = 112 + (i * 39);
                string sizeStr = componentSizes[i];
                Size szSize = TextRenderer.MeasureText(g, sizeStr, fMonoSmall, new Size(70, 18), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int nameW = Math.Max(80, 200 - szSize.Width - 6);

                TextRenderer.DrawText(
                    g,
                    ComponentNames[i],
                    fLabel,
                    new Rectangle(22, rowY, nameW, 17),
                    t.InkPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );

                TextRenderer.DrawText(
                    g,
                    sizeStr,
                    fMonoSmall,
                    new Rectangle(22 + nameW, rowY + 1, 200 - nameW, 16),
                    t.InkMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
                );

                TextRenderer.DrawText(
                    g,
                    ComponentDescs[i],
                    fMicro,
                    new Rectangle(22, rowY + 17, 200, 15),
                    t.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }

            // Section Divider 2
            using (Pen hr = new Pen(t.BorderSubtle, 1f))
            {
                g.DrawLine(hr, 22, 236, 222, 236);
            }

            // Section: TAHAPAN INSTALASI
            TextRenderer.DrawText(
                g,
                "TAHAPAN INSTALASI",
                fSectionLabel,
                new Rectangle(22, 248, 200, 16),
                t.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );

            for (int i = 0; i < StepLabels.Length; i++)
            {
                int rowY = 271 + (i * 38);
                InstallStepState state = stepStates[i];

                RectangleF stepBox = new RectangleF(22f, rowY + 1f, 22f, 22f);
                Color badgeFill = t.Surface;
                Color badgeBorder = t.BorderStrong;
                Color badgeInk = t.InkMuted;
                Color labelInk = t.InkSecondary;

                if (state == InstallStepState.Active)
                {
                    badgeFill = t.AccentSoft;
                    badgeBorder = t.Accent;
                    badgeInk = t.AccentInk;
                    labelInk = t.InkPrimary;
                }
                else if (state == InstallStepState.Done)
                {
                    badgeFill = t.Accent;
                    badgeBorder = t.Accent;
                    badgeInk = Color.White;
                    labelInk = t.InkPrimary;
                }
                else if (state == InstallStepState.Failed)
                {
                    badgeFill = t.Danger;
                    badgeBorder = t.Danger;
                    badgeInk = Color.White;
                    labelInk = t.DangerInk;
                }

                using (GraphicsPath stepPath = UiDrawing.CreateRoundedRectPath(stepBox, 5.5f))
                using (SolidBrush sb = new SolidBrush(badgeFill))
                using (Pen sp = new Pen(badgeBorder, 1f))
                {
                    g.FillPath(sb, stepPath);
                    g.DrawPath(sp, stepPath);
                }

                if (state == InstallStepState.Done)
                {
                    UiDrawing.DrawCheckMark(g, 24f, rowY + 3f, 18f, Color.White, 1.9f);
                }
                else if (state == InstallStepState.Failed)
                {
                    using (Pen xp = new Pen(Color.White, 1.8f))
                    {
                        xp.StartCap = LineCap.Round;
                        xp.EndCap = LineCap.Round;
                        g.DrawLine(xp, 29f, rowY + 8f, 37f, rowY + 16f);
                        g.DrawLine(xp, 37f, rowY + 8f, 29f, rowY + 16f);
                    }
                }
                else
                {
                    string numStr = string.Format("0{0}", i + 1);
                    TextRenderer.DrawText(
                        g,
                        numStr,
                        fMonoSmall,
                        new Rectangle(22, rowY + 1, 22, 22),
                        badgeInk,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
                    );
                }

                Font stFont = (state == InstallStepState.Active || state == InstallStepState.Done) ? fLabel : fSubtle;
                TextRenderer.DrawText(
                    g,
                    StepLabels[i],
                    stFont,
                    new Rectangle(52, rowY + 2, 174, 20),
                    labelInk,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }

            // Environment Readout Box
            RectangleF envRect = new RectangleF(22f, 424f, 200f, 62f);
            using (GraphicsPath envPath = UiDrawing.CreateRoundedRectPath(envRect, 8f))
            using (SolidBrush envBg = new SolidBrush(t.Surface))
            using (Pen envBorder = new Pen(t.BorderSubtle, 1f))
            {
                g.FillPath(envBg, envPath);
                g.DrawPath(envBorder, envPath);
            }

            Color privDotColor = isAdministrator ? t.Accent : t.Warning;
            string privLabel = isAdministrator ? "Hak Akses: Administrator" : "Hak Akses: Standar";
            using (SolidBrush privBrush = new SolidBrush(privDotColor))
            {
                g.FillEllipse(privBrush, 34f, 439f, 8f, 8f);
            }
            TextRenderer.DrawText(
                g,
                privLabel,
                fMicro,
                new Rectangle(48, 434, 162, 18),
                t.InkPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            TextRenderer.DrawText(
                g,
                "PC: " + machineName,
                fMonoSmall,
                new Rectangle(34, 457, 176, 18),
                t.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            TextRenderer.DrawText(
                g,
                "Created by Nizan",
                fMicro,
                new Rectangle(22, 494, 200, 16),
                t.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );
        }

        private void BtnThemeToggle_Click(object sender, EventArgs e)
        {
            ApplyTheme(currentTheme.IsDark ? ThemePalette.Light : ThemePalette.Dark);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (isInstalling) return;
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Pilih folder tujuan instalasi GuardAgent:";
                dlg.ShowNewFolderButton = true;
                string current = txtInstallDir.Text.Trim();
                if (!string.IsNullOrEmpty(current) && Directory.Exists(current))
                {
                    dlg.SelectedPath = current;
                }
                if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                {
                    txtInstallDir.Text = dlg.SelectedPath;
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (!isInstalling)
            {
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isInstalling)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (appIcon != null) { appIcon.Dispose(); appIcon = null; }
                if (fBrandTitle != null) fBrandTitle.Dispose();
                if (fHeading != null) fHeading.Dispose();
                if (fButton != null) fButton.Dispose();
                if (fSectionLabel != null) fSectionLabel.Dispose();
                if (fLabel != null) fLabel.Dispose();
                if (fSubtle != null) fSubtle.Dispose();
                if (fMicro != null) fMicro.Dispose();
                if (fMono != null) fMono.Dispose();
                if (fMonoSmall != null) fMonoSmall.Dispose();
                if (statusTip != null) statusTip.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SetIdleStatus()
        {
            for (int i = 0; i < stepStates.Length; i++)
            {
                stepStates[i] = InstallStepState.Pending;
            }
            activeStepIndex = -1;
            statusPanel.Kind = StatusKind.Idle;
            statusPanel.Title = "Siap memasang komponen agen pada perangkat ini.";
            statusPanel.Detail = "Service berjalan sebagai LocalSystem tanpa pop-up UAC di akun mahasiswa.";
            statusPanel.Percent = 0;
            statusTip.SetToolTip(statusPanel, string.Empty);
            statusPanel.Invalidate();
            pnlRail.Invalidate();
        }

        public void SetPreviewInstalling(int activeStep, int percent, string message)
        {
            for (int i = 0; i < stepStates.Length; i++)
            {
                if (i < activeStep) stepStates[i] = InstallStepState.Done;
                else if (i == activeStep) stepStates[i] = InstallStepState.Active;
                else stepStates[i] = InstallStepState.Pending;
            }
            activeStepIndex = activeStep;
            statusPanel.Kind = StatusKind.Installing;
            statusPanel.Title = message;
            statusPanel.Detail = string.Empty;
            statusPanel.Percent = percent;
            statusTip.SetToolTip(statusPanel, string.Empty);
            statusPanel.Invalidate();
            pnlRail.Invalidate();
        }

        public void SetPreviewSuccess(string targetDir, bool launched)
        {
            ApplySuccessState(targetDir, launched);
        }

        private void SetErrorStatus(string title, string detail)
        {
            statusPanel.Kind = StatusKind.Error;
            statusPanel.Title = title;
            statusPanel.Detail = detail;
            statusPanel.Percent = 0;
            statusTip.SetToolTip(statusPanel, detail);
            statusPanel.Invalidate();
            pnlRail.Invalidate();
        }

        private void ApplySuccessState(string targetDir, bool launched)
        {
            isInstalling = false;
            isCompleted = true;
            for (int i = 0; i < stepStates.Length; i++)
            {
                stepStates[i] = InstallStepState.Done;
            }
            activeStepIndex = 3;

            statusPanel.Kind = StatusKind.Success;
            statusPanel.Title = "GuardAgent berhasil dipasang pada perangkat ini.";
            statusPanel.Detail = launched
                ? string.Format("Lokasi: {0} (agen aktif).", targetDir)
                : string.Format("Lokasi: {0} (siap dikendalikan Admin).", targetDir);
            statusPanel.Percent = 100;
            statusTip.SetToolTip(statusPanel, statusPanel.Detail);
            statusPanel.Invalidate();
            pnlRail.Invalidate();

            btnCancel.Visible = false;
            btnInstall.Enabled = true;
            btnInstall.Text = "Tutup Installer";
            btnInstall.AccessibleName = "Tutup Installer";
            btnInstall.Focus();
        }

        private void SetInputsEnabled(bool enabled)
        {
            txtInstallDir.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            chkAutoStart.Enabled = enabled;
            chkDesktopShortcut.Enabled = enabled;
            chkLaunchAfter.Enabled = enabled;
            btnCancel.Enabled = enabled;
            if (!enabled)
            {
                pnlDirBox.IsFocusedBorder = false;
                pnlDirBox.Invalidate();
            }
        }

        private bool ValidateTargetDirectory(string rawPath, out string targetDir, out string errorMsg)
        {
            targetDir = rawPath != null ? rawPath.Trim() : string.Empty;
            if (string.IsNullOrEmpty(targetDir))
            {
                errorMsg = "Direktori instalasi tidak boleh kosong. Pilih folder tujuan terlebih dahulu.";
                return false;
            }

            char[] invalidChars = Path.GetInvalidPathChars();
            if (targetDir.IndexOfAny(invalidChars) >= 0)
            {
                errorMsg = "Path mengandung karakter tidak valid. Periksa kembali direktori tujuan.";
                return false;
            }

            try
            {
                string filePart = Path.GetFileName(targetDir);
                if (!string.IsNullOrEmpty(filePart) && filePart.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    errorMsg = "Nama folder mengandung karakter terlarang (< > : \" / \\ | ? *).";
                    return false;
                }
            }
            catch
            {
                errorMsg = "Format direktori instalasi tidak valid.";
                return false;
            }

            if (!Path.IsPathRooted(targetDir))
            {
                errorMsg = "Gunakan path absolut lengkap (contoh: C:\\Program Files\\AIBlockerGuard).";
                return false;
            }

            errorMsg = string.Empty;
            return true;
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            if (isCompleted)
            {
                this.Close();
                return;
            }
            if (isInstalling)
            {
                return;
            }

            string targetDir;
            string validationError;
            if (!ValidateTargetDirectory(txtInstallDir.Text, out targetDir, out validationError))
            {
                SetErrorStatus("Direktori Tidak Valid", validationError);
                txtInstallDir.Focus();
                txtInstallDir.Select(txtInstallDir.Text.Length, 0);
                return;
            }

            bool optAutoStart = chkAutoStart.Checked;
            bool optDesktopShortcut = chkDesktopShortcut.Checked;
            bool optLaunchAfter = chkLaunchAfter.Checked;

            isInstalling = true;
            SetInputsEnabled(false);
            btnInstall.Enabled = false;
            btnInstall.Text = "Memasang...";

            SetPreviewInstalling(0, 12, "Langkah 1/4: Menghentikan layanan & proses lama...");

            Thread worker = new Thread(delegate()
            {
                RunInstallWorker(targetDir, optAutoStart, optDesktopShortcut, optLaunchAfter);
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void DispatchUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch { }
        }

        private void RunInstallWorker(string targetDir, bool optAutoStart, bool optDesktopShortcut, bool optLaunchAfter)
        {
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetExe = Path.Combine(targetDir, "AIBlockerGuard.exe");
                string serviceExe = Path.Combine(targetDir, "AIBlockerService.exe");

                // Tahap 1: Hentikan service & proses yang sedang berjalan agar executable dapat diperbarui tanpa konflik
                DispatchUi(delegate()
                {
                    SetPreviewInstalling(0, 18, "Langkah 1/4: Menghentikan layanan & proses lama...");
                });
                RunHiddenProcess("sc.exe", "stop AIBlockerGuardService", 4000);
                foreach (string procName in new string[] { "AIBlockerGuard", "AIBlockerService" })
                {
                    try
                    {
                        foreach (Process p in Process.GetProcessesByName(procName))
                        {
                            try { p.Kill(); p.WaitForExit(3000); } catch { }
                        }
                    }
                    catch { }
                }

                // Tahap 2: Ekstrak biner agen, service, dan atur izin C:\ProgramData\AIBlockerGuard
                DispatchUi(delegate()
                {
                    SetPreviewInstalling(1, 42, "Langkah 2/4: Mengekstrak biner agen & izin sistem...");
                });

                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream resStream = asm.GetManifestResourceStream("AIBlockerGuard.exe"))
                {
                    if (resStream == null)
                    {
                        throw new InvalidOperationException("Paket resource AIBlockerGuard.exe tidak ditemukan.");
                    }
                    using (FileStream fs = new FileStream(targetExe, FileMode.Create, FileAccess.Write))
                    {
                        resStream.CopyTo(fs);
                    }
                }

                using (Stream svcStream = asm.GetManifestResourceStream("AIBlockerService.exe"))
                {
                    if (svcStream != null)
                    {
                        using (FileStream fs = new FileStream(serviceExe, FileMode.Create, FileAccess.Write))
                        {
                            svcStream.CopyTo(fs);
                        }
                    }
                }

                try
                {
                    string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIBlockerGuard");
                    if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                    RunHiddenProcess("icacls.exe", "\"" + appDataDir + "\" /grant *S-1-5-32-545:(OI)(CI)M /T /Q", 3000);
                }
                catch { }

                // Tahap 3: Daftarkan & jalankan Windows Service + tulis Restore-Normal.bat
                DispatchUi(delegate()
                {
                    SetPreviewInstalling(2, 68, "Langkah 3/4: Mendaftarkan layanan Windows Service...");
                });

                if (File.Exists(serviceExe))
                {
                    RunHiddenProcess("sc.exe", "delete AIBlockerGuardService", 3000);
                    RunHiddenProcess("sc.exe", "create AIBlockerGuardService binPath= \"\\\"" + serviceExe + "\\\"\" start= auto DisplayName= \"AI-Blocker Guard System Service\"", 4000);
                    RunHiddenProcess("sc.exe", "description AIBlockerGuardService \"Layanan sistem untuk proteksi Hosts, DNS DoH, dan keamanan ujian AI-Blocker Guard.\"", 2500);
                    RunHiddenProcess("sc.exe", "failure AIBlockerGuardService reset= 86400 actions= restart/5000/restart/5000/restart/5000", 2500);
                    RunHiddenProcess("sc.exe", "start AIBlockerGuardService", 4000);
                }

                string restoreBat = Path.Combine(targetDir, "Restore-Normal.bat");
                File.WriteAllText(restoreBat, "@echo off\r\n\"" + targetExe + "\" --restore\r\n");

                // Tahap 4: Konfigurasi AutoStart & Pintasan Desktop
                DispatchUi(delegate()
                {
                    SetPreviewInstalling(3, 90, "Langkah 4/4: Mengatur startup & pintasan desktop...");
                });

                if (optAutoStart)
                {
                    ConfigureAutoStart(targetExe);
                }

                if (optDesktopShortcut)
                {
                    CreateDesktopShortcut(targetExe);
                }

                if (optLaunchAfter)
                {
                    Process.Start(new ProcessStartInfo(targetExe) { UseShellExecute = true });
                }

                DispatchUi(delegate()
                {
                    ApplySuccessState(targetDir, optLaunchAfter);
                });
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                DispatchUi(delegate()
                {
                    isInstalling = false;
                    if (activeStepIndex >= 0 && activeStepIndex < stepStates.Length)
                    {
                        stepStates[activeStepIndex] = InstallStepState.Failed;
                    }
                    SetInputsEnabled(true);
                    btnInstall.Enabled = true;
                    btnInstall.Text = "Coba Lagi";
                    btnInstall.AccessibleName = "Coba pasang agen lagi";
                    SetErrorStatus(
                        "Instalasi Gagal (Jalankan sebagai Administrator)",
                        errMsg
                    );
                });
            }
        }

        private void CreateDesktopShortcut(string targetExe)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string vbsPath = Path.Combine(Path.GetTempPath(), "create_aiblocker_shortcut.vbs");
                string lnkPath = Path.Combine(desktop, "GuardAgent.lnk");
                string vbs = string.Format(
                    "Set oWS = WScript.CreateObject(\"WScript.Shell\")\r\n" +
                    "Set oLink = oWS.CreateShortcut(\"{0}\")\r\n" +
                    "oLink.TargetPath = \"{1}\"\r\n" +
                    "oLink.WorkingDirectory = \"{2}\"\r\n" +
                    "oLink.Description = \"GuardAgent - Sistem Proteksi Endpoint Ujian\"\r\n" +
                    "oLink.Save\r\n",
                    lnkPath, targetExe, Path.GetDirectoryName(targetExe)
                );
                File.WriteAllText(vbsPath, vbs);
                Process p = Process.Start(new ProcessStartInfo("wscript.exe", "\"" + vbsPath + "\"") { CreateNoWindow = true, UseShellExecute = false });
                if (p != null) p.WaitForExit(3000);
                File.Delete(vbsPath);
            }
            catch { }
        }

        private static void RunHiddenProcess(string fileName, string args, int timeoutMs)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p != null) p.WaitForExit(timeoutMs);
                }
            }
            catch { }
        }

        private void ConfigureAutoStart(string targetExe)
        {
            try
            {
                string runVal = "\"" + targetExe + "\" --autostart";

                // 1. Registry Run Key (HKCU & HKLM) untuk semua user tanpa pop-up UAC karena asInvoker
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                    }
                }
                catch { }

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                    }
                }
                catch { }

                // 2. Hapus Task Scheduler lama (jika ada) agar tidak terjadi elevasi ganda
                RunHiddenProcess("schtasks.exe", "/Delete /TN \"AIBlockerGuardAgent\" /F", 2500);

                // 3. Folder Startup Windows (CommonStartup & User Startup) via VBS background launcher
                try
                {
                    string[] startupDirs = new string[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
                    };

                    foreach (string dir in startupDirs)
                    {
                        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) continue;

                        string vbsPath = Path.Combine(dir, "GuardAgent-Startup.vbs");
                        string vbsCode =
                            "On Error Resume Next\r\n" +
                            "Set WshShell = CreateObject(\"WScript.Shell\")\r\n" +
                            "WshShell.Run \"\"\"" + targetExe.Replace("\"", "\"\"") + "\"\" --autostart\", 0, False\r\n";

                        File.WriteAllText(vbsPath, vbsCode, Encoding.ASCII);
                    }
                }
                catch { }
            }
            catch { }
        }
    }

    internal sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
        }
    }
}
