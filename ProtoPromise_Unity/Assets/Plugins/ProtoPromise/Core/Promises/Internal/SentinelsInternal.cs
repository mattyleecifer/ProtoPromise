﻿#if UNITY_5_5 || NET_2_0 || NET_2_0_SUBSET
#define NET_LEGACY
#endif

#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#pragma warning disable IDE0090 // Use 'new(...)'

using System.Diagnostics;
using System.Threading;

namespace Proto.Promises
{
    partial class Internal
    {
        internal abstract partial class PromiseRefBase : HandleablePromiseBase, ITraceable
        {
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class PromiseCompletionSentinel : HandleablePromiseBase
            {
                // A singleton instance used to mark the promise as completed.
                internal static readonly PromiseCompletionSentinel s_instance = new PromiseCompletionSentinel();

                private PromiseCompletionSentinel() { }

                internal override void Handle(PromiseRefBase handler, object rejectContainer, Promise.State state)
                {
                    throw new System.InvalidOperationException("PromiseCompletionSentinel handled from " + handler);
                }
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class PromiseForgetSentinel : HandleablePromiseBase
            {
                // A singleton instance used to cap off the promise and prevent further awaits.
                internal static readonly PromiseForgetSentinel s_instance = new PromiseForgetSentinel();

                private PromiseForgetSentinel() { }

                internal override void Handle(PromiseRefBase handler, object rejectContainer, Promise.State state)
                {
                    handler.SetCompletionState(rejectContainer, state);
                    handler.MaybeReportUnhandledAndDispose(rejectContainer, state);
                }
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class InvalidAwaitSentinel : PromiseRefBase
            {
                // A singleton instance used to indicate that an await was invalid (after the PromiseMultiAwait was forgotten or PromiseSingleAwait awaited).
                internal static readonly InvalidAwaitSentinel s_instance = new InvalidAwaitSentinel();

                private InvalidAwaitSentinel()
                {
                    _next = this; // Set _next to this so that CompareExchangeWaiter will always fail. This is also used in the object pool so that the _next field will never be null.
                    _promiseId = -5; // Set an id that is unlikely to match (though this should never be used in a Promise struct).
                    // If we don't suppress, the finalizer can run when the AppDomain is unloaded, causing a NullReferenceException. This happens in Unity when switching between editmode and playmode.
                    System.GC.SuppressFinalize(this);
                }

                internal override void Handle(PromiseRefBase handler, object rejectContainer, Promise.State state)
                {
                    throw new System.InvalidOperationException("InvalidAwaitSentinel handled from " + handler);
                }

                internal override void MaybeDispose() { throw new System.InvalidOperationException(); }
                internal override void MaybeReportUnhandledAndDispose(object rejectContainer, Promise.State state) { throw new System.InvalidOperationException(); }
                protected override void OnForget(short promiseId) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase AddWaiter(short promiseId, HandleablePromiseBase waiter, out HandleablePromiseBase previousWaiter) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase GetDuplicate(short promiseId, ushort depth) { throw new System.InvalidOperationException(); }
                internal override bool GetIsCompleted(short promiseId) { throw new System.InvalidOperationException(); }
                internal override bool GetIsValid(short promiseId) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase GetPreserved(short promiseId, ushort depth) { throw new System.InvalidOperationException(); }
                internal override void MaybeMarkAwaitedAndDispose(short promiseId) { throw new System.InvalidOperationException(); }
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class PendingAwaitSentinel : PromiseRefBase
            {
                // A singleton instance indicating that a promise has not yet been awaited. This is used so that we never have to check for null when handling the next promise.
                internal static readonly PendingAwaitSentinel s_instance = new PendingAwaitSentinel();

                private PendingAwaitSentinel()
                {
                    _promiseId = -5; // Set an id that is unlikely to match (though this should never be used in a Promise struct).
                    // If we don't suppress, the finalizer can run when the AppDomain is unloaded, causing a NullReferenceException. This happens in Unity when switching between editmode and playmode.
                    System.GC.SuppressFinalize(this);
                }

                internal override void Handle(PromiseRefBase handler, object rejectContainer, Promise.State state)
                {
                    // This will only be called if the handler was completed before it was awaited (rare).
                    var waiter = handler.ReadNextWaiterAndMaybeSetCompleted();
                    // If waiter is not this, it means the handler was awaited before it was set complete on another thread, so we need to handle it here.
                    if (waiter != this)
                    {
#if PROMISE_PROGRESS
                        // Exchange waiter to read latest and set to InvalidAwaitSentinel to solve race condition with progress.
                        waiter = handler.ExchangeWaiter(InvalidAwaitSentinel.s_instance);
#endif
                        waiter.Handle(handler, rejectContainer, state);
                    }
                    else
                    {
                        handler.SetCompletionState(rejectContainer, state);
                    }
                }

                internal override void MaybeDispose() { throw new System.InvalidOperationException(); }
                internal override void MaybeReportUnhandledAndDispose(object rejectContainer, Promise.State state) { throw new System.InvalidOperationException(); }
                protected override void OnForget(short promiseId) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase AddWaiter(short promiseId, HandleablePromiseBase waiter, out HandleablePromiseBase previousWaiter) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase GetDuplicate(short promiseId, ushort depth) { throw new System.InvalidOperationException(); }
                internal override bool GetIsCompleted(short promiseId) { throw new System.InvalidOperationException(); }
                internal override bool GetIsValid(short promiseId) { throw new System.InvalidOperationException(); }
                internal override PromiseRefBase GetPreserved(short promiseId, ushort depth) { throw new System.InvalidOperationException(); }
                internal override void MaybeMarkAwaitedAndDispose(short promiseId) { throw new System.InvalidOperationException(); }
            }
        } // PromiseRefBase

        internal class BackgroundSynchronizationContextSentinel : SynchronizationContext
        {
            internal static readonly BackgroundSynchronizationContextSentinel s_instance = new BackgroundSynchronizationContextSentinel();

            public override void Post(SendOrPostCallback d, object state) { throw new System.InvalidOperationException(); }
            public override void Send(SendOrPostCallback d, object state) { throw new System.InvalidOperationException(); }
            public override SynchronizationContext CreateCopy() { throw new System.InvalidOperationException(); }
        }
    } // Internal
}