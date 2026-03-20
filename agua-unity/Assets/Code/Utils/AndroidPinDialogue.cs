using System;
using UnityEngine;

namespace CleverEdge
{

    public class AndroidPinDialog : MonoBehaviour
    {
        public static AndroidPinDialog Instance { get; private set; }

        private Action<string> _onOk;
        private Action _onCancel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            gameObject.name = "AndroidPinDialog";
        }

        public void ShowDefaultPinDialogue(Action<string> onOk, Action onCancel = null)
        {
            ShowPinDialog("Enter PIN", "Please enter your PIN to proceed.", onOk, onCancel);
        }
        
        private void ShowPinDialog(string title, string message, Action<string> onOk, Action onCancel = null)
        {
            _onOk = onOk;
            _onCancel = onCancel;

    #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);
                    builder.Call<AndroidJavaObject>("setTitle", title);
                    builder.Call<AndroidJavaObject>("setMessage", message);

                    using var editText = new AndroidJavaObject("android.widget.EditText", activity);

                    // TYPE_CLASS_NUMBER | TYPE_NUMBER_VARIATION_PASSWORD
                    int inputType = 2 | 16;
                    editText.Call("setInputType", inputType);

                    // Optional: hint
                    editText.Call("setHint", "Enter PIN");

                    builder.Call<AndroidJavaObject>("setView", editText);

                    builder.Call<AndroidJavaObject>(
                        "setPositiveButton",
                        "OK",
                        new DialogOnClickListener(() =>
                        {
                            using var editable = editText.Call<AndroidJavaObject>("getText");
                            string value = editable.Call<string>("toString");
                            UnityMainThreadCallback("OnPinDialogOk", value);
                        })
                    );

                    builder.Call<AndroidJavaObject>(
                        "setNegativeButton",
                        "X",
                        new DialogOnClickListener(() =>
                        {
                            UnityMainThreadCallback("OnPinDialogCancel", "");
                        })
                    );

                    using var dialog = builder.Call<AndroidJavaObject>("create");
                    dialog.Call("show");
                }));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to show Android PIN dialog: " + e);
                _onCancel?.Invoke();
            }
    #else
            
            var ok = UnityEditor.EditorUtility.DisplayDialog(title, message, "Correct", "Incorrect");
            if (ok)
                _onOk?.Invoke(Constants.PIN);
            else            
                _onOk?.Invoke("1234");
            
    #endif
        }

        private void UnityMainThreadCallback(string methodName, string value)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                UnityPlayerSendMessage(methodName, value);
            }));
        }

        private void UnityPlayerSendMessage(string methodName, string value)
        {
            // Since we're already back in Unity flow, just call directly.
            switch (methodName)
            {
                case "OnPinDialogOk":
                    _onOk?.Invoke(value);
                    break;

                case "OnPinDialogCancel":
                    _onCancel?.Invoke();
                    break;
            }

            _onOk = null;
            _onCancel = null;
        }

        private class DialogOnClickListener : AndroidJavaProxy
        {
            private readonly Action _onClick;

            public DialogOnClickListener(Action onClick)
                : base("android.content.DialogInterface$OnClickListener")
            {
                _onClick = onClick;
            }

            public void onClick(AndroidJavaObject dialog, int which)
            {
                _onClick?.Invoke();
            }
        }
    }
}