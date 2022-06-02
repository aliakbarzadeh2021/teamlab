using System;
using System.ServiceProcess;
using log4net;
using log4net.Config;
using System.ServiceModel;
using System.Diagnostics;

namespace ASC.FullTextIndex.Service
{
	class Program : ServiceBase
	{
		static void Main(string[] args)
		{
            if (IsDebugBreak(args))
            {
                Debugger.Break();
            }

            var launcher = new FullTextIndexLauncher();
			if (Environment.UserInteractive)
			{
				try
				{
                    launcher.Start();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					Console.ReadKey();
				}

                Console.WriteLine("Press any key to stop fulltext server...");
                Console.ReadKey();

                try
                {
                    launcher.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadKey();
                }
			}
			else
			{
				Run(new Program(launcher));
			}
		}

        private static bool IsDebugBreak(string[] args)
        {
            foreach (var a in args)
            {
                if (string.Compare(a, "-d", true) == 0 || string.Compare(a, "-debugbreak", true) == 0) return true;
            }
            return false;
        }

        private readonly FullTextIndexLauncher launcher;

        public Program(FullTextIndexLauncher launcher)
        {
            this.launcher = launcher;
        }

        protected override void OnStart(string[] args)
        {
            launcher.Start();
        }

        protected override void OnStop()
        {
            launcher.Stop();
        }
	}
}
