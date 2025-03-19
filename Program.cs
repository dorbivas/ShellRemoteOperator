using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


namespace ShellRemoteOperator
{
    class Program
    {
        public static Logger logr = new Logger();

        public static async Task Main(string[] args)
        {
            // If no hosts are passed, add 'localhost' for demo
            List<string> hosts = args.ToList();
            hosts.Add("localhost");

            ShellController shellController = new ShellController(logr);
            await shellController.MockInvoker(hosts);

            // Optional: keep console open if you're running from Visual Studio
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public class Logger : ILogger
    {
        public async Task Log(string message, string level)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            // Build a single log entry string (add date/time if desired)
            //string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {level} | {Task.CurrentId?.ToString() ?? "null"} | {message}";
            string logEntry =  $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {level} | TID:{Thread.CurrentThread.ManagedThreadId} | {message}";       
            try
            {
                // 1) Print to console
                Console.WriteLine(logEntry);
                Console.WriteLine("[DEBUG] Log is being written to: " + logFilePath);

                // 2) Append to log file
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    await sw.WriteLineAsync(logEntry);
                    await sw.FlushAsync(); // Ensure the log is written to the file
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while writing to the log file: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access to the log file is denied: {ex.Message}");
            }
        }
    }
}
