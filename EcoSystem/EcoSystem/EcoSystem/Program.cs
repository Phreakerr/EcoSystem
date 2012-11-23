using System;

namespace EcoSystem
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (EcoSystemGame game = new EcoSystemGame())
            {
                game.Run();
            }
        }
    }
#endif
}

