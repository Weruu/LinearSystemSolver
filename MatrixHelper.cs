using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Клас допоміжних методів для роботи з матрицями
    /// </summary>
    public static class MatrixHelper
    {
        private const double EPSILON = 1e-12; // Константа для порівняння чисел з плаваючою точкою

        /// <summary>
        /// Створює нову матрицю розміром n x (n+1) (розширену матрицю системи)
        /// </summary>
        public static double[,] CreateMatrix(int n)
        {
            return new double[n, n + 1];
        }

        /// <summary>
        /// Повністю копіює матрицю
        /// </summary>
        public static double[,] CloneMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] clone = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    clone[i, j] = matrix[i, j];
                }
            }
            return clone;
        }

        /// <summary>
        /// Виділяє матрицю коефіцієнтів з розширеної матриці системи
        /// </summary>
        public static double[,] ExtractCoefficientMatrix(double[,] augmentedMatrix)
        {
            int n = augmentedMatrix.GetLength(0);
            double[,] coeffMatrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    coeffMatrix[i, j] = augmentedMatrix[i, j];
                }
            }
            return coeffMatrix;
        }

        /// <summary>
        /// Виділяє вектор правих частин з розширеної матриці системи
        /// </summary>
        public static double[] ExtractRightHandSide(double[,] augmentedMatrix)
        {
            int n = augmentedMatrix.GetLength(0);
            double[] rhs = new double[n];

            for (int i = 0; i < n; i++)
            {
                rhs[i] = augmentedMatrix[i, n];
            }
            return rhs;
        }

        /// <summary>
        /// Обчислює визначник квадратної матриці методом Гауса
        /// </summary>
        public static double CalculateDeterminant(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Матриця повинна бути квадратною");

            double[,] temp = CloneSquareMatrix(matrix); // Робоча копія матриці
            double det = 1.0; // Початкове значення визначника
            int swapCount = 0; // Лічильник перестановок рядків

            // Прямий хід методу Гауса
            for (int k = 0; k < n; k++)
            {
                // Пошук рядка з максимальним елементом у поточному стовпці
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(temp[i, k]) > Math.Abs(temp[maxRow, k]))
                        maxRow = i;
                }

                // Перевірка на виродженість матриці
                if (Math.Abs(temp[maxRow, k]) < EPSILON)
                    return 0.0;

                // Перестановка рядків, якщо потрібно
                if (maxRow != k)
                {
                    SwapRows(temp, k, maxRow);
                    swapCount++;
                }

                det *= temp[k, k]; // Домножуємо визначник на діагональний елемент

                // Обнулення елементів нижче головної діагоналі
                for (int i = k + 1; i < n; i++)
                {
                    double factor = temp[i, k] / temp[k, k];
                    for (int j = k; j < n; j++)
                    {
                        temp[i, j] -= factor * temp[k, j];
                    }
                }
            }

            // Коригування знаку визначника за кількістю перестановок
            if (swapCount % 2 == 1)
                det = -det;

            return det;
        }

        /// <summary>
        /// Копіює квадратну матрицю
        /// </summary>
        private static double[,] CloneSquareMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] clone = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    clone[i, j] = matrix[i, j];
                }
            }
            return clone;
        }

        /// <summary>
        /// Міняє місцями два рядки матриці
        /// </summary>
        public static void SwapRows(double[,] matrix, int row1, int row2)
        {
            int cols = matrix.GetLength(1);
            for (int j = 0; j < cols; j++)
            {
                double temp = matrix[row1, j];
                matrix[row1, j] = matrix[row2, j];
                matrix[row2, j] = temp;
            }
        }

        /// <summary>
        /// Знаходить рядок з максимальним елементом у стовпці (для часткового вибору головного елемента)
        /// </summary>
        public static int FindPivotRow(double[,] matrix, int col, int startRow)
        {
            int n = matrix.GetLength(0);
            int maxRow = startRow;

            for (int i = startRow + 1; i < n; i++)
            {
                if (Math.Abs(matrix[i, col]) > Math.Abs(matrix[maxRow, col]))
                    maxRow = i;
            }
            return maxRow;
        }

        /// <summary>
        /// Перевіряє, чи число можна вважати нулем (з урахуванням похибки)
        /// </summary>
        public static bool IsZero(double value)
        {
            return Math.Abs(value) < EPSILON;
        }

        /// <summary>
        /// Обчислює обернену матрицю методом Жордана-Гауса
        /// </summary>
        public static double[,] GetInverseMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Матриця повинна бути квадратною");

            // Перевірка на виродженість
            double det = CalculateDeterminant(matrix);
            if (IsZero(det))
                throw new InvalidOperationException("Матриця вироджена (визначник дорівнює нулю)");

            double[,] inverse = new double[n, n]; // Тут буде результат
            double[,] augmented = new double[n, 2 * n]; // Розширена матриця [A|I]

            // Формуємо розширену матрицю [A|I]
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j]; // Копіюємо вихідну матрицю
                    augmented[i, j + n] = (i == j) ? 1.0 : 0.0; // Одинична матриця
                }
            }

            // Метод Жордана-Гауса з вибором головного елемента
            for (int k = 0; k < n; k++)
            {
                // Пошук рядка з максимальним елементом у поточному стовпці
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(augmented[i, k]) > Math.Abs(augmented[maxRow, k]))
                        maxRow = i;
                }

                // Перестановка рядків
                if (maxRow != k)
                {
                    for (int j = 0; j < 2 * n; j++)
                    {
                        double temp = augmented[k, j];
                        augmented[k, j] = augmented[maxRow, j];
                        augmented[maxRow, j] = temp;
                    }
                }

                double pivot = augmented[k, k];
                if (IsZero(pivot))
                    throw new InvalidOperationException("Матриця вироджена");

                // Нормалізація поточного рядка
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[k, j] /= pivot;
                }

                // Обнулення інших рядків
                for (int i = 0; i < n; i++)
                {
                    if (i != k)
                    {
                        double factor = augmented[i, k];
                        for (int j = 0; j < 2 * n; j++)
                        {
                            augmented[i, j] -= factor * augmented[k, j];
                        }
                    }
                }
            }

            // Виділення оберненої матриці з правої частини
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    inverse[i, j] = augmented[i, j + n];
                }
            }

            return inverse;
        }

        /// <summary>
        /// Множить матрицю на вектор
        /// </summary>
        public static double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
                throw new ArgumentException("Невідповідність розмірів матриці та вектора");

            double[] result = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                result[i] = 0;
                for (int j = 0; j < cols; j++)
                {
                    result[i] += matrix[i, j] * vector[j];
                }
            }
            return result;
        }

        /// <summary>
        /// Обчислює максимальну похибку розв'язку
        /// </summary>
        public static double CalculateMaxError(double[,] originalMatrix, double[] solution)
        {
            int n = solution.Length;
            double maxError = 0.0;

            // Для кожного рівняння обчислюємо похибку
            for (int i = 0; i < n; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < n; j++)
                {
                    sum += originalMatrix[i, j] * solution[j];
                }
                double error = Math.Abs(sum - originalMatrix[i, n]); // Різниця між лівою та правою частиною
                maxError = Math.Max(maxError, error);
            }

            return maxError;
        }

        /// <summary>
        /// Перетворює матрицю у рядкове представлення для виводу
        /// </summary>
        public static string MatrixToString(double[,] matrix, string title = "")
        {
            StringBuilder sb = new StringBuilder();
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine(title);
                sb.AppendLine(new string('-', title.Length));
            }

            // Форматування матриці з рамками
            for (int i = 0; i < rows; i++)
            {
                sb.Append("│ ");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append($"{matrix[i, j],10:F4}"); // Формат числа: 10 знаків, 4 знаки після коми
                    if (j == cols - 2) sb.Append(" │ "); // Розділювач перед останнім стовпцем
                    else if (j < cols - 1) sb.Append("  ");
                }
                sb.AppendLine(" │");
            }
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Визначає тип розв'язку системи (єдиний, безліч, відсутність)
        /// </summary>
        public static SolutionStatus CheckSolutionType(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] coeffMatrix = ExtractCoefficientMatrix(matrix);

            int rankA = CalculateRank(coeffMatrix); // Ранг матриці коефіцієнтів
            int rankAb = CalculateRank(matrix);     // Ранг розширеної матриці

            // Критерій Кронекера-Капеллі
            if (rankA != rankAb)
                return SolutionStatus.NoSolution;      // Система несумісна
            else if (rankA == n)
                return SolutionStatus.UniqueSolution;  // Єдиний розв'язок
            else
                return SolutionStatus.InfiniteSolutions; // Безліч розв'язків
        }

        /// <summary>
        /// Обчислює ранг матриці методом Гауса
        /// </summary>
        public static int CalculateRank(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] temp = CloneMatrix(matrix); // Робоча копія матриці

            int rank = 0;
            for (int col = 0; col < cols && rank < rows; col++)
            {
                // Пошук ненульового елемента в поточному стовпці
                int pivotRow = -1;
                for (int row = rank; row < rows; row++)
                {
                    if (!IsZero(temp[row, col]))
                    {
                        pivotRow = row;
                        break;
                    }
                }

                if (pivotRow == -1) continue; // Весь стовпець нульовий

                // Перестановка рядків
                if (pivotRow != rank)
                {
                    SwapRows(temp, rank, pivotRow);
                }

                // Обнулення елементів нижче поточного
                for (int row = rank + 1; row < rows; row++)
                {
                    if (!IsZero(temp[row, col]))
                    {
                        double factor = temp[row, col] / temp[rank, col];
                        for (int j = col; j < cols; j++)
                        {
                            temp[row, j] -= factor * temp[rank, j];
                        }
                    }
                }
                rank++;
            }

            return rank;
        }
    }
}