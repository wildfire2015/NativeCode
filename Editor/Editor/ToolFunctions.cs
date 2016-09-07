using UnityEngine;
using System.Collections;
using System.IO;

public class ToolFunctions
{

    public static void DeleteFolder(string dir)
    {
        dir = dir.Replace("/", "\\");
        if(Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }
    public static void CreateNewFolder(string dir)
    {
        DeleteFolder(dir);
        Directory.CreateDirectory(dir);
    }
    public static void CheckAndCreateFolder(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
   
    public static void CopyDirectory(string sourcePath, string destinationPath, string except = "",bool bcreatenewdestpath = true)
    {
        if(Directory.Exists(sourcePath))
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            if (bcreatenewdestpath)
            {
                CreateNewFolder(destinationPath);
            }
            else
            {
                CheckAndCreateFolder(destinationPath);
            }
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (except != "" && fsi.FullName.Contains(except))
                {
                    continue;
                }

                string destName = Path.Combine(destinationPath, fsi.Name);
                if (fsi is System.IO.FileInfo)
                    File.Copy(fsi.FullName, destName, true);
                else
                {
                    //Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName, "", bcreatenewdestpath);                   
                }
            }
        }
        
    }
    public static void ClearDirectory(string sourcePath, string except = "")
    {
        if(Directory.Exists(sourcePath))
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (except != "" && fsi.FullName.Contains(except))
                {
                    continue;
                }
                if (fsi is System.IO.FileInfo)
                    File.Delete(fsi.FullName);
                else
                {
                    ClearDirectory(fsi.FullName, except);
                    Directory.Delete(fsi.FullName);
                }
            }
        }
        
    }
    public static void ClearConsole()
    {
        // This simply does "LogEntries.Clear()" the long way:  
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
 
}