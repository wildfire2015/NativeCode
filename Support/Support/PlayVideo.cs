﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using PSupport;
using PSupport.LoadSystem;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace PSupport
{
    /// <summary>
    /// 播放视频类.unity5.6+
    /// </summary>
    public class PlayVideoClass :MonoBehaviour
    {
        private VideoPlayer _mPlayVideo = null;
        private RenderTexture _mRenderTexture = null;
        private RawImage _mRawImage;
        private Text _mChoseTxtObj;
        private Button _mBtn;
        private GameObject VideoUIRoot;


        private UnityAction _mOnFinishVideo = null;

        private string _msCgpath = "";
        private string _msVideoObjectPath = "artlocal/video/cg/VideoPlayerObject";

        private bool _mbClickOnce = false;

        

        /// <summary>
        /// 单例获取
        /// </summary>
        /// <returns></returns>
        static public PlayVideoClass getInstance()
        {
            return SingleMono.getInstance<PlayVideoClass>() as PlayVideoClass;
        }
        /// <summary>
        /// 清理视频播放资源
        /// </summary>
        static public void clear()
        {
            SingleMono.RemoveInstance(typeof(PlayVideoClass).Name);
        }

        /// <summary>
        /// 播放CG视频
        /// </summary>
        /// <param name="spath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tips"></param>
        /// <param name="roate"></param>
        /// <param name="action"></param>
        public void playvideo(string spath,int width,int height,string tips, float roate,UnityAction action)
        {
            _msCgpath = spath;
            string[] paths = { _msCgpath, _msVideoObjectPath };
            _mOnFinishVideo = action;
            ResourceLoadManager.requestRes(paths, eLoadResPath.RP_Resources, (o, r) =>
             {
                 if (r == eLoadedNotify.Load_Successfull)
                 {
                     VideoClip vclip = ResourceLoadManager.getRes(_msCgpath, eLoadResPath.RP_Resources) as VideoClip;
                     if (vclip)
                     {
                        

                         GameObject go = Instantiate(ResourceLoadManager.getRes(_msVideoObjectPath, eLoadResPath.RP_Resources) as GameObject);
                         _mPlayVideo = go.GetComponentInChildren<VideoPlayer>();
                         AudioSource audiosource = go.GetComponentInChildren<AudioSource>();

                         AudioListener[] audiolisteners = FindObjectsOfType<AudioListener>();
                         if (audiolisteners.Length == 0)
                         {
                             go.AddComponent<AudioListener>();
                         }

                         _mPlayVideo.playOnAwake = false;
                         _mPlayVideo.renderMode = VideoRenderMode.RenderTexture;
                         _mRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                         _mPlayVideo.isLooping = false;
                         _mPlayVideo.waitForFirstFrame = true;
                         _mPlayVideo.loopPointReached += _OnVideoFinished;
                         _mPlayVideo.clip = vclip;
                         _mPlayVideo.targetTexture = _mRenderTexture;
                         _mPlayVideo.SetTargetAudioSource(0, audiosource);

                         VideoUIRoot = go.transform.Find("UI").gameObject;


                         _mRawImage = go.transform.Find("UI/RawImage").GetComponent<RawImage>();
                         _mRawImage.texture = _mRenderTexture;
                         _mRawImage.rectTransform.sizeDelta = new Vector2(width, height);
                         _mRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, roate);

                         Text TxtVertical = go.transform.Find("UI/TexVertical").GetComponent<Text>();
                         Text TxtHorizental = go.transform.Find("UI/TexHorizontal").GetComponent<Text>();

                         TxtHorizental.text = tips;
                         TxtVertical.text = tips;

                         TxtHorizental.gameObject.SetActive(false);
                         TxtVertical.gameObject.SetActive(false);

                         if (width > height)
                         {
                             _mChoseTxtObj = TxtHorizental;
                         }
                         else
                         {
                             _mChoseTxtObj = TxtVertical;
                         }

                         _mBtn = go.transform.Find("UI/CatchClickButton").GetComponent<Button>();
                         _mBtn.onClick.AddListener(OnClick);
                         ((RectTransform)(_mBtn.transform)).sizeDelta = new Vector2(width, height);
                         _mBtn.transform.localEulerAngles = new Vector3(0, 0, roate);

                         _mPlayVideo.Play();

                     }
                 }
                 else if (r == eLoadedNotify.Load_NotTotleSuccessfull)
                 {
                     DLoger.LogError(spath + "= load failed!");
                     PlayVideoClass.clear();
                     _mOnFinishVideo();
                 }
             });
            
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnClick()
        {
            if (_mbClickOnce == true)
            {
                VideoUIRoot.SetActive(false);
                _mPlayVideo.frame = (long)_mPlayVideo.frameCount;
            }
            else
            {
                _mChoseTxtObj.gameObject.SetActive(true);
                StartCoroutine(waitOnceClick());
            }
            
        }
        private IEnumerator waitOnceClick()
        {
            _mbClickOnce = true;
            yield return new WaitForSeconds(2.0f);
            _mChoseTxtObj.gameObject.SetActive(false);
            _mbClickOnce = false;
        }
        private void _OnVideoFinished(VideoPlayer videoplayer)
        {
            PlayVideoClass.clear();
            _mOnFinishVideo();
        }

        //private void Update()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        _mPlayVideo.frame = (long)_mPlayVideo.frameCount;
        //    }
        //}
        /// <summary>
        /// 销毁函数
        /// </summary>
        private void OnDestroy()
        {
            if (_mPlayVideo != null)
            {
                _mPlayVideo.clip = null;
                _mPlayVideo.targetTexture = null;
                _mPlayVideo.loopPointReached -= _OnVideoFinished;
                Destroy(_mPlayVideo.gameObject);

            }
            if (_mRenderTexture)
            {
                Destroy(_mRenderTexture);
            }
            _mBtn.onClick.RemoveAllListeners();

            ResourceLoadManager.removeRes(_msCgpath, eLoadResPath.RP_Resources);
            ResourceLoadManager.removeRes(_msVideoObjectPath, eLoadResPath.RP_Resources);

        }
    }
}
