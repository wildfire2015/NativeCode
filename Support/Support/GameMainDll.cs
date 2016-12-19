using UnityEngine;
using PSupport.LoadSystem;
using PSupport;
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
            byte[] bytes = sripttext.bytes;
            Encryption enc = new Encryption();
            bytes = enc.Decrypt(bytes);
            _mStriptdll = System.Reflection.Assembly.Load(bytes);
            System.Type mainClass = _mStriptdll.GetType(_mMainClass);
            gameObject.AddComponent(mainClass);
            ResourceLoadManager.removeRes(_mScriptDllPath, typeof(TextAsset));
        }
        else if (loadedNotify == eLoadedNotify.Load_NotTotleSuccessfull)
        {
            if (ResourceLoadManager.mfuncDllLoadFailed != null)
            {
                ResourceLoadManager.mfuncDllLoadFailed();
            }
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
