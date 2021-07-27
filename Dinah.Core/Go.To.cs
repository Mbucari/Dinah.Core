﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dinah.Core
{
    public static class Go
    {
        public static class To
        {
            // from: https://stackoverflow.com/a/43232486
            public static void Url(string url)
            {
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", url);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            /// <summary>Open folder, select file. If a folderPath is a folder: open parent folder, select folder</summary>
            /// <param name="folderPath"></param>
            /// <returns>False if file/folder not exist</returns>
            public static bool File(string folderPath)
            {
                if (!System.IO.File.Exists(folderPath) && !System.IO.Directory.Exists(folderPath))
                    return false;
                Process.Start("explorer.exe", $"/select, \"{folderPath}\"");
                return true;
            }

            /// <summary>Open folder</summary>
            /// <param name="folderPath"></param>
            /// <returns>False if folder does not exist</returns>
            public static bool Folder(string folderPath)
            {
                if (!System.IO.Directory.Exists(folderPath))
                    return false;
                Process.Start("explorer.exe", $"\"{folderPath}\"");
                return true;
            }
        }
    }
}