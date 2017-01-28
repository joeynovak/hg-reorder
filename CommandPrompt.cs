using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Stn.Svn
{
   public class CommandPrompt
   {
      public static bool LastCommandPassed = true;
      public static string lastCommandError = "";
      public const string UNSUCCESSFUL = "ACKACKACKACK-FAILED";

      public static string RunCommand(string command, bool echoOutput = true, int timeOutInSeconds = 0)
      {
         LastCommandPassed = true;
         StringBuilder sbOut = new StringBuilder();
         StringBuilder sbErr = new StringBuilder();

         Process process = StartCommand(command + " & if errorlevel 1 echo " + UNSUCCESSFUL);
         bool done = false;
         ThreadStart threadStartStdErr = () =>
         {
            while (!process.HasExited)
            {
               string temp = "";

               if (echoOutput && !process.StandardError.EndOfStream)
               {
                  temp = process.StandardError.ReadLine();
                  sbErr.Append(temp + "\r\n");
                  Console.Write(temp + "\r\n");
               }
               
               Thread.Sleep(10);
            }
         };

         Thread threadStdErr = new Thread(threadStartStdErr);

         ThreadStart threadStartStdOut = () =>
         {
            while (!process.HasExited)
            {
               string temp = "";

               if (echoOutput && !process.StandardOutput.EndOfStream)
               {
                  temp = process.StandardOutput.ReadLine();
                  sbOut.Append(temp + "\r\n");
                  Console.Write(temp + "\r\n");
               }
               
               Thread.Sleep(10);
            }
         };

         Thread threadStdOut = new Thread(threadStartStdOut);


         threadStdErr.Start();
         threadStdOut.Start();

         long timeToKillProcessInTicks = DateTime.Now.Ticks + timeOutInSeconds*TimeSpan.TicksPerSecond;

         while (!process.HasExited && (timeOutInSeconds == 0 || DateTime.Now.Ticks < timeToKillProcessInTicks))
         {                                       
            Thread.Sleep(10);
         }

         if(!process.HasExited)
            process.Kill();

         long timeToKillThreadsInTicks = DateTime.Now.Ticks + 30 * TimeSpan.TicksPerSecond;

         while((threadStdErr.IsAlive || threadStdOut.IsAlive) && DateTime.Now.Ticks < timeToKillThreadsInTicks)
            Thread.Sleep(1);

         if(threadStdErr.IsAlive)
            threadStdErr.Abort();

         if (threadStdOut.IsAlive)
            threadStdOut.Abort();

         string result = sbOut.ToString();
         lastCommandError = sbErr.ToString();

         if (result.Split(new string[] { UNSUCCESSFUL}, StringSplitOptions.RemoveEmptyEntries).Length > 2)
            LastCommandPassed = false;

         return sbOut.ToString();         
      }

      public static Process StartCommand(string command)
      {
         Process process = new Process();
         process.StartInfo.FileName = "cmd.exe";
         process.StartInfo.RedirectStandardInput = true;
         process.StartInfo.RedirectStandardOutput = true;
         process.StartInfo.RedirectStandardError = true;
         process.StartInfo.CreateNoWindow = true;
         process.StartInfo.UseShellExecute = false;
         process.Start();

         process.StandardInput.WriteLine(command);
         process.StandardInput.WriteLine("exit");
         process.StandardInput.Flush();
         process.StandardInput.Close();
         return process;

      }
   }
}
