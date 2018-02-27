using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

namespace AppQueueConsole
{
    /// <summary>
    /// Инсталлятор службы для запуска через InstallUtil
    /// </summary>
    [RunInstaller(true)]
    public class AppQueueInstaller : Installer
    {
        public AppQueueInstaller()
        {
            var spi = new ServiceProcessInstaller();
            var si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Username = null;
            spi.Password = null;

            si.DisplayName = Program.ServiceName;
            si.ServiceName = Program.ServiceName;
            si.StartType = ServiceStartMode.Automatic;

            Installers.Add(spi);
            Installers.Add(si);
        }

    }
}
