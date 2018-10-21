using System;
using System.Windows.Forms;


namespace PlanetaryGear
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //  Application.Run(new Form1());
            // Создаем объект-окно
            Form1 mainForm = new Form1();
            // Cвязываем метод OnIdle с событием Application.Idle
            Application.Idle += mainForm.OnIdle;
            // Показываем окно и запускаем цикл обработки сообщений
            Application.Run(mainForm);
        }
    }
}
