using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main()
    {
        const string phpPingUrl = "https://sport.rmuti.ac.th/ping.php";
        const string phpCgiPath = @"C:\php\php-cgi.exe";
        const string phpCgiArgs = "-b 127.0.0.1:9000";

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        try
        {
            var response = await httpClient.GetAsync(phpPingUrl);
            var content = (await response.Content.ReadAsStringAsync()).Trim();

            if (content != "ping")
            {
                Console.WriteLine("Invalid response. Restarting php-cgi...");
                RestartPhpCgi(phpCgiPath, phpCgiArgs);
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now}] php-cgi OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] php-cgi unreachable: {ex.Message}");
            RestartPhpCgi(phpCgiPath, phpCgiArgs);
        }

        // หน่วงเวลาให้เห็นผลลัพธ์ก่อนโปรแกรมปิด
        Thread.Sleep(1500);
        Environment.Exit(0);

        return 0; // จะไม่ถึงจุดนี้เพราะ Environment.Exit จบโปรแกรมไปก่อน
    }

    static void RestartPhpCgi(string path, string args)
    {
        try
        {
            foreach (var process in Process.GetProcessesByName("php-cgi"))
            {
                process.Kill(true);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(startInfo);

            Console.WriteLine($"[{DateTime.Now}] php-cgi restarted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restarting php-cgi: {ex.Message}");
        }
    }
}