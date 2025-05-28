using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Решение СЛАР методом Жордана-Гаусса (полное исключение)
    /// </summary>
    public class JordanGaussSolver
    {
        /// <summary>
        /// Решает систему линейных уравнений методом Жордана-Гаусса
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
                    steps.AppendLine("🔢 МЕТОД ЖОРДАНА-ГАУСА (ПОВНЕ ВИКЛЮЧЕННЯ)");
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

                // Метод Жордана-Гаусса с выбором главного элемента
                if (showSteps)
                {
                    steps.AppendLine("🔄 ПРОЦЕС ПОВНОГО ВИКЛЮЧЕННЯ:");
                    steps.AppendLine(new string('─', 35));
                }

                for (int k = 0; k < n; k++)
                {
                    // Выбор главного элемента
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

                    if (showSteps)
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

                    // Обнуление ВСЕХ остальных строк (не только нижних)
                    for (int i = 0; i < n; i++)
                    {
                        if (i != k)
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

                    if (showSteps)
                    {
                        steps.AppendLine($"✅ Завершено крок {k + 1} - стовпець {k + 1} оброблено\n");
                    }
                }

                // Извлечение решения (оно уже готово в последнем столбце)
                double[] solution = new double[n];
                for (int i = 0; i < n; i++)
                {
                    solution[i] = workMatrix[i, n];
                }

                if (showSteps)
                {
                    steps.AppendLine("🎯 ОТРИМАННЯ РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 25));
                    steps.AppendLine("Після повного виключення розв'язок знаходиться у стовпці вільних членів:");

                    for (int i = 0; i < n; i++)
                    {
                        steps.AppendLine($"x{i + 1} = {solution[i]:F6}");
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