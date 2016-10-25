﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
/*******************************************************************************
* 
*             类名: LoadAsset
*             功能: 资源加载器
*             作者: 彭谦
*             日期: 2014.6.13
*             修改: 2015.12.7
*             
* *****************************************************************************/

namespace PSupport
{
    namespace LoadSystem
    {
        

        internal class LoadAsset : MonoBehaviour
        {
            //static Dictionary<string, Object> _mDicAssetRefCount = new Dictionary<string, Object>();
            //static List<Object> _mListReleasedObjects = new List<Object>();
            static public LoadAsset getInstance()
            {
                return SingleMono.getInstance<LoadAsset>() as LoadAsset;
            }
            public void loadAsset(string sAssetPath, eLoadResPath eloadrespath, string sInputPath, System.Type type, string tag,string sResGroupkey, Hash128 hash, bool basyn, bool bNoUseCatching, bool bautoReleaseBundle, bool bOnlyDownload, bool bloadfromfile)
            {//异步加载
                StartCoroutine(beginToLoad(sAssetPath, eloadrespath,sInputPath, type, tag, sResGroupkey, hash ,basyn, bNoUseCatching,bautoReleaseBundle, bOnlyDownload, bloadfromfile));
            }
            public IEnumerator beginToLoad(string sAssetPath, eLoadResPath eloadrespath, string sInputPath, System.Type type, string tag,string sResGroupkey, Hash128 hash, bool basyn, bool bNoUseCatching,bool bautoReleaseBundle,bool bOnlyDownload,bool bloadfromfile)
            {

                //请求时候的bundle路径
                //请求时候的asset路径
                string assetsbundlepath;
                string assetname;
                string sinputbundlename;
                if (sAssetPath.Contains("|"))
                {
                    assetsbundlepath = sAssetPath.Split('|')[0];
                    assetname = sAssetPath.Split('|')[1];
                    sinputbundlename = Path.GetDirectoryName(sInputPath);
                }
                else
                {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                    assetsbundlepath = sAssetPath;
                    assetname = string.Empty;
                    sinputbundlename = sInputPath;


                }
                //CLog.Log("start to load===" + assetname);
                

                //资源标示
                string sReskey = (sAssetPath + ":" + type.ToString());

                //包路径Hash

                //记录此bundle被要求加载的协程次数,外部可能在一个资源加载挂起时多次调用同一资源的加载协程
                string sAssetbundlepath = assetsbundlepath;





                
                if (_mDicAssetNum.ContainsKey(sReskey))
                {
                    _mDicAssetNum[sReskey]++;
                }
                else
                {
                    _mDicAssetNum.Add(sReskey, 1);
                }
                while (mListLoadingBundle.Count > 5)
                {
                    yield return 1;
                }


                AssetBundle nowAssetBundle = null;

                if (mDicLoadedBundle.ContainsKey(sAssetbundlepath))
                {//如果加载好的bundle,直接取
                    nowAssetBundle = mDicLoadedBundle[sAssetbundlepath];
                }
                else if (mListLoadingBundle.Contains(sAssetbundlepath))
                {
                    while (!mDicLoadedBundle.ContainsKey(sAssetbundlepath))
                    {//这里挂起所有非第一次加载bundl的请求
                        yield return 1;
                    }
                    nowAssetBundle = mDicLoadedBundle[sAssetbundlepath];
                }
                else
                {//这里是第一次加载该bundle
                    //将该bundle加入正在加载列表
                    mListLoadingBundle.Add(sAssetbundlepath);
                    string finalloadbundlepath = "";
                    
                    
                    AssetBundleCreateRequest abcr = null;

                    //如果是从远程下载
                    if (eloadrespath == eLoadResPath.RP_URL)
                    {
                        if (ResourceLoadManager._mbNotDownLoad == true)
                        {//如果设置了不下载资源
                            if (CacheBundleInfo.hasBundle(sinputbundlename))
                            {//如果caching有同名文件,从caching里直接读取
                             //下载路径
                                finalloadbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + sinputbundlename;
                            }
                            else
                            {//否则从包里读取
                                finalloadbundlepath = sAssetbundlepath;
                            }
                        }
                        //检查cache配置,如果还没有,或者不使用caching,则从资源服务器下载该bundle
                        else if (!CacheBundleInfo.isCaching(sinputbundlename, hash.ToString()) || bNoUseCatching)
                        {
                            DLoger.Log("WebRquest开始下载bundle:=" + sAssetbundlepath);
                            UnityWebRequest webrequest =  UnityWebRequest.Get(sAssetbundlepath);


                            yield return webrequest.Send();

                            //下载完毕,存入缓存路径
                            if (webrequest.isError)
                            {
                                DLoger.LogError("download=" + sAssetbundlepath + "=failed!=" + webrequest.error);
                                //下载失败
                                ResourceLoadManager._removeLoadingResFromList(sReskey);
                                ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, false);
                            }
                            else
                            {
                                DLoger.Log("WebRquest成功下载bundle:=" + sAssetbundlepath);
                                if (!bNoUseCatching)
                                {//如果使用caching,则将下载的bundle写入指定路径

                                    //下载路径
                                    finalloadbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + sinputbundlename;
                                    DLoger.Log("开始写入Caching:bundle:=" + finalloadbundlepath);
                                    string dir = Path.GetDirectoryName(finalloadbundlepath);
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }
                                    if (File.Exists(finalloadbundlepath))
                                    {
                                        File.Delete(finalloadbundlepath);
                                    }

                                    FileStream fs = new FileStream(finalloadbundlepath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, webrequest.downloadHandler.data.Length);
                                    fs.Write(webrequest.downloadHandler.data, 0, webrequest.downloadHandler.data.Length);
                                    fs.Flush();
                                    fs.Close();
                                    fs.Dispose();

                                    //写入caching配置
                                    CacheBundleInfo.updateBundleInfo(sinputbundlename, hash.ToString());
                                    CacheBundleInfo.saveBundleInfo();
                                    DLoger.Log("成功写入Caching:bundle:=" + finalloadbundlepath);
                                }
                                else
                                {
                                    if (!bOnlyDownload)
                                    {
                                        
                                        abcr = AssetBundle.LoadFromMemoryAsync(webrequest.downloadHandler.data);
                                        yield return abcr;
                                        
                                        if (abcr.isDone)
                                        {
                                            nowAssetBundle = abcr.assetBundle;
                                        }
                                        abcr = null;
                                    }
                                    

                                }
                            }
                            //下载完毕释放webrequest
                            if (webrequest != null)
                            {
                                webrequest.Dispose();
                                webrequest = null;
                            }
                            

                        }
                        else if (CacheBundleInfo.isCaching(sinputbundlename, hash.ToString()))
                        {
                            //下载路径
                            finalloadbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + sinputbundlename;
                        }
                       
                    }
                    else if (eloadrespath == eLoadResPath.RP_Caching)
                    {
                        if (CacheBundleInfo.hasBundle(sinputbundlename))
                        {//如果caching有同名文件,从caching里直接读取
                            //下载路径
                            finalloadbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + sinputbundlename;
                        }
                        else
                        {//否则从包里读取
                            finalloadbundlepath = ResourceLoadManager.mResourceStreamingAssets + "/" + sinputbundlename;
                        }
                    }
                    else
                    {//否则就是读取包中的路径
                        finalloadbundlepath = sAssetbundlepath;
                    }
  

