﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete

using System;

namespace Proto.Promises
{
    partial class Promise
    {
        /// <summary>
        /// Deferred base. An instance of this can be used to handle the state of the attached <see cref="Promise"/>, except resolve. You must use <see cref="Deferred"/> or <see cref="Promise{T}.Deferred"/> to handle resolve.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public abstract class DeferredBase : ICancelableAny, IRetainable
        {
            /// <summary>
            /// The <see cref="Promise"/> that this controls.
            /// </summary>
            public Promise Promise { get; protected set; }

            public State State { get { return Promise._state; } }

            internal DeferredBase() { }

            ~DeferredBase()
            {
                if (State == State.Pending)
                {
                    // Deferred wasn't handled.
                    AddRejectionToUnhandledStack(UnhandledDeferredException.instance, Promise);
                }
            }

            /// <summary>
            /// Retain this instance and the linked <see cref="Promise"/>.
            /// <para/>This should always be paired with a call to <see cref="Release"/>
            /// </summary>
            public void Retain()
            {
                Promise.Retain();
            }

            /// <summary>
            /// Release this instance and the linked <see cref="Promise"/>.
            /// <para/>This should always be paired with a call to <see cref="Retain"/>
            /// </summary>
            public void Release()
            {
                Promise.Release();
            }

            /// <summary>
            /// Reject the linked <see cref="Promise"/> with <paramref name="reason"/>.
            /// </summary>
            public void Reject<TReject>(TReject reason)
            {
                var promise = Promise;
                ValidateOperation(promise, 1);

                if (State == State.Pending)
                {
                    promise.RejectDirect(ref reason, 1);
                }
                else
                {
                    AddRejectionToUnhandledStack(reason, null);
                    Logger.LogWarning("Deferred.Reject - Deferred is not in the pending state.");
                }
            }


            /// <summary>
            /// Report progress between 0 and 1.
            /// </summary>
#if !PROMISE_PROGRESS
            [Obsolete("Progress is disabled. Remove PROTO_PROMISE_PROGRESS_DISABLE from your compiler symbols to enable progress reports.", true)]
#endif
            public void ReportProgress(float progress)
            {
                var promise = Promise;
                ValidateProgress(1);
                ValidateOperation(promise, 1);
                ValidateProgress(progress, 1);

                if (State != State.Pending)
                {
                    Logger.LogWarning("Deferred.ReportProgress - Deferred is not in the pending state.");
                    return;
                }

                promise.ReportProgress(progress);
            }

            /// <summary>
            /// Cancel the linked <see cref="Promise"/> without a reason.
            /// </summary>
            public void Cancel()
            {
                var promise = Promise;
                ValidateOperation(promise, 1);

                if (State == State.Pending)
                {
                    promise.CancelDirect();
                }
                else
                {
                    Logger.LogWarning("Deferred.Cancel - Deferred is not in the pending state.");
                }
            }

            /// <summary>
            /// Cancel the linked <see cref="Promise"/> with <paramref name="reason"/>.
            /// </summary>
            public void Cancel<TCancel>(TCancel reason)
            {
                var promise = Promise;
                ValidateOperation(promise, 1);

                if (State == State.Pending)
                {
                    promise.CancelDirect(ref reason);
                }
                else
                {
                    Logger.LogWarning("Deferred.Cancel - Deferred is not in the pending state.");
                }
            }
        }

        /// <summary>
        /// An instance of this is used to handle the state of the <see cref="DeferredBase.Promise"/>.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public abstract class Deferred : DeferredBase
        {
            internal Deferred() { }

            /// <summary>
            /// Resolve the linked <see cref="Promise"/>.
            /// </summary>
            public void Resolve()
            {
                var promise = Promise;
                ValidateOperation(promise, 1);

                if (State == State.Pending)
                {
                    promise.ResolveDirect();
                }
                else
                {
                    Logger.LogWarning("Deferred.Resolve - Deferred is not in the pending state.");
                    return;
                }
            }
        }
    }

    public partial class Promise<T>
    {
        /// <summary>
        /// An instance of this is used to handle the state of the <see cref="Promise"/>.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public abstract new class Deferred : DeferredBase
        {
            /// <summary>
            /// The <see cref="Promise{T}"/> that this controls.
            /// </summary>
            public new Promise<T> Promise { get { return (Promise<T>) base.Promise; } protected set { base.Promise = value; } }

            internal Deferred() { }

            /// <summary>
            /// Resolve the linked <see cref="Promise{T}"/> with <paramref name="value"/>.
            /// </summary>
            public void Resolve(T value)
            {
                var promise = Promise;
                ValidateOperation(promise, 1);

                if (State == State.Pending)
                {
                    promise.ResolveDirect(ref value);
                }
                else
                {
                    Logger.LogWarning("Deferred.Resolve - Deferred is not in the pending state.");
                    return;
                }
            }
        }
    }

    partial class Promise
    {
        partial class Internal
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public sealed class DeferredInternal0 : Deferred
            {
                public DeferredInternal0(Promise target)
                {
                    Promise = target;
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            public sealed class DeferredInternal<T> : Promise<T>.Deferred
            {
                public DeferredInternal(Promise<T> target)
                {
                    Promise = target;
                }
            }
        }
    }
}