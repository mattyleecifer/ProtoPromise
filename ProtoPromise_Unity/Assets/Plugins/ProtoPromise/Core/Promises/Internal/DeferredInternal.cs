﻿#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable 0420 // A reference to a volatile field will not be treated as volatile

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Proto.Promises
{
    partial class Internal
    {
        internal interface IDeferredPromise : ITraceable
        {
            int DeferredId { get; }
            bool TryIncrementDeferredIdAndUnregisterCancelation(int deferredId);
            void RejectDirect(IRejectContainer reasonContainer);
            void CancelDirect();
#if PROMISE_PROGRESS
            bool TryReportProgress(int deferredId, float progress);
#endif
        }

        internal static class DeferredPromiseHelper
        {
            internal static bool GetIsValidAndPending(IDeferredPromise _this, int deferredId)
            {
                return _this != null && _this.DeferredId == deferredId;
            }

            internal static bool TryIncrementDeferredIdAndUnregisterCancelation(IDeferredPromise _this, int deferredId)
            {
                return _this != null && _this.TryIncrementDeferredIdAndUnregisterCancelation(deferredId);
            }

            internal static bool TryReportProgress(IDeferredPromise _this, int deferredId, float progress)
            {
                ValidateProgressValue(progress, "progress", 1);
#if !PROMISE_PROGRESS
                return GetIsValidAndPending(_this, deferredId);
#else
                return _this != null && _this.TryReportProgress(deferredId, progress);
#endif
            }
        }

        partial class PromiseRefBase
        {
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal partial struct DeferredIdAndProgress
            {
                [MethodImpl(InlineOption)]
                internal bool TryIncrementId(int deferredId)
                {
                    unchecked
                    {
                        return Interlocked.CompareExchange(ref _id, deferredId + 1, deferredId) == deferredId;
                    }
                }

                [MethodImpl(InlineOption)]
                internal void IncrementId()
                {
                    // Used when canceled from the token.
                    Interlocked.Increment(ref _id);
                }
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal abstract partial class DeferredPromiseBase<TResult> : PromiseSingleAwait<TResult>, IDeferredPromise
            {
                public int DeferredId
                {
                    [MethodImpl(InlineOption)]
                    get { return _idAndProgress._id; }
                }

                protected DeferredPromiseBase() { }

                ~DeferredPromiseBase()
                {
                    try
                    {
                        if (State == Promise.State.Pending)
                        {
                            // Deferred wasn't handled.
                            ReportRejection(UnhandledDeferredException.instance, this);
                        }
                    }
                    catch (Exception e)
                    {
                        // This should never happen.
                        ReportRejection(e, this);
                    }
                }


                [MethodImpl(InlineOption)]
                public virtual bool TryIncrementDeferredIdAndUnregisterCancelation(int deferredId)
                {
                    bool success = _idAndProgress.TryIncrementId(deferredId);
                    MaybeThrowIfInPool(this, success);
                    return success;
                }

                public void RejectDirect(IRejectContainer reasonContainer)
                {
                    HandleNextInternal(reasonContainer, Promise.State.Rejected);
                }

                [MethodImpl(InlineOption)]
                public void CancelDirect()
                {
                    ThrowIfInPool(this);
                    HandleNextInternal(null, Promise.State.Canceled);
                }
            }

            // The only purpose of this is to cast the ref when converting a DeferredBase to a Deferred(<T>) to avoid extra checks.
            // Otherwise, DeferredPromise<T> would be unnecessary and this would be implemented in the base class.
#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal partial class DeferredPromise<TResult> : DeferredPromiseBase<TResult>
            {
                protected DeferredPromise() { }

                internal override void MaybeDispose()
                {
                    Dispose();
                    ObjectPool.MaybeRepool(this);
                }

                [MethodImpl(InlineOption)]
                private static DeferredPromise<TResult> GetFromPoolOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<DeferredPromise<TResult>>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new DeferredPromise<TResult>()
                        : obj.UnsafeAs<DeferredPromise<TResult>>();
                }

                internal static DeferredPromise<TResult> GetOrCreate()
                {
                    var promise = GetFromPoolOrCreate();
                    promise.Reset();
                    return promise;
                }

                [MethodImpl(InlineOption)]
                internal static bool TryResolve(DeferredPromise<TResult> _this, int deferredId,
#if CSHARP_7_3_OR_NEWER
                    in
#endif
                    TResult value)
                {
                    if (_this != null && _this.TryIncrementDeferredIdAndUnregisterCancelation(deferredId))
                    {
                        _this.ResolveDirect(value);
                        return true;
                    }
                    return false;
                }

                [MethodImpl(InlineOption)]
                internal static bool TryResolveVoid(DeferredPromise<TResult> _this, int deferredId)
                {
                    if (_this != null && _this.TryIncrementDeferredIdAndUnregisterCancelation(deferredId))
                    {
                        _this.ResolveDirectVoid();
                        return true;
                    }
                    return false;
                }

                [MethodImpl(InlineOption)]
                internal void ResolveDirect(
#if CSHARP_7_3_OR_NEWER
                    in
#endif
                    TResult value)
                {
                    _result = value;
                    HandleNextInternal(null, Promise.State.Resolved);
                }

                [MethodImpl(InlineOption)]
                internal void ResolveDirectVoid()
                {
                    ThrowIfInPool(this);
                    HandleNextInternal(null, Promise.State.Resolved);
                }
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            internal sealed partial class DeferredPromiseCancel<TResult> : DeferredPromise<TResult>, ICancelable
            {
                private DeferredPromiseCancel() { }

                internal override void MaybeDispose()
                {
                    Dispose();
                    _cancelationRegistration = default(CancelationRegistration);
                    ObjectPool.MaybeRepool(this);
                }

                [MethodImpl(InlineOption)]
                new private static DeferredPromiseCancel<TResult> GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<DeferredPromiseCancel<TResult>>();
                    return obj == InvalidAwaitSentinel.s_instance
                        ? new DeferredPromiseCancel<TResult>()
                        : obj.UnsafeAs<DeferredPromiseCancel<TResult>>();
                }

                internal static DeferredPromiseCancel<TResult> GetOrCreate(CancelationToken cancelationToken)
                {
                    var promise = GetOrCreate();
                    promise.Reset();
                    cancelationToken.TryRegister(promise, out promise._cancelationRegistration);
                    return promise;
                }

                private bool TryUnregisterCancelation()
                {
                    return TryUnregisterAndIsNotCanceling(ref _cancelationRegistration);
                }

                public override bool TryIncrementDeferredIdAndUnregisterCancelation(int deferredId)
                {
                    return base.TryIncrementDeferredIdAndUnregisterCancelation(deferredId)
                        && TryUnregisterCancelation(); // If TryUnregisterCancelation returns false, it means the CancelationSource was canceled.
                }

                void ICancelable.Cancel()
                {
                    ThrowIfInPool(this);
                    // A simple increment is sufficient.
                    // If the CancelationSource was canceled before the Deferred was completed, even if the Deferred was completed before the cancelation was invoked, the cancelation takes precedence.
                    _idAndProgress.IncrementId();
                    CancelDirect();
                }
            }
        } // class PromiseRef
    } // class Internal
} // namespace Proto.Promises