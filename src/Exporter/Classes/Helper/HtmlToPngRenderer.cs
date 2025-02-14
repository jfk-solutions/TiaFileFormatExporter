using System.Diagnostics;

public class HtmlToPngRenderer
{
    public static async Task RenderHtmlToPngAsync(string url, string outputPath, int width, int height)
    {
        var chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        var args = $"--headless --screenshot --window-size={width}x{height} --disable-software-rasterizer --screenshot=\"{outputPath}\" {url}";

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    await process.WaitForExitAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
