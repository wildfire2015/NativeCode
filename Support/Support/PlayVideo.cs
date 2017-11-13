using System;
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

        public enum eVideoFromType
        {
            eVFT_Local,
            eVFT_URL
        };


        private VideoPlayer _mPlayVideo = null;
        private RenderTexture _mRenderTexture = null;
        private RawImage _mRawImage;
        private Camera _mCamera;
        private Text _mTxtChoseTxtObj;
        private Text _mTxtURLInfoObj;
        private Button _mBtn;
        private GameObject VideoUIRoot;


        private UnityAction _mOnFinishVideo = null;

        private string _msCgpath = "";
        private string _msCgURL = "";
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
        /// <param name="evft"></param>
        public void playvideo(string spath,int width,int height,string tips,string urldownloadingtips, float roate,UnityAction action,eVideoFromType evft = eVideoFromType.eVFT_Local)
        {
            List<string> loadpath = new List<string>();
            
            if (evft == eVideoFromType.eVFT_Local)
            {
                _msCgpath = spath;
                loadpath.Add(_msCgpath);
            }
            else
            {
                _msCgURL = spath;
            }
            loadpath.Add(_msVideoObjectPath);
            string[] paths = loadpath.ToArray();
            _mOnFinishVideo = action;
            ResourceLoadManager.requestRes(paths, eLoadResPath.RP_Resources, (o, r) =>
             {
                 if (r == eLoadedNotify.Load_Successfull)
                 {
                     
                     //播放组件初始化
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
                     _mPlayVideo.targetTexture = _mRenderTexture;


                     _mTxtURLInfoObj = go.transform.Find("UI/RawImage/TexWaitURL").GetComponent<Text>();

                     //这里设置clip和url,不然声音播不出来,不能在上面设置之前
                     if (evft == eVideoFromType.eVFT_Local)
                     {
                         _mPlayVideo.source = VideoSource.VideoClip;
                         VideoClip vclip = ResourceLoadManager.getRes(_msCgpath, eLoadResPath.RP_Resources) as VideoClip;
                         _mPlayVideo.clip = vclip;
                         _mTxtURLInfoObj.gameObject.SetActive(false);
                     }
                     else
                     {
                         _mPlayVideo.source = VideoSource.Url;
                         _mPlayVideo.url = _msCgURL;
                         _mPlayVideo.controlledAudioTrackCount = 1;
                         _mPlayVideo.EnableAudioTrack(0, true);
                         _mTxtURLInfoObj.gameObject.SetActive(true);
                         _mTxtURLInfoObj.text = urldownloadingtips;
                         _mPlayVideo.targetCameraAlpha = 0;
                         _mPlayVideo.prepareCompleted += _OnURLVideoReady;
                         _mPlayVideo.errorReceived += _OnURLVideoError;

                     }
                     _mPlayVideo.SetTargetAudioSource(0, audiosource);

                     

                     //_mPlayVideo.audioOutputMode = VideoAudioOutputMode.AudioSource;






                     VideoUIRoot = go.transform.Find("UI").gameObject;


                     _mCamera = go.transform.Find("Camera").GetComponent<Camera>();

                     _adapteScreenResolution(_mCamera);

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
                         _mTxtChoseTxtObj = TxtHorizental;
                     }
                     else
                     {
                         _mTxtChoseTxtObj = TxtVertical;
                     }

                     _mBtn = go.transform.Find("UI/CatchClickButton").GetComponent<Button>();
                     _mBtn.onClick.AddListener(OnClick);
                     //((RectTransform)(_mBtn.transform)).sizeDelta = new Vector2(width, height);
                     //_mBtn.transform.localEulerAngles = new Vector3(0, 0, roate);
                     //_mPlayVideo.playOnAwake = true;
                     _mPlayVideo.Play();
                 }
                 else if (r == eLoadedNotify.Load_NotTotleSuccessfull)
                 {
                     DLoger.LogError(spath + "= load failed!");
                     _Finish();
                 }
             });
            
        }

        private void _OnURLVideoError(VideoPlayer source, string message)
        {
            _mTxtURLInfoObj.text = message;
        }

        private void _OnURLVideoReady(VideoPlayer source)
        {
            _mTxtURLInfoObj.gameObject.SetActive(false);
            _mPlayVideo.targetCameraAlpha = 1;
        }

        private void _adapteScreenResolution(Camera camera)
        {
            //IphoneX 安全区域比例达到1.95大于1.77可以不用特殊处理
            float fixRate = 16f / 9f;
            float currentRate = (float)Screen.height / (float)Screen.width;
            if (Mathf.Abs(currentRate - fixRate) > 0.01f)//显示比例差小于0.01认为相同
            {
                Rect rect = camera.rect;
                if (fixRate > currentRate)
                {
                    float w = Screen.height / fixRate;
                    float off = (Screen.width - w) * 0.5f;
                    rect.x = off / Screen.width;
                    rect.width = 1 - (off / Screen.width * 2);
                }
                else
                {
                    float h = Screen.width * fixRate;
                    float off = (Screen.height - h) * 0.5f;
                    rect.y = off / Screen.height;
                    rect.height = 1 - (off / Screen.height * 2);
                }
                camera.rect = rect;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnClick()
        {
            if (_mbClickOnce == true)
            {
                VideoUIRoot.SetActive(false);
                if (_mPlayVideo.isPlaying)
                {
                    _mPlayVideo.frame = (long)_mPlayVideo.frameCount;
                }
                else
                {
                    _Finish();
                }
                
            }
            else
            {
                _mTxtChoseTxtObj.gameObject.SetActive(true);
                StartCoroutine(waitOnceClick());
                
            }
            
        }
        private IEnumerator waitOnceClick()
        {
            _mbClickOnce = true;
            yield return new WaitForSeconds(2.0f);
            _mTxtChoseTxtObj.gameObject.SetActive(false);
            _mbClickOnce = false;
        }
        private void _OnVideoFinished(VideoPlayer videoplayer)
        {
            _Finish();
        }
        private void _Finish()
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
                _mPlayVideo.prepareCompleted -= _OnURLVideoReady;
                _mPlayVideo.errorReceived -= _OnURLVideoError;
                Destroy(_mPlayVideo.gameObject);

            }
            if (_mRenderTexture)
            {
                Destroy(_mRenderTexture);
            }
            _mBtn.onClick.RemoveAllListeners();
            if (_msCgpath != "")
            {
                ResourceLoadManager.removeRes(_msCgpath, eLoadResPath.RP_Resources);
            }
            ResourceLoadManager.removeRes(_msVideoObjectPath, eLoadResPath.RP_Resources);
            StopCoroutine(waitOnceClick());
        }
    }
}