                    if (nowAssetBundle == null && finalloadbundlepath != "")
                    {//如果bundle没有创建(如果没创建,则说明是下载下来并且caching,或者直接读app包;如果创建了,则是url读取并且不caching这种情况)

                        if (!bOnlyDownload)
                        {
                            if (bloadfromfile)
                            {
                                DLoger.Log("开始加载bundle:AssetBundle.LoadFromFile= " + finalloadbundlepath);
                                //nowAssetBundle = AssetBundle.LoadFromFile(finalloadbundlepath);
                                abcr = AssetBundle.LoadFromFileAsync(finalloadbundlepath);
                                yield return abcr;

                                if (abcr.isDone)
                                {
                                    nowAssetBundle = abcr.assetBundle;
                                }
                                abcr = null;

                            }
                            else
                            {//从memery加载,对于小而多的Object的加载这个IO更少,但是内存会更大
                                DLoger.Log("开始加载bundle:AssetBundle.LoadFromMemery= " + finalloadbundlepath);
                                byte[] bts = null;
                                WWW www = null;
                                if (eloadrespath == eLoadResPath.RP_URL || eloadrespath == eLoadResPath.RP_Caching)
                                {//从caching加载
                                    bts = File.ReadAllBytes(finalloadbundlepath);
                                    
                                }
                                else
                                {

                                    string wwwpath = ResourceLoadManager.mResourceStreamingAssetsForWWW + sinputbundlename;
                                    DLoger.Log("开始www= " + wwwpath);
                                    www = new WWW(wwwpath);
                                    yield return www;
                                    if (www.isDone && www.error == null)
                                    {
                                        bts = www.bytes;
                                    }
                                    else
                                    {
                                        DLoger.LogError(www.error);
                                    }
                                }
                                if (bts != null)
                                {
                                    //nowAssetBundle = AssetBundle.LoadFromMemory(bts);
                                    abcr = AssetBundle.LoadFromMemoryAsync(bts);
                                    yield return abcr;

                                    if (abcr.isDone)
                                    {
                                        nowAssetBundle = abcr.assetBundle;
                                    }
                                }
                                
                                abcr = null;
                                if (www != null)
                                {
                                    www.Dispose();
                                    www = null;
                                }
                            }
                            
                        }
                        
                    }
                    mListLoadingBundle.Remove(sAssetbundlepath);
                    mDicLoadedBundle.Add(sAssetbundlepath, nowAssetBundle);

                }

