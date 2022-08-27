using UnityEngine;

namespace DesignMode.Singleton
{
    public class Singleton<T>: MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType (typeof(T)) as T;
                    if (_instance == null) {
                        GameObject obj = new GameObject ();
                        obj.hideFlags = HideFlags.HideAndDontSave;//隐藏实例化的new game object，下同
                        _instance = obj.AddComponent (typeof(T)) as T;
                    }
                }
                return _instance;
            }
        }
    }
}