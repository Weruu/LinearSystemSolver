using System;

namespace LinearSystemSolver
{
    /// <summary>
    /// Статус решения системы уравнений
    /// </summary>
    public enum SolutionStatus
    {
        UniqueSolution,     // Единственное решение
        InfiniteSolutions,  // Бесконечное множество решений
        NoSolution,        // Нет решений (несовместна)
        Error              // Ошибка в вычислениях
    }

    /// <summary>
    /// Результат решения системы линейных уравнений
    /// </summary>
    public class SolverResult
    {
        /// <summary>
        /// Статус решения
        /// </summary>
        public SolutionStatus Status { get; set; }

        /// <summary>
        /// Решение системы (если найдено)
        /// </summary>
        public double[] Solution { get; set; }

        /// <summary>
        /// Максимальная погрешность проверки
        /// </summary>
        public double MaxError { get; set; }

        /// <summary>
        /// Детальные шаги решения
        /// </summary>
        public string Steps { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Определитель матрицы коэффициентов
        /// </summary>
        public double Determinant { get; set; }

        public SolverResult()
        {
            Steps = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}