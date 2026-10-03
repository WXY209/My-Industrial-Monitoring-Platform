using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页温度、压力曲线的独立显示控件。</summary>
    public sealed class OverviewChartControl : UserControl
    {
        private readonly Chart temperatureChart;
        private readonly Chart pressureChart;

        public OverviewChartControl()
        {
            BackColor = Color.FromArgb(241, 245, 249);
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor, Margin = new Padding(0, 0, 0, 6) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); Controls.Add(grid);
            temperatureChart = CreateChartCard(grid, 0, "▦  实时温度 (°C)", 30, 70, 5, 60, Color.Red, "°C", Color.Red);
            pressureChart = CreateChartCard(grid, 1, "▦  实时压力 (MPa)", 0.8, 2.2, 0.1, 1.8, Color.FromArgb(255, 153, 51), "MPa", Color.Blue);
        }

        public void ShowReadings(IList<SensorReading> readings)
        {
            temperatureChart.Series[0].Points.Clear(); pressureChart.Series[0].Points.Clear();
            if (readings == null) return;
            for (int i = 0; i < readings.Count; i++)
            {
                temperatureChart.Series[0].Points.AddXY(i, readings[i].Temperature);
                pressureChart.Series[0].Points.AddXY(i, readings[i].Pressure);
            }
        }

        private static Chart CreateChartCard(TableLayoutPanel parent, int column, string titleText, double minimum, double maximum, double interval, double threshold, Color thresholdColor, string axisTitle, Color seriesColor)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8), Margin = new Padding(0, 0, 5, 0) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); card.Controls.Add(layout);
            layout.Controls.Add(new Label { Text = titleText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(57, 75, 96), Font = new Font("微软雅黑", 8.5F, FontStyle.Bold) }, 0, 0);
            var chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, Palette = ChartColorPalette.None, BorderlineColor = Color.White, BorderlineDashStyle = ChartDashStyle.Solid };
            var area = new ChartArea("Main") { BackColor = Color.White };
            area.AxisX.Minimum = 0; area.AxisX.Maximum = 10; area.AxisX.Interval = 0.5; area.AxisX.IsReversed = false; area.AxisX.MajorGrid.Enabled = false; area.AxisX.LabelStyle.ForeColor = Color.Gray; area.AxisX.LabelStyle.Font = new Font("微软雅黑", 7.5F); area.AxisX.LineColor = Color.Gainsboro;
            area.AxisY.Minimum = minimum; area.AxisY.Maximum = maximum; area.AxisY.Interval = interval; area.AxisY.Title = axisTitle; area.AxisY.TitleFont = new Font("微软雅黑", 9F); area.AxisY.LabelStyle.ForeColor = Color.Gray; area.AxisY.LabelStyle.Font = new Font("微软雅黑", 7.5F); area.AxisY.MajorGrid.LineColor = Color.FromArgb(237, 239, 242); area.AxisY.LineColor = Color.Gainsboro;
            area.AxisY.StripLines.Add(new StripLine { Interval = 0, IntervalOffset = threshold, StripWidth = 0, BorderColor = thresholdColor, BorderWidth = 2, BorderDashStyle = ChartDashStyle.Dash });
            chart.ChartAreas.Add(area); chart.Series.Add(new Series(axisTitle == "°C" ? "Temperature" : "Pressure") { ChartArea = "Main", ChartType = SeriesChartType.Spline, BorderWidth = 2, Color = seriesColor });
            layout.Controls.Add(chart, 0, 1); parent.Controls.Add(card, column, 0); return chart;
        }
    }
}
