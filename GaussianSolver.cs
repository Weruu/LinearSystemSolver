using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Решение СЛАР методом Гауса с выбором главного элемента
    /// </summary>
    public class GaussianSolver
    {
        /// <summary>
        /// Решает систему линейных уравнений методом Гауса
        /// </summary>
        /// <param name="matrix">Расширенная матрица системы</param>
        /// <param name="showSteps">Показывать шаги решения</param>
        /// <returns>Результат решения</returns>
        public static SolverResult Solve(double[,] matrix, bool showSteps = false)
        {
            var result = new SolverResult();
            StringBuilder steps = new StringBuilder();

            try
            {
                int n = matrix.GetLength(0);
                double[,] workMatrix = MatrixHelper.CloneMatrix(matrix);
                double[,] originalMatrix = MatrixHelper.CloneMatrix(matrix);

                if (showSteps)
                {
                    steps.AppendLine("🔢 МЕТОД ГАУСА З ВИБОРОМ ГОЛОВНОГО ЕЛЕМЕНТА");
                    steps.AppendLine(new string('═', 50));
                    steps.AppendLine("\n📋 Початкова розширена матриця:");
                    steps.Append(MatrixHelper.MatrixToString(workMatrix));
                }

                // Проверяем тип решения
                result.Status = MatrixHelper.CheckSolutionType(workMatrix);
                if (result.Status != SolutionStatus.UniqueSolution)
                {
                    if (showSteps)
                    {
                        steps.AppendLine($"⚠️ Система {(result.Status == SolutionStatus.NoSolution ? "несумісна" : "має нескінченну множину розв'язків")}");
                    }
                    result.Steps = steps.ToString();
                    return result;
                }

                // Прямой ход метода Гауса с выбором главного элемента
                if (showSteps)
                {
                    steps.AppendLine("🔽 ПРЯМИЙ ХІД:");
                    steps.AppendLine(new string('─', 30));
                }

                for (int k = 0; k < n; k++)
                {
                    // Выбор главного элемента (частичное pivoting)
                    int maxRow = MatrixHelper.FindPivotRow(workMatrix, k, k);

                    if (MatrixHelper.IsZero(workMatrix[maxRow, k]))
                    {
                        result.Status = SolutionStatus.Error;
                        result.ErrorMessage = $"Нульовий головний елемент у стовпці {k + 1}";
                        result.Steps = steps.ToString();
                        return result;
                    }

                    // Перестановка строк
                    if (maxRow != k)
                    {
                        MatrixHelper.SwapRows(workMatrix, k, maxRow);
                        if (showSteps)
                        {
                            steps.AppendLine($"🔄 Крок {k + 1}: Переставляємо рядки {k + 1} ↔ {maxRow + 1}");
                            steps.Append(MatrixHelper.MatrixToString(workMatrix));
                        }
                    }

                    double pivot = workMatrix[k, k];

                    if (showSteps && maxRow == k)
                    {
                        steps.AppendLine($"➡️ Крок {k + 1}: Головний елемент a[{k + 1}][{k + 1}] = {pivot:F4}");
                    }

                    // Нормализация строки k
                    for (int j = k; j < n + 1; j++)
                    {
                        workMatrix[k, j] /= pivot;
                    }

                    if (showSteps)
                    {
                        steps.AppendLine($"📏 Нормалізуємо рядок {k + 1} (ділимо на {pivot:F4}):");
                        steps.Append(MatrixHelper.MatrixToString(workMatrix));
                    }

                    // Обнуление нижних строк
                    for (int i = k + 1; i < n; i++)
                    {
                        double factor = workMatrix[i, k];
                        if (!MatrixHelper.IsZero(factor))
                        {
                            for (int j = k; j < n + 1; j++)
                            {
                                workMatrix[i, j] -= factor * workMatrix[k, j];
                            }

                            if (showSteps)
                            {
                                steps.AppendLine($"🧮 R{i + 1} = R{i + 1} - ({factor:F4}) × R{k + 1}:");
                                steps.Append(MatrixHelper.MatrixToString(workMatrix));
                            }
                        }
                    }
                }

                // Обратный ход метода Гауса
                if (showSteps)
                {
                    steps.AppendLine("🔼 ЗВОРОТНИЙ ХІД:");
                    steps.AppendLine(new string('─', 30));
                }

                double[] solution = new double[n];
                for (int i = n - 1; i >= 0; i--)
                {
                    solution[i] = workMatrix[i, n];

                    for (int j = i + 1; j < n; j++)
                    {
                        solution[i] -= workMatrix[i, j] * solution[j];
                    }

                    if (showSteps)
                    {
                        StringBuilder calc = new StringBuilder();
                        calc.Append($"x{i + 1} = {workMatrix[i, n]:F4}");

                        for (int j = i + 1; j < n; j++)
                        {
                            if (!MatrixHelper.IsZero(workMatrix[i, j]))
                            {
                                calc.Append($" - ({workMatrix[i, j]:F4}) × {solution[j]:F4}");
                            }
                        }
                        calc.Append($" = {solution[i]:F6}");

                        steps.AppendLine(calc.ToString());
                    }
                }

                // Проверка точности
                result.MaxError = MatrixHelper.CalculateMaxError(originalMatrix, solution);

                if (showSteps)
                {
                    steps.AppendLine("\n✅ ПЕРЕВІРКА РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 25));

                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0;
                        StringBuilder check = new StringBuilder();

                        for (int j = 0; j < n; j++)
                        {
                            sum += originalMatrix[i, j] * solution[j];
                            if (j > 0) check.Append(" + ");
                            check.Append($"{originalMatrix[i, j]:F2}×{solution[j]:F4}");
                        }

                        check.Append($" = {sum:F6} ≈ {originalMatrix[i, n]:F4}");
                        double error = Math.Abs(sum - originalMatrix[i, n]);
                        check.Append($" (похибка: {error:E2})");

                        steps.AppendLine(check.ToString());
                    }

                    steps.AppendLine($"\n🎯 Максимальна похибка: {result.MaxError:E3}");
                }

                result.Solution = solution;
                result.Status = SolutionStatus.UniqueSolution;
                result.Steps = steps.ToString();

                return result;
            }
            catch (Exception ex)
            {
                result.Status = SolutionStatus.Error;
                result.ErrorMessage = ex.Message;
                result.Steps = steps.ToString();
                return result;
            }
        }
    }
}