using System;
using System.IO;

namespace NPTP.InputSystemWrapper.Utilities
{
    internal static class FileReadWrite
    {
        internal static bool TryWriteToFile(string filePath, string fileContents)
        {
            try
            {
                File.WriteAllText(filePath, fileContents);
                return true;
            }
            catch (Exception e)
            {
                ISWDebug.Log($"File could not be read: {e.Message}");
                return false;
            }
        }

        internal static bool TryReadLinesFromFile(string filePath, out string fileContents)
        {
            fileContents = string.Empty;

            try
            {
                if (!File.Exists(filePath))
                    return false;

                fileContents = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                ISWDebug.Log($"File could not be read: {e.Message}");
            }
            
            return !string.IsNullOrEmpty(fileContents);
        }
    }
}