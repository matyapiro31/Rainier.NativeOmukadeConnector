/*************************************************************************
* Rainier Native Omukade Connector
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using System;
using System.Collections.Concurrent;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Rainier.NativeOmukadeConnector
{
    internal class FileIOUtils
    {
        internal static ConcurrentBag<string> LoadFile(string path, string format = "text")
        {
            // Read all file data as string. If format is directory, return all files in directory.
            if (format != "directory" && !File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }
            if (format == "directory" && !Directory.Exists(path))
            {
                Plugin.SharedLogger.LogInfo($"Directory not found: {path}. Creating empty directory...");
                Directory.CreateDirectory(path);
                return new ConcurrentBag<string>();
            }
            switch (format)
            {
                case "text":
                    // Split fileData by new line and return as ConcurrentBag.
                    string fileData = File.ReadAllText(path);
                    return new ConcurrentBag<string>(fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                case "json":
                    throw new NotImplementedException("JSON format not implemented.");
                case "image-zip":
                    FastZip fastZip = new FastZip();
                    string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    try
                    {
                        fastZip.ExtractZip(path, tempDir, null);
                        string[] imageFiles = Directory.GetFiles(tempDir);
                        ConcurrentBag<string> imageBase64Strings = new ConcurrentBag<string>();
                        foreach (string imageFile in imageFiles)
                        {
                            // Return all image files as base64 string in ConcurrentBag.
                            byte[] imageBytes = File.ReadAllBytes(imageFile);
                            string base64String = Convert.ToBase64String(imageBytes);
                            imageBase64Strings.Add(base64String);
                            File.Delete(imageFile);
                        }
                        return imageBase64Strings;
                    }
                    catch (Exception e)
                    {
                        Plugin.SharedLogger.LogError($"Error extracting zip file: {e.Message}");
                        return new ConcurrentBag<string>();
                    }
                case "directory":
                    // Return all files in directory as ConcurrentBag.
                    string[] files = Directory.GetFiles(path);
                    ConcurrentBag<string> filesData = new ConcurrentBag<string>();
                    foreach (string file in files)
                    {
                        filesData.Add(File.ReadAllText(file));
                    }
                    return filesData;
                default:
                    throw new NotImplementedException($"{format} format not implemented.");
            }
        }
    }
}