using System;

namespace LinearSystemSolver
{
    /// <summary>
    /// Перелік можливих станів розв'язку системи рівнянь
    /// </summary>
    public enum SolutionStatus
    {
        /// <summary>Система має єдиний розв'язок</summary>
        UniqueSolution,

        /// <summary>Система має нескінченну кількість розв'язків</summary>
        InfiniteSolutions,

        /// <summary>Система несумісна (немає розв'язків)</summary>
        NoSolution,

        /// <summary>Виникла помилка під час обчислень</summary>
        Error
    }

    /// <summary>
    /// Клас, що містить результати розв'язання системи лінійних рівнянь
    /// </summary>
    public class SolverResult
    {
        /// <summary>
        /// Статус розв'язку системи (з переліку SolutionStatus)
        /// </summary>
        public SolutionStatus Status { get; set; }

        /// <summary>
        /// Масив значень змінних - розв'язок системи
        /// Заповнюється лише при Status = UniqueSolution
        /// </summary>
        public double[] Solution { get; set; }

        /// <summary>
        /// Максимальна похибка при перевірці розв'язку
        /// Обчислюється як максимальна різниця між лівою та правою частиною рівнянь
        /// </summary>
        public double MaxError { get; set; }

        /// <summary>
        /// Детальний опис кроків розв'язання
        /// Містить проміжні результати обчислень
        /// </summary>
        public string Steps { get; set; }

        /// <summary>
        /// Повідомлення про помилку (заповнюється при Status = Error)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Визначник матриці коефіцієнтів системи
        /// Допомагає аналізувати особливості системи
        /// </summary>
        public double Determinant { get; set; }

        /// <summary>
        /// Конструктор за замовчуванням
        /// Ініціалізує рядкові властивості
        /// </summary>
        public SolverResult()
        {
            Steps = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}