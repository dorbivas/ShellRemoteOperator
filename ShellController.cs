namespace ShellRemoteOperator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;

    public class ShellController
    {
        private readonly ILogger _logger;

        public ShellController(ILogger logger)
        {
            _logger = logger;
        }

        public async Task MockInvoker(List<string> hosts)
        {
            await _logger.Log("Invoking all hosts...", "INFO");

            // Create a list of tasks for each host
            var tasks = hosts.Select(host => InvokeShell(host)).ToList();

            // Wait for all the tasks to complete in parallel
            var results = await Task.WhenAll(tasks);

            // Count how many were unsuccessful
            int failCount = results.Count(success => !success);

            if (failCount == 0)
                await _logger.Log("Invoking complete!", "INFO");
            else
                await _logger.Log($"Invoking shell failed on {failCount} host(s)!", "ERROR");
        }

        private async Task<bool> InvokeShell(string host)
        {
            try
            {
                await _logger.Log($"Invoking on {host}...", "INFO");

                if (host == "localhost")
                {
                    // Launch cmd.exe, echo "hello world", wait 3 seconds, and exit
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c echo hello world && timeout /t 3",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    process.WaitForExit();

                    // If there's an error output, log and throw to simulate failure
                    if (!string.IsNullOrEmpty(error))
                    {
                        await _logger.Log($"Error: {error}", "ERROR");
                        throw new Exception(error);
                    }

                    await _logger.Log($"Shell Output: {output}", "INFO");
                }
                else
                {
                    // Simulate non-localhost: do some "work," possibly fail
                    await Task.Delay(1000);
                    if (new Random().Next(1, 4) == 2)
                    {
                        await _logger.Log($"Invoking failed on {host}!", "ERROR");
                        throw new Exception("Invoking failed!");
                    }
                    await _logger.Log($"Invoking on {host} succeeded!", "INFO");
                }

                return true;
            }
            catch (Exception e)
            {
                await _logger.Log($"{e.Message} (Host: {host})", "ERROR");
                return false;
            }
        }
    }
}
