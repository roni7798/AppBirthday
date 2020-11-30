using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppBirthday
{
    static class Program
    {

        static void Main()
        {
#if DEBUG
            AppBirthday app = new AppBirthday();
            app.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
             {
                 new AppBirthday()
             };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}

