using System.Drawing;
using System.Windows.Forms;

namespace Schedule.WinForms.Helpers;

public static class ModernStyles
{
    // Цвета
    public static readonly Color PrimaryColor = Color.FromArgb(0, 120, 212);
    public static readonly Color PrimaryHoverColor = Color.FromArgb(0, 90, 158);
    public static readonly Color BackgroundColor = Color.White;
    public static readonly Color SurfaceColor = Color.FromArgb(249, 249, 249);
    public static readonly Color BorderColor = Color.FromArgb(229, 229, 229);
    public static readonly Color TextPrimaryColor = Color.FromArgb(32, 32, 32);
    public static readonly Color TextSecondaryColor = Color.FromArgb(96, 96, 96);
    public static readonly Color SuccessColor = Color.FromArgb(16, 124, 16);
    public static readonly Color DangerColor = Color.FromArgb(196, 43, 28);
    public static readonly Color WarningColor = Color.FromArgb(255, 185, 0);
    public static readonly Color InfoColor = Color.FromArgb(0, 120, 212);

    // Шрифты
    public static readonly Font TitleFont = new Font("Segoe UI", 24, FontStyle.Bold);
    public static readonly Font SubtitleFont = new Font("Segoe UI", 18, FontStyle.Bold);
    public static readonly Font HeadingFont = new Font("Segoe UI", 14, FontStyle.Bold);
    public static readonly Font BodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
    public static readonly Font SmallFont = new Font("Segoe UI", 9, FontStyle.Regular);

    public static void ApplyModernStyle(DataGridView dgv)
    {
        // Основные настройки
        dgv.BackgroundColor = BackgroundColor;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.EnableHeadersVisualStyles = false;
        dgv.GridColor = BorderColor;
        dgv.Font = BodyFont;

        // Стиль заголовков столбцов
        dgv.ColumnHeadersDefaultCellStyle.BackColor = SurfaceColor;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimaryColor;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceColor;
        dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimaryColor;
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
        dgv.ColumnHeadersHeight = 40;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        // Стиль строк
        dgv.DefaultCellStyle.BackColor = BackgroundColor;
        dgv.DefaultCellStyle.ForeColor = TextPrimaryColor;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 243, 243);
        dgv.DefaultCellStyle.SelectionForeColor = TextPrimaryColor;
        dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
        dgv.RowTemplate.Height = 45;

        // Альтернативные строки
        dgv.AlternatingRowsDefaultCellStyle.BackColor = SurfaceColor;
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 243, 243);
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimaryColor;

        // Стиль заголовков строк
        dgv.RowHeadersVisible = false;

        // Поведение
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.ReadOnly = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    public static Button CreateModernButton(string text, Color? backgroundColor = null)
    {
        var btn = new Button
        {
            Text = text,
            Font = BodyFont,
            BackColor = backgroundColor ?? PrimaryColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Height = 36,
            Padding = new Padding(16, 0, 16, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static TabControl CreateModernTabControl()
    {
        var tab = new TabControl
        {
            Font = BodyFont,
            Padding = new Point(12, 6)
        };
        return tab;
    }
}
