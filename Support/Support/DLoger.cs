using UnityEngine;
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
        /// 输出到控制台
        /// </summary>
        /// <param name="message"></param>
        static public void Log(object message)
        {
            Log(message, null);
        }
        /// <summary>
        /// 输出到控制台
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        static public void Log(object message, Object context)
        {
            if (EnableLog)
            {
                Debug.Log(message, context);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        static public void LogError(object message)
        {
            LogError(message, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        static public void LogError(object message, Object context)
        {
            if (EnableLog)
            {
                Debug.LogError(message, context);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        static public void LogWarning(object message)
        {
            LogWarning(message, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        static public void LogWarning(object message, Object context)
        {
            if (EnableLog)
            {
                Debug.LogWarning(message, context);
            }
        }
    }
}
