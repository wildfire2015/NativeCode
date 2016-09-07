using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
/*******************************************************************************
* 
*             类名: BuildUIScriptFromPrefab
*             功能: 根据ui的prefab生成代码,包括初始化需要得到的控件
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*            
*             
* *****************************************************************************/

/// <summary>
///dicUIType.Add("Img"  ,  "Image");
///dicUIType.Add("Btn"  ,  "Button");
///dicUIType.Add("Txt"  ,  "Text");
///dicUIType.Add("Tran" ,  "Transform");
///dicUIType.Add("RecTran", "RectTransform");
///dicUIType.Add("Scrb" ,  "Scrollbar");
///dicUIType.Add("Sld"  ,  "Slider");
///dicUIType.Add("Ipt"  ,  "InputField")
///dicUIType.Add("Scrr" ,  "ScrollRect")
///dicUIType.Add("RImg" ,  "RawImage");
///dicUIType.Add("Tog"  ,  "Toggle");
/// </summary>
public class BuildUIScriptFromPrefab
{
    [MenuItem("Build/BuildUIScriptFromPrefab")]
    public static void BuildUIScript()
    {
        //UI类型缩写和类型的对应
        Dictionary<string, string> dicUIType = new Dictionary<string, string>();
        dicUIType.Add("Img", "Image");
        dicUIType.Add("Btn", "Button");
        dicUIType.Add("Txt", "Text");
        dicUIType.Add("Tran", "Transform");
        dicUIType.Add("RecTran", "RectTransform");
        dicUIType.Add("Scrb", "Scrollbar");
        dicUIType.Add("Sld", "Slider");
        dicUIType.Add("Ipt", "InputField");
        dicUIType.Add("Scrr", "ScrollRect");
        dicUIType.Add("RImg", "RawImage");
        dicUIType.Add("Tog", "Toggle");
        

        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("请选择需要生产脚本的UI对象!");
            return;
        }
        GameObject[] selectobjs = Selection.gameObjects;
        foreach (GameObject go in selectobjs)
        {
            //被选中的
            GameObject selectobj = go.transform.root.gameObject;
            if (selectobj.GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning("请选择需要生产脚本的UI对象!");
                return;
            }

            Transform[] transforms = selectobj.GetComponentsInChildren<Transform>(true);
            //需要获取控件的树路径
            string[] path = new string[transforms.Length];

            var uiitem = from trans in transforms where trans.name.Contains('_') && dicUIType.Keys.Contains(trans.name.Split('_')[0]) select trans;
            int itemnum = 0;

            //初始化代码
            string loadedcontant = "";
            //定义代码
            string memberstring = "";
            //释放代码
            string releasestring = "";

            foreach (Transform itemtran in uiitem)
            {
                memberstring += "public " + dicUIType[itemtran.name.Split('_')[0]] + " " + itemtran.name + " = null;\r\n\t";
                releasestring += itemtran.name + " = null;\r\n\t\t";
                Transform tr = itemtran;
                List<string> pathlist = new List<string>();
                pathlist.Add(tr.name);
                while (tr != itemtran.root)
                {
                    tr = tr.parent;
                    pathlist.Add(tr.name);
                }
                for (int i = pathlist.Count - 2; i >= 0; i--)
                {
                    path[itemnum] = path[itemnum] + "/" + pathlist[i];
                }
                path[itemnum] = path[itemnum].Substring(1);

                loadedcontant += itemtran.name + " = " + "mUIShowObj.transform.Find(\"" + path[itemnum] + "\").GetComponent<" + dicUIType[itemtran.name.Split('_')[0]] + ">();\r\n\t\t";
                itemnum++;
            }




            var scriptpaths =
                from findcspaths in AssetDatabase.GetAllAssetPaths()
                where findcspaths.Contains("Assets/Scripts") && Path.GetFileNameWithoutExtension(findcspaths) == selectobj.name
                select findcspaths;
            if (scriptpaths.Count() == 0)
            {//第一次创建

                string NewUIClass = Editor.Properties.Resources.NewUIClass;
                NewUIClass = NewUIClass.Replace("#UIName#", selectobj.name);
                NewUIClass = NewUIClass.Replace("#OnAutoLoadedUIObj#", loadedcontant);
                NewUIClass = NewUIClass.Replace("#OnAutoRelease#", releasestring);
                NewUIClass = NewUIClass.Replace("#Member#", memberstring);

                FileStream file = new FileStream(Application.dataPath + "/Scripts/" + selectobj.name + ".cs", FileMode.CreateNew);
                StreamWriter fileW = new StreamWriter(file, System.Text.Encoding.UTF8);
                fileW.Write(NewUIClass);
                fileW.Flush();
                fileW.Close();
                file.Close();


                Debug.Log("创建脚本 " + Application.dataPath + "/Scripts/" + selectobj.name + ".cs 成功!");

            }
            else if (scriptpaths.Count() > 1)
            {
                foreach (string cspath in scriptpaths)
                {
                    Debug.LogError("有重复脚本 " + cspath + "===" + selectobj.name + ".cs 更新失败!");
                }

            }
            else
            {

                string cspath = scriptpaths.ToArray<string>()[0];

                FileStream file = new FileStream(Path.GetFullPath(cspath), FileMode.Open);
                StreamReader filer = new StreamReader(file);
                string ExitUIClass = filer.ReadToEnd();
                filer.Close();
                file.Close();

                string splitstr = "//auto generatescript,do not make script under this line==";



                string unchangestr = ExitUIClass.Substring(0, ExitUIClass.IndexOf(splitstr));

                string NewUIClass = Editor.Properties.Resources.NewUIClass;
                string changestr = NewUIClass.Substring(NewUIClass.IndexOf(splitstr) + splitstr.Length, NewUIClass.Length - (NewUIClass.IndexOf(splitstr) + splitstr.Length));


                changestr = changestr.Replace("#OnAutoLoadedUIObj#", loadedcontant);
                changestr = changestr.Replace("#OnAutoRelease#", releasestring);
                changestr = changestr.Replace("#Member#", memberstring);

                string finalstr = unchangestr + splitstr + changestr;

                file = new FileStream(Path.GetFullPath(cspath), FileMode.Create);
                StreamWriter fileW = new StreamWriter(file, System.Text.Encoding.UTF8);
                fileW.Write(finalstr);
                fileW.Flush();
                fileW.Close();
                file.Close();

                Debug.Log("更新脚本 " + selectobj.name + ".cs 成功!");
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

        }
    }



}
