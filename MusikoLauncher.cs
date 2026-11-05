using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            string executableFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string adjacentPlayer = Path.Combine(executableFolder, "index.html");
            string playerFile = File.Exists(adjacentPlayer) ? adjacentPlayer : ExtractBundledPlayer();
            string edge = FindEdge();
            if (edge == null)
            {
                MessageBox.Show("Microsoft Edge was not found. Install or repair Edge, then open Musiko again.", "Musiko", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = edge;
            start.Arguments = "--app=\"" + new Uri(playerFile).AbsoluteUri + "\" --new-window";
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            Process.Start(start);
        }
        catch (Exception error)
        {
            MessageBox.Show("Musiko could not start.\r\n\r\n" + error.Message, "Musiko", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string ExtractBundledPlayer()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Musiko");
        Directory.CreateDirectory(folder);
        string destination = Path.Combine(folder, "index.html");
        using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Musiko.Player.html"))
        {
            if (resource == null) throw new InvalidOperationException("The bundled player page is missing.");
            using (MemoryStream content = new MemoryStream())
            {
                resource.CopyTo(content);
                byte[] bundled = content.ToArray();
                bool matches = false;
                if (File.Exists(destination))
                {
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] current = File.ReadAllBytes(destination);
                        matches = Convert.ToBase64String(sha.ComputeHash(current)) == Convert.ToBase64String(sha.ComputeHash(bundled));
                    }
                }
                if (!matches)
                {
                    string temporary = destination + ".new";
                    File.WriteAllBytes(temporary, bundled);
                    if (File.Exists(destination)) File.Delete(destination);
                    File.Move(temporary, destination);
                }
            }
        }
        return destination;
    }

    private static string FindEdge()
    {
        string[] candidates = new string[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft\\Edge\\Application\\msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft\\Edge\\Application\\msedge.exe")
        };
        foreach (string candidate in candidates) if (File.Exists(candidate)) return candidate;
        string path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (string directory in path.Split(Path.PathSeparator))
        {
            try
            {
                string candidate = Path.Combine(directory.Trim(), "msedge.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch { }
        }
        return null;
    }
}
