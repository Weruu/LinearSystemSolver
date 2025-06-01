using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Клас для розв'язування системи лінійних алгебраїчних рівнянь (СЛАР) 
    /// методом Гауса з вибором головного елемента (з частковими поворотами)
    /// 
    /// Метод Гауса складається з двох етапів:
    /// 1. Прямий хід - приведення матриці до трикутного вигляду
    /// 2. Зворотний хід - знаходження розв'язків від останнього до першого
    /// </summary>
    public class GaussianSolver
    {
        /// <summary>
        /// Головний метод для розв'язування системи лінійних рівнянь методом Гауса
        /// </summary>
        /// <param name="matrix">Розширена матриця системи (коефіцієнти + вільні члени)</param>
        /// <param name="showSteps">Чи показувати покрокове розв'язання</param>
        /// <returns>Результат розв'язування з розв'язком та додатковою інформацією</returns>
        public static SolverResult Solve(double[,] matrix, bool showSteps = false)
        {
            // Створюємо об'єкт для зберігання результатів
            var result = new SolverResult();

            // StringBuilder для збереження покрокових пояснень
            StringBuilder steps = new StringBuilder();

            try
            {
                // Отримуємо розмірність системи (кількість рівнянь)
                int n = matrix.GetLength(0);

                // Створюємо робочу копію матриці, щоб не змінювати оригінал
                double[,] workMatrix = MatrixHelper.CloneMatrix(matrix);

                // Зберігаємо оригінальну матрицю для перевірки розв'язку
                double[,] originalMatrix = MatrixHelper.CloneMatrix(matrix);

                // Якщо потрібно показувати кроки - додаємо заголовок
                if (showSteps)
                {
                    steps.AppendLine("🔢 МЕТОД ГАУСА З ВИБОРОМ ГОЛОВНОГО ЕЛЕМЕНТА");
                    steps.AppendLine(new string('═', 50));
                    steps.AppendLine("\n📋 Початкова розширена матриця:");
                    steps.Append(MatrixHelper.MatrixToString(workMatrix));
                }

                // Перевіряємо тип розв'язку системи (єдиний, нескінченна множина, або немає розв'язку)
                result.Status = MatrixHelper.CheckSolutionType(workMatrix);

                // Якщо система не має єдиного розв'язку - завершуємо роботу
                if (result.Status != SolutionStatus.UniqueSolution)
                {
                    if (showSteps)
                    {
                        string statusMessage = result.Status == SolutionStatus.NoSolution
                            ? "несумісна (немає розв'язків)"
                            : "має нескінченну множину розв'язків";
                        steps.AppendLine($"⚠️ Система {statusMessage}");
                    }
                    result.Steps = steps.ToString();
                    return result;
                }

                // ======================== ПРЯМИЙ ХІД МЕТОДУ ГАУСА ========================
                // Мета: привести матрицю до верхньотрикутного вигляду
                if (showSteps)
                {
                    steps.AppendLine("🔽 ПРЯМИЙ ХІД:");
                    steps.AppendLine(new string('─', 30));
                }

                // Проходимо по кожному стовпцю (k - номер поточного кроку)
                for (int k = 0; k < n; k++)
                {
                    // ============ ВИБІР ГОЛОВНОГО ЕЛЕМЕНТА ============
                    // Шукаємо рядок з найбільшим за модулем елементом у поточному стовпці
                    // (це потрібно для зменшення похибок обчислень)
                    int maxRow = MatrixHelper.FindPivotRow(workMatrix, k, k);

                    // Перевіряємо, чи не є головний елемент нулем
                    if (MatrixHelper.IsZero(workMatrix[maxRow, k]))
                    {
                        result.Status = SolutionStatus.Error;
                        result.ErrorMessage = $"Нульовий головний елемент у стовпці {k + 1}";
                        result.Steps = steps.ToString();
                        return result;
                    }

                    // ============ ПЕРЕСТАНОВКА РЯДКІВ ============
                    // Якщо найбільший елемент не в поточному рядку - міняємо рядки місцями
                    if (maxRow != k)
                    {
                        MatrixHelper.SwapRows(workMatrix, k, maxRow);
                        if (showSteps)
                        {
                            steps.AppendLine($"🔄 Крок {k + 1}: Переставляємо рядки {k + 1} ↔ {maxRow + 1}");
                            steps.Append(MatrixHelper.MatrixToString(workMatrix));
                        }
                    }

                    // Зберігаємо значення головного елемента
                    double pivot = workMatrix[k, k];

                    if (showSteps && maxRow == k)
                    {
                        steps.AppendLine($"➡️ Крок {k + 1}: Головний елемент a[{k + 1}][{k + 1}] = {pivot:F4}");
                    }

                    // ============ НОРМАЛІЗАЦІЯ ПОТОЧНОГО РЯДКА ============
                    // Ділимо всі елементи поточного рядка на головний елемент
                    // Це робить головний елемент рівним 1
                    for (int j = k; j < n + 1; j++)
                    {
                        workMatrix[k, j] /= pivot;
                    }

                    if (showSteps)
                    {
                        steps.AppendLine($"📏 Нормалізуємо рядок {k + 1} (ділимо на {pivot:F4}):");
                        steps.Append(MatrixHelper.MatrixToString(workMatrix));
                    }

                    // ============ ОБНУЛЕННЯ ЕЛЕМЕНТІВ ПІД ГОЛОВНИМ ============
                    // Обнуляємо всі елементи під головним елементом у поточному стовпці
                    for (int i = k + 1; i < n; i++)
                    {
                        // Коефіцієнт, на який множимо рядок k для віднімання від рядка i
                        double factor = workMatrix[i, k];

                        // Якщо елемент не нульовий - виконуємо операцію
                        if (!MatrixHelper.IsZero(factor))
                        {
                            // Виконуємо операцію: Ri = Ri - factor * Rk
                            // Це обнулює елемент workMatrix[i, k]
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

                // ======================== ЗВОРОТНИЙ ХІД МЕТОДУ ГАУСА ========================
                // Мета: знайти значення невідомих, починаючи з останньої
                if (showSteps)
                {
                    steps.AppendLine("🔼 ЗВОРОТНИЙ ХІД:");
                    steps.AppendLine(new string('─', 30));
                }

                // Масив для зберігання розв'язків
                double[] solution = new double[n];

                // Йдемо від останнього рівняння до першого
                for (int i = n - 1; i >= 0; i--)
                {
                    // Початкове значення - вільний член поточного рівняння
                    solution[i] = workMatrix[i, n];

                    // Віднімаємо добутки коефіцієнтів на вже знайдені невідомі
                    for (int j = i + 1; j < n; j++)
                    {
                        solution[i] -= workMatrix[i, j] * solution[j];
                    }

                    // Показуємо процес обчислення для поточної невідомої
                    if (showSteps)
                    {
                        StringBuilder calc = new StringBuilder();
                        calc.Append($"x{i + 1} = {workMatrix[i, n]:F4}");

                        // Додаємо інформацію про віднімання
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

                // ======================== ПЕРЕВІРКА ТОЧНОСТІ РОЗВ'ЯЗКУ ========================
                // Обчислюємо максимальну похибку розв'язку
                result.MaxError = MatrixHelper.CalculateMaxError(originalMatrix, solution);

                // Показуємо детальну перевірку розв'язку
                if (showSteps)
                {
                    steps.AppendLine("\n✅ ПЕРЕВІРКА РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 25));

                    // Перевіряємо кожне рівняння системи
                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0; // Сума лівої частини рівняння
                        StringBuilder check = new StringBuilder();

                        // Обчислюємо ліву частину рівняння
                        for (int j = 0; j < n; j++)
                        {
                            sum += originalMatrix[i, j] * solution[j];
                            if (j > 0) check.Append(" + ");
                            check.Append($"{originalMatrix[i, j]:F2}×{solution[j]:F4}");
                        }

                        // Порівнюємо з правою частиною та обчислюємо похибку
                        check.Append($" = {sum:F6} ≈ {originalMatrix[i, n]:F4}");
                        double error = Math.Abs(sum - originalMatrix[i, n]);
                        check.Append($" (похибка: {error:E2})");

                        steps.AppendLine(check.ToString());
                    }

                    steps.AppendLine($"\n🎯 Максимальна похибка: {result.MaxError:E3}");
                }

                // ======================== ФОРМУВАННЯ РЕЗУЛЬТАТУ ========================
                result.Solution = solution;
                result.Status = SolutionStatus.UniqueSolution;
                result.Steps = steps.ToString();

                return result;
            }
            catch (Exception ex)
            {
                // Обробка помилок, що можуть виникнути під час обчислень
                result.Status = SolutionStatus.Error;
                result.ErrorMessage = ex.Message;
                result.Steps = steps.ToString();
                return result;
            }
        }
    }
}