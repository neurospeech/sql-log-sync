using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {


            /*if (args?.Length > 0) {

                string p = string.Concat(args).ToLower().Trim();
                if (p.StartsWith("--install")) {
                    ManagedInstallerClass.InstallHelper(new string[] {
                         typeof(Program).Assembly.Location
                    });
                    return;
                }
                if (p.StartsWith("--uninstall")) {
                    ManagedInstallerClass.InstallHelper(new string[] {
                        "/u",
                         typeof(Program).Assembly.Location
                    });
                    return;
                }
            }*/

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
