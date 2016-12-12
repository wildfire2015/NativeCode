using UnityEngine;
using System.Collections.Generic;
namespace PSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class SingleMono : MonoBehaviour
    {
        private static GameObject m_Container = null;
        private static string m_Name = "SingleMono";
        private static Dictionary<string, Object> m_SingletonMap = new Dictionary<string, Object>();
        private static bool m_IsDestroying = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool IsDestroying
        {
            get { return m_IsDestroying; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static bool IsCreatedInstance(string Name)
        {
            if (m_Container == null)
            {
                return false;
            }
            if (m_SingletonMap != null && m_SingletonMap.ContainsKey(Name))
            {
                return true;
            }
            return false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Object getInstance<T>() where T: MonoBehaviour
        {
            if (m_Container == null)
            {
                m_Container = new GameObject();
                m_Container.name = m_Name;
                m_Container.AddComponent(typeof(SingleMono));
            }
            if (!m_SingletonMap.ContainsKey(typeof(T).Name))
            {
                T t = m_Container.AddComponent<T>();
                if (t != null)
                {
                    m_SingletonMap.Add(typeof(T).Name, t);
                }
                else
                {
                    DLoger.LogWarning("Singleton Type ERROR! (" + typeof(T).Name + ")");
                }
            }
            return m_SingletonMap[typeof(T).Name];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        public static void RemoveInstance(string Name)
        {
            if (m_Container != null && m_SingletonMap.ContainsKey(Name))
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)(m_SingletonMap[Name]));
                m_SingletonMap.Remove(Name);

                DLoger.LogWarning("Singleton REMOVE! (" + Name + ")");
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnApplicationQuit()
        {
            if (m_Container != null)
            {
                Destroy(m_Container);
                m_Container = null;
                m_IsDestroying = true;
            }
        }

    }
}