                if (nowAssetBundle != null)
                {//加载assetsbundle成功

                    /*注释掉秒删,会造成资源重复加载
                    //缓存中不用的的资源,秒删
                    //Caching.expirationDelay = 1;
                    */
                    //DLoger.Log("成功加载bundle : " + assetsbundlepath + "===successful!");


                    AssetBundle assetbundle = nowAssetBundle;
                    //AssetBundle assetbundle = mywww.assetBundle;
                    //AssetBundle assetbundle = mDicLoadedBundle[sAssetbundlepath];

                    if (assetname != string.Empty)
                    {
                        //DLoger.Log("开始读取= " + assetname + "= in =" + assetsbundlepath);

                        Object t = null;
                        //开始加载asset
                        if (basyn)
                        {//如果是异步加载

                            if (mDicLoadingAssets.ContainsKey(sReskey))
                            {//如果正在加载,则返回等待

                                while (!ResourceLoadManager._isLoadedRes(sReskey))
                                {
                                    yield return 1;
                                }

                            }
                            else
                            {//否则,开始加载

                                //文件对象名称
                                //CLog.Log("begin to load asset ==" + assetname);
                                if (!mDicLoadingAssetstime.ContainsKey(sReskey))
                                {
                                    mDicLoadingAssetstime.Add(sReskey, Time.realtimeSinceStartup);
                                }
                                AssetBundleRequest request = assetbundle.LoadAssetAsync(assetname, type);
                                mDicLoadingAssets.Add(sReskey, request);

                                //第一个要求加载此资源的在这挂起
                                yield return request;

                            }
                            //加载完毕
                            //                    CLog.Log("load asset ==" + assetname + "===successful!");
                            AssetBundleRequest myrequest = mDicLoadingAssets[sReskey];


                            t = myrequest.asset as Object;
                            myrequest = null;

                        }
                        else
                        {
                            //CLog.Log("begin to load asset ==" + assetname);
                            if (!mDicLoadingAssetstime.ContainsKey(sReskey))
                            {
                                mDicLoadingAssetstime.Add(sReskey, Time.realtimeSinceStartup);
                            }
                            
                            t = assetbundle.LoadAsset(assetname, type) as Object;

                        }

                        if (t != null)
                        {//加载成功,加入资源管理器,执行回调
                            float fusetime = -1.0f;
                            if (mDicLoadingAssetstime.ContainsKey(sReskey))
                            {
                                fusetime = (Time.realtimeSinceStartup - mDicLoadingAssetstime[sReskey]);
                            }
                            DLoger.Log("assetbundle.LoadAsset:成功读取= " + assetname + "= in =" + sAssetbundlepath + "===successful!time :" + fusetime);

                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, t, tag, sInputPath);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, true);
                        }
                        else
                        {

                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, false);
                            DLoger.LogError("Load===" + sAssetPath + "===Failed");
                        }
                        mDicLoadingAssetstime.Remove(sReskey);
                    }
                    else
                    {//只加载assetbundle的资源,不加载asset的时候的操作

                        if (bautoReleaseBundle)
                        {
                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                        }
                        else
                        {
                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, assetbundle, tag);
                        }


                        ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, true);

                    }

                }
                else
                {//只是下载bundle,并不加载
                    ResourceLoadManager._removeLoadingResFromList(sReskey);
                    ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, true);
                }




                //处理完此资源的加载协程,对请求此资源的加载协程计数减一
                _mDicAssetNum[sReskey]--;

                if (_mDicAssetNum[sReskey] == 0)
                {//如果所有加载此资源的协程都处理完毕,释放资源
                    mDicLoadingAssets.Remove(sReskey);
                    _mDicAssetNum.Remove(sReskey);
                }

            }


            void Start()
            {
                //Unity 不支持 gc 通知的函数
                //System.GC.RegisterForFullGCNotification(99, 99);
                //StartCoroutine(_releaseResLoop());
                //StartCoroutine(waitForGCComplete());
                //StartCoroutine(unloadBundleRun());
            }
            void Update()
            {
                _releaseResLoop();
                _waitForGCComplete();
                _unloadBundleRun();
            }
            /// <summary>
            /// 每隔一定时间,释放资源
            /// </summary>
            /// <returns></returns>
            private void _releaseResLoop()
            {
                List<Object> listReleasedObjects = ResourceLoadManager._mListReleasedObjects;


                if (listReleasedObjects.Count > 0)
                {
                    //if (ResourceLoadManager.mbEditorMode == false)
                    //{
                    //for (int i = 0; i < listReleasedObjects.Count; i++)
                    //{
                    //    Resources.UnloadAsset(listReleasedObjects[i]);
                    //}
                    //}

                    listReleasedObjects.Clear();
                    //if (ResourceLoadManager.mbEditorMode == true)
                    //{
                    //这里释放所有未引用的资源,因为Resources.UnloadAsset会导致unity editor crash,可能是unity5.4的bug

                    ResourceLoadManager._beginUnloadUnUsedAssets();


                    //}

                }



            }

            private void _waitForGCComplete()
            {
                //while (true)
                //{
                if (ResourceLoadManager.mbUnLoadUnUsedResDone == false || ResourceLoadManager.mbStartDoUnload == true)
                {
                    if (ResourceLoadManager.mbStartDoUnload == true)
                    {
                        if (mao == null)
                        {
                            DLoger.Log("开始执行 Resources.UnloadUnusedAssets()");
                            mao = Resources.UnloadUnusedAssets();
                        }
                        
                        if (mao.isDone)
                        {
                            DLoger.Log("====开始GC====");
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();

                            ResourceLoadManager.mbStartDoUnload = false;
                            ResourceLoadManager.mbUnLoadUnUsedResDone = true;
                            mao = null;
                        }
      
                        
                    }

                }
                //yield return 1;
                //yield return new WaitForSeconds(5);
                //}
            }
            private void _unloadBundleRun()
            {

                if (ResourceLoadManager.mBAutoRelease == true)
                {
                    float fnowtime = Time.time;
                    if (fnowtime - ResourceLoadManager._mfReleaseBundleTime > 2.0f)
                    {
                        ResourceLoadManager._mfReleaseBundleTime = fnowtime;
                        Dictionary<string, int>.Enumerator it = ResourceLoadManager._mDicBundlescounts.GetEnumerator();
                        while (it.MoveNext())
                        {
                            string sAssetbundlepath = it.Current.Key;
                            if (ResourceLoadManager._getDepBundleUesed(sAssetbundlepath))
                            {
                                continue;
                            }
                            else
                            {

                                //如果用到这个bundle的协程全部结束

                                if (mDicLoadedBundle.ContainsKey(sAssetbundlepath))
                                {

                                    //已经被释放(加载过程中,某些bundle计数为0了之后,没有马上调用unload,然后新的加载需求又使得计数增加,就会造成多次unload请求,所以有空的情况产生)
                                    //if (mywww.assetBundle != null)
                                    //{
                                    if (mDicLoadedBundle[sAssetbundlepath] != null)
                                    {
                                        mDicLoadedBundle[sAssetbundlepath].Unload(false);
                                    }

                                    //mywww.Dispose();

                                    //}
                                    mDicLoadedBundle.Remove(sAssetbundlepath);
                                    DLoger.Log("释放bundle:=" + sAssetbundlepath);
                                    //mDicLoadedBundle[sAssetbundlepath].Unload(false);
                                    //mDicLoadedBundle.Remove(sAssetbundlepath);

                                    //DLoger.Log("www count:" + mDicLoadedWWW.Count);
                                }



                                //if (mDicLoadedWWW.Count == 1 && mDicLoadingWWW.Count == 0)
                                //{
                                //    foreach (int item in mDicLoadedWWW.Keys)
                                //    {
                                //        //DLoger.Log(mDicbundleNum[item] + "," + item);

                                //        DLoger.Log(mDicLoadedWWW[item].assetBundle.name);

                                //    }
                                //}
                                //DLoger.Log(mDicLoadedWWW.Count + "," + mDicLoadingWWW.Count);
                            }
                        }
                    }
                    
                   

                }


            }
            internal void reset()
            {
                _mDicAssetNum = new Dictionary<string, int>();
                mao = null;
                Dictionary<string, AssetBundle>.Enumerator it = mDicLoadedBundle.GetEnumerator();
                while (it.MoveNext())
                {
                    it.Current.Value.Unload(false);
                }
                mDicLoadedBundle = new Dictionary<string, AssetBundle>();
                mListLoadingBundle = new List<string>();
                mDicLoadingAssets = new Dictionary<string, AssetBundleRequest>();
                mDicLoadingAssetstime = new Dictionary<string, float>();
            }   
            //记录同一资源加载协程的个数
            private Dictionary<string, int> _mDicAssetNum = new Dictionary<string, int>();
            //记录同一assetsbundle加载协程的个数
            //private Dictionary<string, Hashtable> _mDicbundleNum = new Dictionary<string, Hashtable>();

            AsyncOperation mao = null;

            ////记录正在下载的UnityWebRequest
            //private Dictionary<string, UnityWebRequest> mDicLoadingWebRequest = new Dictionary<string, UnityWebRequest>();
            ////记录已经下载的UnityWebRequest
            //private Dictionary<string, UnityWebRequest> mDicLoadedWebRequest = new Dictionary<string, UnityWebRequest>();

            //记录正在加载的AssetBundleCreateRequest
            //private Dictionary<string, AssetBundleCreateRequest> mDicLoadingBundleRequest = new Dictionary<string, AssetBundleCreateRequest>();
            //记录正在加载的AssetBundleCreateRequest
            //private List<string> mListLoadingBundleRequest = new List<string>();
            //记录已经加载的bundle
            private Dictionary<string, AssetBundle> mDicLoadedBundle = new Dictionary<string, AssetBundle>();
            //记录正在加载的bundle
            private List<string> mListLoadingBundle = new List<string>();
            //记录正在加载的资源请求
            private Dictionary<string, AssetBundleRequest> mDicLoadingAssets = new Dictionary<string, AssetBundleRequest>();
            //记录正在加载的资源请求的时间
            private Dictionary<string, float> mDicLoadingAssetstime = new Dictionary<string, float>();

        }
       
    }
   
}


