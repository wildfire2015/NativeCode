using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*******************************************************************************
 * 
 *             类名: ResourceLoadManager
 *             功能: 资源加载管理器
 *             作者: 彭谦
 *             日期: 2014.6.13
 *             修改:
 *             备注:该资源加载模块是适应unity5的新的加载机制,用提供的公共接口(若干重载)加载资源,资源原文件夹路径都为Resources下的路径,配合打包插件,会打包对应的
 *             assetbundle文件,只需修改开关mbusepack便可在源资源和打包资源之间切换,无需关心依赖资源,和缓存资源的管理,此模块会自动从下载路径(资源服务器)下载更新
 *             的assetbundle文件,如果没有更新的assetbundle,默认用缓存中的
 *             
 * *****************************************************************************/



namespace PSupport
{
    namespace LoadSystem
    {
        /// <summary>
        /// 回调函数的委托
        /// </summary>
        /// <param name="obj">回调参数</param>
        /// <param name="loadedNotify">加载结果</param>
        public delegate void ProcessDelegateArgc(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull);

        /// <summary>
        /// 资源加载管理器
        /// </summary>
        public class ResourceLoadManager
        {

            private ResourceLoadManager() { }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath">路径</param>
            /// <param name="proc">加载结果回调</param>
            /// <param name="o">回调传递参数</param>
            /// <param name="basyn">解压是否异步</param>
            /// <param name="stag">资源tag</param>
            public static void requestRes(string spath, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag, bool bloadfromfile = true)
            {
                if (mbuseassetbundle)
                {
                    _checkDependenceList(new CloadParam(spath, typeof(Object), eLoadResPath.RP_URL, stag, proc, o, basyn, bloadfromfile));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    string[] stags = new string[1];
                    spaths[0] = spath;
                    types[0] = typeof(Object);
                    eloadResTypes[0] = eLoadResPath.RP_Resources;
                    stags[0] = stag;
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type">资源类型</param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn">解压是否异步</param>
            /// <param name="stag">资源tag</param>
            public static void requestRes(string spath, System.Type type, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag,bool bloadfromfile = true)
            {
                if (mbuseassetbundle)
                {
                    _checkDependenceList(new CloadParam(spath, type, eLoadResPath.RP_URL, stag, proc, o, basyn, bloadfromfile));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    string[] stags = new string[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eLoadResPath.RP_Resources;
                    stags[0] = stag;
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath">资源路径名称数组(相对于Resources/),不要带扩展名</param>
            /// <param name="eloadResType">加载资源的路径类型,如果设置了Resources,是不受管理器mbuseassetbundle影响的</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调函数的参数</param>
            /// <param name="basyn"> 解压是否异步</param>
            /// <param name="stag">资源tag</param>

            public static void requestRes(string spath, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag,bool bloadfromfile = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, typeof(Object), eloadResType, stag, proc, o, basyn,bloadfromfile));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    string[] stags = new string[1];
                    spaths[0] = spath;
                    types[0] = typeof(Object);
                    eloadResTypes[0] = eloadResType;
                    stags[0] = stag;
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>


            public static void requestRes(string[] spaths, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = typeof(Object);
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    string[] stags = new string[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        stags[i] = mSdefaultTag;
                    }
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile));
                }
                else
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = typeof(Object);
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    string[] stags = new string[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        stags[i] = mSdefaultTag;
                    }
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }


            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="type">类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="stag">资源tag</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>

            public static void requestRes(string[] spaths, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag, bool bloadfromfile = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = type;
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    string[] stags = new string[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        stags[i] = stag;
                    }
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile));
                }
                else
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = type;
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    string[] stags = new string[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        stags[i] = stag;
                    }
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }

            }


            /// <summary>
            /// 加载某个bundle但是不加载依赖,一般作为更新包用
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="stag">资源tag</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>

            public static void requestDownloadBundle(string[] spaths, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = typeof(AssetBundle);
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    string[] stags = new string[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        stags[i] = stag;
                    }
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn,true,false,true,true);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="stag">资源tag</param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn">解压是否异步</param>
            /// 
            /// <returns></returns>

            public static void requestRes(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag, bool bloadfromfile = true)
            {

                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, type, eloadResType, stag, proc, o, basyn,bloadfromfile));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    string[] stags = new string[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eloadResType;
                    stags[0] = stag;
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }


            }

            /// <summary>
            /// 读取的资源的bundle不会自动释放,要手动调用unloadNoAutoReleasebundle
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn"></param>
            /// <param name="stag">资源tag</param>
            public static void requestResNoAutoRelease(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true)
            {

                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, type, eloadResType, msNoAutoRelease, proc, o, basyn, bloadfromfile, false));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    string[] stags = new string[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eloadResType;
                    stags[0] = msNoAutoRelease;
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile, false,false);
                }


            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="spaths"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn"></param>
            /// <param name="bloadfromfile"></param>
            public static void requestResNoAutoRelease(string[] spaths, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true)
            {
                System.Type[] types = new System.Type[spaths.Length];
                eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                string[] stags = new string[spaths.Length];
                for (int i = 0; i < spaths.Length; i++)
                {
                    types[i] = type;
                    eloadResTypes[i] = eloadResType;
                    stags[i] = msNoAutoRelease;
                }
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile, false));
                }
                else
                {
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile, false, false);
                }


            }
            /// <summary>
            /// 释放对应调用requestResNoAutoRelease的资源
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            public static void unloadNoAutoRelease(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                if (mbuseassetbundle)
                {
                    AssetBundleManifest mainfest = null;
                    eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                    {

                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_URL);
                        loadrespath = eLoadResPath.RP_URL;

                    }

                    else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                    {

                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                        loadrespath = eLoadResPath.RP_StreamingAssets;
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                    {

                        mainfest = _getAssetBundleManifest(eloadResType);
                        loadrespath = eloadResType;
                    }
                    string[] depbundles = mainfest.GetAllDependencies(respath);
                    foreach (string bundlepath in depbundles)
                    {
                        string sReskeybundle = _getResKey(bundlepath, typeof(AssetBundle), eloadResType);
                        _releaseAssetDependenceBundle(sReskeybundle);
                        removeRes(bundlepath, typeof(AssetBundle), eloadResType);
                    }
                    string sReskey = _getResKey(respath, type, eloadResType);
                    _releaseAssetDependenceBundle(sReskey);
                    removeRes(respath, type, eloadResType);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="types">所有路径对应的类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>
            /// <param name="stag">资源tag</param>

            public static void requestRes(string[] spaths, System.Type[] types, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, string stag = mSdefaultTag, bool bloadfromfile = true)
            {
                eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                for (int i = 0; i < spaths.Length; i++)
                {
                    eloadResTypes[i] = eloadResType;
                }
                string[] stags = new string[spaths.Length];
                for (int i = 0; i < spaths.Length; i++)
                {
                    stags[i] = stag;
                }
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile));
                }
                else
                {
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile);
                }
            }

            /// <summary>
            /// 更新服务器上最新的资源
            /// </summary>
            /// <param name="loadedproc">回调函数</param>
            /// <param name="updateOnlyPack">这里的资源包只做更新,不做初始化下载,就是说,除非本地已经下载过这个包,才会参与包更新,如果本地没有这个包,这不会去下载</param>
            public static void updateToLatestBundles(ProcessDelegateArgc loadedproc, string[] updateOnlyPack)
            {
                CacheBundleInfo.initBundleInfo();
                Hashtable proc_pack = new Hashtable();
                proc_pack.Add("proc", loadedproc);
                proc_pack.Add("updateOnlyPack", updateOnlyPack);
                if (mbuseassetbundle)
                {
                    string[] tags = new string[1];
                    tags[0] = "InFini";
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                    {
                        //,意味着只要读取资源服务器的资源
                        if (_mURLAssetBundleManifest == null)
                        {
                            string[] paths = new string[1];
                            System.Type[] tps = new System.Type[1];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[1];

                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                            tps[0] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_URL;


                            _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                             {
                                 if (e == eLoadedNotify.Load_Successfull)
                                 {
                                     string[] assetspaths = new string[1];
                                     System.Type[] assetstps = new System.Type[1];
                                     eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                     assetspaths[0] = _getAssetsConfigByLoadStyle();
                                     assetstps[0] = typeof(TextAsset);
                                     assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                     _makeAssetBundleManifest();
                                     _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedLatestManifestForUpdate, proc_pack);
                                 }
                                 else if (e == eLoadedNotify.Load_Failed)
                                 {
                                     DLoger.LogError("load AssetBundleManifest error!");
                                 }
                             }, null, true, true);
                        }
                        else
                        {
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                    {
                        //如果指明了要读取本地资源,并且设置了本地资源目录
                        if (_mLocalAssetBundleManifest == null)
                        {
                            string[] paths = new string[1];
                            System.Type[] tps = new System.Type[1];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                            tps[0] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_StreamingAssets;

                            _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                             {
                                 if (e == eLoadedNotify.Load_Successfull)
                                 {
                                     string[] assetspaths = new string[1];
                                     System.Type[] assetstps = new System.Type[1];
                                     eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                     assetspaths[0] = _getAssetsConfigByLoadStyle();
                                     assetstps[0] = typeof(TextAsset);
                                     assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                     _makeAssetBundleManifest();
                                     _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedLatestManifestForUpdate, proc_pack);
                                 }
                                 else if (e == eLoadedNotify.Load_Failed)
                                 {
                                     DLoger.LogError("load AssetBundleManifest error!");
                                 }
                             }, null, true, true);
                        }
                        else
                        {
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                    {
                        //如果URL的资源表或者本地资源表没有加载,则一起加载
                        if (_mURLAssetBundleManifest == null || _mLocalAssetBundleManifest == null)
                        {
                            //为加载依赖关系列表,先读取依赖关系表
                            string[] paths = new string[2];
                            System.Type[] tps = new System.Type[2];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[2];
                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                            paths[1] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                            tps[0] = typeof(AssetBundleManifest);
                            tps[1] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_URL;
                            eloadResTypes[1] = eLoadResPath.RP_StreamingAssets;
                            tags = new string[2];
                            tags[0] = "InFini";
                            tags[1] = "InFini";
                            _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                             {
                                 if (e == eLoadedNotify.Load_Successfull)
                                 {
                                     string[] assetspaths = new string[1];
                                     System.Type[] assetstps = new System.Type[1];
                                     eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                     assetspaths[0] = _getAssetsConfigByLoadStyle();
                                     assetstps[0] = typeof(TextAsset);
                                     assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                     _makeAssetBundleManifest();
                                     _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedLatestManifestForUpdate, proc_pack);
                                 }
                                 else if (e == eLoadedNotify.Load_Failed)
                                 {
                                     DLoger.LogError("load AssetBundleManifest error!");
                                 }
                             }, null, true, true);

                        }
                        else
                        {
                            //已经加载完
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else
                    {
                        DLoger.LogError("you request a assetsbundle from  error Paths!");
                    }

                }
                else
                {
                    _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                }

            }
            /// <summary>
            /// 处理当前场景物体的引用资源
            /// </summary>
            /// <param name="sScenePath"></param>
            public static void doSceneObjectAssetsRef(string sScenePath)
            {
                if (_mDicAssetsRefConfig.ContainsKey(sScenePath))
                {
                    GameObject[] gobs = Object.FindObjectsOfType<GameObject>();
                    for (int i = 0; i < gobs.Length; i++)
                    {
                        _doWithAssetRefToObject(gobs[i], sScenePath);
                    }
                }

            }
           
            /// <summary>
            /// 获得Manifest的bundle的名字,URL上的名字和本地的名字不能重名
            /// </summary>
            /// <param name="eloadrespath"></param>
            /// <returns></returns>
            internal static string _getStreamingAssetsNameByLoadStyle(eLoadResPath eloadrespath)
            {
                if (eloadrespath == eLoadResPath.RP_URL)
                {
                    return mBundlesInfoFileName + "URL" + "/AssetBundleManifest";
                }
                else
                {
                    return mBundlesInfoFileName + "/AssetBundleManifest";
                }
            }
            internal static string _getAssetsConfigByLoadStyle()
            {

                return "assetsbundles/config/assetsref/assetpathconfig";

            }
            //检查依赖列表
            private static void _checkDependenceList(CloadParam p)
            {
                CacheBundleInfo.initBundleInfo();
                eLoadResPathState eloadresstate = _getLoadResPathState();
                string[] tags = new string[1];
                tags[0] = "InFini";
                if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                {
                    //,意味着只要读取资源服务器的资源

                    if (_mURLAssetBundleManifest == null)
                    {
                        string[] paths = new string[1];
                        System.Type[] tps = new System.Type[1];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                        tps[0] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_URL;

                        _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                         {
                             if (e == eLoadedNotify.Load_Successfull)
                             {
                                 string[] assetspaths = new string[1];
                                 System.Type[] assetstps = new System.Type[1];
                                 eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                 assetspaths[0] = _getAssetsConfigByLoadStyle();
                                 assetstps[0] = typeof(TextAsset);
                                 assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                 _makeAssetBundleManifest();
                                 _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedAssetBundleManifestForDepdence, p);
                             }
                             else if (e == eLoadedNotify.Load_Failed)
                             {
                                 DLoger.LogError("load AssetBundleManifest error!");
                             }
                         }, null, true, true);
                    }
                    else
                    {
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                {
                    //如果指明了要读取本地资源,并且设置了本地资源目录
                    if (_mLocalAssetBundleManifest == null)
                    {
                        string[] paths = new string[1];
                        System.Type[] tps = new System.Type[1];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                        tps[0] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_StreamingAssets;

                        _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                         {
                             if (e == eLoadedNotify.Load_Successfull)
                             {
                                 string[] assetspaths = new string[1];
                                 System.Type[] assetstps = new System.Type[1];
                                 eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                 assetspaths[0] = _getAssetsConfigByLoadStyle();
                                 assetstps[0] = typeof(TextAsset);
                                 assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                 _makeAssetBundleManifest();
                                 _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedAssetBundleManifestForDepdence, p);
                             }
                             else if (e == eLoadedNotify.Load_Failed)
                             {
                                 DLoger.LogError("load AssetBundleManifest error!");
                             }
                         }, null, true, true);
                    }
                    else
                    {
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                {
                    //如果URL的资源表或者本地资源表没有加载,则一起加载
                    if (_mURLAssetBundleManifest == null || _mLocalAssetBundleManifest == null)
                    {
                        //为加载依赖关系列表,先读取依赖关系表
                        string[] paths = new string[2];
                        System.Type[] tps = new System.Type[2];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[2];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                        paths[1] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                        tps[0] = typeof(AssetBundleManifest);
                        tps[1] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_URL;
                        eloadResTypes[1] = eLoadResPath.RP_StreamingAssets;
                        tags = new string[2];
                        tags[0] = "InFini";
                        tags[1] = "InFini";
                        _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                         {
                             if (e == eLoadedNotify.Load_Successfull)
                             {
                                 string[] assetspaths = new string[1];
                                 System.Type[] assetstps = new System.Type[1];
                                 eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                 assetspaths[0] = _getAssetsConfigByLoadStyle();
                                 assetstps[0] = typeof(TextAsset);
                                 assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                 _makeAssetBundleManifest();
                                 _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, _OnLoadedAssetBundleManifestForDepdence, p);
                             }
                             else if (e == eLoadedNotify.Load_Failed)
                             {
                                 DLoger.LogError("load AssetBundleManifest error!");
                             }
                         }, null, true, true);

                    }
                    else
                    {
                        //已经加载完
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                }
            }
            private static void _makeAssetBundleManifest()
            {
                if (_mURLAssetBundleManifest == null && mResourcesURLAddress != string.Empty)
                {
                    _mURLAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL), typeof(AssetBundleManifest), eLoadResPath.RP_URL);

                }
                if (_mLocalAssetBundleManifest == null && mResourceStreamingAssets != string.Empty)
                {
                    _mLocalAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets), typeof(AssetBundleManifest), eLoadResPath.RP_StreamingAssets);

                }
            }

            //已经加载完依赖列表
            private static void _OnLoadedAssetBundleManifestForDepdence(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {//开始加载依赖资源
                if (loadedNotify == eLoadedNotify.Load_Successfull)
                {
                   
                    _makeRefAssetsConfig();
                    CloadParam param = (CloadParam)obj;
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    List<string> depBundleNameList = new List<string>();
                    List<eLoadResPath> depBundleLoadPathlist = new List<eLoadResPath>();
                    for (int i = 0; i < param.mpaths.Length; i++)
                    {
                        string bundlepath = "";
                        if (param.mtypes[i] != typeof(AssetBundle))
                        {
                            bundlepath = Path.GetDirectoryName(param.mpaths[i]);
                        }
                        else
                        {
                            bundlepath = param.mpaths[i];
                        }
                        if (_mDicLoadedRes.ContainsKey(_getResKey(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i])))
                        {
                            continue;
                        }
                        AssetBundleManifest mainfest = null;
                        eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                        if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                        {

                            mainfest = _getAssetBundleManifest(eLoadResPath.RP_URL);
                            loadrespath = eLoadResPath.RP_URL;

                        }

                        else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                        {

                            mainfest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                            loadrespath = eLoadResPath.RP_StreamingAssets;
                        }
                        else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                        {

                            mainfest = _getAssetBundleManifest(param.meloadResTypes[i]);
                            loadrespath = param.meloadResTypes[i];
                        }
                        else
                        {
                            DLoger.LogError("you request a assetsbundle from  error Paths!");
                            return;
                        }
                        string[] deppaths = mainfest.GetAllDependencies(bundlepath);
                        for (int j = 0; j < deppaths.Length; j++)
                        {
                            _addAssetDependenceBundle(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i], param.mtags[i], deppaths[j], loadrespath);
                            if (!depBundleNameList.Contains(deppaths[j]))
                            {
                                depBundleNameList.Add(deppaths[j]);
                                depBundleLoadPathlist.Add(loadrespath);
                            }
                        }
                        _addAssetDependenceBundle(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i], param.mtags[i], bundlepath, loadrespath);
                        if (!depBundleNameList.Contains(bundlepath))
                        {
                            depBundleNameList.Add(bundlepath);
                            depBundleLoadPathlist.Add(loadrespath);
                        }

                    }


                    if (depBundleNameList.Count != 0)
                    {

                        _loadDependenceBundles(depBundleNameList.ToArray(), depBundleLoadPathlist.ToArray(), _OnloadedDependenceBundles, param);
                    }
                    else
                    {
                        _OnloadedDependenceBundles(param, eLoadedNotify.Load_Successfull);
                    }

                }
            }
            internal static void _addAssetDependenceBundle(string respath, System.Type restype, eLoadResPath eresloadrespath, string tag, string bundlepath, eLoadResPath ebundleloadrespath)
            {
                string ireskey = _getResKey(respath, restype, eresloadrespath);
                string sbundlekey = _getRealPath(bundlepath, typeof(AssetBundle), ebundleloadrespath).msRealPath;
                if (!_mDicAssetsDependBundles.ContainsKey(ireskey))
                {
                    _mDicAssetsDependBundles.Add(ireskey, new List<string>());

                }
                _mDicAssetsDependBundles[ireskey].Add(sbundlekey);
                if (!_mDicBundlescounts.ContainsKey(sbundlekey))
                {
                    _mDicBundlescounts.Add(sbundlekey, 0);
                }
                _mDicBundlescounts[sbundlekey]++;
            }
            internal static void _releaseAssetDependenceBundle(string sReskey)
            {
                if (_mDicAssetsDependBundles.ContainsKey(sReskey))
                {
                    List<string> depbundles = _mDicAssetsDependBundles[sReskey];
                    foreach (string depbundlekey in depbundles)
                    {
                        if (_mDicBundlescounts.ContainsKey(depbundlekey))
                        {
                            _mDicBundlescounts[depbundlekey]--;
                            if (_mDicBundlescounts[depbundlekey] == 0)
                            {
                                _mDicBundlescounts.Remove(depbundlekey);
                            }


                        }
                    }
                    _mDicAssetsDependBundles.Remove(sReskey);
                }
            }
            internal static bool _getDepBundleUesed(string ibundlekey)
            {
                if (_mDicBundlescounts.ContainsKey(ibundlekey))
                {
                    return _mDicBundlescounts[ibundlekey] != 0;
                }
                else
                {
                    return false;
                }
            }


            private static void _OnLoadedLatestManifestForUpdate(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {//加载完manifest
                if (loadedNotify == eLoadedNotify.Load_Successfull)
                {
                    ProcessDelegateArgc proc = (ProcessDelegateArgc)((Hashtable)obj)["proc"];
                    List<string> updateOnlyPacks = new List<string>((string[])((Hashtable)obj)["updateOnlyPack"]);
                    for (int i = 0; i < updateOnlyPacks.Count; i++)
                    {
                        updateOnlyPacks[i] = _getRealPath(updateOnlyPacks[i], typeof(AssetBundle), eLoadResPath.RP_URL).msRealPath;
                    }
                    if (mbuseassetbundle)
                    {
                        List<string> needUpdateBundleList = new List<string>();
                        List<eLoadResPath> needUpdateBundleResPathList = new List<eLoadResPath>();
                        _makeRefAssetsConfig();
                        AssetBundleManifest mainfest = _mURLAssetBundleManifest;
                        if (mainfest != null)
                        {
                            string[] bundles = mainfest.GetAllAssetBundles();
                            for (int i = 0; i < bundles.Length; i++)
                            {
                                CPathAndHash pathhash = _getRealPath(bundles[i], typeof(AssetBundle), eLoadResPath.RP_URL);
                                //如果不是从远程下载,则不跟新
                                if (pathhash.meLoadResType != eLoadResPath.RP_URL)
                                {
                                    continue;
                                }
                                //如果在排除之外并且没有下载过,也不跟新
                                if (updateOnlyPacks.Contains(pathhash.msRealPath) && !CacheBundleInfo.hasBundle(pathhash.msRealPath))
                                {//这里判断那些不需要获取的资源包(例如各个国家的语言包)
                                    continue;
                                }
                                //如果caching已经有,也不跟新
                                if (CacheBundleInfo.isCaching(pathhash.msRealPath, pathhash.mHash.ToString()) == false)
                                {
                                    needUpdateBundleList.Add(bundles[i]);
                                    needUpdateBundleResPathList.Add(pathhash.meLoadResType);
                                }
                            }
                        }
                        
                        if (needUpdateBundleList.Count != 0)
                        {
                            //System.Type[] types = new System.Type[needUpdateBundleList.Count];
                            //for (int i = 0; i < needUpdateBundleList.Count; i++)
                            //{
                            //    types[i] = typeof(AssetBundle);
                            //}
                            //string[] stags = new string[needUpdateBundleList.Count];
                            //for (int i = 0; i < needUpdateBundleList.Count; i++)
                            //{
                            //    stags[i] = mSdefaultTag;
                            //}
                            //_requestRes(needUpdateBundleList.ToArray(), types, needUpdateBundleResPathList.ToArray(), stags, proc);
                            proc(needUpdateBundleList, eLoadedNotify.Load_Successfull);
                        }
                        else
                        {
                            proc(null, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else
                    {
                        proc(null, eLoadedNotify.Load_Successfull);
                    }
                }
            }
            /// <summary>
            /// 根据资源引用表,生成配置
            /// </summary>
            private static void _makeRefAssetsConfig()
            {


                Dictionary<string, Dictionary<string, AssetsKey>> DicAssetsRefConfig = _mDicAssetsRefConfig;
                if (DicAssetsRefConfig.Keys.Count != 0)
                {
                    return;
                }

                TextAsset AssetsRefConfig = (TextAsset)getRes(_getAssetsConfigByLoadStyle(), typeof(TextAsset), eLoadResPath.RP_URL);
                StringReader sr = new StringReader(AssetsRefConfig.ToString());
                uint objsnum = uint.Parse(sr.ReadLine());
                for (int i = 0; i < objsnum; i++)
                {
                    //读取无效行
                    string line = sr.ReadLine();
                    string objname = sr.ReadLine();

                    if (!DicAssetsRefConfig.ContainsKey(objname))
                    {
                        DicAssetsRefConfig.Add(objname, new Dictionary<string, AssetsKey>());
                    }
                    else
                    {
                        DLoger.LogError(objname + "===预制件名重复!");
                    }
                    int assetsnum = 0;
                    string errorline = "";
                    try
                    {
                        errorline = sr.ReadLine();
                        assetsnum = int.Parse(errorline);
                    }
                    catch
                    {
                        DLoger.LogWarning(errorline + "1");
                    }

                    for (int a = 0; a < assetsnum; a++)
                    {
                        //读取无效行
                        string nouse = sr.ReadLine();
                        string assetname = sr.ReadLine();
                        string key = sr.ReadLine();

                        if (!DicAssetsRefConfig[objname].ContainsKey(assetname))
                        {
                            DicAssetsRefConfig[objname].Add(assetname, new AssetsKey(key));
                        }
                        else
                        {
                            DLoger.LogError(objname + ":" + assetname + "资源名重复!");
                        }

                        if (assetname.Split(':')[1] == typeof(Material).ToString())
                        {
                            int texporpnum;
                            errorline = sr.ReadLine();
                            if (!int.TryParse(errorline, out texporpnum))
                            {
                                DLoger.LogWarning(errorline + "2");
                            }
                            if (texporpnum != 0)
                            {
                                DicAssetsRefConfig[objname][assetname].mlistMatTexPropName = new List<string>(texporpnum);

                                for (int tp = 0; tp < texporpnum; tp++)
                                {
                                    DicAssetsRefConfig[objname][assetname].mlistMatTexPropName.Add(sr.ReadLine());
                                }
                            }

                        }


                    }



                }
                removeRes(_getAssetsConfigByLoadStyle(), typeof(TextAsset), eLoadResPath.RP_URL);


            }

            private static void _loadDependenceBundles(string[] dependbundlepath, eLoadResPath[] eloadResTypes, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                System.Type[] types = new System.Type[dependbundlepath.Length];
                CloadParam param = (CloadParam)o;
                for (int i = 0; i < types.Length; i++)
                {
                    types[i] = typeof(AssetBundle);
                }
                string[] stags = new string[dependbundlepath.Length];
                for (int i = 0; i < dependbundlepath.Length; i++)
                {
                    if (param.mbautoreleasebundle)
                    {
                        stags[i] = mSdefaultTag;
                    }
                    else
                    {
                        stags[i] = msNoAutoRelease;
                    }
                    
                }
                _requestRes(dependbundlepath, types, eloadResTypes, stags, proc, o, basyn, param.mbloadfromfile, false, param.mbautoreleasebundle);
            }
            private static void _OnloadedDependenceBundles(object o, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {
                if (loadedNotify == eLoadedNotify.Load_Successfull && o != null)
                {
                    CloadParam param = (CloadParam)o;
                    _requestRes(param.mpaths, param.mtypes, param.meloadResTypes, param.mtags, param.mproc, param.mo, param.mbasyn, param.mbloadfromfile, false, param.mbautoreleasebundle);
                }
            }
            //加载资源组,指定每个资源的类型,资源都加载完会执行回调proc
            private static void _requestRes(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, string[] stags, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true, bool bNoUseCatching = false, bool bautoReleaseBundle = true,bool bonlydownload = false)
            {
                
                //将请求的资源组加入正在加载的资源组列表,并返回资源组ID
                string sResGroupKey = _makeResGroupMap(spaths, types, eloadResTypes, stags, proc, o);
                for (int i = 0; i < spaths.Length; i++)
                {
                    
                    string truepath = _getRealPath(spaths[i], types[i], eloadResTypes[i]).msRealPath;
                   
                    string sResKey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    //如果资源组中的此个资源已经加载完毕(剔除资源组中已经加载完毕的资源)
                    if (_mDicLoadedRes.ContainsKey(sResKey))
                    {
                        //将该资源从资源组中移除
                        _removePathInResGroup(sResGroupKey, sResKey, true, bautoReleaseBundle);
                        continue;

                    }
                    if (mbuseassetbundle == true && eloadResTypes[i] != eLoadResPath.RP_Resources)
                    {//如果是打包读取,并且不直接从Resources读取

                        //异步读取资源

                        string temppath;
                        if (types[i] != typeof(AssetBundle))
                        {//不是只加载AssetBundle
                            string name = Path.GetFileNameWithoutExtension(spaths[i]);
                            string dir = spaths[i].Substring(0, spaths[i].Length - name.Length - 1);
                            temppath = dir + "|" + name;
                        }
                        else
                        {
                            temppath = spaths[i];
                        }
                        Hash128 hash;
                        _getRealLoadResPathType(temppath, eloadResTypes[i], out hash);
                        LoadAsset.getInstance().loadAsset(truepath, spaths[i],types[i], stags[i], sResGroupKey, hash, basyn, bNoUseCatching, bautoReleaseBundle, bonlydownload, bloadfromfile);
                        if (!_mListLoadingRes.Contains(sResKey))
                        {
                            _mListLoadingRes.Add(sResKey);
                        }


                    }
                    else if (types[i] != typeof(AssetBundle))
                    {//否则直接读取Resources散开资源

                        Object t = null;

                        t = Resources.Load(truepath, types[i]);

                        if (t != null)
                        {
                            Hashtable reshash = new Hashtable();
                            reshash.Add("Object",t);
                            reshash.Add("Tag",stags[i]);
                            reshash.Add("InResources", true);
                            reshash.Add("IsAssetsBundle", false);
                            _mDicLoadedRes.Add(sResKey, reshash);
                            _removePathInResGroup(sResGroupKey, sResKey, true, bautoReleaseBundle);

                        }
                        else
                        {
                            _removePathInResGroup(sResGroupKey, sResKey, false, bautoReleaseBundle);
                            DLoger.LogError("Load===" + spaths[i] + "===Failed");
                        }
                        continue;
                    }
                    else
                    {
                        _removePathInResGroup(sResGroupKey, sResKey, true, bautoReleaseBundle);
                    }
                }
            }
            /// <summary>
            /// 获得已经加ResourcesManager.request加载完毕的对象
            /// </summary>
            /// <param name="respath">路径</param>
            /// <param name="type">类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <returns></returns>
            public static Object getRes(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, type, eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    return _getResObject(skey);
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// 获得已经加ResourcesManager.request加载完毕的对象
            /// </summary>
            /// <param name="respath">路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <returns></returns>
            public static Object getRes(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                return getRes(respath, typeof(Object), eloadResType);
            }
            /// <summary>
            /// 获取某个资源的tag
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <returns></returns>
            public static string getResTag(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, type, eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    return _getResObjectTag(skey);
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// 设置存在资源的tag
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="tag"></param>
            public static void setResTag(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL,string tag = mSdefaultTag)
            {
                string skey = _getResKey(respath, type, eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    _mDicLoadedRes[skey]["Tag"] = tag;
                }
            }

            /// <summary>
            /// 清空所有缓存资源
            /// </summary>
            //public static void clearAllRes()
            //{
            //    List<string> zeroref = new List<string>(_mDicLoadedRes.Keys);
            //    if (zeroref.Count > 0)
            //    {
            //        for (int i = 0; i < zeroref.Count; i++)
            //        {
            //            _removeRes(zeroref[i]);
            //        }
            //        _mDicLoadedRes.Clear();
            //        _beginUnloadUnUsedAssets();
            //    }

            //}
            /// <summary>
            /// 清除标记为tag的资源
            /// </summary>
            /// <param name="tag"></param>
            public static void clearAllRes(string tag)
            {
                List<string> zeroref = new List<string>(_mDicLoadedRes.Keys);
                if (zeroref.Count > 0)
                {
                    for (int i = 0; i < zeroref.Count; i++)
                    {
                        if (_getResObjectTag(zeroref[i]) == tag || _getResObjectTag(zeroref[i]) == "")
                        {
                            _removeRes(zeroref[i]);
                        }

                    }
                    _beginUnloadUnUsedAssets();
                }
                
            }
            /// <summary>
            /// 清除默认tag
            /// </summary>
            public static void clearAllDefaultRes()
            {
                clearAllRes(mSdefaultTag);
            }

            /// <summary>
            /// 开始执行卸载冗余资源和GC
            /// </summary>
            /// <returns></returns>
            static public void startUnloadUnusedAssetAndGC()
            {
                if (mbuseassetbundle == true)
                {
                    mbStartDoUnload = true;
                }
               
            }
            /// <summary>
            /// 判断卸载冗余资源和GC是否执行完毕
            /// </summary>
            /// <returns></returns>
            static public bool isUnloadUnusedAssetAndGCDone()
            {
                if (mbuseassetbundle == true)
                {
                    return mbStartDoUnload == false && mbUnLoadUnUsedResDone == true;
                }
                else
                {
                    return true;
                }
                
            }

            /// <summary>
            /// 清除除这个tag以外的资源
            /// </summary>
            /// <param name="excepttag"></param>
            //public static void clearAllexcept(string excepttag)
            //{
            //    List<string> zeroref = new List<string>(_mDicLoadedRes.Keys);
            //    if (zeroref.Count > 0)
            //    {
            //        for (int i = 0; i < zeroref.Count; i++)
            //        {
            //            if (_getResObjectTag(zeroref[i]) != excepttag)
            //            {
            //                _removeRes(zeroref[i]);
            //            }

            //        }
            //        _beginUnloadUnUsedAssets();
            //    }

            //}

            /// <summary>
            /// 标记可以开始做资源卸载
            /// </summary>
            internal static void _beginUnloadUnUsedAssets()
            {
                if (mbuseassetbundle)
                {
                    mbUnLoadUnUsedResDone = false;
                }
                else
                {
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            } 

            /// <summary>
            /// 返回加载对象
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            internal static Object _getResObject(string key)
            {
                if (_mDicLoadedRes.ContainsKey(key))
                {
                    return _mDicLoadedRes[key]["Object"] as Object;
                }
                else
                {
                    return null;
                }
            }
            internal static string _getResObjectTag(string key)
            {
                if (_mDicLoadedRes.ContainsKey(key))
                {
                    return _mDicLoadedRes[key]["Tag"] as string;
                }
                else
                {
                    return "";
                }
            }
            internal static bool _getResObjectInResources(string key)
            {
                if (_mDicLoadedRes.ContainsKey(key))
                {
                    return (bool)_mDicLoadedRes[key]["InResources"];
                }
                else
                {
                    return false;
                }
            }
            internal static bool _getResObjectIsAssetsBundle(string key)
            {
                if (_mDicLoadedRes.ContainsKey(key))
                {
                    return (bool)_mDicLoadedRes[key]["IsAssetsBundle"];
                }
                else
                {
                    return false;
                }
            }
           

            /// <summary>
            /// 移除资源
            /// </summary>
            /// <param name="respath">资源路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            public static void removeRes(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                removeRes<Object>(respath, eloadResType);
            }
            /// <summary>
            /// 移除资源
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="respath"></param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            public static void removeRes<T>(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, typeof(T), eloadResType);
                _removeRes(skey,true);

            }
            /// <summary>
            /// 移除资源,专门在LoadAsset里面移除bundle用
            /// </summary>
            /// <param name="sReskey"></param>
            /// <param name="bunloadUnusedAssets"></param>
            internal static void _removeRes(string sReskey,bool bunloadUnusedAssets = false)
            {
                if (_mDicLoadedRes.ContainsKey(sReskey))
                {

                    if (mbEditorMode == true)
                    {
                        if (mbuseassetbundle == true && _getResObjectInResources(sReskey) == false)
                        {
                            if (_getResObjectIsAssetsBundle(sReskey) == false)
                            {
                                //Resources.UnloadAsset(_getResObject(sReskey));
                                Object.DestroyImmediate(_getResObject(sReskey), true);
                                
                                
                                DLoger.Log("删除资源===" + sReskey + "=====");
                            }
                            else
                            {
                                DLoger.Log("删除AssetsBundle===" + sReskey + "=====");
                            }
                        }

                    }
                    else
                    {
                        if (_getResObjectIsAssetsBundle(sReskey) == false)
                        {
                            //Resources.UnloadAsset(_getResObject(sReskey));
                            Object.DestroyImmediate(_getResObject(sReskey), true);
                            

                            DLoger.Log("删除资源===" + sReskey + "=====");
                        }
                        else
                        {
                            DLoger.Log("删除AssetsBundle===" + sReskey + "=====");
                        }
                    }
                    _mDicLoadedRes.Remove(sReskey);


                }
                if (bunloadUnusedAssets == true)
                {
                    _beginUnloadUnUsedAssets();
                }
                
            }
            /// <summary>
            /// 移除资源
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            public static void removeRes(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, type, eloadResType);
                _removeRes(skey,true);

            }
            /// <summary>
            /// 
            /// </summary>
            public static void tool_Printinfo()
            {
                List<string> nowResref = new List<string>();
                if (mbEditorMode == true)
                {
                    StreamWriter sw = new StreamWriter(Application.dataPath + "/缓存资源信息.txt");
                    sw.WriteLine("*********开始缓存的Object资源:" + _mDicLoadedRes.Count + "*********");
                    DLoger.Log("*********开始缓存的Object资源:" + _mDicLoadedRes.Count + "*********");
                    foreach (string key in _mDicLoadedRes.Keys)
                    {
                        string info = "===" + key + ":" + ((Object)_mDicLoadedRes[key]["Object"]).name + "==tag:" + (string)_mDicLoadedRes[key]["Tag"];
                        sw.WriteLine(info);
                        DLoger.Log(info);
                        nowResref.Add(info);
                    }
                    List<string> listkeys = new List<string>(_mDicAssetRef.Keys);
                    listkeys = listkeys.FindAll((o) => { return _mDicAssetRef[o].IsAlive &&(Object)_mDicAssetRef[o].Target != null; });
                    sw.WriteLine("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    DLoger.Log("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    foreach (string key in listkeys)
                    {
                        string info = "===" + key + ":" + ((Object)_mDicAssetRef[key].Target).name;
                        sw.WriteLine(info);
                        DLoger.Log(info);
                        nowResref.Add(info);
                    }
                    if (_mlistRefObjForDebug.Count != 0)
                    {
                        DLoger.Log("*********和上一次抓取信息的对比:*********");
                        sw.WriteLine("*********和上一次抓取信息的对比:*********");
                        foreach (string key in nowResref)
                        {
                            if (!_mlistRefObjForDebug.Contains(key))
                            {
                                sw.WriteLine(key);
                                DLoger.Log(key);
                            }
                        }
                    }
                    _mlistRefObjForDebug = new List<string>(nowResref);

                    sw.Flush();
                    sw.Close();
                }
                else
                {

                    DLoger.Log("*********开始缓存的Object资源:" + _mDicLoadedRes.Count + "*********");
                    foreach (string key in _mDicLoadedRes.Keys)
                    {
                        string info = "===" + key + ":" + _getResObject(key).name + "==tag:" + _getResObjectTag(key);
                        DLoger.Log(info);
                        nowResref.Add(info);
                    }
                    List<string> listkeys = new List<string>(_mDicAssetRef.Keys);
                    listkeys = listkeys.FindAll((o) => { return _mDicAssetRef[o].IsAlive && (Object)_mDicAssetRef[o].Target != null; });
                    DLoger.Log("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    foreach (string key in listkeys)
                    {
                        string info = "===" + key + ":" + ((Object)_mDicAssetRef[key].Target).name;
                        DLoger.Log(info);
                        nowResref.Add(info);
                    }
                    if (_mlistRefObjForDebug.Count != 0)
                    {
                        DLoger.Log("*********和上一次抓取信息的对比:*********");
                        foreach (string key in nowResref)
                        {
                            if (!_mlistRefObjForDebug.Contains(key))
                            {
                                DLoger.Log(key);
                            }
                        }
                    }
                    _mlistRefObjForDebug = new List<string>(nowResref);
                }
                
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="skey"></param>
            /// <param name="t"></param>
            /// <param name="tag"></param>
            /// <param name="sRequestPath"></param>
            internal static void _addResAndRemoveInLoadingList(string skey, Object t, string tag = mSdefaultTag, string sRequestPath = "")
            {

                if (!_mDicLoadedRes.ContainsKey(skey))
                {
                    _doWithAssetRefToObject(t, sRequestPath);
                    Hashtable reshash = new Hashtable();
                    reshash.Add("Object", t);
                    reshash.Add("Tag", tag);
                    reshash.Add("InResources", false);
                    reshash.Add("IsAssetsBundle", sRequestPath == "");
                    _mDicLoadedRes.Add(skey, reshash);
                    _mListLoadingRes.Remove(skey);
                }

            }
            internal static bool _hasLoadingRes()
            {
                return _mListLoadingRes.Count != 0;
            }
            /// <summary>
            ///由于释放bundle后,引用的资源会脱离管理,再次加载bundle,加载出来的资源,会在内存生成新的拷贝,所以这里做资源的管理和计数,
            ///避免同一份资源在内存中多份
            /// </summary>
            /// <param name="o"></param>
            /// <param name="sobjkey">在资源引用配置里面的key值</param>
            internal static void _doWithAssetRefToObject(Object o, string sobjkey)
            {

                
                if (!_mDicAssetsRefConfig.ContainsKey(sobjkey) || mbuseassetbundle == false)
                {
                    return;
                }

                #region 对预制件进行处理
                GameObject go = o as GameObject;
                if (go != null)
                {
                    Component[] comps = go.GetComponentsInChildren<Component>(true);
                    for (int i = 0; i < comps.Length; i++)
                    {
                        string snamekey = string.Empty;
                        Object obj = null;
                        System.Type type = typeof(Object);
                        //处理字体
                        Text text = comps[i] as Text;
                        if (text != null)
                        {
                            obj = text.font;
                            type = typeof(Font);

                            if (obj != null)
                            {
                                Material mat = text.font.material;
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    text.font = (Font)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                    //text.font.material = mat;
                                }

                            }
                        }
                        //处理声音
                        AudioSource audio = comps[i] as AudioSource;
                        if (audio != null)
                        {
                            obj = audio.clip;
                            type = typeof(AudioClip);

                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    audio.clip = (AudioClip)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                }

                            }

                        }
                        //处理动画
                        Animator amt = comps[i] as Animator;
                        if (amt != null)
                        {
                            obj = amt.runtimeAnimatorController;
                            type = typeof(RuntimeAnimatorController);

                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    amt.runtimeAnimatorController = (RuntimeAnimatorController)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                }

                            }

                        }
                        //网格
                        MeshFilter meshfilter = comps[i] as MeshFilter;
                        if (meshfilter != null)
                        {
                            obj = meshfilter.sharedMesh;
                            type = typeof(Mesh);

                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    meshfilter.sharedMesh = (Mesh)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                }

                            }
                        }
                        SkinnedMeshRenderer skinmesh = comps[i] as SkinnedMeshRenderer;
                        if (skinmesh != null)
                        {
                            obj = skinmesh.sharedMesh;
                            type = typeof(Mesh);

                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    skinmesh.sharedMesh = (Mesh)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                }

                            }
                        }
                        ParticleSystemRenderer particlerender = comps[i] as ParticleSystemRenderer;
                        if (particlerender != null)
                        {
                            obj = particlerender.mesh;
                            type = typeof(Mesh);

                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    particlerender.mesh = (Mesh)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                }

                            }
                        }
                        Image image = comps[i] as Image;
                        SpriteRenderer spr = comps[i] as SpriteRenderer;
                        Sprite spt = null;

                        if (image != null)
                        {
                            spt = image.sprite;
                        }
                        if (spr != null)
                        {
                            spt = spr.sprite;
                        }
                        if (spt != null)
                        {
                            Texture alphatexture = spt.associatedAlphaSplitTexture;
                            obj = spt.texture;
                            type = typeof(Texture);
                            if (obj != null)
                            {
                                snamekey = obj.name + ":" + type;
                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                {
                                    Texture tex = (Texture)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                    string spritename = spt.name;
                                    if (!spt.name.Contains("(replace)"))
                                    {
                                        spritename = spt.name + "(replace)";
                                    }
                                    
                                    spt = Sprite.Create((Texture2D)tex, spt.rect, new Vector2(spt.pivot.x/spt.rect.width, spt.pivot.y/spt.rect.height),spt.pixelsPerUnit,0,SpriteMeshType.FullRect,spt.border);
                                    spt.name = spritename;

                                    
                                    if (image != null)
                                    {
                                        image.sprite = spt;
                                    }
                                    if (spr != null)
                                    {
                                        spr.sprite  = spt;
                                    }
                                }

                            }
                            //sprite 有alpha图,则要对材质上的贴图去冗余,这里是Android ETC1+ALPHA的情况
                            if (alphatexture != null)
                            {
                                Material mat = null;
                                //如果有材质,处理材质
                                if (image != null)
                                {
                                    mat = image.material;
                                }
                                if (spr != null)
                                {
                                    mat = spr.sharedMaterial;
                                }
                                if (mat != null)
                                {
                                    obj = mat.GetTexture("_MainTex");
                                    type = typeof(Texture);
                                    if (obj != null)
                                    {
                                        snamekey = obj.name + ":" + type;
                                        if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                        {
                                            Texture tex = (Texture)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                            mat.SetTexture("_MainTex", tex);
                                        }
                                    }
                                    obj = mat.GetTexture("_MyAlphaTex");
                                    type = typeof(Texture);
                                    if (obj != null)
                                    {
                                        snamekey = obj.name + ":" + type;
                                        if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                        {
                                            Texture tex = (Texture)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                            mat.SetTexture("_MyAlphaTex", tex);
                                        }
                                    }
                                    if (image != null)
                                    {
                                        image.material = mat;
                                    }
                                    if (spr != null)
                                    {
                                        spr.sharedMaterial = mat;
                                    }
                                }
                                
                            }
                        }



                        //贴图,shader
                        Renderer render = comps[i] as Renderer;
                        if (render != null)
                        {
                            Material[] mats = render.sharedMaterials;
                            for (int m = 0; m < mats.Length; m++)
                            {
                                //贴图
                                if (mats[m] != null)
                                {
                                    
                                    Material mat = mats[m];
                                    int renderqueue = mat.renderQueue;

                                    snamekey = mat.name + ":" + typeof(Material);
                                    if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                    {
                                        string[] proname = _mDicAssetsRefConfig[sobjkey][snamekey].mlistMatTexPropName.ToArray();
                                        for (int texpr = 0; texpr < proname.Length; texpr++)
                                        {
                                            Texture tex = mat.GetTexture(proname[texpr]);
                                            if (tex != null)
                                            {
                                                obj = tex;
                                                type = typeof(Texture);
                                                snamekey = obj.name + ":" + type;
                                                if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                                {
                                                    render.sharedMaterials[m].SetTexture(proname[texpr], (Texture)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj));
                                                }
                                            }

                                        }
                                        Shader shader = mat.shader;
                                        if (shader != null)
                                        {
                                            obj = shader;
                                            type = typeof(Shader);
                                            snamekey = obj.name + ":" + type;
                                            if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                            {
                                                mat.shader = (Shader)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, obj);
                                            }
                                        }
                                    }
                                    mat.renderQueue = renderqueue;
                                }
                            }
                        }


                    }

                }
                else
                {
                    System.Type deptype = typeof(Object);
                    Texture texobj = o as Texture;
                    Shader shaderobj = o as Shader;
                    AudioClip audioclip = o as AudioClip;
                    Mesh mesh = o as Mesh;
                    RuntimeAnimatorController rc = o as RuntimeAnimatorController;
                    Material mat = o as Material;
                    string snamekey = "";
                    if (texobj != null)
                    {
                        deptype = typeof(Texture);
                    }
                    if (shaderobj != null)
                    {
                        deptype = typeof(Shader);
                    }
                    if (audioclip != null)
                    {
                        deptype = typeof(AudioClip);
                    }
                    if (mesh != null)
                    {
                        deptype = typeof(Mesh);
                    }
                    if (rc != null)
                    {
                        deptype = typeof(RuntimeAnimatorController);
                    }
                    if (mat != null)
                    {
                        deptype = typeof(Material);
                    }
                    snamekey = o.name + ":" + deptype;
                    if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                    {
                        if (mat == null)
                        {
                            o = _doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, o);
                        }
                        else
                        {
                            int renderqueue = mat.renderQueue;
                            snamekey = mat.name + ":" + typeof(Material);
                            if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                            {
                                string[] proname = _mDicAssetsRefConfig[sobjkey][snamekey].mlistMatTexPropName.ToArray();
                                for (int texpr = 0; texpr < proname.Length; texpr++)
                                {
                                    Texture tex = mat.GetTexture(proname[texpr]);
                                    if (tex != null)
                                    {
                                        deptype = typeof(Texture);
                                        snamekey = tex.name + ":" + deptype;
                                        if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                        {
                                            mat.SetTexture(proname[texpr], (Texture)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, tex));
                                        }
                                    }

                                }
                                Shader shader = mat.shader;
                                if (shader != null)
                                {
                                    deptype = typeof(Shader);
                                    snamekey = shader.name + ":" + deptype;
                                    if (_mDicAssetsRefConfig[sobjkey].ContainsKey(snamekey))
                                    {
                                        mat.shader = (Shader)_doWithAssetRefCount(_mDicAssetsRefConfig[sobjkey][snamekey].msKey, shader);
                                    }
                                }
                            }
                            mat.renderQueue = renderqueue;
                        }
                    }


                }
                #endregion

                //_mListReleasedObjects.Clear();
                //Resources.UnloadUnusedAssets();
                //System.GC.Collect();

            }

            internal static Object _doWithAssetRefCount(string key, Object obj)
            {
                //如果没有此资源
                if (!_mDicAssetRef.ContainsKey(key))
                {
                    _mDicAssetRef.Add(key, new System.WeakReference(obj));
                }
                //如果弱引用资源已经销毁
                if (_mDicAssetRef.ContainsKey(key) && (!_mDicAssetRef[key].IsAlive || (Object)_mDicAssetRef[key].Target == null))
                {
                    _mDicAssetRef[key] = null;
                    _mDicAssetRef[key] = new System.WeakReference(obj);
                }
                if (!_mListReleasedObjects.Contains(obj) && obj.GetInstanceID() != ((Object)_mDicAssetRef[key].Target).GetInstanceID())
                {
                    _mListReleasedObjects.Add(obj);
                }
                return (Object)_mDicAssetRef[key].Target;
                //Debug.LogWarning(key + ":" + _mDicAssetRefCount[key].miRefCount);
            }

            
            /// <summary>
            /// 是否是已经加载完毕的资源
            /// </summary>
            /// <param name="skey">(sAssetPath + type.ToString())</param>
            /// <returns></returns>
            internal static bool _isLoadedRes(string skey)
            {
                return _mDicLoadedRes.ContainsKey(skey);
            }
            internal static void _removeLoadingResFromList(string skey)
            {
                if (_mListLoadingRes.Contains(skey))
                {
                    _mListLoadingRes.Remove(skey);
                }

            }
            //获取最终的eloadrespath
            private static eLoadResPath _getRealLoadResPathType(string sAssetPath, eLoadResPath eloadResType, out Hash128 hash)
            {
                hash = new Hash128();
                eLoadResPath finalloadrespath;

                string assetsbundlepath;
                if (sAssetPath.Contains("|"))
                {
                    assetsbundlepath = sAssetPath.Split('|')[0];

                }
                else
                {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                    assetsbundlepath = sAssetPath;
                }

                if (eloadResType == eLoadResPath.RP_Resources)
                {
                    finalloadrespath = eloadResType;
                }
                eLoadResPathState eloadrespathstate = _getLoadResPathState();
                if (eloadrespathstate == eLoadResPathState.LS_ReadURLOnly)
                {
                    if (_mURLAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mURLAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath = eLoadResPath.RP_URL;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadStreamingOnly)
                {
                    if (_mLocalAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    if (_mLocalAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && eloadResType == eLoadResPath.RP_URL)
                {
                    if (_mLocalAssetBundleManifest == null || _mURLAssetBundleManifest == null)
                    {//说明还没有加载manifest,无须比较,按照设定的来加载
                        finalloadrespath = eloadResType;
                    }
                    else
                    {

                        if (assetsbundlepath.Contains(mBundlesInfoFileName))
                        {
                            return eloadResType;
                        }
                        Hash128 hashlocal = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                        Hash128 hashurl = _mURLAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                        if (hashlocal.GetHashCode() == hashurl.GetHashCode())
                        {//如果本地有,则从本地读取
                            hash = hashlocal;
                            finalloadrespath = eLoadResPath.RP_StreamingAssets;
 
                        }
                        else
                        {

                            hash = hashurl;
                            finalloadrespath = eLoadResPath.RP_URL;

                        }
                    }
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                    finalloadrespath = eLoadResPath.RP_Resources;
                }
                return finalloadrespath;
            }

            //获取资源的真实路径
            private static CPathAndHash _getRealPath(string respath, System.Type type, eLoadResPath eloadResType)
            {
                CPathAndHash pathhash = new CPathAndHash();
                if (mbuseassetbundle == false || eloadResType == eLoadResPath.RP_Resources)
                {
                    pathhash.msRealPath = respath;
                    pathhash.meLoadResType = eLoadResPath.RP_Resources;
                }
                else
                {

                    string temppath;
                    if (type != typeof(AssetBundle))
                    {//不是只加载AssetBundle
                        string name = Path.GetFileNameWithoutExtension(respath);
                        string dir = respath.Substring(0, respath.Length - name.Length - 1);
                        temppath = dir + "|" + name;
                    }
                    else
                    {
                        temppath = respath;
                    }
                    pathhash.meLoadResType = _getRealLoadResPathType(temppath, eloadResType, out pathhash.mHash);
                    string address = _getResAddressByPath(pathhash.meLoadResType);
                    pathhash.msRealPath = address + temppath;
                }
                return pathhash;
            }
            //获得资源的资源ID
            private static string _getResKey(string respath, System.Type type, eLoadResPath eloadResType)
            {
                return (_getRealPath(respath, type, eloadResType).msRealPath + ":" + type.ToString());
            }
            //将资源ID从正在加载的资源组中移除,并判断是否资源组全部加载完,全部加载完毕,执行回调
            internal static void _removePathInResGroup(string sReseskey, string sReskey, bool bsuccessful, bool bautorelease)
            {
                CResesState rs;
                if (_mDicLoadingResesGroup.ContainsKey(sReseskey))
                {
                    rs = _mDicLoadingResesGroup[sReseskey];
                    int index = rs.mlistpathskey.FindIndex(0, delegate (string s) { return s == sReskey; });
                    string path = "";
                    if (index != -1)
                    {
                        path = rs.mlistpaths[index];
                        rs.mlistpaths.Remove(path);
                        //DLoger.Log("加载====" + path + "=====完毕!" + bsuccessful.ToString());
                        rs.mlistpathskey.Remove(sReskey);
                    }

                    if (rs.mlistpathskey.Count == 0)
                    {
                        eLoadedNotify eloadnotify = bsuccessful == true ? eLoadedNotify.Load_OneSuccessfull : eLoadedNotify.Load_Failed;
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            Hashtable loadedinfo = new Hashtable();
                            loadedinfo.Add("path", path);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            loadedinfo.Add("object", _getResObject(sReskey));
                            rs.listproc[i](eloadnotify == eLoadedNotify.Load_Failed ? rs.listobj[i] : loadedinfo, eloadnotify);
                        }
                        eloadnotify = bsuccessful == true ? eLoadedNotify.Load_Successfull : eLoadedNotify.Load_Failed;
                        _mDicLoadingResesGroup.Remove(sReseskey);
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            rs.listproc[i](rs.listobj[i], eloadnotify);
                        }
                        rs.listproc.Clear();
                        rs.listobj.Clear();

                    }
                    else
                    {
                        eLoadedNotify eloadnotify = bsuccessful == true ? eLoadedNotify.Load_OneSuccessfull : eLoadedNotify.Load_Failed;
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            Hashtable loadedinfo = new Hashtable();
                            loadedinfo.Add("path", path);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            loadedinfo.Add("object", _getResObject(sReskey));
                            rs.listproc[i](eloadnotify == eLoadedNotify.Load_Failed ? rs.listobj[i] : loadedinfo, eloadnotify);
                        }
                    }
                    if (bautorelease)
                    {
                        _releaseAssetDependenceBundle(sReskey);
                    }
                }

            }
            //资源组产生一个资源组的唯一ID
            private static string _makeResGroupMap(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, string[] stags, ProcessDelegateArgc proc, object o)
            {//将资源组和回调函数记录

                CResesState rs;
                string skey;
                _getResGroupMapKey(spaths, types, eloadResTypes, stags, out rs, out skey);
                if (!_mDicLoadingResesGroup.ContainsKey(skey))
                {
                    if (proc != null)
                    {
                        rs.listproc.Add(proc);
                        rs.listobj.Add(o);
                    }
                    _mDicLoadingResesGroup.Add(skey, rs);
                }
                else
                {
                    if (proc != null)
                    {
                        _mDicLoadingResesGroup[skey].listproc.Add(proc);
                        _mDicLoadingResesGroup[skey].listobj.Add(o);
                    }
                }
                return skey;
            }
            private static void _getResGroupMapKey(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, string[] stags, out CResesState resstate, out string skey)
            {
                CResesState rs = new CResesState();
                string paths = "";
                rs.maxpaths = spaths.Length;
                for (int i = 0; i < spaths.Length; i++)
                {
                    string pathkey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    string truepath = _getRealPath(spaths[i], types[i], eloadResTypes[i]).msRealPath;
                    rs.mlistpathskey.Add(pathkey);
                    rs.mlistpaths.Add(truepath);
                    rs.mlistpathstag.Add(stags[i]);
                    paths += truepath;
                }

                resstate = rs;
                skey = paths;
            }
            internal static string _getResAddressByPath(eLoadResPath eloadResType)
            {
                if (eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    return mResourceStreamingAssets;
                }
                else
                {
                    return mResourcesURLAddress;
                }
            }
            internal static AssetBundleManifest _getAssetBundleManifest(eLoadResPath eloadResType)
            {
                if (eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    return _mLocalAssetBundleManifest;
                }
                else
                {
                    return _mURLAssetBundleManifest;
                }
            }
            internal static eLoadResPathState _getLoadResPathState()
            {
                if (mResourcesURLAddress != string.Empty && mResourceStreamingAssets != string.Empty)
                {
                    return eLoadResPathState.LS_ReadURLForUpdate;
                }
                else if ((mResourcesURLAddress != string.Empty && mResourceStreamingAssets == string.Empty)
                        || (mResourcesURLAddress != string.Empty && mResourceStreamingAssets != string.Empty))
                {
                    return eLoadResPathState.LS_ReadURLOnly;
                }
                else if (mResourcesURLAddress == string.Empty && mResourceStreamingAssets != string.Empty)
                {
                    return eLoadResPathState.LS_ReadStreamingOnly;
                }
                else
                {
                    return eLoadResPathState.LS_EmptyAddress;
                }
            }
            /// <summary>
            /// 是否读取打包资源
            /// </summary>
            public static bool mbuseassetbundle = true;
            /// <summary>
            /// 指定资源的网络路径
            /// </summary>
            public static string mResourcesURLAddress = string.Empty;
            /// <summary>
            /// 指定资源的网络路径
            /// </summary>
            public static string mResourceStreamingAssets = string.Empty;
            /// <summary>
            /// 指定资源的网络路径,WWW读取的
            /// </summary>
            public static string mResourceStreamingAssetsForWWW = string.Empty;
            /// <summary>
            /// 是否开启自动释放
            /// </summary>
            public static bool mBAutoRelease = true;

            /// <summary>
            /// 是否是编辑器模式,编辑器模式下设置true,防止在内存清理的时候,误删除掉本地资源prefab
            /// </summary>
            public static bool mbEditorMode = true;

            /// <summary>
            /// 用来存储上次输出的资源缓存信息,用来着对比输出
            /// </summary>
            private static List<string> _mlistRefObjForDebug = new List<string>();

            /// <summary>
            /// 设定记录bundle信息的文件名
            /// </summary>
            private static string mBundlesInfoFileName = "StreamingAssets";
            /// <summary>
            /// 默认资源标志
            /// </summary>
            public const string mSdefaultTag = "ResDefaultTag";
            public const string msNoAutoRelease = "NoAutoRelease";
            /// <summary>
            /// AssetBundleManifest对象
            /// </summary>
            internal static AssetBundleManifest _mURLAssetBundleManifest = null;
            internal static AssetBundleManifest _mLocalAssetBundleManifest = null;

            /// <summary>
            /// 返回是否清理无用资源结束
            /// </summary>
            internal static bool mbUnLoadUnUsedResDone = true;
            /// <summary>
            /// 释放资源的协程返回
            /// </summary>
            internal static bool mbStartDoUnload = false;
            private static Dictionary<string, Hashtable> _mDicLoadedRes = new Dictionary<string, Hashtable>();
            private static List<string> _mListLoadingRes = new List<string>();
            private static Dictionary<string, CResesState> _mDicLoadingResesGroup = new Dictionary<string, CResesState>();
            /// <summary>
            /// 记录依赖包的引用和计数
            /// </summary>
            private static Dictionary<string, List<string>> _mDicAssetsDependBundles = new Dictionary<string, List<string>>();
            private static Dictionary<string, int> _mDicBundlescounts = new Dictionary<string, int>();

            /// <summary>
            /// 记录资源引用的Dic,每隔一段时间,都会删除资源中引用计数为0的资源
            /// </summary>
            internal static Dictionary<string, System.WeakReference> _mDicAssetRef = new Dictionary<string, System.WeakReference>();
            /// <summary>
            /// 放置需要销毁的资源,每隔一段时间都会遍历该列表,销毁资源
            /// </summary>
            internal static List<Object> _mListReleasedObjects = new List<Object>();
            /// <summary>
            /// 记录每个预制件所依赖资源的路径
            /// </summary>
            internal static Dictionary<string, Dictionary<string, AssetsKey>> _mDicAssetsRefConfig = new Dictionary<string, Dictionary<string, AssetsKey>>();



        }
        /// <summary>
        /// 加载结果
        /// </summary>
        public enum eLoadedNotify
        {
            /// <summary>
            /// 加载失败
            /// </summary>
            Load_Failed,
            /// <summary>
            /// 加载一个成功
            /// </summary>
            Load_OneSuccessfull,
            /// <summary>
            /// 加载全部完成
            /// </summary>
            Load_Successfull
        }
        /// <summary>
        /// 枚举3种读取资源的来源
        /// </summary>
        public enum eLoadResPath
        {
            /// <summary>
            /// 读取Resources下的资源
            /// </summary>
            RP_Resources,
            /// <summary>
            /// 读取本地StreamingAssets下的资源
            /// </summary>
            RP_StreamingAssets,
            /// <summary>
            /// 读取网络路径
            /// </summary>
            RP_URL,
            /// <summary>
            /// 未知
            /// </summary>
            RP_Unknow

        }
        internal enum eLoadResPathState
        {
            /// <summary>
            /// 只读取URL的资源
            /// </summary>
            LS_ReadURLOnly,
            /// <summary>
            /// 只读取StreamingAssets的资源
            /// </summary>
            LS_ReadStreamingOnly,
            /// <summary>
            /// 优先读取URL的资源
            /// </summary>
            LS_ReadURLForUpdate,
            /// <summary>
            /// 没有配置资源路径
            /// </summary>
            LS_EmptyAddress,
        }

        internal class CacheBundleInfo
        {
            static private bool mbisInit = false;
            static private Dictionary<string, string> _mdicBundleInfo = new Dictionary<string, string>();
            static private string _smCachinginfofile = Application.persistentDataPath + "/cachinginfo.txt"; 
            static public void initBundleInfo()
            {
                if (mbisInit == false)
                {
                    FileStream fs = new FileStream(_smCachinginfofile, FileMode.OpenOrCreate);
                    StreamReader sr = new StreamReader(fs);
                    string snum = sr.ReadLine();
                    int inum = 0;
                    if (int.TryParse(snum, out inum))
                    {
                        for (int i = 0; i < inum; i++)
                        {
                            string bundlepath = sr.ReadLine();
                            string shash = sr.ReadLine();
                            _mdicBundleInfo.Add(bundlepath, shash);
                        }
                    }
                    sr.Close();
                    fs.Close();
                    mbisInit = true;
                }
                
            }
            static public void saveBundleInfo()
            {
                StreamWriter sw = new StreamWriter(_smCachinginfofile, false);
                List<string> listbundles = new List<string>(_mdicBundleInfo.Keys);
                sw.WriteLine(listbundles.Count);
                for (int i = 0; i < _mdicBundleInfo.Keys.Count; i++)
                {
                    sw.WriteLine(listbundles[i]);
                    sw.WriteLine(_mdicBundleInfo[listbundles[i]]);
                }
                sw.Flush();
                sw.Close();
            }
            static public void updateBundleInfo(string path, string hash)
            {
                if (_mdicBundleInfo.ContainsKey(path))
                {
                    _mdicBundleInfo[path] = hash;
                }
                else
                {
                    _mdicBundleInfo.Add(path, hash);
                }
            }
            static public bool hasBundle(string bundlepath)
            {
                return _mdicBundleInfo.ContainsKey(bundlepath);
            }

            static public bool isCaching(string bundlepath, string hash)
            {
                return _mdicBundleInfo.ContainsKey(bundlepath) && _mdicBundleInfo[bundlepath] == hash;
            }
        }

        internal class AssetsKey
        {
            public AssetsKey(string skey)
            {
                msKey = skey;
            }
            public string msKey = null;
            public List<string> mlistMatTexPropName = null;
        }

       
        internal class CPathAndHash
        {
            public string msRealPath;
            public eLoadResPath meLoadResType;
            public Hash128 mHash;
        }

        //资源组管理
        internal class CResesState
        {

            public List<string> mlistpathskey = new List<string>();
            public List<string> mlistpaths = new List<string>();
            public List<string> mlistpathstag = new List<string>();
            public List<ProcessDelegateArgc> listproc = new List<ProcessDelegateArgc>();
            public List<object> listobj = new List<object>();
            public int maxpaths = 0;
        }
        //加载参数
        internal class CloadParam
        {
            public string[] mpaths = null;
            public System.Type[] mtypes = null;
            public eLoadResPath[] meloadResTypes = null;
            public string[] mtags = null;
            public ProcessDelegateArgc mproc = null;
            public object mo = null;
            public bool mbasyn = true;
            public bool mbautoreleasebundle = true;
            public bool mbloadfromfile = true;

            public CloadParam(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, string tag = "", ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true, bool bautoreleasebundle = true)
            {
                string[] spaths = new string[1];
                System.Type[] types = new System.Type[1];
                eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                string[] stags = new string[1];
                spaths[0] = spath;
                types[0] = type;
                eloadResTypes[0] = eloadResType;
                stags[0] = tag;


                mpaths = spaths;
                mtypes = types;
                mproc = proc;
                mtags = stags;
                mo = o;
                mbasyn = basyn;
                mbautoreleasebundle = bautoreleasebundle;
                mbloadfromfile = bloadfromfile;
                meloadResTypes = eloadResTypes;
            }
            public CloadParam(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, string[] tags, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true, bool bautoreleasebundle = true)
            {
                mpaths = spaths;
                mtypes = types;
                meloadResTypes = eloadResTypes;
                mtags = tags;

                mproc = proc;
                mo = o;
                mbasyn = basyn;
                mbautoreleasebundle = bautoreleasebundle;
                mbloadfromfile = bloadfromfile;

            }
        }
    }

}

