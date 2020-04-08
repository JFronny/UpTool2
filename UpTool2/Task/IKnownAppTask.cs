using System;
using System.Collections.Generic;
using System.Text;
using UpToolLib.DataStructures;

namespace UpTool2.Task
{
    abstract class IKnownAppTask : IAppTask
    {
        public abstract App App { get; }
        public abstract void Run();

        public override string ToString() => $"{TrimEnd(GetType().Name, "Task")} {App.Name}";
        
        private static string TrimEnd(string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }
    }
}
