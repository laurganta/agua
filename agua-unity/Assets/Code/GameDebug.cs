namespace CleverEdge
{
    public static class GameDebug
    {
        public static void Log(string message)
        {
#if DEVELOMENT_BUILD || UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#endif
        }
        
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}