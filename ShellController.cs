namespace ShellRemoteOperator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class ShellController
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, string> _osVersions = new ConcurrentDictionary<string, string>();

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
            int failCount = results.Count(result => !result.success);

            if (failCount == 0)
                await _logger.Log("Invoking complete!", "INFO");
            else
                await _logger.Log($"Invoking shell failed on {failCount} host(s)!", "ERROR");

            foreach (var kvp in _osVersions)
            {
                await _logger.Log($"Host: {kvp.Key}, OS Version: {kvp.Value}", "INFO");
            }
        }

        private async Task<(bool success, string osVersion)> InvokeShell(string host)
        {
            try
            {
                await _logger.Log($"Invoking on {host}...", "INFO");

                string osVersion = string.Empty;

                if (host == "localhost")
                {
                    // Use PowerShell to get the OS version
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = "-Command \"(Get-CimInstance Win32_OperatingSystem).Version\"",
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

                    if (!string.IsNullOrEmpty(error))
                    {
                        await _logger.Log($"Error: {error}", "ERROR");
                        throw new Exception(error);
                    }

                    osVersion = output.Trim();
                    await _logger.Log($"OS Version: {osVersion}", "INFO");
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
                    osVersion = "Simulated OS Version"; // Replace with actual OS version retrieval for remote hosts
                    await _logger.Log($"Invoking on {host} succeeded!", "INFO");
                }

                _osVersions[host] = osVersion;
                return (true, osVersion);
            }
            catch (Exception e)
            {
                await _logger.Log($"{e.Message} (Host: {host})", "ERROR");
                return (false, string.Empty);
            }
        }
    }
}
