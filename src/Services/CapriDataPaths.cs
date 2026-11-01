using System;
using System.IO;

namespace CaprICE.Services
{
    public static class CapriDataPaths
    {
        public static string DirectoryPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri");
            }
        }
    }
}
