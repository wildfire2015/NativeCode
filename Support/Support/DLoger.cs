using UnityEngine;
using System.Collections.Generic;
namespace PSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class DLoger
    {
        /// <summary>
        /// 
        /// </summary>
        static public bool EnableLog = false;

        /// <summary>
        /// 
        /// </summary>
        static public List<string> msOnlyTags = new List<string>();

        /// <summary>
        /// 输出到控制台
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stag"></param>
        static public void Log(object message,string stag = "")
        {
            Log(message, null, stag);
        }
        /// <summary>
        /// 输出到控制台
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="stag"></param>
        static public void Log(object message, Object context, string stag = "")
        {
            if (EnableLog)
            {
                if ((msOnlyTags.Count != 0 && msOnlyTags.Contains(stag)) || msOnlyTags.Count == 0)
                {
                    Debug.Log(message, context);
                }
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stag"></param>
        static public void LogError(object message, string stag = "")
        {
            LogError(message, null, stag);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="stag"></param>
        static public void LogError(object message, Object context, string stag = "")
        {
            if (EnableLog)
            {
                if ((msOnlyTags.Count != 0 && msOnlyTags.Contains(stag)) || msOnlyTags.Count == 0)
                {
                    Debug.LogError(message, context);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stag"></param>
        static public void LogWarning(object message, string stag = "")
        {
            LogWarning(message, null, stag);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="stag"></param>
        static public void LogWarning(object message, Object context, string stag = "")
        {
            if (EnableLog)
            {
                if ((msOnlyTags.Count != 0 && msOnlyTags.Contains(stag)) || msOnlyTags.Count == 0)
                {
                    Debug.LogWarning(message, context);
                }
            }
        }
    }
}
