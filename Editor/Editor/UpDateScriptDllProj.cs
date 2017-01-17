using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Xml;
using System;
using System.Collections.Generic;
/*******************************************************************************
* 
*             类名: UpDateScriptDllProj
*             功能: 生成/更新SpriteDll工程,自动关联工程中Scrpit文件夹下的代码
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*             备注:
*             
* *****************************************************************************/
public class UpDateScriptDllProj
{
    [MenuItem("Build/BulidNativeScriptDll")]
    public static void BulidNativeScriptDll()
    {
        BulidNativeScriptToDll();
    }
    //[MenuItem("Build/BulidScriptDll")]
    public static void BulidScriptDll()
    {
        BulidScriptDllBytes(EditorUserBuildSettings.activeBuildTarget);

    }
    private static string getCSProjectName()
    {
        DirectoryInfo dinf = new DirectoryInfo(System.Environment.CurrentDirectory);
        string dirname = Path.GetFileNameWithoutExtension(System.Environment.CurrentDirectory);
        foreach (FileInfo fileinfo in dinf.GetFiles("*.csproj", SearchOption.TopDirectoryOnly))
        {
            if (fileinfo.Name.Contains(dirname) && !fileinfo.Name.Contains("Editor"))
            {
                return fileinfo.FullName;
            }
        }
        Debug.LogError("没有找到" + dirname + ".csproj工程!,生成代码dll失败!请先生成项目的csproj工程!");
        return "";
    }
    private static void BulidScriptDllBytes(BuildTarget bt)
    {
        if (bt == BuildTarget.iOS)
        {
            return;
        }
        
        string Scriptdllcsproj = Editor.Properties.Resources.Scriptdllcsproj;
        
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(Scriptdllcsproj);
        XmlDocument xmldocGameTrunk = new XmlDocument();
        xmldocGameTrunk.Load(getCSProjectName());
        
        string debugmacro = xmldocGameTrunk.GetElementsByTagName("DefineConstants").Item(0).InnerText;
        debugmacro = debugmacro.Replace("UNITY_EDITOR;", "");
        debugmacro = debugmacro.Replace("UNITY_EDITOR_64;", "");
        debugmacro = debugmacro.Replace("UNITY_EDITOR_WIN;", "");
        string releasemacro = xmldocGameTrunk.GetElementsByTagName("DefineConstants").Item(1).InnerText;
        releasemacro = releasemacro.Replace("DEBUG;", "");
        releasemacro = releasemacro.Replace("UNITY_EDITOR;", "");
        releasemacro = releasemacro.Replace("UNITY_EDITOR_64;", "");
        releasemacro = releasemacro.Replace("UNITY_EDITOR_WIN;", "");
        XmlNode includenode = xmldoc.GetElementsByTagName("ItemGroup").Item(1);
        includenode.RemoveAll();
        
        string[] allassetpaths = AssetDatabase.GetAllAssetPaths();

        var scriptpaths =
            from path in allassetpaths
            where path.Contains("Assets/Scripts/") && !Directory.Exists(path) && Path.GetExtension(path) == ".cs"
            select path.Replace('/','\\');

        foreach(string scriptpath in scriptpaths)
        {
            XmlElement compile = xmldoc.CreateElement("Compile",xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
            compile.SetAttribute("Include", scriptpath);
            includenode.AppendChild(compile);
        }

        #region 计算MD5
        List<byte> listMacroAndScriptBytes = new List<byte>();
        DirectoryInfo ScriptsFolder = new DirectoryInfo(Application.dataPath + "/Scripts");
        foreach (FileInfo item in ScriptsFolder.GetFiles("*.cs",SearchOption.AllDirectories))
        {
            listMacroAndScriptBytes.AddRange(File.ReadAllBytes(item.FullName));
        }
        listMacroAndScriptBytes.AddRange(System.Text.Encoding.UTF8.GetBytes(releasemacro));
        byte[] hashbyte = listMacroAndScriptBytes.ToArray();

        System.Security.Cryptography.MD5CryptoServiceProvider md5CSP = new System.Security.Cryptography.MD5CryptoServiceProvider();

        byte[] scriptMD5Bytes = md5CSP.ComputeHash(hashbyte);

        string scriptMD5 = System.BitConverter.ToString(scriptMD5Bytes);
        #endregion

        XmlNode referencenode = xmldoc.GetElementsByTagName("ItemGroup").Item(0);
        referencenode.RemoveAll();
        XmlNode trunkreferencenode = xmldocGameTrunk.GetElementsByTagName("ItemGroup").Item(0);
        
        foreach (var  node in trunkreferencenode.ChildNodes)
        {
            XmlNode xmlnode = (node as XmlNode).Clone();

            XmlElement reference = xmldoc.CreateElement("Reference", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
            if (!xmlnode.Attributes["Include"].InnerText.Contains("UnityEditor"))
            {
                reference.SetAttribute("Include", xmlnode.Attributes["Include"].InnerText);

                if (xmlnode.ChildNodes.Count > 0)
                {
                    XmlNode pathnode = xmldoc.CreateNode(XmlNodeType.Element, "HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
                    pathnode.InnerText = xmlnode.FirstChild.InnerText;
                    reference.AppendChild(pathnode);
                }
                referencenode.AppendChild(reference);
            }

            //XmlNode node = xmldoc.CreateNode(XmlNodeType.Element, "HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
            //node.InnerText = refrencedllpath;
            //reference.AppendChild(node);

        }
        //XmlElement assetmblereference = xmldoc.CreateElement("Reference", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
        //assetmblereference.SetAttribute("Include", "Assembly-CSharp");
        //XmlNode assetmpathnode = xmldoc.CreateNode(XmlNodeType.Element, "HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
        //assetmpathnode.InnerText = "Temp/UnityVS_bin/Release/Assembly-CSharp.dll".Replace('/','\\');
        //assetmblereference.AppendChild(assetmpathnode);
        //referencenode.AppendChild(assetmblereference);
        //var refrencedllpaths =
        //    from path in allassetpaths
        //    where path.Contains("Assets/Plugins") && !Directory.Exists(path) && Path.GetExtension(path).Contains("dll")
        //    select path.Replace('/', '\\');

        //foreach (string refrencedllpath in refrencedllpaths)
        //{
        //    XmlElement reference = xmldoc.CreateElement("Reference", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
        //    reference.SetAttribute("Include", Path.GetFileNameWithoutExtension(refrencedllpath));
        //    XmlNode node = xmldoc.CreateNode(XmlNodeType.Element,"HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
        //    node.InnerText = refrencedllpath;
        //    reference.AppendChild(node);
        //    referencenode.AppendChild(reference);
        //}
        
        XmlNode UnityBuildTarget = xmldoc.GetElementsByTagName("UnityBuildTarget").Item(0);
        UnityBuildTarget.InnerText = xmldocGameTrunk.GetElementsByTagName("UnityBuildTarget").Item(0).InnerText;

        XmlNode TargetFrameworkVersion = xmldoc.GetElementsByTagName("TargetFrameworkVersion").Item(0);
        TargetFrameworkVersion.InnerText = xmldocGameTrunk.GetElementsByTagName("TargetFrameworkVersion").Item(0).InnerText;

        XmlNode TargetFrameworkProfile = xmldoc.GetElementsByTagName("TargetFrameworkProfile").Item(0);
        TargetFrameworkProfile.InnerText = xmldocGameTrunk.GetElementsByTagName("TargetFrameworkProfile").Item(0).InnerText;

        XmlNode UnityProjectType = xmldoc.GetElementsByTagName("UnityProjectType").Item(0);
        UnityProjectType.InnerText = xmldocGameTrunk.GetElementsByTagName("UnityProjectType").Item(0).InnerText;


        XmlNode UnityVersion = xmldoc.GetElementsByTagName("UnityVersion").Item(0);
        UnityVersion.InnerText = Application.unityVersion;


        XmlNode DefineConstantsdebug = xmldoc.GetElementsByTagName("DefineConstants").Item(0);
        DefineConstantsdebug.InnerText = debugmacro;

        DefineConstantsdebug = xmldoc.GetElementsByTagName("DefineConstants").Item(1);
        DefineConstantsdebug.InnerText = releasemacro;


        string Scriptdllprojpath = System.Environment.CurrentDirectory + "/Scriptdll.csproj";
        xmldoc.Save(Scriptdllprojpath);



        

        //System.Diagnostics.Process process = System.Diagnostics.Process.Start("C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\MSBuild", "/p:Configuration=Release " + Scriptdllprojpath);

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //process.StartInfo.FileName = "C:\\Program Files\\Unity\\MonoDevelop\\bin\\MSBuild\\dotnet.4.0\\MonoDevelop.Projects.Formats.MSBuild.exe";
        process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\MSBuild.exe";
        process.StartInfo.Arguments = "/p:Configuration=Release " + Scriptdllprojpath;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        process.WaitForExit();
  

        
        
        string srcdll = Environment.CurrentDirectory + "/Temp/ScriptDll_bin/Release/scriptdll.dll";
        string destdll = Application.dataPath + "/Resources/assetsbundles/scriptdll/scriptdll.bytes";
        string md5file = Application.dataPath + "/Resources/assetsbundles/scriptdll/scriptdllmd5.bytes";
        ToolFunctions.CreateNewFolder(Application.dataPath + "/Resources/assetsbundles/scriptdll");
        try
        {
            File.Copy(srcdll, destdll);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            return;
        }
        PSupport.Encryption ecp = new PSupport.Encryption();
        byte[] bytes = File.ReadAllBytes(destdll);
        bytes = ecp.Encrypt(bytes);
        File.WriteAllBytes(destdll, bytes);
        File.WriteAllText(md5file, scriptMD5);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        ToolFunctions.DeleteFolder(Environment.CurrentDirectory + "/Temp/ScriptDll_bin");
        ToolFunctions.DeleteFolder(Environment.CurrentDirectory + "/Temp/ScriptDll_obj");


        File.Delete(Scriptdllprojpath);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();




        Debug.Log("生成:Assets/Resources/assetsbundles/scriptdll/Scriptdll.bytes----成功!!!");

    }

    private static void BulidNativeScriptToDll()
    {
      
        string NativeScriptdllcsproj = Editor.Properties.Resources.NativeScriptdllcsproj;
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(NativeScriptdllcsproj);
        XmlDocument xmldocGameTrunk = new XmlDocument();
        xmldocGameTrunk.Load(getCSProjectName());
        XmlNode includenode = xmldoc.GetElementsByTagName("ItemGroup").Item(1);
        includenode.RemoveAll();

        DirectoryInfo dirinfo = new DirectoryInfo(System.Environment.CurrentDirectory + "\\NativeScriptForDll\\");
        FileInfo[] fileinfos = dirinfo.GetFiles();
        
        //string[] allassetpaths = AssetDatabase.GetAllAssetPaths();

        //var scriptpaths =
        //    from path in allassetpaths
        //    where path.Contains("Editor/NativeScriptForDll") && !Directory.Exists(path)
        //    select path.Replace('/', '\\');

        foreach (FileInfo scriptpathinfo in fileinfos)
        {
            string scriptpath = scriptpathinfo.FullName;
            if (Path.GetExtension(scriptpath) == ".cs")
            {
                XmlElement compile = xmldoc.CreateElement("Compile", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
                compile.SetAttribute("Include", scriptpath);
                includenode.AppendChild(compile);
            }
            
        }
        XmlNode referencenode = xmldoc.GetElementsByTagName("ItemGroup").Item(0);
        referencenode.RemoveAll();
        XmlNode trunkreferencenode = xmldocGameTrunk.GetElementsByTagName("ItemGroup").Item(0);

        foreach (var node in trunkreferencenode.ChildNodes)
        {
            XmlNode xmlnode = (node as XmlNode).Clone();
            if (xmlnode.InnerText.Contains("NativeScript"))
            {
                continue;
            }
            XmlElement reference = xmldoc.CreateElement("Reference", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
            if (!xmlnode.Attributes["Include"].InnerText.Contains("UnityEditor"))
            {
                reference.SetAttribute("Include", xmlnode.Attributes["Include"].InnerText);

                if (xmlnode.ChildNodes.Count > 0)
                {
                    XmlNode pathnode = xmldoc.CreateNode(XmlNodeType.Element, "HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
                    pathnode.InnerText = xmlnode.FirstChild.InnerText;
                    reference.AppendChild(pathnode);
                }
                referencenode.AppendChild(reference);
            }

            //XmlNode node = xmldoc.CreateNode(XmlNodeType.Element, "HintPath", xmldoc.GetElementsByTagName("Project").Item(0).NamespaceURI);
            //node.InnerText = refrencedllpath;
            //reference.AppendChild(node);

        }

        string Scriptdllprojpath = System.Environment.CurrentDirectory + "/NativeScriptDll.csproj";
        xmldoc.Save(Scriptdllprojpath);





        //System.Diagnostics.Process process = System.Diagnostics.Process.Start("C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\MSBuild", "/p:Configuration=Release " + Scriptdllprojpath);

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //process.StartInfo.FileName = "C:\\Program Files\\Unity\\MonoDevelop\\bin\\MSBuild\\dotnet.12.0\\MonoDevelop.Projects.Formats.MSBuild.exe";
        process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\MSBuild.exe";
        process.StartInfo.Arguments = "/p:Configuration=Release " + Scriptdllprojpath;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        process.WaitForExit();


       


        string srcdll = Environment.CurrentDirectory + "/Temp/NativeStriptDll_bin/Release/NativeScript.dll";
        string destdll = Application.dataPath + "/Plugins/NativeScript.dll";

        try
        {
            File.Copy(srcdll, destdll,true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        File.Copy(srcdll, destdll,true);
        ToolFunctions.DeleteFolder(Environment.CurrentDirectory + "/Temp/NativeStriptDll_bin");
        ToolFunctions.DeleteFolder(Environment.CurrentDirectory + "/Temp/NativeStriptDll_obj");
        
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        AssetDatabase.ImportAsset("Assets/Plugins/", ImportAssetOptions.ForceSynchronousImport);

        File.Delete(Scriptdllprojpath);


        Debug.Log(Application.dataPath + "/Plugins/NativeScript.dll ----成功!!!");
    }
}
