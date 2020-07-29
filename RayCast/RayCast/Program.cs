using System.Windows.Forms;

namespace RayCast
{
    static class Program
    {
        static void Main()
        {
            // Run the main program
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Sketch());
        }
    }

    public class Canvas : Panel
    {
        public Canvas()
        {
            ResizeRedraw = true;
            DoubleBuffered = true;
        }
    }
}
