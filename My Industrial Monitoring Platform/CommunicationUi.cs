using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    internal static class CommunicationUi
    {
        internal static readonly Color Accent = Color.FromArgb(55, 130, 245);
        internal static readonly Color Text = Color.FromArgb(44, 62, 80);
        internal static readonly Color Muted = Color.FromArgb(71, 85, 105);

        internal static Panel CreateCard(string title, out Panel body)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var heading = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Text
            };
            body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            layout.Controls.Add(heading, 0, 0);
            layout.Controls.Add(body, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        internal static TableLayoutPanel CreateFieldsTable(int rows)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = rows,
                Height = rows * 48,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            for (int i = 0; i < rows; i++)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
            return table;
        }

        internal static void AddFieldPair(TableLayoutPanel table, int row, string label1, Control value1, string label2, Control value2)
        {
            var firstLabel = CreateFieldLabel(label1);
            var secondLabel = CreateFieldLabel(label2);
            firstLabel.Dock = DockStyle.Fill;
            secondLabel.Dock = DockStyle.Fill;
            value1.Dock = DockStyle.Fill;
            value2.Dock = DockStyle.Fill;
            value1.Margin = new Padding(2, 7, 8, 4);
            value2.Margin = new Padding(2, 7, 2, 4);
            table.Controls.Add(firstLabel, 0, row);
            table.Controls.Add(value1, 1, row);
            table.Controls.Add(secondLabel, 2, row);
            table.Controls.Add(value2, 3, row);
        }

        internal static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 8.5F),
                ForeColor = Muted,
                Margin = new Padding(2, 5, 2, 2)
            };
        }

        internal static ComboBox CreateCombo(params string[] items)
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 8.5F),
                Margin = new Padding(2)
            };
            combo.Items.AddRange(items);
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            return combo;
        }

        internal static NumericUpDown CreateNumber(int min, int max, int value)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 8.5F),
                ThousandsSeparator = false
            };
        }

        internal static Button CreateButton(string text, Color color)
        {
            var button = new Button
            {
                Text = text,
                Width = 90,
                Height = 30,
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8.5F),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        internal static DataGridView CreateGrid(params string[] headers)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 34,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                RowHeadersVisible = false,
                RowTemplate = { Height = 34 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                SelectionBackColor = Color.FromArgb(235, 242, 250),
                SelectionForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("微软雅黑", 8F),
                Padding = new Padding(3)
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Muted,
                Font = new Font("微软雅黑", 8F, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(248, 250, 252),
                SelectionForeColor = Muted,
                Padding = new Padding(3)
            };
            foreach (string header in headers) grid.Columns.Add(header, header);
            return grid;
        }
    }
}
