using System;
using System.CommandLine;
using UpToolLib;
using UpToolLib.Tool;

namespace UpToolCLI
{
    public static class Program
    {
        private static readonly UtLibFunctions Functions = new UtLibFunctions();

        public static int Main(string[] args)
        {
            MutexLock.Lock();
            try
            {
                XmlTool.FixXml();
                ExternalFunctionalityManager.Init(Functions);
                RootCommand rootCommand = new RootCommand();
                
                PackageManagement.RegisterCommands(rootCommand);
                CacheManagement.RegisterCommands(rootCommand);
                ReposManagement.RegisterCommands(rootCommand);
                Other.RegisterCommands(rootCommand);
                
                return rootCommand.InvokeAsync(args).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILED: {e}");
                return 1;
            }
            finally
            {
                MutexLock.Unlock();
            }
        }
    }
}