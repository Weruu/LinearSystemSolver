using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Text;

namespace LinearSystemSolver
{
    public partial class MainForm : Form
    {
        private int size = 3;
        private DataGridView dataGridView;
        private Button solveButton;
        private Button clearButton;
        private Button saveButton;
        private Button loadButton;
        private ComboBox methodComboBox;
        private TextBox resultTextBox;
        private NumericUpDown sizeNumericUpDown;
        private CheckBox showStepsCheckBox;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this.Text = "Розв'язання СЛАР точними методами";
            this.ClientSize = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель управления
            Panel controlPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(980, 80),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(controlPanel);

            // Размер системы
            Label sizeLabel = new Label
            {
                Text = "Розмір системи:",
                Location = new Point(10, 15),
                AutoSize = true
            };
            controlPanel.Controls.Add(sizeLabel);

            sizeNumericUpDown = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 10,
                Value = size,
                Location = new Point(10, 35),
                Width = 60
            };
            sizeNumericUpDown.ValueChanged += SizeNumericUpDown_ValueChanged;
            controlPanel.Controls.Add(sizeNumericUpDown);

            // Метод решения
            Label methodLabel = new Label
            {
                Text = "Метод розв'язання:",
                Location = new Point(90, 15),
                AutoSize = true
            };
            controlPanel.Controls.Add(methodLabel);

            methodComboBox = new ComboBox
            {
                Location = new Point(90, 35),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            methodComboBox.Items.AddRange(new object[] {
                "Метод Гауса (з вибором головного елемента)",
                "Метод Жордана-Гауса (повне виключення)",
                "Матричний метод (через обернену матрицю)",
            });
            methodComboBox.SelectedIndex = 0;
            controlPanel.Controls.Add(methodComboBox);

            // Показывать шаги
            showStepsCheckBox = new CheckBox
            {
                Text = "Показати кроки розв'язання",
                Location = new Point(310, 35),
                AutoSize = true
            };
            controlPanel.Controls.Add(showStepsCheckBox);

            // Кнопки
            solveButton = new Button
            {
                Text = "Розв'язати",
                Location = new Point(520, 30),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen
            };
            solveButton.Click += SolveButton_Click;
            controlPanel.Controls.Add(solveButton);

            clearButton = new Button
            {
                Text = "Очистити",
                Location = new Point(610, 30),
                Size = new Size(80, 30)
            };
            clearButton.Click += ClearButton_Click;
            controlPanel.Controls.Add(clearButton);

            saveButton = new Button
            {
                Text = "Зберегти",
                Location = new Point(700, 30),
                Size = new Size(80, 30)
            };
            saveButton.Click += SaveButton_Click;
            controlPanel.Controls.Add(saveButton);

            loadButton = new Button
            {
                Text = "Завантажити",
                Location = new Point(790, 30),
                Size = new Size(90, 30)
            };
            loadButton.Click += LoadButton_Click;
            controlPanel.Controls.Add(loadButton);

            // Таблица для ввода данных
            dataGridView = new DataGridView
            {
                Location = new Point(10, 100),
                Size = new Size(600, 400),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersWidth = 50
            };
            InitializeDataGridView();
            this.Controls.Add(dataGridView);

            // Результаты
            Label resultLabel = new Label
            {
                Text = "Результати:",
                Location = new Point(630, 100),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold)
            };
            this.Controls.Add(resultLabel);

            resultTextBox = new TextBox
            {
                Location = new Point(630, 120),
                Size = new Size(350, 380),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.White
            };
            this.Controls.Add(resultTextBox);

