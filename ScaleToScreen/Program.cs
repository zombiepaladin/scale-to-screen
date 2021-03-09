using System;

namespace ScaleToScreen
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new ScaleToScreenGame())
                game.Run();
        }
    }
}
