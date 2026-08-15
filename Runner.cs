using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var psi = new ProcessStartInfo("dotnet", "run --project src/SportsBooking.API/SportsBooking.API.csproj")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var proc = Process.Start(psi);
        await Task.Delay(5000); 

        try 
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
            var client = new HttpClient(handler);
            var result = await client.GetAsync("https://127.0.0.1:55814/swagger/v1/swagger.json");
            var content = await result.Content.ReadAsStringAsync();
            Console.WriteLine("STATUS: " + result.StatusCode);
            Console.WriteLine(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        proc.Kill(true);
    }
}
