using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;

namespace EUResto
{
    public class MainForm : Form
    {
        private const double ExchangeRate = 1.95583;

        private readonly TextBox _amountDueEuro;
        private readonly TextBox _paidLeva;
        private readonly TextBox _paidEuro;
        private readonly TextBox _changeEuro;
        private readonly TextBox _changeLeva;
        private readonly Label _statusLabel;
        private TextBox _activeInput;

        public MainForm()
        {
            Text = "EU Resto";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(650, 460);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var padding = 12;
            var labelWidth = 160;
            var amountWidth = 180;
            var inputWidth = 160;

            var amountLabel = CreateLabel("Сметка в ЕВРО:", padding, padding, labelWidth);
            _amountDueEuro = CreateInput(amountLabel.Right + 10, amountLabel.Top, amountWidth);
            _amountDueEuro.Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold);
            _amountDueEuro.BackColor = Color.FromArgb(255, 235, 240);

            // Оставяме допълнително разстояние от около 1 см под "Сметка в ЕВРО" за полето "Платени ЕВРО".
            var paidEuroLabel = CreateLabel("Платени ЕВРО:", padding, amountLabel.Bottom + 38, labelWidth);
            _paidEuro = CreateInput(paidEuroLabel.Right + 10, paidEuroLabel.Top, inputWidth);
            _paidEuro.BackColor = Color.FromArgb(225, 239, 255);
            _paidEuro.Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold);

            var paidLevaLabel = CreateLabel("Платени ЛВ:", padding, paidEuroLabel.Bottom + 12, labelWidth);
            _paidLeva = CreateInput(paidLevaLabel.Right + 10, paidLevaLabel.Top, inputWidth);
            _paidLeva.BackColor = Color.FromArgb(225, 245, 225);
            _paidLeva.Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold);

            var changeEuroLabel = CreateLabel("Ресто в ЕВРО:", padding, paidLevaLabel.Bottom + 24, labelWidth);
            _changeEuro = CreateOutput(changeEuroLabel.Right + 10, changeEuroLabel.Top, inputWidth);
            _changeEuro.BackColor = Color.FromArgb(225, 239, 255);
            _changeEuro.Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);

            var changeLevaLabel = CreateLabel("Ресто в ЛВ:", padding, changeEuroLabel.Bottom + 12, labelWidth);
            _changeLeva = CreateOutput(changeLevaLabel.Right + 10, changeLevaLabel.Top, inputWidth);
            _changeLeva.BackColor = Color.FromArgb(225, 245, 225);
            _changeLeva.Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);

            var calculateButton = new Button
            {
                Text = "Изчисли",
                Location = new Point(padding, changeLevaLabel.Bottom + 18),
                Size = new Size(labelWidth + inputWidth + 10, 36)
            };
            calculateButton.Click += (sender, args) => CalculateChange();
            Controls.Add(calculateButton);

            _statusLabel = new Label
            {
                Text = "Курс: 1 EUR = 1.95583 BGN",
                AutoSize = true,
                Location = new Point(padding, calculateButton.Bottom + 8),
                ForeColor = Color.Gray
            };
            Controls.Add(_statusLabel);

            var keypadOffset = Math.Max(amountWidth, inputWidth);
            var keypadPanel = BuildKeypad(new Point(padding + labelWidth + keypadOffset + 50, padding));
            Controls.Add(keypadPanel);

            _activeInput = _amountDueEuro;
            _amountDueEuro.TextChanged += (sender, args) => CalculateChange();
            _paidLeva.TextChanged += (sender, args) => CalculateChange();
            _paidEuro.TextChanged += (sender, args) => CalculateChange();

            CalculateChange();
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 4),
                Width = width,
                AutoSize = false
            };
            Controls.Add(label);
            return label;
        }

        private TextBox CreateInput(int x, int y, int width)
        {
            var box = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                TextAlign = HorizontalAlignment.Right
            };
            box.Enter += (sender, args) => OnInputEnter(box);
            box.Click += (sender, args) => OnInputClick(box);
            Controls.Add(box);
            return box;
        }

        private TextBox CreateOutput(int x, int y, int width)
        {
            var box = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                TextAlign = HorizontalAlignment.Right,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(box);
            return box;
        }

        private Panel BuildKeypad(Point origin)
        {
            var panel = new Panel
            {
                Location = origin,
                Size = new Size(240, 300)
            };

            var buttons = new[]
            {
                "7","8","9",
                "4","5","6",
                "1","2","3",
                "0",".","C"
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = new Button
                {
                    Text = buttons[i],
                    Size = new Size(70, 50),
                    Location = new Point((i % 3) * 80, (i / 3) * 60),
                    Font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Bold)
                };

                switch (buttons[i])
                {
                    case "C":
                        btn.Click += (sender, args) => ClearAllFields();
                        break;
                    default:
                        var value = buttons[i];
                        btn.Click += (sender, args) => AppendToActiveInput(value);
                        break;
                }

                panel.Controls.Add(btn);
            }

            var calcBtn = new Button
            {
                Text = "=",
                Size = new Size(230, 40),
                Location = new Point(0, 240)
            };
            calcBtn.Click += (sender, args) => CalculateChange();
            panel.Controls.Add(calcBtn);

            return panel;
        }

        private void AppendToActiveInput(string value)
        {
            if (_activeInput == null)
            {
                _activeInput = _amountDueEuro;
            }

            if (value == "." && _activeInput.SelectionLength == 0 && _activeInput.Text.Contains("."))
            {
                return;
            }

            if (_activeInput.SelectionLength > 0)
            {
                _activeInput.Text = value;
                _activeInput.SelectionStart = _activeInput.Text.Length;
                return;
            }

            _activeInput.Text += value;
            _activeInput.SelectionStart = _activeInput.Text.Length;
        }

        private void ClearAllFields()
        {
            _amountDueEuro.Clear();
            _paidEuro.Clear();
            _paidLeva.Clear();
            _changeEuro.Clear();
            _changeLeva.Clear();
            _activeInput = _amountDueEuro;
            _amountDueEuro.Focus();
            CalculateChange();
        }

        private void OnInputEnter(TextBox box)
        {
            ActivateAndSelectAll(box);
        }

        private void OnInputClick(TextBox box)
        {
            ActivateAndSelectAll(box);
        }

        private void ActivateAndSelectAll(TextBox box)
        {
            _activeInput = box;
            box.SelectAll();
        }

        private void CalculateChange()
        {
            var amountDue = ParseValue(_amountDueEuro.Text);
            var paidLeva = ParseValue(_paidLeva.Text);
            var paidEuro = ParseValue(_paidEuro.Text);

            var paidEuroFromLeva = paidLeva / ExchangeRate;
            var totalPaidEuro = paidEuro + paidEuroFromLeva;
            var changeEuro = totalPaidEuro - amountDue;
            var changeLeva = changeEuro * ExchangeRate;

            _changeEuro.Text = changeEuro.ToString("F2", CultureInfo.InvariantCulture);
            _changeLeva.Text = changeLeva.ToString("F2", CultureInfo.InvariantCulture);

            _statusLabel.Text = $"Курс: 1 EUR = {ExchangeRate} BGN | Платено евро: {totalPaidEuro:F2}";
        }

        private static double ParseValue(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0.0;
            }

            var normalized = text.Replace(',', '.');
            if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return 0.0;
        }
    }
}
