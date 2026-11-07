using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

internal static class Program
{
    private static string AppFolder
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Musiko"); }
    }

    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeFolder = Path.GetDirectoryName(exePath);
            bool inAppFolder = string.Equals(
                Path.GetFullPath(exeFolder).TrimEnd('\\'),
                Path.GetFullPath(AppFolder).TrimEnd('\\'),
                StringComparison.OrdinalIgnoreCase);

            if (args.Length > 0 && args[0].Equals("--install", StringComparison.OrdinalIgnoreCase))
            {
                CreateShortcuts(exePath);
                MessageBox.Show("Musiko shortcuts were created on your Desktop and in the Start Menu.", "Musiko", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string adjacentPlayer = Path.Combine(exeFolder, "index.html");
            string playerFile = (!inAppFolder && File.Exists(adjacentPlayer)) ? adjacentPlayer : ExtractBundledPlayer();
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

            // First run from a downloaded copy: offer shortcuts once.
            string marker = Path.Combine(AppFolder, "shortcuts.done");
            if (!inAppFolder && !File.Exists(marker))
            {
                Directory.CreateDirectory(AppFolder);
                File.WriteAllText(marker, "asked");
                DialogResult answer = MessageBox.Show(
                    "Add Musiko shortcuts to your Desktop and Start Menu?\r\n\r\nMusiko will be kept in your user folder, so you can delete this downloaded file afterwards.",
                    "Musiko", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes) CreateShortcuts(exePath);
            }
        }
        catch (Exception error)
        {
            MessageBox.Show("Musiko could not start.\r\n\r\n" + error.Message, "Musiko", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void CreateShortcuts(string exePath)
    {
        Directory.CreateDirectory(AppFolder);
        string installed = Path.Combine(AppFolder, "Musiko.exe");
        if (!string.Equals(Path.GetFullPath(exePath), Path.GetFullPath(installed), StringComparison.OrdinalIgnoreCase))
            File.Copy(exePath, installed, true);

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string programs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Musiko");
        Directory.CreateDirectory(programs);
        MakeShortcut(Path.Combine(desktop, "Musiko.lnk"), installed);
        MakeShortcut(Path.Combine(programs, "Musiko.lnk"), installed);
    }

    private static void MakeShortcut(string shortcutPath, string target)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        object shell = Activator.CreateInstance(shellType);
        object link = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
        Type linkType = link.GetType();
        linkType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, link, new object[] { target });
        linkType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, link, new object[] { Path.GetDirectoryName(target) });
        linkType.InvokeMember("Description", BindingFlags.SetProperty, null, link, new object[] { "Open the Musiko music player" });
        linkType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, link, new object[] { target + ",0" });
        linkType.InvokeMember("Save", BindingFlags.InvokeMethod, null, link, null);
    }

    private static string ExtractBundledPlayer()
    {
        string folder = AppFolder;
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
