using System.ServiceProcess;

namespace AppQueueConsole
{
    /// <summary>
    /// Класс для реализации в виде службы
    /// </summary>
    class AppQueueService : ServiceBase
    {
        public AppQueueService()
        {
            ServiceName = Program.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            Program.Start(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }
}

