﻿using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Клас для розв'язання систем лінійних рівнянь матричним методом (через обернену матрицю)
    /// </summary>
    public class MatrixMethodSolver
    {
        /// <summary>
        /// Розв'язує систему лінійних рівнянь матричним методом
        /// Система Ax = b розв'язується як x = A⁻¹b
        /// </summary>
        /// <param name="matrix">Розширена матриця системи</param>
        /// <param name="showSteps">Чи показувати кроки розв'язання</param>
        /// <returns>Результат розв'язання</returns>
        public static SolverResult Solve(double[,] matrix, bool showSteps = false)
        {
            var result = new SolverResult(); // Об'єкт для зберігання результату
            StringBuilder steps = new StringBuilder(); // Для збереження кроків розв'язання

            try
            {
                int n = matrix.GetLength(0); // Розмірність системи
                double[,] originalMatrix = MatrixHelper.CloneMatrix(matrix); // Зберігаємо оригінальну матрицю

                // Якщо потрібно показувати кроки
                if (showSteps)
                {
                    steps.AppendLine("🔢 МАТРИЧНИЙ МЕТОД (ЧЕРЕЗ ОБЕРНЕНУ МАТРИЦЮ)");
                    steps.AppendLine(new string('═', 50));
                    steps.AppendLine("\n📋 Початкова система: Ax = b");
                    steps.AppendLine("💡 Розв'язок: x = A⁻¹ × b\n");
                }

                // Виділяємо матрицю коефіцієнтів A та вектор правих частин b
                double[,] A = MatrixHelper.ExtractCoefficientMatrix(matrix);
                double[] b = MatrixHelper.ExtractRightHandSide(matrix);

                // Виведення інформації про кроки
                if (showSteps)
                {
                    steps.AppendLine("📊 Матриця коефіцієнтів A:");
                    steps.Append(MatrixHelper.MatrixToString(A));

                    steps.AppendLine("📊 Вектор вільних членів b:");
                    for (int i = 0; i < n; i++)
                    {
                        steps.AppendLine($"b{i + 1} = {b[i]:F4}");
                    }
                    steps.AppendLine();
                }

                // Обчислюємо визначник матриці A
                result.Determinant = MatrixHelper.CalculateDeterminant(A);

                if (showSteps)
                {
                    steps.AppendLine($"🧮 Визначник матриці A: det(A) = {result.Determinant:F6}");
                }

                // Перевіряємо, чи матриця не вироджена (визначник ≠ 0)
                if (MatrixHelper.IsZero(result.Determinant))
                {
                    result.Status = SolutionStatus.Error;
                    result.ErrorMessage = "Матриця коефіцієнтів вироджена (визначник дорівнює нулю)";

                    if (showSteps)
                    {
                        steps.AppendLine("❌ Матриця вироджена! Обернена матриця не існує.");
                        steps.AppendLine("   Неможливо використовувати матричний метод.");
                    }

                    result.Steps = steps.ToString();
                    return result;
                }

                if (showSteps)
                {
                    steps.AppendLine("✅ Матриця невироджена, обернена матриця існує.\n");
                }

                // Обчислюємо обернену матрицю A⁻¹
                double[,] A_inv;
                try
                {
                    A_inv = MatrixHelper.GetInverseMatrix(A);
                }
                catch (Exception ex)
                {
                    result.Status = SolutionStatus.Error;
                    result.ErrorMessage = $"Помилка при обчисленні оберненої матриці: {ex.Message}";
                    result.Steps = steps.ToString();
                    return result;
                }

                if (showSteps)
                {
                    steps.AppendLine("🔄 Обернена матриця A⁻¹:");
                    steps.Append(MatrixHelper.MatrixToString(A_inv));
                }

                // Обчислюємо розв'язок: x = A⁻¹ × b
                double[] solution = MatrixHelper.MultiplyMatrixVector(A_inv, b);

                if (showSteps)
                {
                    steps.AppendLine("🧮 ОБЧИСЛЕННЯ РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 30));
                    steps.AppendLine("x = A⁻¹ × b\n");

                    // Виводимо деталі обчислення для кожного x
                    for (int i = 0; i < n; i++)
                    {
                        StringBuilder calc = new StringBuilder();
                        calc.Append($"x{i + 1} = ");

                        for (int j = 0; j < n; j++)
                        {
                            if (j > 0) calc.Append(" + ");
                            calc.Append($"({A_inv[i, j]:F4}) × {b[j]:F4}");
                        }

                        calc.Append($" = {solution[i]:F6}");
                        steps.AppendLine(calc.ToString());
                    }
                    steps.AppendLine();
                }

                // Перевіряємо точність розв'язку
                result.MaxError = MatrixHelper.CalculateMaxError(originalMatrix, solution);

                if (showSteps)
                {
                    steps.AppendLine("✅ ПЕРЕВІРКА РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 25));
                    steps.AppendLine("Підставляємо знайдені значення у початкову систему:\n");

                    // Для кожного рівняння показуємо перевірку
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

                    // Оцінюємо точність розв'язку
                    if (result.MaxError < 1e-10)
                    {
                        steps.AppendLine("✅ Розв'язок знайдено з високою точністю!");
                    }
                    else if (result.MaxError < 1e-6)
                    {
                        steps.AppendLine("✅ Розв'язок знайдено з достатньою точністю.");
                    }
                    else
                    {
                        steps.AppendLine("⚠️ Увага: Велика похибка може свідчити про погану обумовленість матриці.");
                    }
                }

                // Заповнюємо результат
                result.Solution = solution;
                result.Status = SolutionStatus.UniqueSolution;
                result.Steps = steps.ToString();

                return result;
            }
            catch (Exception ex)
            {
                // Обробка помилок
                result.Status = SolutionStatus.Error;
                result.ErrorMessage = ex.Message;
                result.Steps = steps.ToString();
                return result;
            }
        }
    }
}