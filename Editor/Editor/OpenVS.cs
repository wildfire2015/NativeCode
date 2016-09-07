using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Callbacks;
/*******************************************************************************
* 
*             类名: BuildPlayer
*             功能: 自动生成包,会把必要的操作全部执行一遍,时间较慢
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*            
*             
* *****************************************************************************/
public class OpenVS
{
    [MenuItem("OpenVS/OpenVS")]
    public static void OpenVS2015()
    {
        string mainpath = System.Environment.CurrentDirectory + "\\" + Directory.GetParent(Application.dataPath).Name + ".sln";
        if(File.Exists(mainpath))
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(mainpath);
            process.WaitForExit();
        }
        string nativepath = Directory.GetParent(System.Environment.CurrentDirectory) + "\\NativeCode\\NativeCode.sln";
        if (File.Exists(nativepath))
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(nativepath);
            process.WaitForExit();
        }
    }
   
}
