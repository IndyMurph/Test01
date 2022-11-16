using System.ServiceProcess;

namespace PIAdaptMRP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var servicesToRun = new ServiceBase[]
            {
                new PiAdaptMrp(args)
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
