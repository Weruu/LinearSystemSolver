using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Клас для розв'язування системи лінійних алгебраїчних рівнянь (СЛАР) 
    /// методом Жордана-Гауса (повне виключення)
    /// 
    /// Відмінність від звичайного методу Гауса:
    /// - Звичайний метод Гауса: приводить матрицю до верхньотрикутного вигляду, потім зворотний хід
    /// - Метод Жордана-Гауса: приводить матрицю до діагонального вигляду за один прохід
    /// 
    /// Переваги методу Жордана-Гауса:
    /// - Не потрібен зворотний хід - розв'язок готовий одразу
    /// - Простіше для програмування
    /// - Менше накопичення похибок округлення
    /// </summary>
    public class JordanGaussSolver
    {
        /// <summary>
        /// Головний метод для розв'язування системи лінійних рівнянь методом Жордана-Гауса
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
                // Отримуємо розмірність системи (кількість рівнянь/невідомих)
                int n = matrix.GetLength(0);

                // Створюємо робочу копію матриці для виконання перетворень
                double[,] workMatrix = MatrixHelper.CloneMatrix(matrix);

                // Зберігаємо оригінальну матрицю для фінальної перевірки розв'язку
                double[,] originalMatrix = MatrixHelper.CloneMatrix(matrix);

                // Якщо потрібно показувати кроки - додаємо красивий заголовок
                if (showSteps)
                {
                    steps.AppendLine("🔢 МЕТОД ЖОРДАНА-ГАУСА (ПОВНЕ ВИКЛЮЧЕННЯ)");
                    steps.AppendLine(new string('═', 50));
                    steps.AppendLine("\n📋 Початкова розширена матриця:");
                    steps.Append(MatrixHelper.MatrixToString(workMatrix));
                }

                // ======================== ПЕРЕВІРКА ТИПУ РОЗВ'ЯЗКУ ========================
                // Визначаємо, чи має система єдиний розв'язок, нескінченну множину або немає розв'язків
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

                // ======================== ПРОЦЕС ПОВНОГО ВИКЛЮЧЕННЯ ========================
                // Головна відмінність від звичайного методу Гауса:
                // тут ми обнуляємо ВСІ елементи стовпця (і вгорі, і внизу від головного)
                if (showSteps)
                {
                    steps.AppendLine("🔄 ПРОЦЕС ПОВНОГО ВИКЛЮЧЕННЯ:");
                    steps.AppendLine(new string('─', 35));
                }

                // Проходимо по кожному стовпцю матриці (k - номер поточного кроку)
                for (int k = 0; k < n; k++)
                {
                    // ============ ВИБІР ГОЛОВНОГО ЕЛЕМЕНТА ============
                    // Шукаємо рядок з найбільшим за модулем елементом у поточному стовпці k
                    // Це робиться для підвищення числової стійкості алгоритму
                    int maxRow = MatrixHelper.FindPivotRow(workMatrix, k, k);

                    // Перевіряємо, чи не є головний елемент занадто малим (практично нульовим)
                    if (MatrixHelper.IsZero(workMatrix[maxRow, k]))
                    {
                        result.Status = SolutionStatus.Error;
                        result.ErrorMessage = $"Нульовий головний елемент у стовпці {k + 1}";
                        result.Steps = steps.ToString();
                        return result;
                    }

                    // ============ ПЕРЕСТАНОВКА РЯДКІВ ============
                    // Якщо найбільший елемент не в поточному рядку k - міняємо рядки місцями
                    // Це гарантує, що головний елемент буде максимальним за модулем
                    if (maxRow != k)
                    {
                        MatrixHelper.SwapRows(workMatrix, k, maxRow);
                        if (showSteps)
                        {
                            steps.AppendLine($"🔄 Крок {k + 1}: Переставляємо рядки {k + 1} ↔ {maxRow + 1}");
                            steps.Append(MatrixHelper.MatrixToString(workMatrix));
                        }
                    }

                    // Зберігаємо значення головного елемента для подальших обчислень
                    double pivot = workMatrix[k, k];

                    if (showSteps)
                    {
                        steps.AppendLine($"➡️ Крок {k + 1}: Головний елемент a[{k + 1}][{k + 1}] = {pivot:F4}");
                    }

                    // ============ НОРМАЛІЗАЦІЯ ПОТОЧНОГО РЯДКА ============
                    // Ділимо всі елементи k-го рядка на головний елемент
                    // Це робить головний елемент рівним 1, що спрощує подальші обчислення
                    for (int j = k; j < n + 1; j++)
                    {
                        workMatrix[k, j] /= pivot;
                    }

                    if (showSteps)
                    {
                        steps.AppendLine($"📏 Нормалізуємо рядок {k + 1} (ділимо на {pivot:F4}):");
                        steps.Append(MatrixHelper.MatrixToString(workMatrix));
                    }

                    // ============ ПОВНЕ ВИКЛЮЧЕННЯ - ГОЛОВНА ВІДМІННІСТЬ ============
                    // На відміну від звичайного методу Гауса, тут ми обнуляємо елементи 
                    // у ВСІХ рядках (крім поточного), а не тільки в рядках нижче
                    // Це дозволяє привести матрицю до діагонального вигляду за один прохід
                    for (int i = 0; i < n; i++)
                    {
                        // Пропускаємо поточний рядок k (його ми вже нормалізували)
                        if (i != k)
                        {
                            // Коефіцієнт для виключення елемента workMatrix[i, k]
                            double factor = workMatrix[i, k];

                            // Якщо елемент не нульовий - виконуємо операцію виключення
                            if (!MatrixHelper.IsZero(factor))
                            {
                                // Виконуємо операцію: Ri = Ri - factor * Rk
                                // Це робить елемент workMatrix[i, k] рівним нулю
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

                    // Підсумок поточного кроку
                    if (showSteps)
                    {
                        steps.AppendLine($"✅ Завершено крок {k + 1} - стовпець {k + 1} оброблено\n");
                    }
                }

                // ======================== ОТРИМАННЯ РОЗВ'ЯЗКУ ========================
                // Після повного виключення матриця має діагональний вигляд:
                // [1  0  0 | b1]    x1 = b1
                // [0  1  0 | b2] => x2 = b2  
                // [0  0  1 | b3]    x3 = b3
                // 
                // Розв'язок знаходиться просто у стовпці вільних членів!
                double[] solution = new double[n];
                for (int i = 0; i < n; i++)
                {
                    // i-та невідома дорівнює елементу в останньому стовпці i-того рядка
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

                // ======================== ПЕРЕВІРКА ТОЧНОСТІ РОЗВ'ЯЗКУ ========================
                // Обчислюємо максимальну похибку, підставивши розв'язок в оригінальну систему
                result.MaxError = MatrixHelper.CalculateMaxError(originalMatrix, solution);

                // Показуємо детальну перевірку для кожного рівняння
                if (showSteps)
                {
                    steps.AppendLine("\n✅ ПЕРЕВІРКА РОЗВ'ЯЗКУ:");
                    steps.AppendLine(new string('─', 25));

                    // Перевіряємо кожне рівняння оригінальної системи
                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0; // Сума лівої частини i-го рівняння
                        StringBuilder check = new StringBuilder();

                        // Обчислюємо ліву частину рівняння: a[i,0]*x[0] + a[i,1]*x[1] + ... + a[i,n-1]*x[n-1]
                        for (int j = 0; j < n; j++)
                        {
                            sum += originalMatrix[i, j] * solution[j];
                            if (j > 0) check.Append(" + ");
                            check.Append($"{originalMatrix[i, j]:F2}×{solution[j]:F4}");
                        }

                        // Порівнюємо з правою частиною та обчислюємо абсолютну похибку
                        check.Append($" = {sum:F6} ≈ {originalMatrix[i, n]:F4}");
                        double error = Math.Abs(sum - originalMatrix[i, n]);
                        check.Append($" (похибка: {error:E2})");

                        steps.AppendLine(check.ToString());
                    }

                    steps.AppendLine($"\n🎯 Максимальна похибка: {result.MaxError:E3}");
                }

                // ======================== ФОРМУВАННЯ ФІНАЛЬНОГО РЕЗУЛЬТАТУ ========================
                result.Solution = solution;
                result.Status = SolutionStatus.UniqueSolution;
                result.Steps = steps.ToString();

                return result;
            }
            catch (Exception ex)
            {
                // Обробка будь-яких помилок, що можуть виникнути під час обчислень
                // (наприклад, переповнення, ділення на нуль тощо)
                result.Status = SolutionStatus.Error;
                result.ErrorMessage = ex.Message;
                result.Steps = steps.ToString();
                return result;
            }
        }
    }
}