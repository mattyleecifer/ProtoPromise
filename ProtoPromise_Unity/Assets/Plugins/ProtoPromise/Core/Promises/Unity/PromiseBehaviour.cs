﻿using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Proto.Promises
{
    partial struct Promise<T>
    {
        // Promise is backed by Promise<Internal.VoidResult>, so we don't need a static constructor for it.
        static Promise()
        {
            // Set default rejection and warning handlers to route to UnityEngine.Debug.
            Promise.Config.UncaughtRejectionHandler = Debug.LogException;
            Promise.Config.WarningHandler = Debug.LogWarning;
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
            {
                // Create a PromiseBehaviour instance before any promise actions are made.
                new GameObject("Proto.Promises.Unity.PromiseBehaviour").AddComponent<Unity.PromiseBehaviour>();
            }
        }
    }

    namespace Unity // I would have nested this within Promise, but you can only change the execution order of un-nested behaviours, so add a nested namespace instead.
    {
#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        public sealed class PromiseBehaviour : MonoBehaviour
        {
            private static PromiseBehaviour _instance;

            private void Start()
            {
                if (_instance != null)
                {
                    Promise.Manager.LogWarning("There can only be one instance of PromiseBehaviour. Destroying new instance.");
                    Destroy(this);
                    return;
                }
                DontDestroyOnLoad(gameObject);
                gameObject.hideFlags = HideFlags.HideAndDontSave; // Don't show in hierarchy and don't destroy.
                _instance = this;
                StartCoroutine(UpdateRoutine());
            }

            private void OnDestroy()
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                {
                    if (_instance == this)
                    {
                        Promise.Manager.LogWarning("PromiseBehaviour destroyed! Promise callbacks will no longer be automatically invoked!");
                        _instance = null;
                    }
                }
            }

            private IEnumerator UpdateRoutine()
            {
                while (true)
                {
                    yield return null;
                    // Invoke progress delegates during the normal coroutine cycle.
                    Promise.Manager.HandleCompletesAndProgress();
                }
            }

            private void Update()
            {
                Promise.Manager.HandleCompletes();
            }

            // Optionally add extra HandleCompletes calls for LateUpdate, FixedUpdate, WaitForEndOfFrame, etc.
        }
    }
}