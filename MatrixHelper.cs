using System;
using System.Text;

namespace LinearSystemSolver
{
    /// <summary>
    /// Вспомогательные методы для работы с матрицами
    /// </summary>
    public static class MatrixHelper
    {
        private const double EPSILON = 1e-12; // Точность для сравнения с нулем

        /// <summary>
        /// Создает новую матрицу размером n x (n+1)
        /// </summary>
        public static double[,] CreateMatrix(int n)
        {
            return new double[n, n + 1];
        }

        /// <summary>
        /// Клонирует матрицу
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
        /// Извлекает матрицу коэффициентов из расширенной матрицы
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
        /// Извлекает вектор правых частей из расширенной матрицы
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
        /// Вычисляет определитель матрицы методом Гаусса
        /// </summary>
        public static double CalculateDeterminant(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Матрица должна быть квадратной");

            double[,] temp = CloneSquareMatrix(matrix);
            double det = 1.0;
            int swapCount = 0;

            for (int k = 0; k < n; k++)
            {
                // Поиск максимального элемента для частичного выбора главного элемента
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(temp[i, k]) > Math.Abs(temp[maxRow, k]))
                        maxRow = i;
                }

                // Проверка на вырожденность
                if (Math.Abs(temp[maxRow, k]) < EPSILON)
                    return 0.0;

                // Перестановка строк
                if (maxRow != k)
                {
                    SwapRows(temp, k, maxRow);
                    swapCount++;
                }

                det *= temp[k, k];

                // Прямой ход Гаусса
                for (int i = k + 1; i < n; i++)
                {
                    double factor = temp[i, k] / temp[k, k];
                    for (int j = k; j < n; j++)
                    {
                        temp[i, j] -= factor * temp[k, j];
                    }
                }
            }

            // Учитываем знак от перестановок
            if (swapCount % 2 == 1)
                det = -det;

            return det;
        }

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
        /// Меняет местами две строки в матрице
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
        /// Находит строку с максимальным элементом в столбце (для частичного pivoting)
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
        /// Проверяет, является ли число практически нулевым
        /// </summary>
        public static bool IsZero(double value)
        {
            return Math.Abs(value) < EPSILON;
        }

        /// <summary>
        /// Вычисляет обратную матрицу методом Жордана-Гаусса
        /// </summary>
        public static double[,] GetInverseMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Матрица должна быть квадратной");

            // Проверяем определитель
            double det = CalculateDeterminant(matrix);
            if (IsZero(det))
                throw new InvalidOperationException("Матрица вырождена (определитель равен нулю)");

            double[,] inverse = new double[n, n];
            double[,] augmented = new double[n, 2 * n];

            // Создаем расширенную матрицу [A|I]
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                    augmented[i, j + n] = (i == j) ? 1.0 : 0.0;
                }
            }

            // Метод Жордана-Гаусса с выбором главного элемента
            for (int k = 0; k < n; k++)
            {
                // Поиск максимального элемента в столбце
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(augmented[i, k]) > Math.Abs(augmented[maxRow, k]))
                        maxRow = i;
                }

                // Перестановка строк
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
                    throw new InvalidOperationException("Матрица вырождена");

                // Нормализация строки k
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[k, j] /= pivot;
                }

                // Обнуление других строк
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

            // Извлекаем обратную матрицу из правой части
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
        /// Умножает матрицу на вектор
        /// </summary>
        public static double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
                throw new ArgumentException("Несовместимые размеры матрицы и вектора");

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
        /// Вычисляет максимальную погрешность решения
        /// </summary>
        public static double CalculateMaxError(double[,] originalMatrix, double[] solution)
        {
            int n = solution.Length;
            double maxError = 0.0;

            for (int i = 0; i < n; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < n; j++)
                {
                    sum += originalMatrix[i, j] * solution[j];
                }
                double error = Math.Abs(sum - originalMatrix[i, n]);
                maxError = Math.Max(maxError, error);
            }

            return maxError;
        }

        /// <summary>
        /// Преобразует матрицу в строковое представление
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

            for (int i = 0; i < rows; i++)
            {
                sb.Append("│ ");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append($"{matrix[i, j],10:F4}");
                    if (j == cols - 2) sb.Append(" │ "); // Разделитель перед последним столбцом
                    else if (j < cols - 1) sb.Append("  ");
                }
                sb.AppendLine(" │");
            }
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Проверяет ранги матриц для определения типа решения
        /// </summary>
        public static SolutionStatus CheckSolutionType(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] coeffMatrix = ExtractCoefficientMatrix(matrix);

            int rankA = CalculateRank(coeffMatrix);
            int rankAb = CalculateRank(matrix);

            if (rankA != rankAb)
                return SolutionStatus.NoSolution;
            else if (rankA == n)
                return SolutionStatus.UniqueSolution;
            else
                return SolutionStatus.InfiniteSolutions;
        }

        /// <summary>
        /// Вычисляет ранг матрицы
        /// </summary>
        public static int CalculateRank(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] temp = CloneMatrix(matrix);

            int rank = 0;
            for (int col = 0; col < cols && rank < rows; col++)
            {
                // Находим ненулевой элемент в столбце
                int pivotRow = -1;
                for (int row = rank; row < rows; row++)
                {
                    if (!IsZero(temp[row, col]))
                    {
                        pivotRow = row;
                        break;
                    }
                }

                if (pivotRow == -1) continue; // Весь столбец нулевой

                // Меняем строки местами
                if (pivotRow != rank)
                {
                    SwapRows(temp, rank, pivotRow);
                }

                // Обнуляем элементы ниже главного
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