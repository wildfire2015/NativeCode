using UnityEngine;
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
            public void loadAsset(string sAssetPath, eLoadResPath eloadrespath, string sInputPath, System.Type type,
                string tag, string sResGroupkey, string md5, bool basyn, bool bNoUseCatching,
                bool bautoReleaseBundle, bool bOnlyDownload, bool bloadfromfile)
            {//异步加载
                Hashtable loadparam = new Hashtable();
                loadparam["sAssetPath"] = sAssetPath;
                loadparam["eloadrespath"] = eloadrespath;
                loadparam["sInputPath"] = sInputPath;
                loadparam["type"] = type;
                loadparam["tag"] = tag;
                loadparam["sResGroupkey"] = sResGroupkey;
                loadparam["md5"] = md5;
                loadparam["basyn"] = basyn;
                loadparam["bNoUseCatching"] = bNoUseCatching;
                loadparam["bautoReleaseBundle"] = bautoReleaseBundle;
                loadparam["bOnlyDownload"] = bOnlyDownload;
                loadparam["bloadfromfile"] = bloadfromfile;

                _mListLoadingRequest.Add(loadparam);

                // StartCoroutine(beginToLoad(sAssetPath, eloadrespath,sInputPath, type, tag, sResGroupkey, hash ,basyn, bNoUseCatching,bautoReleaseBundle, bOnlyDownload, bloadfromfile));
            }
            public IEnumerator beginToLoad(string sAssetPath, eLoadResPath eloadrespath, string sInputPath, System.Type type, string tag, string sResGroupkey, string md5, bool basyn, bool bNoUseCatching, bool bautoReleaseBundle, bool bOnlyDownload, bool bloadfromfile)
            {

                //请求时候的bundle路径
                //请求时候的asset路径
                string assetsbundlepath;
                string assetname;
                string sinputbundlename;
                string sinputbundlenamewithoutpostfix;
                if (sAssetPath.Contains("|"))
                {
                    assetsbundlepath = sAssetPath.Split('|')[0];
                    assetname = sAssetPath.Split('|')[1];
                    sinputbundlenamewithoutpostfix = Path.GetDirectoryName(sInputPath);
                    sinputbundlename = sinputbundlenamewithoutpostfix + ResourceLoadManager.msBundlePostfix;
                }
                else
                {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                    assetsbundlepath = sAssetPath;
                    assetname = string.Empty;
                    sinputbundlenamewithoutpostfix = sInputPath;
                    sinputbundlename = sInputPath + ResourceLoadManager.msBundlePostfix;
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
                //while (ListLoadingBundle.Count > 5)
                //{
                //    yield return 1;
                //}


                AssetBundle nowAssetBundle = null;
                bool bdownloadbundlesuccess = true;
                Dictionary<string, AssetBundle> DicLoadedBundle = ResourceLoadManager._mDicLoadedBundle;
                List<string> ListLoadingBundle = ResourceLoadManager._mListLoadingBundle;

                if (DicLoadedBundle.ContainsKey(sAssetbundlepath))
                {//如果加载好的bundle,直接取
                    nowAssetBundle = DicLoadedBundle[sAssetbundlepath];
                    if (nowAssetBundle == null)
                    {
                        DLoger.LogError("loaded bundle== " + sAssetbundlepath + "is null");
                    }
                }
                else if (ListLoadingBundle.Contains(sAssetbundlepath))
                {
                    while (!DicLoadedBundle.ContainsKey(sAssetbundlepath))
                    {//这里挂起所有非第一次加载bundl的请求
                        yield return 1;
                    }
                    nowAssetBundle = DicLoadedBundle[sAssetbundlepath];
                    if (nowAssetBundle == null)
                    {
                        DLoger.LogError("loaded bundle== " + sAssetbundlepath + "is null");
                    }
                }
                else
                {//这里是第一次加载该bundle
                    //将该bundle加入正在加载列表
                    ListLoadingBundle.Add(sAssetbundlepath);
                    string finalloadbundlepath = "";



                    AssetBundleCreateRequest abcr = null;

                    //如果是从远程下载
                    if (eloadrespath == eLoadResPath.RP_URL)
                    {
                        if (ResourceLoadManager._mbNotDownLoad == true)
                        {//如果设置了不下载资源
                            if (CacheBundleInfo.hasBundle(sinputbundlenamewithoutpostfix))
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
                        else if (!CacheBundleInfo.isCaching(sinputbundlenamewithoutpostfix, md5.ToString()) || bNoUseCatching)
                        {
                            DLoger.Log("WebRquest开始下载bundle:=" + sAssetbundlepath);
                            UnityWebRequest webrequest = UnityWebRequest.Get(sAssetbundlepath);
                            AsyncOperation asop = webrequest.Send();

                            Dictionary<string, ulong> dicdownbundle = ResourceLoadManager.mDicDownloadingBundleBytes;
                            if (!dicdownbundle.ContainsKey(sinputbundlename))
                            {
                                dicdownbundle.Add(sinputbundlename, 0);
                            }
                            else
                            {
                                DLoger.LogError("重复下载bundle：" + sAssetbundlepath);
                                dicdownbundle[sinputbundlename] = 0;
                            }
                            while (!asop.isDone)
                            {
                                dicdownbundle[sinputbundlename] = webrequest.downloadedBytes;
                                //DLoger.Log("downloadbundle data bytes:" + sAssetbundlepath + ":" + dicdownbundle[sAssetbundlepath],"down");
                                yield return null;
                            }
                            dicdownbundle[sinputbundlename] = webrequest.downloadedBytes;
                            DLoger.Log("downloadbundle data bytes:" + sAssetbundlepath + ":" + dicdownbundle[sinputbundlename], "down");
                            //下载完毕,存入缓存路径
                            if (webrequest.isError)
                            {
                                bdownloadbundlesuccess = false;
                                DLoger.LogError("download=" + sAssetbundlepath + "=failed!=" + webrequest.error);
                                //下载失败
                                ResourceLoadManager._removeLoadingResFromList(sReskey);
                                ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath,sInputPath, false);
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
                                    CacheBundleInfo.updateBundleInfo(sinputbundlenamewithoutpostfix, md5.ToString());
                                    CacheBundleInfo.saveBundleInfo();
                                    DLoger.Log("成功写入Caching:bundle:=" + finalloadbundlepath);
                                }
                                else
                                {
                                    if (!bOnlyDownload)
                                    {
                                        DLoger.Log("LoadFromMemoryAsync:" + sAssetbundlepath);
                                        abcr = AssetBundle.LoadFromMemoryAsync(webrequest.downloadHandler.data);
                                        yield return abcr;

                                        if (abcr.isDone)
                                        {
                                            nowAssetBundle = abcr.assetBundle;

                                        }
                                        else
                                        {
                                            bdownloadbundlesuccess = false;
                                            DLoger.LogError("LoadFromMemoryAsync=" + sAssetbundlepath + "=failed!=");
                                            //下载失败
                                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, false);
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
                        else if (CacheBundleInfo.isCaching(sinputbundlenamewithoutpostfix, md5.ToString()))
                        {
                            //下载路径
                            finalloadbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + sinputbundlename;
                        }

                    }
                    else if (eloadrespath == eLoadResPath.RP_Caching)
                    {
                        if (CacheBundleInfo.hasBundle(sinputbundlenamewithoutpostfix))
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
                                else
                                {
                                    bdownloadbundlesuccess = false;
                                    DLoger.LogError("LoadFromMemoryAsync=" + sAssetbundlepath + "=failed!=");
                                    //下载失败
                                    ResourceLoadManager._removeLoadingResFromList(sReskey);
                                    ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, false);
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
                                    else
                                    {
                                        bdownloadbundlesuccess = false;
                                        DLoger.LogError("LoadFromMemoryAsync=" + sAssetbundlepath + "=failed!=");
                                        //下载失败
                                        ResourceLoadManager._removeLoadingResFromList(sReskey);
                                        ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, false);
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
                    ListLoadingBundle.Remove(sAssetbundlepath);
                    if (nowAssetBundle != null)
                    {
                        DicLoadedBundle.Add(sAssetbundlepath, nowAssetBundle);
                    }


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

                            if (_mDicLoadingAssets.ContainsKey(sReskey))
                            {//如果正在加载,则返回等待

                                while (!_mDicLoadingAssets[sReskey].isDone)
                                {
                                    yield return 1;
                                }

                            }
                            else
                            {//否则,开始加载

                                //文件对象名称
                                //CLog.Log("begin to load asset ==" + assetname);
                                if (!_mDicLoadingAssetstime.ContainsKey(sReskey))
                                {
                                    _mDicLoadingAssetstime.Add(sReskey, Time.realtimeSinceStartup);
                                }
                                AssetBundleRequest request = assetbundle.LoadAssetAsync(assetname, type);
                                _mDicLoadingAssets.Add(sReskey, request);

                                //第一个要求加载此资源的在这挂起
                                yield return request;

                            }
                            //加载完毕
                            //                    CLog.Log("load asset ==" + assetname + "===successful!");
                            AssetBundleRequest myrequest = _mDicLoadingAssets[sReskey];
                            t = myrequest.asset as Object;
                            myrequest = null;

                            //处理完此资源的加载协程,对请求此资源的加载协程计数减一
                            _mDicAssetNum[sReskey]--;

                            if (_mDicAssetNum[sReskey] == 0)
                            {//如果所有加载此资源的协程都处理完毕,释放资源
                                _mDicLoadingAssets.Remove(sReskey);
                                _mDicAssetNum.Remove(sReskey);
                            }

                        }
                        else
                        {
                            //CLog.Log("begin to load asset ==" + assetname);
                            if (!_mDicLoadingAssetstime.ContainsKey(sReskey))
                            {
                                _mDicLoadingAssetstime.Add(sReskey, Time.realtimeSinceStartup);
                            }

                            t = assetbundle.LoadAsset(assetname, type) as Object;

                            //处理完此资源的加载协程,对请求此资源的加载协程计数减一
                            _mDicAssetNum[sReskey]--;

                            if (_mDicAssetNum[sReskey] == 0)
                            {//如果所有加载此资源的协程都处理完毕,释放资源
                                _mDicLoadingAssets.Remove(sReskey);
                                _mDicAssetNum.Remove(sReskey);
                            }

                        }
                        if (ResourceLoadManager.mbLoadAssetWait)
                        {
                            yield return 1;
                        }
                        _miloadingAssetNum--;
                        //DLoger.Log("加载=" + sAssetPath + "=完毕当前_miloadingAssetNum - 1:" + _miloadingAssetNum);
                        if (t != null)
                        {//加载成功,加入资源管理器,执行回调
                            float fusetime = -1.0f;
                            if (_mDicLoadingAssetstime.ContainsKey(sReskey))
                            {
                                fusetime = (Time.realtimeSinceStartup - _mDicLoadingAssetstime[sReskey]);
                            }
                            DLoger.Log("assetbundle.LoadAsset:成功读取= " + assetname + "= in =" + sAssetbundlepath + "===successful!time :" + fusetime);

                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, t, tag, sInputPath);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, true);
                        }
                        else
                        {

                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, false);
                            DLoger.LogError("Load===" + sAssetPath + "===Failed");
                        }
                        _mDicLoadingAssetstime.Remove(sReskey);
                    }
                    else
                    {//只加载assetbundle的资源,不加载asset的时候的操作

                        if (ResourceLoadManager.mbLoadAssetWait)
                        {
                            yield return 1;
                        }
                        _miloadingAssetNum--;
                        //DLoger.Log("加载=" + sAssetPath + "=完毕当前_miloadingAssetNum - 1:" + _miloadingAssetNum);
                        if (bautoReleaseBundle)
                        {
                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                        }
                        else
                        {
                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, assetbundle, tag);
                        }


                        ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, true);

                    }

                }
                else if (assetname == string.Empty)
                {//如果不加载asset,说明只是下载bundle,并不加载
                    if (ResourceLoadManager.mbLoadAssetWait)
                    {
                        yield return 1;
                    }
                    _miloadingAssetNum--;
                    //DLoger.Log("加载=" + sAssetPath + "=完毕当前_miloadingAssetNum - 1:" + _miloadingAssetNum);
                    if (bdownloadbundlesuccess)
                    {
                        ResourceLoadManager._removeLoadingResFromList(sReskey);
                        ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, true);
                    }
                }
                else
                {//bundle下载出错,依赖其的assetname加载也算出错
                    if (ResourceLoadManager.mbLoadAssetWait)
                    {
                        yield return 1;
                    }
                    _miloadingAssetNum--;
                    //DLoger.Log("加载=" + sAssetPath + "=完毕当前_miloadingAssetNum - 1:" + _miloadingAssetNum);
                    ResourceLoadManager._removeLoadingResFromList(sReskey);
                    ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, sAssetPath, sInputPath, false);
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
                _LoadAssetList();
                //_releaseResLoop();
                _unloadBundleRun();
               // _waitForGCComplete();
                

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

                    //ResourceLoadManager._beginUnloadUnUsedAssets();


                    //}

                }



            }

            private void _waitForGCComplete()
            {
                //while (true)
                //{
                if (ResourceLoadManager._mbUnLoadUnUsedResDone == false && ResourceLoadManager.mbStartDoUnload == true)
                {
                    
                    if (_mao == null)
                    {
                        DLoger.Log("开始执行 Resources.UnloadUnusedAssets()");
                        ResourceLoadManager._mListReleasedObjects.Clear();
                        _mao = Resources.UnloadUnusedAssets();
                        //_mfunloadtime = Time.time;
                    }

                    if (_mao.isDone /*|| Time.time - _mfunloadtime > 3.0f*/)
                    {
                        DLoger.Log("====开始GC====");
                        System.GC.Collect();
                        //System.GC.WaitForPendingFinalizers();
                        System.GC.Collect();
                        //System.GC.WaitForPendingFinalizers();
                        DLoger.Log("====GC完毕====");
                        ResourceLoadManager.mbStartDoUnload = false;
                        ResourceLoadManager._mbUnLoadUnUsedResDone = true;
                        _mao = null;
                        //_mfunloadtime = 0;
                    }
                    //else
                    //{
                    //    DLoger.LogWarning("Resources.UnloadUnusedAssets() 正在等待!");
                    //}

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
                    if (fnowtime - ResourceLoadManager._mfLastReleaseBundleTime > 2.0f)
                    {
                        ResourceLoadManager._mfLastReleaseBundleTime = fnowtime;

                        Dictionary<string, AssetBundle> DicLoadedBundle = ResourceLoadManager._mDicLoadedBundle;

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

                                if (DicLoadedBundle.ContainsKey(sAssetbundlepath))
                                {

                                    //已经被释放(加载过程中,某些bundle计数为0了之后,没有马上调用unload,然后新的加载需求又使得计数增加,就会造成多次unload请求,所以有空的情况产生)
                                    //if (mywww.assetBundle != null)
                                    //{
                                    if (DicLoadedBundle[sAssetbundlepath] != null)
                                    {
                                        DLoger.Log("释放bundle:=" + sAssetbundlepath);
                                        DicLoadedBundle[sAssetbundlepath].Unload(false);
                                    }

                                    //mywww.Dispose();

                                    //}
                                    DicLoadedBundle.Remove(sAssetbundlepath);

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
                        bool bbundlereleased = ResourceLoadManager.checkBundleReleased();
                        if (bbundlereleased && _miloadingAssetNum == 0)
                        {//非常驻bundle都释放,并且没有正在加载的协程
                            if (ResourceLoadManager._mSetRemovedObjects.Count != 0)
                            {
                                HashSet<string>.Enumerator ithash = ResourceLoadManager._mSetRemovedObjects.GetEnumerator();
                                while(ithash.MoveNext())
                                {
                                    ResourceLoadManager._removeRes(ithash.Current);
                                }
                                ResourceLoadManager._mSetRemovedObjects.Clear();
                                DLoger.Log("DestroyImmediate Objects 完毕!");
                            }
                            //如果这里调用在GC完毕之后,会有逻辑层判断是否GC完毕卡死的风险,故而不能在这里调用,但是现在把 _waitForGCComplete()
                            //放到同一桢的统一函数内部,保证调用次序,所以这里可以加上

                            ResourceLoadManager._beginUnloadUnUsedAssets();

                            List<Object> listReleasedObjects = ResourceLoadManager._mListReleasedObjects;
                            if (listReleasedObjects.Count > 0)
                            {

                                for (int i = 0; i < listReleasedObjects.Count; i++)
                                {
                                    Resources.UnloadAsset(listReleasedObjects[i]);
                                }
                                listReleasedObjects.Clear();
                                //Resources.UnloadUnusedAssets();

                            }

                        }
                        else
                        {
                            DLoger.Log("bundle没有释放完,或者加载Asset没有完毕!:" + bbundlereleased + ":" + _miloadingAssetNum);
                        }
                        _waitForGCComplete();

                    }



                }




            }

            private void _LoadAssetList()
            {

                while ((_miloadingAssetNum < ResourceLoadManager.miMaxLoadAssetsNum || ResourceLoadManager.miMaxLoadAssetsNum == -1) && _mListLoadingRequest.Count > 0)
                {
                    List<Hashtable>.Enumerator it = _mListLoadingRequest.GetEnumerator();
                    if (it.MoveNext())
                    {
                        Hashtable loadparam = it.Current;
                        string sAssetPath = (string)loadparam["sAssetPath"];
                        eLoadResPath eloadrespath = (eLoadResPath)loadparam["eloadrespath"];
                        string sInputPath = (string)loadparam["sInputPath"];
                        System.Type type = (System.Type)loadparam["type"];
                        string tag = (string)loadparam["tag"];
                        string sResGroupkey = (string)loadparam["sResGroupkey"];
                        string hash = (string)loadparam["md5"];
                        bool basyn = (bool)loadparam["basyn"];
                        bool bNoUseCatching = (bool)loadparam["bNoUseCatching"];
                        bool bautoReleaseBundle = (bool)loadparam["bautoReleaseBundle"];
                        bool bOnlyDownload = (bool)loadparam["bOnlyDownload"];
                        bool bloadfromfile = (bool)loadparam["bloadfromfile"];
                        StartCoroutine(beginToLoad(sAssetPath, eloadrespath, sInputPath, type, tag, sResGroupkey, hash, basyn, bNoUseCatching, bautoReleaseBundle, bOnlyDownload, bloadfromfile));
                        _miloadingAssetNum++;
                        //DLoger.Log("加载=" + sAssetPath + "=完毕当前_miloadingAssetNum + 1:" + _miloadingAssetNum);
                        _mListLoadingRequest.Remove(loadparam);
                    }



                }

            }

            internal void reset()
            {
                _mDicAssetNum = new Dictionary<string, int>();
                _mao = null;
                _mDicLoadingAssets = new Dictionary<string, AssetBundleRequest>();
                _mDicLoadingAssetstime = new Dictionary<string, float>();
                _mListLoadingRequest = new List<Hashtable>();
                _miloadingAssetNum = 0;
            }
            //记录同一资源加载协程的个数
            private Dictionary<string, int> _mDicAssetNum = new Dictionary<string, int>();
            //记录同一assetsbundle加载协程的个数
            //private Dictionary<string, Hashtable> _mDicbundleNum = new Dictionary<string, Hashtable>();
            /// <summary>
            /// unload异步等待
            /// </summary>
            private AsyncOperation _mao = null;
            //private float _mfunloadtime = 0;

            ////记录正在下载的UnityWebRequest
            //private Dictionary<string, UnityWebRequest> mDicLoadingWebRequest = new Dictionary<string, UnityWebRequest>();
            ////记录已经下载的UnityWebRequest
            //private Dictionary<string, UnityWebRequest> mDicLoadedWebRequest = new Dictionary<string, UnityWebRequest>();

            //记录正在加载的AssetBundleCreateRequest
            //private Dictionary<string, AssetBundleCreateRequest> mDicLoadingBundleRequest = new Dictionary<string, AssetBundleCreateRequest>();
            //记录正在加载的AssetBundleCreateRequest
            //private List<string> mListLoadingBundleRequest = new List<string>();

            //记录正在加载的资源请求
            private Dictionary<string, AssetBundleRequest> _mDicLoadingAssets = new Dictionary<string, AssetBundleRequest>();
            //记录正在加载的资源请求的时间
            private Dictionary<string, float> _mDicLoadingAssetstime = new Dictionary<string, float>();

            private List<Hashtable> _mListLoadingRequest = new List<Hashtable>();
            /// <summary>
            /// 
            /// </summary>
            private int _miloadingAssetNum = 0;

        }

    }

}


