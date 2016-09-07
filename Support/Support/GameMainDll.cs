using UnityEngine;
using PSupport.LoadSystem;
/// <summary>
/// 
/// </summary>
public class GameMainDll : MonoBehaviour {

    string _mScriptDllPath = "assetsbundles/scriptdll/scriptdll";
    string _mMainClass = "GameMain";
    System.Reflection.Assembly _mStriptdll = null;

    void Start()
    {
        ResourceLoadManager.requestRes(_mScriptDllPath, typeof(TextAsset),eLoadResPath.RP_URL,onLoadedScriptDll);
    }
    //public void lanch()
    //{
    //    ResourceLoadManager.requestRes(_mScriptDllPath, typeof(TextAsset), onLoadedScriptDll);

    //}
    void onLoadedScriptDll(object o, eLoadedNotify loadedNotify)
    {
        if(loadedNotify == eLoadedNotify.Load_Successfull)
        {
            TextAsset sripttext = ResourceLoadManager.getRes(_mScriptDllPath, typeof(TextAsset)) as TextAsset;
            _mStriptdll = System.Reflection.Assembly.Load(sripttext.bytes);
            System.Type mainClass = _mStriptdll.GetType(_mMainClass);
            gameObject.AddComponent(mainClass);
        }
    }
    /// <summary>
    /// 获取链接库中脚本类型
    /// </summary>
    /// <param name="sScriptName">脚本名字</param>
    /// <returns></returns>
	public System.Type getScriptType(string sScriptName)
    {
        return _mStriptdll.GetType(sScriptName);
    }
    //public string MainClass
    //{
    //    set { _mMainClass = value; }
    //}
    //public string ScriptDllPath
    //{
    //    set { _mScriptDllPath = value; }
    //}


}
