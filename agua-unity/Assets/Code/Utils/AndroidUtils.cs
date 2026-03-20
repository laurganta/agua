using UnityEditor;
using UnityEngine;

namespace CleverEdge
{
    public class AndroidDialog
    {
        public static void Show(string title, string message, string yesLabel, string noLabel, System.Action onYes, System.Action onNo)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity))
                {
                    builder.Call<AndroidJavaObject>("setTitle", title);
                    builder.Call<AndroidJavaObject>("setMessage", message);

                    // YES button
                    builder.Call<AndroidJavaObject>(
                        "setPositiveButton",
                        yesLabel,
                        new DialogCallback(() => onYes?.Invoke())
                    );

                    // NO button
                    builder.Call<AndroidJavaObject>(
                        "setNegativeButton",
                        noLabel,
                        new DialogCallback(() => onNo?.Invoke())
                    );

                    var dialog = builder.Call<AndroidJavaObject>("create");
                    dialog.Call("show");
                }
            }));
        }
#else
            #if UNITY_EDITOR
            if (EditorUtility.DisplayDialog(title, message, yesLabel, noLabel))
            {
                onYes?.Invoke();
            }
            else
            {
                onNo?.Invoke();
            }
            #endif
#endif
        }

        class DialogCallback : AndroidJavaProxy
        {
            private System.Action callback;

            public DialogCallback(System.Action callback)
                : base("android.content.DialogInterface$OnClickListener")
            {
                this.callback = callback;
            }

            public void onClick(AndroidJavaObject dialog, int which)
            {
                callback?.Invoke();
            }
        }
    }
    
    public class AndroidUtils
    {
        public static void Show(string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (var toastClass = new AndroidJavaClass("android.widget.Toast"))
                {
                    var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                    var toast = toastClass.CallStatic<AndroidJavaObject>(
                        "makeText",
                        context,
                        message,
                        toastClass.GetStatic<int>("LENGTH_SHORT")
                    );

                    toast.Call("show");
                }
            }));
        }
#else
        GameDebug.Log($"Showing Android toast: {message}");            
#endif
        }
        
    }
}