            // Статус бар
            Label statusLabel = new Label
            {
                Text = "Готовий до роботи. Введіть коефіцієнти системи рівнянь.",
                Location = new Point(10, 520),
                AutoSize = true,
                ForeColor = Color.Blue
            };
            this.Controls.Add(statusLabel);
        }

        private void InitializeDataGridView()
        {
            dataGridView.Columns.Clear();
            dataGridView.Rows.Clear();

            // Добавляем столбцы для коэффициентов
            for (int i = 0; i < size; i++)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
                {
                    Name = $"col{i}",
                    HeaderText = $"x�{i + 1}",
                    Width = 70,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                };
                dataGridView.Columns.Add(col);
            }

            // Добавляем столбец для свободных членов
            DataGridViewTextBoxColumn colB = new DataGridViewTextBoxColumn
            {
                Name = "colB",
                HeaderText = "b",
                Width = 70,
                DefaultCellStyle = {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    BackColor = Color.LightYellow
                }
            };
            dataGridView.Columns.Add(colB);

            // Добавляем строки
            for (int i = 0; i < size; i++)
            {
                int rowIndex = dataGridView.Rows.Add();
                dataGridView.Rows[rowIndex].HeaderCell.Value = $"Рівняння {i + 1}";

                // Устанавливаем значения по умолчанию (единичная матрица)
                for (int j = 0; j < size + 1; j++)
                {
                    if (j < size)
                        dataGridView.Rows[rowIndex].Cells[j].Value = (i == j) ? "1" : "0";
                    else
                        dataGridView.Rows[rowIndex].Cells[j].Value = "1";
                }
            }

            // Настройка внешнего вида
            dataGridView.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
        }

        private void SizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            size = (int)sizeNumericUpDown.Value;
            InitializeDataGridView();
            resultTextBox.Clear();
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            try
            {
                double[,] matrix = new double[size, size + 1];

                // Считываем данные из DataGridView
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size + 1; j++)
                    {
                        string value = dataGridView.Rows[i].Cells[j].Value?.ToString()?.Replace(',', '.');
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            MessageBox.Show($"Будь ласка, заповніть всі комірки. Помилка у рядку {i + 1}, стовпці {j + 1}",
                                "Помилка введення", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dataGridView.CurrentCell = dataGridView.Rows[i].Cells[j];
                            return;
                        }

                        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out matrix[i, j]))
                        {
                            MessageBox.Show($"Невірний формат числа у рядку {i + 1}, стовпці {j + 1}: '{value}'",
                                "Помилка формату", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dataGridView.CurrentCell = dataGridView.Rows[i].Cells[j];
                            return;
                        }
                    }
                }

                SolverResult result;
                string methodName = methodComboBox.SelectedItem.ToString();
                bool showSteps = showStepsCheckBox.Checked;

                // Выбираем метод решения
                switch (methodComboBox.SelectedIndex)
                {
                    case 0: // Метод Гауса
                        result = GaussianSolver.Solve(matrix, showSteps);
                        break;
                    case 1: // Метод Жордана-Гауса
                        result = JordanGaussSolver.Solve(matrix, showSteps);
                        break;
                    case 2: // Матричный метод
                        result = MatrixMethodSolver.Solve(matrix, showSteps);
                        break;
                    default:
                        throw new InvalidOperationException("Невідомий метод розв'язання");
                }

                DisplayResult(result, methodName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при розв'язанні системи:\n{ex.Message}",
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultTextBox.Text = $"ПОМИЛКА: {ex.Message}";
            }
        }

        private void DisplayResult(SolverResult result, string methodName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"═══ {methodName} ═══\n");

            switch (result.Status)
            {
                case SolutionStatus.UniqueSolution:
                    sb.AppendLine("📊 РОЗВ'ЯЗОК ЗНАЙДЕНО:");
                    sb.AppendLine(new string('─', 25));
                    for (int i = 0; i < result.Solution.Length; i++)
                    {
                        sb.AppendLine($"x₍{i + 1}₎ = {result.Solution[i]:F8}");
                    }

                    sb.AppendLine($"\n🔍 Перевірка точності:");
                    sb.AppendLine($"Максимальна похибка: {result.MaxError:E3}");

                    break;

                case SolutionStatus.InfiniteSolutions:
                    sb.AppendLine("∞ СИСТЕМА МАЄ НЕСКІНЧЕННУ МНОЖИНУ РОЗВ'ЯЗКІВ");
                    break;

                case SolutionStatus.NoSolution:
                    sb.AppendLine("✖ СИСТЕМА НЕСУМІСНА (НЕМАЄ РОЗВ'ЯЗКІВ)");
                    break;

                case SolutionStatus.Error:
                    sb.AppendLine($"❌ ПОМИЛКА: {result.ErrorMessage}");
                    break;
            }

            if (!string.IsNullOrEmpty(result.Steps))
            {
                sb.AppendLine("\n" + new string('═', 40));
                sb.AppendLine("📝 КРОКИ РОЗВ'ЯЗАННЯ:");
                sb.AppendLine(new string('═', 40));
                sb.AppendLine(result.Steps);
            }

            resultTextBox.Text = sb.ToString();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            InitializeDataGridView();
            resultTextBox.Clear();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Зберегти матрицю"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"{size}"); // Размер системы

                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size + 1; j++)
                        {
                            string value = dataGridView.Rows[i].Cells[j].Value?.ToString() ?? "0";
                            sb.Append(value);
                            if (j < size) sb.Append("\t");
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Матрицю збережено успішно!", "Збереження",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Завантажити матрицю"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openDialog.FileName);
                    if (lines.Length < 2)
                    {
                        MessageBox.Show("Некоректний формат файлу!", "Помилка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int newSize = int.Parse(lines[0]);
                    if (newSize < 2 || newSize > 10)
                    {
                        MessageBox.Show("Розмір системи повинен бути від 2 до 10!", "Помилка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sizeNumericUpDown.Value = newSize;
                    size = newSize;
                    InitializeDataGridView();

                    for (int i = 0; i < size && i + 1 < lines.Length; i++)
                    {
                        string[] values = lines[i + 1].Split('\t');
                        for (int j = 0; j < Math.Min(values.Length, size + 1); j++)
                        {
                            dataGridView.Rows[i].Cells[j].Value = values[j];
                        }
                    }

                    MessageBox.Show("Матрицю завантажено успішно!", "Завантаження",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}