using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            /// <param name="bloadfromfile">是否用loadfromfile方式加载</param>
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
                    mDicDownloadingBundleBytes = new  Dictionary<string, ulong>();
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
                string[] spaths = new string[1];
                spaths[0] = spath;
                requestResNoAutoRelease(spaths, type, eloadResType, proc, o, basyn, bloadfromfile);
                //if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                //{
                //    _checkDependenceList(new CloadParam(spath, type, eloadResType, msNoAutoRelease, proc, o, basyn, bloadfromfile, false));
                //}
                //else
                //{
                //    string[] spaths = new string[1];
                //    System.Type[] types = new System.Type[1];
                //    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                //    string[] stags = new string[1];
                //    spaths[0] = spath;
                //    types[0] = type;
                //    eloadResTypes[0] = eloadResType;
                //    stags[0] = msNoAutoRelease;
                //    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile, false,false);
                //}


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
                
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    List<string> listpaths = new List<string>();
                    List<System.Type> listtypes = new List<System.Type>();
                    List<eLoadResPath> listeloadResTypes = new List<eLoadResPath>();
                    List<string> listtags = new List<string>();
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        if (!_mListNoAutoReleaseBundle.Contains(spaths[i]))
                        {
                            listpaths.Add(spaths[i]);
                            listtypes.Add(type);
                            listeloadResTypes.Add(eloadResType);
                            listtags.Add(msNoAutoRelease);
                        }
                        _addNoAutoReleaseBundlePath(spaths[i]);

                    }
                    if (listpaths.Count == 0)
                    {
                        if (proc != null)
                        {
                            proc(o, eLoadedNotify.Load_Successfull);
                        }
                        
                    }
                    else
                    {
                        _checkDependenceList(new CloadParam(listpaths.ToArray(), listtypes.ToArray(), listeloadResTypes.ToArray(), listtags.ToArray(), proc, o, basyn, bloadfromfile, false));
                    }
                    
                }
                else
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
                    _requestRes(spaths, types, eloadResTypes, stags, proc, o, basyn, bloadfromfile, false, false);
                }


            }
            /// <summary>
            /// 加入不自动释放列表
            /// </summary>
            /// <param name="inputbundpath"></param>
            private static void _addNoAutoReleaseBundlePath(string inputbundpath)
            {
                if (!_mListNoAutoReleaseBundle.Contains(inputbundpath))
                {
                    _mListNoAutoReleaseBundle.Add(inputbundpath);
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
                if (mbuseassetbundle && _mListNoAutoReleaseBundle.Contains(respath))
                {
                    BundleInfoConfig mainfest = null;
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
                        
                        string sReskeybundle = _getRealPath(bundlepath, typeof(AssetBundle), eloadResType).msRealPath;
                        _doBundleCount(sReskeybundle, false);
                       
                    }
                    string sReskey = _getRealPath(respath, typeof(AssetBundle), eloadResType).msRealPath;
                    _doBundleCount(sReskey, false);
                    _mListNoAutoReleaseBundle.Remove(respath);

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
            /// 
            /// </summary>
            /// <param name="spaths"></param>
            /// <param name="types"></param>
            /// <param name="eloadResTypes"></param>
            /// <param name="stags"></param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn"></param>
            /// <param name="bloadfromfile"></param>
            public static void requestRes(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes , string[] stags, ProcessDelegateArgc proc = null, object o = null, bool basyn = true,  bool bloadfromfile = true)
            {
                bool bAllInResources = true;
                for (int i = 0; i < eloadResTypes.Length; i++)
                {
                    if (eloadResTypes[i] != eLoadResPath.RP_Resources)
                    {
                        bAllInResources = false;
                        break;
                    }
                }
                if (mbuseassetbundle && !bAllInResources)
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
                Hashtable proc_pack = new Hashtable();
                proc_pack.Add("proc", loadedproc);
                proc_pack.Add("updateOnlyPack", updateOnlyPack);
                _preLoadManifestAndResConfing(_OnLoadedLatestManifestForUpdate, proc_pack);

            }
            /// <summary>
            /// 处理当前场景物体的引用资源
            /// </summary>
            /// <param name="sScenePath"></param>
            public static void doSceneObjectAssetsRef(string sScenePath)
            {
                if (_mDicAssetsRefConfig.ContainsKey(sScenePath.GetHashCode()))
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
                if (eloadrespath == eLoadResPath.RP_StreamingAssets)
                {
                    return mBundlesInfoFileName + "_ABConfig" + "/AssetbundleInfoConfig";
                }
                else
                {
                    return mBundlesInfoFileName + "URL_ABConfig" + "/AssetbundleInfoConfig";
                }
            }
            internal static string _getAssetsConfigByLoadStyle(bool blocal = false)
            {

                return blocal == false ? "assetsbundles/config/assetsref/assetpathconfig" :
                    "local/assetsref/localassetpathconfig";

            }
            //检查依赖列表
            private static void _checkDependenceList(CloadParam p)
            {
                _preLoadManifestAndResConfing(_OnLoadedAssetBundleManifestForDepdence, p);
            }

            private static void _preLoadManifestAndResConfing(ProcessDelegateArgc proc, object p)
            {
                CacheBundleInfo.initBundleInfo();
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
                            tps[0] = typeof(TextAsset);
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
                                    _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, proc, p);
                                }
                                else if (e == eLoadedNotify.Load_NotTotleSuccessfull)
                                {
                                    DLoger.LogError("load AssetBundleManifest error!");
                                    if (proc != null)
                                    {
                                        proc(p, eLoadedNotify.Load_NotTotleSuccessfull);
                                    }
                                    

                                }
                            }, null, true, true, true);
                        }
                        else
                        {
                            if (proc != null)
                            {
                                proc(p, eLoadedNotify.Load_Successfull);
                            }
                            
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
                            tps[0] = typeof(TextAsset);
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
                                    _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, proc, p);
                                }
                                else if (e == eLoadedNotify.Load_NotTotleSuccessfull)
                                {
                                    DLoger.LogError("load AssetBundleManifest error!");
                                    if (proc != null)
                                    {
                                        proc(p, eLoadedNotify.Load_NotTotleSuccessfull);
                                    }
                                    

                                }
                            }, null, true, true, true);
                        }
                        else
                        {
                            if (proc != null)
                            {
                                proc(p, eLoadedNotify.Load_Successfull);
                            }
                           
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
                            tps[0] = typeof(TextAsset);
                            tps[1] = typeof(TextAsset);
                            eloadResTypes[0] = eLoadResPath.RP_URL;
                            eloadResTypes[1] = eLoadResPath.RP_StreamingAssets;
                            tags = new string[2];
                            tags[0] = "InFini";
                            tags[1] = "InFini";
                            //为了防止资源服务器上的StreamingAssetsURL和客户端的StreamingAssets一样
                            int maxloadnum = miMaxLoadAssetsNum;
                            miMaxLoadAssetsNum = 1;
                            _requestRes(paths, tps, eloadResTypes, tags, (o, e) =>
                            {
                                if (e == eLoadedNotify.Load_OneSuccessfull || e == eLoadedNotify.Load_Failed)
                                {
                                    Hashtable hashinfo = (Hashtable)o;
                                    string path = (string)hashinfo["path"];
                                    string sAssetbundlepath = "";
                                    if (path.Contains("|"))
                                    {
                                        sAssetbundlepath = path.Split('|')[0];
                                    }
                                    else
                                    {
                                        sAssetbundlepath = path;
                                    }
                                    if (_mDicLoadedBundle.ContainsKey(sAssetbundlepath))
                                    {
                                        if (_mDicLoadedBundle[sAssetbundlepath] != null)
                                        {
                                            _mDicLoadedBundle[sAssetbundlepath].Unload(false);
                                        }

                                        _mDicLoadedBundle.Remove(sAssetbundlepath);
                                        DLoger.Log("释放bundle:=" + sAssetbundlepath);
                                    }
                                    
                                }
                                if (e == eLoadedNotify.Load_Successfull)
                                {
                                    miMaxLoadAssetsNum = maxloadnum;
                                    string[] assetspaths = new string[1];
                                    System.Type[] assetstps = new System.Type[1];
                                    eLoadResPath[] assetseloadResTypes = new eLoadResPath[1];
                                    assetspaths[0] = _getAssetsConfigByLoadStyle();
                                    assetstps[0] = typeof(TextAsset);
                                    assetseloadResTypes[0] = eLoadResPath.RP_URL;
                                    _makeAssetBundleManifest();
                                    _requestRes(assetspaths, assetstps, assetseloadResTypes, tags, proc, p);
                                }
                                else if (e == eLoadedNotify.Load_NotTotleSuccessfull)
                                {
                                    DLoger.LogError("load AssetBundleManifest error!");
                                    if (proc != null)
                                    {
                                        proc(p, eLoadedNotify.Load_NotTotleSuccessfull);
                                    }
                                    

                                }
                            }, null, true, true, true);

                        }
                        else
                        {
                            //已经加载完
                            if (proc != null)
                            {
                                proc(p, eLoadedNotify.Load_Successfull);
                            }
                            
                        }
                    }
                    else
                    {
                        DLoger.LogError("you request a assetsbundle from  error Paths!");
                    }

                }
                else
                {
                    if (proc != null)
                    {
                        proc(p, eLoadedNotify.Load_Successfull);
                    }
                    
                }
            }
            private static void _makeAssetBundleManifest()
            {
                if (_mURLAssetBundleManifest == null && mResourcesURLAddress != string.Empty)
                {
                    TextAsset txt = (TextAsset)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL), typeof(TextAsset), eLoadResPath.RP_URL);
                    _mURLAssetBundleManifest = new BundleInfoConfig();
                    _mURLAssetBundleManifest.initBundleInfoConfig(txt.ToString());
                    removeRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL), typeof(TextAsset), eLoadResPath.RP_URL);
                    
                    //string[] listbundles = _mURLAssetBundleManifest.GetAllAssetBundles();
                    //for (int i = 0; i < listbundles.Length; i++)
                    //{
                    //    _mDicURLBundlesHash.Add(listbundles[i], _mURLAssetBundleManifest.GetAssetBundleHash(listbundles[i]));
                    //}
                }
                if (_mLocalAssetBundleManifest == null && mResourceStreamingAssets != string.Empty)
                {
                    TextAsset txt = (TextAsset)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets), typeof(TextAsset), eLoadResPath.RP_StreamingAssets);
                    _mLocalAssetBundleManifest = new BundleInfoConfig();
                    _mLocalAssetBundleManifest.initBundleInfoConfig(txt.ToString());
                    removeRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets), typeof(TextAsset), eLoadResPath.RP_StreamingAssets);
                    //string[] listbundles = _mLocalAssetBundleManifest.GetAllAssetBundles();
                    //for (int i = 0; i < listbundles.Length; i++)
                    //{
                    //    _mDicLocalBundlesHash.Add(listbundles[i], _mLocalAssetBundleManifest.GetAssetBundleHash(listbundles[i]));
                    //}
                }
            }

            //已经加载完依赖列表
            private static void _OnLoadedAssetBundleManifestForDepdence(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {//开始加载依赖资源
                if (loadedNotify == eLoadedNotify.Load_Successfull)
                {
                   
                    _makeRefAssetsConfig();
                    CloadParam param = (CloadParam)obj;
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
                        BundleInfoConfig manifest = null;
                        eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                        _getShouldUseManifest(param.meloadResTypes[i], out manifest, out loadrespath);
                        if (manifest != null)
                        {
                            string[] deppaths = manifest.GetAllDependencies(bundlepath);
                            bool biscontained = _mDicLoadedRes.ContainsKey(_getResKey(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i]));
                            for (int j = 0; j < deppaths.Length; j++)
                            {
                                string depbundletruepath = _getRealPath(deppaths[j], typeof(AssetBundle), loadrespath).msRealPath;
                                
                                _doBundleCount(depbundletruepath);
                                
                                //_addAssetDependenceBundle(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i], deppaths[j], loadrespath);
                                if (!depBundleNameList.Contains(deppaths[j]) && !biscontained)
                                {
                                    depBundleNameList.Add(deppaths[j]);
                                    depBundleLoadPathlist.Add(loadrespath);
                                }
                            }
                            string truepath = _getRealPath(bundlepath, typeof(AssetBundle), loadrespath).msRealPath;
                            
                            _doBundleCount(truepath);
                           
                            //_addAssetDependenceBundle(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i], bundlepath, loadrespath);
                            if (!depBundleNameList.Contains(bundlepath) && !biscontained)
                            {

                                depBundleNameList.Add(bundlepath);
                                depBundleLoadPathlist.Add(loadrespath);
                            }
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
                else if (loadedNotify == eLoadedNotify.Load_NotTotleSuccessfull)
                {
                    CloadParam param = (CloadParam)obj;
                    _OnloadedDependenceBundles(param, eLoadedNotify.Load_NotTotleSuccessfull);
                }
            }
            private static void _getShouldUseManifest(eLoadResPath eloadrespath,out BundleInfoConfig manifest,out eLoadResPath eoutloadrespath)
            {
                eLoadResPathState eloadresstate = _getLoadResPathState();
                if (eloadrespath == eLoadResPath.RP_Resources)
                {
                    eoutloadrespath = eLoadResPath.RP_Resources;
                    manifest = null;
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                {

                    manifest = _getAssetBundleManifest(eLoadResPath.RP_URL);
                    eoutloadrespath = eLoadResPath.RP_URL;

                }

                else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                {

                    manifest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                    eoutloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                {

                    manifest = _getAssetBundleManifest(eloadrespath);
                    eoutloadrespath = eloadrespath;
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                    eoutloadrespath = eLoadResPath.RP_Unknow;
                    manifest = null;
                }
            }
            //internal static void _addAssetDependenceBundle(string respath, System.Type restype, eLoadResPath eresloadrespath, string bundlepath, eLoadResPath ebundleloadrespath)
            //{
            //    string ireskey = _getResKey(respath, restype, eresloadrespath);
            //    string sbundlekey = _getRealPath(bundlepath, typeof(AssetBundle), ebundleloadrespath).msRealPath;
            //    if (!_mDicAssetsDependBundles.ContainsKey(ireskey))
            //    {
            //        _mDicAssetsDependBundles.Add(ireskey, new List<string>());

            //    }
            //    _mDicAssetsDependBundles[ireskey].Add(sbundlekey);
                
            //}
            //internal static void _releaseAssetDependenceBundle(string sReskey)
            //{
            //    if (_mDicAssetsDependBundles.ContainsKey(sReskey))
            //    {
            //        List<string> depbundles = _mDicAssetsDependBundles[sReskey];
            //        foreach (string depbundlekey in depbundles)
            //        {
            //            if (_mDicBundlescounts.ContainsKey(depbundlekey))
            //            {
            //                _doBundleCount(depbundlekey, false);
            //            }
            //        }
            //        _mDicAssetsDependBundles.Remove(sReskey);
            //    }
            //}
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
            internal static void _doBundleCount(string ibundlekey, bool badd = true)
            {
                
                if (!_mDicBundlescounts.ContainsKey(ibundlekey))
                {
                    _mDicBundlescounts.Add(ibundlekey, 0);
                }
                _mDicBundlescounts[ibundlekey] = badd ? _mDicBundlescounts[ibundlekey] + 1 : _mDicBundlescounts[ibundlekey] - 1;
                //DLoger.Log("bundle 计数:" + ibundlekey + ":" + _mDicBundlescounts[ibundlekey]);
                if (_mDicBundlescounts[ibundlekey] < 0)
                {
                    DLoger.LogError("assetbundle count is less then 0:" + ibundlekey);
                }
                if (_mDicBundlescounts[ibundlekey] == 0)
                {
                    _removeRes((ibundlekey + ":" + (typeof(AssetBundle)).ToString()));
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
                        BundleInfoConfig mainfest = _mURLAssetBundleManifest;
                        if (mainfest != null)
                        {
                            
                            string[] bundles = mainfest.GetAllAssetBundles();
                            List<string> listbundles = new List<string>(bundles);
                            //删除缓存中不需要的bundle
                            CacheBundleInfo.clearNoUsedBundle(listbundles);
                            for (int i = 0; i < bundles.Length; i++)
                            {
                                
                                CPathAndHash pathhash = _getRealPath(bundles[i], typeof(AssetBundle), eLoadResPath.RP_URL);
                                //如果不是从远程下载,则不跟新
                                if (pathhash.meLoadResType != eLoadResPath.RP_URL)
                                {
                                    continue;
                                }
                                //如果在排除之外并且没有下载过,也不跟新
                                if (updateOnlyPacks.Contains(bundles[i]) && !CacheBundleInfo.hasBundle(bundles[i]))
                                {//这里判断那些不需要获取的资源包(例如各个国家的语言包)
                                    continue;
                                }
                                //如果caching已经有,也不跟新
                                if (CacheBundleInfo.isCaching(bundles[i], pathhash.mMD5.ToString()) == false)
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
                            if (proc != null)
                            {
                                proc(needUpdateBundleList, eLoadedNotify.Load_Successfull);
                            }
                            
                        }
                        else
                        {
                            if (proc != null)
                            {
                                proc(null, eLoadedNotify.Load_Successfull);
                            }
                            
                        }
                    }
                    else
                    {
                        if (proc != null)
                        {
                            proc(null, eLoadedNotify.Load_Successfull);
                        }
                        
                    }
                }
                else if (loadedNotify == eLoadedNotify.Load_NotTotleSuccessfull)
                {
                    ProcessDelegateArgc proc = (ProcessDelegateArgc)((Hashtable)obj)["proc"];
                    List<string> updateOnlyPacks = new List<string>((string[])((Hashtable)obj)["updateOnlyPack"]);
                    if (proc != null)
                    {
                        proc(null, eLoadedNotify.Load_NotTotleSuccessfull);
                    }
                   
                }
            }
            /// <summary>
            /// 根据资源引用表,生成配置
            /// </summary>
            private static void _makeRefAssetsConfig(bool blocal = false)
            {
                Dictionary<int, Dictionary<int, AssetsKey>> DicAssetsRefConfig = blocal == false ? _mDicAssetsRefConfig : _mDicLocalAssetsRefConfig;
                eLoadResPath loadrespath = blocal == false ? eLoadResPath.RP_URL : eLoadResPath.RP_Resources;
                if (DicAssetsRefConfig.Keys.Count != 0)
                {
                    return;
                }
                
                TextAsset AssetsRefConfig = (TextAsset)getRes(_getAssetsConfigByLoadStyle(blocal), typeof(TextAsset), loadrespath);
                StringReader sr = new StringReader(AssetsRefConfig.text);
                uint objsnum = uint.Parse(sr.ReadLine());
                for (int i = 0; i < objsnum; i++)
                {
                    //读取无效行
                    string line = sr.ReadLine();
                    string objname = sr.ReadLine();
                    int objnamehashcode = int.Parse(sr.ReadLine());

                    if (!DicAssetsRefConfig.ContainsKey(objnamehashcode))
                    {
                        DicAssetsRefConfig.Add(objnamehashcode, new Dictionary<int, AssetsKey>());
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
                        DLoger.LogError(errorline + "1");
                    }

                    for (int a = 0; a < assetsnum; a++)
                    {
                        //读取无效行
                        string nouse = sr.ReadLine();
                        string assetname = sr.ReadLine();
                        string key = sr.ReadLine();
                        int assetnamehashcode = int.Parse(sr.ReadLine());
                        int keyhashcode = int.Parse(sr.ReadLine());

                        if (!DicAssetsRefConfig[objnamehashcode].ContainsKey(assetnamehashcode))
                        {
                            DicAssetsRefConfig[objnamehashcode].Add(assetnamehashcode, new AssetsKey(keyhashcode));
                        }
                        else
                        {
                            DLoger.LogError(objname + ":" + assetname + "资源名重复!");
                        }

                        if (assetname.Contains(typeof(Material).ToString()))
                        {
                            int texporpnum;
                            errorline = sr.ReadLine();
                            if (!int.TryParse(errorline, out texporpnum))
                            {
                                DLoger.LogError(errorline + "2");
                            }
                            if (texporpnum != 0)
                            {
                                DicAssetsRefConfig[objnamehashcode][assetnamehashcode].mlistMatTexPropName = new List<string>(texporpnum);

                                for (int tp = 0; tp < texporpnum; tp++)
                                {
                                    DicAssetsRefConfig[objnamehashcode][assetnamehashcode].mlistMatTexPropName.Add(sr.ReadLine());
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
                    _requestRes(param.mpaths, param.mtypes, param.meloadResTypes, param.mtags, _doReleaseBundleCount, param, param.mbasyn, param.mbloadfromfile, false, param.mbautoreleasebundle);
                }
                else if (loadedNotify == eLoadedNotify.Load_NotTotleSuccessfull)
                {
                    CloadParam param = (CloadParam)o;
                    param.mproc(param.mo, eLoadedNotify.Load_NotTotleSuccessfull);
                }
            }
            private static void _doReleaseBundleCount(object o, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {
                
                if ((loadedNotify == eLoadedNotify.Load_Successfull || loadedNotify == eLoadedNotify.Load_NotTotleSuccessfull) && o != null)
                {
                    CloadParam param = (CloadParam)o;
                    if (param.mbautoreleasebundle)
                    {
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
                            BundleInfoConfig manifest = null;
                            eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                            _getShouldUseManifest(param.meloadResTypes[i], out manifest, out loadrespath);
                            if (manifest != null)
                            {
                                string[] deppaths = manifest.GetAllDependencies(bundlepath);
                                for (int j = 0; j < deppaths.Length; j++)
                                {
                                    string depbundletruepath = _getRealPath(deppaths[j], typeof(AssetBundle), loadrespath).msRealPath;
                                    _doBundleCount(depbundletruepath, false);
   
                                }
                                string truepath = _getRealPath(bundlepath, typeof(AssetBundle), loadrespath).msRealPath;
                                _doBundleCount(truepath, false);
                                
                                
                            }

                        }

                    }
                    if (param.mproc != null)
                    {
                        param.mproc(param.mo, loadedNotify);
                    }
                    
                }
                else if (loadedNotify == eLoadedNotify.Load_OneSuccessfull || loadedNotify == eLoadedNotify.Load_Failed)
                {
                    Hashtable loadedinfo = (Hashtable)o;
                    ProcessDelegateArgc proc = ((CloadParam)loadedinfo["procobj"]).mproc;
                    if (proc != null)
                    {
                        proc(o, loadedNotify);
                    }
                    
                }
               
            }
            //加载资源组,指定每个资源的类型,资源都加载完会执行回调proc
            private static void _requestRes(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, string[] stags, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bloadfromfile = true, bool bNoUseCatching = false, bool bautoReleaseBundle = true,bool bonlydownload = false)
            {
                
                //将请求的资源组加入正在加载的资源组列表,并返回资源组ID
                string sResGroupKey = _makeResGroupMap(spaths, types, eloadResTypes, stags, proc, o);
                for (int i = 0; i < spaths.Length; i++)
                {
                    CPathAndHash pathhash = _getRealPath(spaths[i], types[i], eloadResTypes[i]);
                    string truepath = pathhash.msRealPath;
                    string assetsbundlepath;
                    if (truepath.Contains("|"))
                    {
                        assetsbundlepath = truepath.Split('|')[0];

                    }
                    else
                    {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                        assetsbundlepath = truepath;
                    }
                    //对要加载的bundle加入到计数设置
                    _doBundleCount(assetsbundlepath);
                    _doBundleCount(assetsbundlepath,false);

                    eLoadResPath finalloadrespath = pathhash.meLoadResType;


                    string sResKey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    if (_mSetRemovedObjects.Contains(sResKey))
                    {
                        _mSetRemovedObjects.Remove(sResKey);
                        if (_mDicLoadedRes.ContainsKey(sResKey))
                        {
                            _mDicLoadedRes[sResKey]["Tag"] = stags[i];
                        }

                    }
                    //如果资源组中的此个资源已经加载完毕(剔除资源组中已经加载完毕的资源)
                    if (_mDicLoadedRes.ContainsKey(sResKey))
                    {
                        if (_mDicLoadedRes[sResKey] == null)
                        {
                            DLoger.LogError("Loaded Asset:=" + sResKey + "==is null!");
                        }
                        //将该资源从资源组中移除
                        _removePathInResGroup(sResGroupKey, sResKey, truepath, spaths[i], true);
                        continue;

                    }
                    if (mbuseassetbundle == true && eloadResTypes[i] != eLoadResPath.RP_Resources)
                    {//如果是打包读取,并且不直接从Resources读取

                        //异步读取资源
                        CPathAndHash pashhash = _getRealPath(spaths[i], types[i], eloadResTypes[i]);
                        //string temppath;
                        //if (types[i] != typeof(AssetBundle))
                        //{//不是只加载AssetBundle
                        //    string name = Path.GetFileNameWithoutExtension(spaths[i]);
                        //    string dir = spaths[i].Substring(0, spaths[i].Length - name.Length - 1);
                        //    temppath = dir + "|" + name;
                        //}
                        //else
                        //{
                        //    temppath = spaths[i];
                        //}
                        string md5 = pathhash.mMD5;
                        eLoadResPath elp = pathhash.meLoadResType;
                        if (elp == eLoadResPath.RP_Unknow)
                        {
                            DLoger.LogError("load error:" + assetsbundlepath + " == not contains in local or url");
                            _removePathInResGroup(sResGroupKey, sResKey, truepath, spaths[i], false);

                        }
                        else
                        {
                            LoadAsset.getInstance().loadAsset(truepath, finalloadrespath, spaths[i], types[i], stags[i], sResGroupKey, md5, basyn, bNoUseCatching, bautoReleaseBundle, bonlydownload, bloadfromfile);
                            if (!_mListLoadingRes.Contains(sResKey))
                            {
                                _mListLoadingRes.Add(sResKey);
                            }
                        }
                        


                    }
                    else if (types[i] != typeof(AssetBundle))
                    {//否则直接读取Resources散开资源

                        Object t = null;

                        t = Resources.Load(truepath, types[i]);

                        if (t != null)
                        {
                            //这里加载本地资源去冗余列表
                            if (getRes(_getAssetsConfigByLoadStyle(true), typeof(TextAsset), eLoadResPath.RP_Resources) == null)
                            {
                                Object obj = Resources.Load(_getAssetsConfigByLoadStyle(true), typeof(TextAsset));
                                string slocalassetrefkey = _getResKey(_getAssetsConfigByLoadStyle(true), typeof(TextAsset), eLoadResPath.RP_Resources);
                                Hashtable hash = new Hashtable();
                                hash.Add("Object", obj);
                                hash.Add("Tag", stags[i]);
                                hash.Add("InResources", true);
                                hash.Add("IsAssetsBundle", false);
                                _mDicLoadedRes.Add(slocalassetrefkey, hash);
                            }
                            _makeRefAssetsConfig(true);

                            if (mbEditorMode == false)
                            {
                                _doWithAssetRefToObject(t, spaths[i], true);
                            }
                            
                            Hashtable reshash = new Hashtable();
                            reshash.Add("Object",t);
                            reshash.Add("Tag",stags[i]);
                            reshash.Add("InResources", true);
                            reshash.Add("IsAssetsBundle", false);
                            _mDicLoadedRes.Add(sResKey, reshash);
                            _removePathInResGroup(sResGroupKey, sResKey, truepath, spaths[i], true);

                        }
                        else
                        {
                            _removePathInResGroup(sResGroupKey, sResKey, truepath, spaths[i], false);
                            DLoger.LogError("Load===" + spaths[i] + "===Failed");
                        }
                        continue;
                    }
                    else
                    {
                        _removePathInResGroup(sResGroupKey, sResKey, truepath, spaths[i], true);
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
            /// 检查非常住bundle是否都释放完毕
            /// </summary>
            /// <param name="tag"></param>
            internal static bool checkBundleReleased()
            {
                if (mbuseassetbundle)
                {
                    BundleInfoConfig mainfest = null;
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    if (eloadresstate == eLoadResPathState.LS_ReadURLOnly ||
                        eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                    {

                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_URL);

                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                    {
                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                    }
                    List<string>.Enumerator it = _mListNoAutoReleaseBundle.GetEnumerator();
                    HashSet<string> setNoAutoReleasebundle = new HashSet<string>(_mListNoAutoReleaseBundle);
                    while (it.MoveNext())
                    {
                        string inputpath = it.Current;
                        string[] depbundles = mainfest.GetAllDependencies(inputpath);
                        for (int i = 0; i < depbundles.Length; i++)
                        {
                            setNoAutoReleasebundle.Add(depbundles[i]);
                        }
                        
                    }
                    return setNoAutoReleasebundle.Count == _mDicLoadedBundle.Count;

                }
                return true;
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
                            addToRemoveRes(zeroref[i]);
                        }

                    }
                    //_beginUnloadUnUsedAssets();
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
            /// 重置所有设置
            /// </summary>
            public static void reset()
            {
                mbuseassetbundle = true;

                mResourcesURLAddress = string.Empty;
                mResourceStreamingAssets = string.Empty;
                mResourceStreamingAssetsForWWW = string.Empty;
                msCachingPath = "HD";
                mBAutoRelease = true;

                _mbNotDownLoad = false;

                mbEditorMode = true;

                _mlistRefObjForDebug = new List<string>();

                mBundlesInfoFileName = "StreamingAssets";

                _mfLastReleaseBundleTime = 0;
                _mfMAXReleaseBundleTime = -1;

                miMaxLoadAssetsNum = -1;

                _mURLAssetBundleManifest = null;
                _mLocalAssetBundleManifest = null;
                //_mDicURLBundlesHash = new Dictionary<string, Hash128>();
                //_mDicLocalBundlesHash = new Dictionary<string, Hash128>();

                _mbUnLoadUnUsedResDone = true;
                mbStartDoUnload = false;
                _mDicLoadedRes = new Dictionary<string, Hashtable>();
                _mListLoadingRes = new List<string>();
                _mDicLoadingResesGroup = new Dictionary<string, CResesState>();

                _mDicBundlescounts = new Dictionary<string, int>();


                _mDicAssetRef = new Dictionary<int, System.WeakReference>();

                _mListReleasedObjects = new List<Object>();
                _mDicAssetsRefConfig = new Dictionary<int, Dictionary<int, AssetsKey>>();


                Dictionary<string, AssetBundle>.Enumerator it = _mDicLoadedBundle.GetEnumerator();
                while (it.MoveNext())
                {
                    it.Current.Value.Unload(false);
                }
                _mDicLoadedBundle = new Dictionary<string, AssetBundle>();
                _mListLoadingBundle = new List<string>();
                _mListNoAutoReleaseBundle = new List<string>();
                mDicDownloadingBundleBytes = new Dictionary<string, ulong>();
                LoadAsset.getInstance().StopAllCoroutines();
                LoadAsset.getInstance().reset();
                SingleMono.RemoveInstance("LoadAsset");
                CacheBundleInfo.reset();
            }

            /// <summary>
            /// 开始执行卸载冗余资源和GC
            /// </summary>
            /// <returns></returns>
            static public void startUnloadUnusedAssetAndGC()
            {
                if (mbuseassetbundle == true && SingleMono.IsCreatedInstance("LoadAsset"))
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
                    DLoger.Log("mbStartDoUnload:" + mbStartDoUnload + ",_mbUnLoadUnUsedResDone:" + _mbUnLoadUnUsedResDone);
                    return mbStartDoUnload == false && _mbUnLoadUnUsedResDone == true;
                }
                else
                {
                    return true;
                }
                
            }
            /// <summary>
            /// 清除关于远程下载的信息,使得程序都从包里读取
            /// </summary>
            /// <returns></returns>
            static public void closeURL(bool bclose = true)
            {
                _mbNotDownLoad = bclose;
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
                    _mbUnLoadUnUsedResDone = false;
                }
                else
                {
                    DLoger.Log("====开始GC====1");
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    DLoger.Log("====GC完毕====1");
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
                //_removeRes(skey,true);
                addToRemoveRes(skey);

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

                                string tag = _getResObjectTag(sReskey);
                                DLoger.Log("删除资源===" + sReskey + "=====tag:" + tag);
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


                            string tag = _getResObjectTag(sReskey);
                            DLoger.Log("删除资源===" + sReskey + "=====tag:" + tag);
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
                    //_beginUnloadUnUsedAssets();
                }
                
            }
            internal static void addToRemoveRes(string reskey)
            {
                _mSetRemovedObjects.Add(reskey);
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
                //_removeRes(skey,true);
                addToRemoveRes(skey);
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
                    List<int> listkeys = new List<int>(_mDicAssetRef.Keys);
                    listkeys = listkeys.FindAll((o) => { return (Object)_mDicAssetRef[o].Target != null  && _mDicAssetRef[o].IsAlive; });
                    sw.WriteLine("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    DLoger.Log("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    foreach (int key in listkeys)
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
                    List<int> listkeys = new List<int>(_mDicAssetRef.Keys);
                    listkeys = listkeys.FindAll((o) => { return (Object)_mDicAssetRef[o].Target != null && _mDicAssetRef[o].IsAlive; });
                    DLoger.Log("*********开始打印缓存的Assets资源:" + listkeys.Count + " *********");
                    foreach (int key in listkeys)
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
            internal static void _doWithAssetRefToObject(Object o, string sobjkey,bool blocal = false)
            {
                Dictionary<int, Dictionary<int, AssetsKey>> DicAssetsRefConfig = blocal == false ? _mDicAssetsRefConfig : _mDicLocalAssetsRefConfig;
                int iobjkey = sobjkey.GetHashCode();
                if (!DicAssetsRefConfig.ContainsKey(iobjkey.GetHashCode()) || mbuseassetbundle == false)
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
                        //去掉oncullstatechange
                        MaskableGraphic mg = comps[i] as MaskableGraphic;
                        if (mg != null)
                        {
                            mg.onCullStateChanged = null;
                        }

                        //处理字体
                        Text text = comps[i] as Text;
                        if (text != null)
                        {
                            obj = text.font;
                            type = typeof(Font);

                            if (obj != null)
                            {
                                Material mat = text.font.material;
                                Shader shader = mat.shader;
                                Texture tex = mat.mainTexture;
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    text.font = (Font)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    audio.clip = (AudioClip)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    amt.runtimeAnimatorController = (RuntimeAnimatorController)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    meshfilter.sharedMesh = (Mesh)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    skinmesh.sharedMesh = (Mesh)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {//如果资源引用配置里面有该资源记录
                                 //增加资源计数,并且替catch资源
                                    particlerender.mesh = (Mesh)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                }

                            }
                        }
                        //处理UI

                        RawImage rawimage = comps[i] as RawImage;
                        if (rawimage != null)
                        {
                            obj = rawimage.material;
                            type = typeof(Material);
                            if (obj != null)
                            {
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {
                                    rawimage.material = (Material)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                }
                            }
                            obj = rawimage.texture;
                            type = typeof(Texture);
                            if (obj != null)
                            {
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {
                                    rawimage.texture = (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                }
                            }
                        }
                        Button btn = comps[i] as Button;
                        if (btn != null)
                        {
                            btn.onClick = null;
                        }
                        ScrollRect scr = comps[i] as ScrollRect;
                        if (scr != null)
                        {
                            scr.onValueChanged = null;
                        }
                        

                        Image image = comps[i] as Image;
                        SpriteRenderer spr = comps[i] as SpriteRenderer;
                        Sprite spt = null;

                        if (image != null)
                        {
                            spt = image.sprite;

                            obj = image.material;
                            type = typeof(Material);
                            if (obj != null)
                            {
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {
                                    image.material = (Material)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                }
                            }
                        }
                        if (spr != null)
                        {
                            spt = spr.sprite;

                            obj = spr.sharedMaterial;
                            type = typeof(Material);
                            if (obj != null)
                            {
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {
                                    spr.sharedMaterial = (Material)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                }
                            }

                        }
                        if (spt != null)
                        {
                            Texture alphatexture = spt.associatedAlphaSplitTexture;
                            obj = spt.texture;
                            type = typeof(Texture);
                            if (obj != null)
                            {
                                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                _mTempStringBuilder.Append(obj.name);
                                _mTempStringBuilder.Append(":");
                                _mTempStringBuilder.Append(type);
                                snamekey = _mTempStringBuilder.ToString();
                                int namekeyhashcode = snamekey.GetHashCode();
                                if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                {
                                    Texture tex = (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                    string spritename = spt.name;
                                    if (!spt.name.Contains("(replace)"))
                                    {
                                        spritename = spt.name + "(replace)";
                                    }
                                    try
                                    {
                                        spt = Sprite.Create((Texture2D)tex, spt.rect, new Vector2(spt.pivot.x / spt.rect.width, spt.pivot.y / spt.rect.height), spt.pixelsPerUnit, 0, SpriteMeshType.FullRect, spt.border);
                                    }
                                    catch(System.Exception ex)
                                    {
                                        DLoger.LogError("请立即修复 == Do Ref Asset Error: Create Sprite Error:" + comps[i].name + "==:Ex:" + ex);
                                        spt = null;
                                    }
                                    if (spt != null)
                                    {
                                        spt.name = spritename;


                                        if (image != null)
                                        {
                                            image.sprite = spt;
                                        }
                                        if (spr != null)
                                        {
                                            spr.sprite = spt;
                                        }
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
                                        _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                        _mTempStringBuilder.Append(obj.name);
                                        _mTempStringBuilder.Append(":");
                                        _mTempStringBuilder.Append(type);
                                        snamekey = _mTempStringBuilder.ToString();
                                        int namekeyhashcode = snamekey.GetHashCode();
                                        if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                        {
                                            Texture tex = (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                            mat.SetTexture("_MainTex", tex);
                                        }
                                    }
                                    obj = mat.GetTexture("_MyAlphaTex");
                                    type = typeof(Texture);
                                    if (obj != null)
                                    {
                                        _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                        _mTempStringBuilder.Append(obj.name);
                                        _mTempStringBuilder.Append(":");
                                        _mTempStringBuilder.Append(type);
                                        snamekey = _mTempStringBuilder.ToString();
                                        int namekeyhashcode = snamekey.GetHashCode();
                                        if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                        {
                                            Texture tex = (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                            mat.SetTexture("_MyAlphaTex", tex);
                                        }
                                    }
                                    else
                                    {
                                        DLoger.LogError("get alphtex from shader failed!:" + go.name + "/" + comps[i].transform.name + "/" + mat.name);
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
                            Material[] matstemp = new Material[render.sharedMaterials.Length];
                            //材质
                            for (int m = 0; m < render.sharedMaterials.Length; m++)
                            {
                                obj = render.sharedMaterials[m];
                                matstemp[m] = (Material)obj;
                                type = typeof(Material);
                                if (obj != null)
                                {
                                    _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                    _mTempStringBuilder.Append(obj.name);
                                    _mTempStringBuilder.Append(":");
                                    _mTempStringBuilder.Append(type);
                                    snamekey = _mTempStringBuilder.ToString();
                                    int namekeyhashcode = snamekey.GetHashCode();
                                    if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                    {
                                        matstemp[m] = (Material)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
                                    }
                                }
                            }
                            render.sharedMaterials = matstemp;
                            Material[] mats = render.sharedMaterials;
                            for (int m = 0; m < mats.Length; m++)
                            {
                                //贴图
                                if (mats[m] != null)
                                {
                                    
                                    Material mat = mats[m];
                                    int renderqueue = mat.renderQueue;
                                    _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                    _mTempStringBuilder.Append(mat.name);
                                    _mTempStringBuilder.Append(":");
                                    _mTempStringBuilder.Append(typeof(Material).ToString());
                                    snamekey = _mTempStringBuilder.ToString();
                                    int namekeyhashcode = snamekey.GetHashCode();
                                    if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                    {
                                        if (DicAssetsRefConfig[iobjkey][namekeyhashcode].mlistMatTexPropName != null)
                                        {
                                            string[] proname = DicAssetsRefConfig[iobjkey][namekeyhashcode].mlistMatTexPropName.ToArray();
                                            for (int texpr = 0; texpr < proname.Length; texpr++)
                                            {
                                                Texture tex = mat.GetTexture(proname[texpr]);
                                                if (tex != null)
                                                {
                                                    obj = tex;
                                                    type = typeof(Texture);
                                                    _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                                    _mTempStringBuilder.Append(obj.name);
                                                    _mTempStringBuilder.Append(":");
                                                    _mTempStringBuilder.Append(type);
                                                    snamekey = _mTempStringBuilder.ToString();
                                                    namekeyhashcode = snamekey.GetHashCode();
                                                    if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                                    {
                                                        render.sharedMaterials[m].SetTexture(proname[texpr], (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj));
                                                    }
                                                }
                                                else
                                                {
                                                    DLoger.LogError("assetresconfig:" + sobjkey + "/" + go.name + "/" + comps[i].transform.name + "/" + mat.name + "/" + proname[texpr] + ":error!");
                                                }

                                            }
                                        }
                                        
                                        Shader shader = mat.shader;
                                        if (shader != null)
                                        {
                                            obj = shader;
                                            type = typeof(Shader);
                                            _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                            _mTempStringBuilder.Append(obj.name);
                                            _mTempStringBuilder.Append(":");
                                            _mTempStringBuilder.Append(type);
                                            snamekey = _mTempStringBuilder.ToString();
                                            namekeyhashcode = snamekey.GetHashCode();
                                            if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                            {
                                                mat.shader = (Shader)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, obj);
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
                    _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                    _mTempStringBuilder.Append(o.name);
                    _mTempStringBuilder.Append(":");
                    _mTempStringBuilder.Append(deptype);
                    snamekey = _mTempStringBuilder.ToString();
                    int namekeyhashcode = snamekey.GetHashCode();
                    if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                    {
                        if (mat == null)
                        {
                            o = _doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, o);
                        }
                        else
                        {
                            int renderqueue = mat.renderQueue;
                            _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                            _mTempStringBuilder.Append(mat.name);
                            _mTempStringBuilder.Append(":");
                            _mTempStringBuilder.Append(typeof(Material).ToString());
                            snamekey = _mTempStringBuilder.ToString();
                            namekeyhashcode = snamekey.GetHashCode();
                            if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                            {
                                mat = (Material)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, o);

                                string[] proname = DicAssetsRefConfig[iobjkey][namekeyhashcode].mlistMatTexPropName.ToArray();
                                for (int texpr = 0; texpr < proname.Length; texpr++)
                                {
                                    Texture tex = mat.GetTexture(proname[texpr]);
                                    if (tex != null)
                                    {
                                        deptype = typeof(Texture);
                                        _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                        _mTempStringBuilder.Append(tex.name);
                                        _mTempStringBuilder.Append(":");
                                        _mTempStringBuilder.Append(deptype);
                                        snamekey = _mTempStringBuilder.ToString();
                                        namekeyhashcode = snamekey.GetHashCode();
                                        if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                        {
                                            mat.SetTexture(proname[texpr], (Texture)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, tex));
                                        }
                                    }

                                }
                                Shader shader = mat.shader;
                                if (shader != null)
                                {
                                    deptype = typeof(Shader);
                                    _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                                    _mTempStringBuilder.Append(shader.name);
                                    _mTempStringBuilder.Append(":");
                                    _mTempStringBuilder.Append(deptype);
                                    snamekey = _mTempStringBuilder.ToString();
                                    namekeyhashcode = snamekey.GetHashCode();
                                    if (DicAssetsRefConfig[iobjkey].ContainsKey(namekeyhashcode))
                                    {
                                        mat.shader = (Shader)_doWithAssetRefCount(DicAssetsRefConfig[iobjkey][namekeyhashcode].miKey, shader);
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

            internal static Object _doWithAssetRefCount(int key, Object obj)
            {
                //如果没有此资源
                if (!_mDicAssetRef.ContainsKey(key))
                {
                    _mDicAssetRef.Add(key, new System.WeakReference(obj));
                }
                //如果弱引用资源已经销毁
                if (_mDicAssetRef.ContainsKey(key) && ((Object)_mDicAssetRef[key].Target == null) || !_mDicAssetRef[key].IsAlive)
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
            private static eLoadResPath _getRealLoadResPathType(string sAssetPath, eLoadResPath eloadResType, out string MD5)
            {
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
                assetsbundlepath = assetsbundlepath.Split('.')[0];

                if (eloadResType == eLoadResPath.RP_Resources)
                {
                    finalloadrespath = eloadResType;
                }
                eLoadResPathState eloadrespathstate = _getLoadResPathState();
                if (eloadrespathstate == eLoadResPathState.LS_ReadURLOnly)
                {
                    if (_mURLAssetBundleManifest != null 
                        && !assetsbundlepath.Contains(mBundlesInfoFileName) 
                        && _mURLAssetBundleManifest.IsContainsBundle(assetsbundlepath))
                    {
                        MD5 = _mURLAssetBundleManifest.getBundleMD5(assetsbundlepath);
                    }
                    else
                    {
                        MD5 = "";
                    }
                    finalloadrespath = eLoadResPath.RP_URL;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadStreamingOnly)
                {
                    if (_mLocalAssetBundleManifest != null 
                        && !assetsbundlepath.Contains(mBundlesInfoFileName) 
                        && _mLocalAssetBundleManifest.IsContainsBundle(assetsbundlepath))
                    {
                        MD5 = _mLocalAssetBundleManifest.getBundleMD5(assetsbundlepath);
                    }
                    else
                    {
                        MD5 = "";
                    }
                    finalloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    if (_mLocalAssetBundleManifest != null 
                        && !assetsbundlepath.Contains(mBundlesInfoFileName) 
                        && _mLocalAssetBundleManifest.IsContainsBundle(assetsbundlepath))
                    {
                        MD5 = _mLocalAssetBundleManifest.getBundleMD5(assetsbundlepath);
                    }
                    else
                    {
                        MD5 = "";
                    }
                                
                           
                    finalloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && (eloadResType == eLoadResPath.RP_URL || eloadResType == eLoadResPath.RP_Caching))
                {
                    if (_mLocalAssetBundleManifest == null || _mURLAssetBundleManifest == null)
                    {//说明还没有加载manifest,无须比较,按照设定的来加载
                   
                        MD5 = "";
                        finalloadrespath = eloadResType;
                    }
                    else
                    {

                        if (assetsbundlepath.Contains(mBundlesInfoFileName))
                        {
                            MD5 = "";
                            return eloadResType;
                        }
                        if (!_mLocalAssetBundleManifest.IsContainsBundle(assetsbundlepath) 
                            && !_mURLAssetBundleManifest.IsContainsBundle(assetsbundlepath))
                        {
                            MD5 = "";
                            finalloadrespath = eLoadResPath.RP_Unknow;

                        }
                        else
                        {

                            string md5local = _mLocalAssetBundleManifest.getBundleMD5(assetsbundlepath);
                            string md5url = _mURLAssetBundleManifest.getBundleMD5(assetsbundlepath);
                            if (md5local == md5url)
                            {//如果本地有,则从本地读取
                                MD5 = md5local;
                                finalloadrespath = eLoadResPath.RP_StreamingAssets;

                            }
                            else
                            {

                                MD5 = md5url;
                                finalloadrespath = eloadResType;

                            }
                        }
                        
                    }
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                    MD5 = "";
                    finalloadrespath = eLoadResPath.RP_Resources;
                }
                return finalloadrespath;
            }

            //获取资源的真实路径
            private static CPathAndHash _getRealPath(string respath, System.Type type, eLoadResPath eloadResType)
            {
                CPathAndHash pathhash = _mTempPathAndHash;
                if (respath == "")
                {
                    DLoger.LogError("input path is empty!!");
                    return pathhash;
                }
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
                        _mTempStringBuilderForGetRealPath.Remove(0, _mTempStringBuilderForGetRealPath.Length);
                        _mTempStringBuilderForGetRealPath.Append(respath);
                        int i = respath.LastIndexOf("/");
                        _mTempStringBuilderForGetRealPath[i] = '|';
                        _mTempStringBuilderForGetRealPath.Insert(i, msBundlePostfix);
                        temppath = _mTempStringBuilderForGetRealPath.ToString();
                    }
                    else
                    {
                        temppath = respath + msBundlePostfix;
                    }
                    pathhash.meLoadResType = _getRealLoadResPathType(temppath, eloadResType, out pathhash.mMD5);
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
            internal static void _removePathInResGroup(string sReseskey, string sReskey, string struepath, string sinputpath, bool bsuccessful)
            {
                CResesState rs;
                if (_mDicLoadingResesGroup.ContainsKey(sReseskey))
                {
                    rs = _mDicLoadingResesGroup[sReseskey];
                    if (bsuccessful == false)
                    {
                        Debug.LogError("加载=" + sinputpath + "=失败!");
                        rs.mbtotlesuccessful = false;
                    }
                    int index = rs.mlistpathskey.FindIndex(0, delegate (string s) { return s == sReskey; });
                    string truepath = "";
                    string inputpath = "";
                    if (index != -1)
                    {
                        truepath = rs.mlisttruepaths[index];
                        inputpath = rs.mlistinputpaths[index];
                        rs.mlisttruepaths.Remove(truepath);
                        //DLoger.Log("加载====" + path + "=====完毕!" + bsuccessful.ToString());
                        rs.mlistpathskey.Remove(sReskey);

                        rs.mlistinputpaths.Remove(inputpath);
                    }
                    else
                    {
                        truepath = struepath;
                        inputpath = sinputpath;
                    }

                    if (rs.mlistpathskey.Count == 0)
                    {
                        eLoadedNotify eloadnotify = bsuccessful == true ? eLoadedNotify.Load_OneSuccessfull : eLoadedNotify.Load_Failed;
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            Hashtable loadedinfo = new Hashtable();
                            loadedinfo.Add("path", truepath);
                            loadedinfo.Add("inputpath", inputpath);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            loadedinfo.Add("object", _getResObject(sReskey));
                            loadedinfo.Add("procobj", rs.listobj[i]);
                            if (rs.listproc[i] != null)
                            {
                                rs.listproc[i](loadedinfo, eloadnotify);
                            }
                            
                        }
                        eloadnotify = rs.mbtotlesuccessful == true ? eLoadedNotify.Load_Successfull : eLoadedNotify.Load_NotTotleSuccessfull;
                        _mDicLoadingResesGroup.Remove(sReseskey);
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            if (rs.listproc[i] != null)
                            {
                                rs.listproc[i](rs.listobj[i], eloadnotify);
                            }
                            
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
                            loadedinfo.Add("path", truepath);
                            loadedinfo.Add("inputpath", inputpath);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            loadedinfo.Add("object", _getResObject(sReskey));
                            loadedinfo.Add("procobj", rs.listobj[i]);
                            if (rs.listproc[i] != null)
                            {
                                rs.listproc[i](loadedinfo, eloadnotify);
                            }
                            
                        }
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
                _mTempStringBuilder.Remove(0, _mTempStringBuilder.Length);
                rs.maxpaths = spaths.Length;
                for (int i = 0; i < spaths.Length; i++)
                {
                    string pathkey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    string truepath = _getRealPath(spaths[i], types[i], eloadResTypes[i]).msRealPath;
                    rs.mlistpathskey.Add(pathkey);
                    rs.mlisttruepaths.Add(truepath);
                    rs.mlistinputpaths.Add(spaths[i]);
                    rs.mlistpathstag.Add(stags[i]);
                    _mTempStringBuilder.Append(truepath);
                }

                resstate = rs;
                skey = _mTempStringBuilder.ToString();
            }
            internal static string _getResAddressByPath(eLoadResPath eloadResType)
            {
                if (eloadResType == eLoadResPath.RP_Unknow)
                {
                    return "";
                }
                else if (eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    return mResourceStreamingAssets;
                }
                else
                {
                    return mResourcesURLAddress;
                }
            }
            internal static BundleInfoConfig _getAssetBundleManifest(eLoadResPath eloadResType)
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
            /// caching文件夹
            /// </summary>
            public static string msCachingPath = "HD";

            /// <summary>
            /// bundle后缀
            /// </summary>
            public static string msBundlePostfix = "";

            /// <summary>
            /// 是否开启自动释放
            /// </summary>
            public static bool mBAutoRelease = true;

            /// <summary>
            /// 是否从远程下载资源
            /// </summary>
            internal static bool _mbNotDownLoad = false;

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
            /// <summary>
            /// 不自动释放资源标志
            /// </summary>
            public const string msNoAutoRelease = "NoAutoRelease";
            /// <summary>
            /// 记录清楚bundle时间
            /// </summary>
            internal static float _mfLastReleaseBundleTime = 0;
            /// <summary>
            /// 释放bundle的间隔
            /// </summary>
            internal static float _mfMAXReleaseBundleTime = -1;

            /// <summary>
            /// 同时加载asset的最大数量
            /// </summary>
            public static int miMaxLoadAssetsNum = -1;

            /// <summary>
            /// 是否加载完asset等待(同步加载会卡进程,导致回调不能返回)
            /// </summary>
            public static bool mbLoadAssetWait = false;

            /// <summary>
            /// 
            /// </summary>
            public static System.Action mfuncDllLoadFailed = null;
            /// <summary>
            /// AssetBundleManifest对象
            /// </summary>
            internal static BundleInfoConfig _mURLAssetBundleManifest = null;
            internal static BundleInfoConfig _mLocalAssetBundleManifest = null;
            //internal static Dictionary<string,Hash128> _mDicURLBundlesHash = new Dictionary<string, Hash128>();
            //internal static Dictionary<string,Hash128> _mDicLocalBundlesHash = new Dictionary<string, Hash128>();

            

            /// <summary>
            /// 记录已经加载的bundle
            /// </summary>
            internal static Dictionary<string, AssetBundle> _mDicLoadedBundle = new Dictionary<string, AssetBundle>();
            /// <summary>
            /// 记录正在加载的bundle
            /// </summary>
            internal static List<string> _mListLoadingBundle = new List<string>();
            /// <summary>
            /// 不自动释放的请求的bundle路径
            /// </summary>
            internal static List<string> _mListNoAutoReleaseBundle = new List<string>();
            /// <summary>
            /// 记录每个bundle的下载进度
            /// </summary>
            public static Dictionary<string, ulong> mDicDownloadingBundleBytes = new Dictionary<string, ulong>();


            /// <summary>
            /// 返回是否清理无用资源结束
            /// </summary>
            internal static bool _mbUnLoadUnUsedResDone = true;
            /// <summary>
            /// 释放资源的协程返回
            /// </summary>
            internal static bool mbStartDoUnload = false;
            private static Dictionary<string, Hashtable> _mDicLoadedRes = new Dictionary<string, Hashtable>();
            private static List<string> _mListLoadingRes = new List<string>();
            private static Dictionary<string, CResesState> _mDicLoadingResesGroup = new Dictionary<string, CResesState>();
            /// <summary>
            /// 记录bundle的引用和计数
            /// </summary>
            //private static Dictionary<string, List<string>> _mDicAssetsDependBundles = new Dictionary<string, List<string>>();
            internal static Dictionary<string, int> _mDicBundlescounts = new Dictionary<string, int>();

            /// <summary>
            /// 记录资源引用的Dic,每隔一段时间,都会删除资源中引用计数为0的资源
            /// </summary>
            internal static Dictionary<int, System.WeakReference> _mDicAssetRef = new Dictionary<int, System.WeakReference>();
            /// <summary>
            /// 放置需要销毁的资源,每隔一段时间都会遍历该列表,销毁资源
            /// </summary>
            internal static List<Object> _mListReleasedObjects = new List<Object>();

            /// <summary>
            /// 放置需要销毁的Object,当非常住bundle释放完毕,最终释放这些Object
            /// </summary>
            internal static HashSet<string> _mSetRemovedObjects = new HashSet<string>();
            /// <summary>
            /// 记录每个预制件所依赖资源的路径
            /// </summary>
            internal static Dictionary<int, Dictionary<int, AssetsKey>> _mDicAssetsRefConfig = new Dictionary<int, Dictionary<int, AssetsKey>>();

            /// <summary>
            /// 记录本地每个预制件所依赖资源的路径
            /// </summary>
            internal static Dictionary<int, Dictionary<int, AssetsKey>> _mDicLocalAssetsRefConfig = new Dictionary<int, Dictionary<int, AssetsKey>>();

            //临时变量的缓存，避免new太频繁
            private static CPathAndHash _mTempPathAndHash = new CPathAndHash();
            private static StringBuilder _mTempStringBuilder = new StringBuilder();
            private static StringBuilder _mTempStringBuilderForGetRealPath = new StringBuilder();

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
            Load_Successfull,
            /// <summary>
            /// 加载全部完成
            /// </summary>
            Load_NotTotleSuccessfull
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
            /// 读取网络路径,没有就从读取本地StreamingAssets下的资源里读取
            /// </summary>
            RP_URL,
            /// <summary>
            /// 优先Caching,没有就从读取本地StreamingAssets下的资源里读取
            /// </summary>
            RP_Caching,
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
            static private string _smCachinginfofile = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/cachinginfo.txt";
            static public void initBundleInfo()
            {
                _smCachinginfofile = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/cachinginfo.txt";
                if (mbisInit == false)
                {
                    if (!Directory.Exists(Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath);
                    }
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
            static private void _deleteBundleInCaching(string bundlepath)
            {
                if (hasBundle(bundlepath))
                {
                    _mdicBundleInfo.Remove(bundlepath);
                    saveBundleInfo();
                    string scachingbundlepath = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/" + bundlepath;
                    if (File.Exists(scachingbundlepath))
                    {
                        File.Delete(scachingbundlepath);
                    }
                    
                }
            }
            static public void clearNoUsedBundle(List<string> listbundlepaths)
            {
                List<string> listkeys = new  List<string>(_mdicBundleInfo.Keys);
                for (int i = 0; i < listkeys.Count; i++)
                {
                    if (!listbundlepaths.Contains(listkeys[i]))
                    {//如果caching里面有,但是服务器上没有的bundle,则是无用的bundle,删除掉
                        _deleteBundleInCaching(listkeys[i]);
                    }
                }
            }
            static public void reset()
            {
                mbisInit = false;
                _mdicBundleInfo = new Dictionary<string, string>();
                _smCachinginfofile = Application.persistentDataPath + "/bundles/" + ResourceLoadManager.msCachingPath + "/cachinginfo.txt";
            }
        }
        public class BundleInfoConfig
        {
            internal class BundleInfo
            {
                public uint muSize;
                public string msMD5;
                public List<int> mListDepdenceBundleName = null;
            }
            private List<string> _mListDepdenceBundleName = new List<string>();
            private Dictionary<string, BundleInfo> _mDicBundleInfoConfig = new Dictionary<string, BundleInfo>();
            public void initBundleInfoConfig(string text)
            {
                StringReader sr = new StringReader(text);
                int num = int.Parse(sr.ReadLine());
                sr.ReadLine();
                for (int i = 0; i < num; i++)
                {
                    BundleInfo binfo = new BundleInfo();
                    string bundlepath = sr.ReadLine();
                    uint size = uint.Parse(sr.ReadLine());
                    string md5 = sr.ReadLine();
                    binfo.muSize = size;
                    binfo.msMD5 = md5;
                    int depnum = int.Parse(sr.ReadLine());
                    if (depnum != 0)
                    {
                        binfo.mListDepdenceBundleName = new List<int>();
                    }
                    for (int d = 0; d < depnum; d++)
                    {
                        string depbundlepath = sr.ReadLine();
                        int index = _mListDepdenceBundleName.FindIndex(o => { return o == depbundlepath; });
                        if (index == -1)
                        {
                            _mListDepdenceBundleName.Add(depbundlepath);
                            index = _mListDepdenceBundleName.Count - 1;
                        }
                        if (!binfo.mListDepdenceBundleName.Contains(index))
                        {
                            binfo.mListDepdenceBundleName.Add(index);
                        }
                    }
                    if (!_mDicBundleInfoConfig.ContainsKey(bundlepath))
                    {
                        _mDicBundleInfoConfig.Add(bundlepath, binfo);
                    }
                    sr.ReadLine();
                }
            }
            public string getBundleInfoConfig()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(_mDicBundleInfoConfig.Count.ToString());
                Dictionary<string, BundleInfo>.Enumerator it = _mDicBundleInfoConfig.GetEnumerator();
                while(it.MoveNext())
                {
                    sb.AppendLine("===============");
                    string bundlepath = it.Current.Key;
                    sb.AppendLine(bundlepath);
                    sb.AppendLine(it.Current.Value.muSize.ToString());
                    sb.AppendLine(it.Current.Value.msMD5);
                    if (it.Current.Value.mListDepdenceBundleName != null && it.Current.Value.mListDepdenceBundleName.Count != 0)
                    {
                        sb.AppendLine(it.Current.Value.mListDepdenceBundleName.Count.ToString());
                        for (int i = 0; i < it.Current.Value.mListDepdenceBundleName.Count; i++)
                        {
                            sb.AppendLine(_mListDepdenceBundleName[it.Current.Value.mListDepdenceBundleName[i]]);
                        }
                    }
                    else
                    {
                        sb.AppendLine("0");
                    }
                    
                }
                return sb.ToString();
            }
            public  string[] GetAllAssetBundles()
            {
                if (_mDicBundleInfoConfig.Count != 0)
                {
                    return new List<string>(_mDicBundleInfoConfig.Keys).ToArray();
                }
                return new string[0];
                
            }
            public string[] GetAllDependencies(string bundlepath)
            {
                if (_mDicBundleInfoConfig.ContainsKey(bundlepath))
                {
                    if (_mDicBundleInfoConfig[bundlepath].mListDepdenceBundleName != null)
                    {
                        List<string> deplist = new List<string>();
                        for (int i = 0; i < _mDicBundleInfoConfig[bundlepath].mListDepdenceBundleName.Count; i++)
                        {
                            deplist.Add(_mListDepdenceBundleName[_mDicBundleInfoConfig[bundlepath].mListDepdenceBundleName[i]]);
                        }
                        return deplist.ToArray();
                    }
                    
                }
                return new string[0];
                
            }
            public bool IsContainsBundle(string bundlepath)
            {
                return _mDicBundleInfoConfig.ContainsKey(bundlepath);
            }
            public string getBundleMD5(string bundlepath)
            {
                if (_mDicBundleInfoConfig.ContainsKey(bundlepath))
                {
                    return _mDicBundleInfoConfig[bundlepath].msMD5;
                }
                return "";
            }
            public uint getBundleSize(string bundlepath)
            {
                if (_mDicBundleInfoConfig.ContainsKey(bundlepath))
                {
                    return _mDicBundleInfoConfig[bundlepath].muSize;
                }
                return 0;
            }

            public void setBundleInfo(string bundlepath, uint size,string md5, List<string> listDepdenceBundleName)
            {
                BundleInfo binfo = null;
                if (_mDicBundleInfoConfig.ContainsKey(bundlepath))
                {
                    binfo = _mDicBundleInfoConfig[bundlepath];
                    
                }
                else
                {
                    _mDicBundleInfoConfig.Add(bundlepath, new BundleInfo());
                    binfo = _mDicBundleInfoConfig[bundlepath];
                }
                binfo.muSize = size;
                binfo.msMD5 = md5;
                if (listDepdenceBundleName.Count != 0)
                {
                    binfo.mListDepdenceBundleName = new List<int>();
                    for (int i = 0; i < listDepdenceBundleName.Count; i++)
                    {
                        string depbundlename = listDepdenceBundleName[i];
                        int index = _mListDepdenceBundleName.FindIndex(o => { return o == depbundlename; });
                        if (index == -1)
                        {
                            _mListDepdenceBundleName.Add(depbundlename);
                            index = _mListDepdenceBundleName.Count - 1;
                        }
                        if (!binfo.mListDepdenceBundleName.Contains(index))
                        {
                            binfo.mListDepdenceBundleName.Add(index);
                        }

                    }
                }
               
                
            }
        }
        internal class AssetsKey
        {
            public AssetsKey(int skey)
            {
                miKey = skey;
            }
            public int miKey = 0;
            public List<string> mlistMatTexPropName = null;
        }

       
        internal class CPathAndHash
        {
            public string msRealPath;
            public eLoadResPath meLoadResType;
            public string mMD5;
        }

        //资源组管理
        internal class CResesState
        {
            public List<string> mlistpathskey = new List<string>();
            public List<string> mlisttruepaths = new List<string>();
            public List<string> mlistinputpaths = new List<string>();
            public List<string> mlistpathstag = new List<string>();
            public List<ProcessDelegateArgc> listproc = new List<ProcessDelegateArgc>();
            public List<object> listobj = new List<object>();
            public int maxpaths = 0;
            public bool mbtotlesuccessful = true;
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

