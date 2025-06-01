using System;
using System.Windows.Forms;

namespace LinearSystemSolver
{
    /// <summary>
    /// Головний клас програми - точка входу в додаток
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Головний метод програми - точка входу
        /// </summary>
        [STAThread] // Атрибут, що вказує на використання однопотокової квартири (STA) для COM-компонентів
        static void Main()
        {
            // Налаштування візуальних стилів для додатка
            Application.EnableVisualStyles();

            // Встановлення стандартного режиму відображення тексту (GDI+ замість GDI)
            Application.SetCompatibleTextRenderingDefault(false);

            // Запуск головної форми додатка - MainForm
            Application.Run(new MainForm());
        }
    }
}