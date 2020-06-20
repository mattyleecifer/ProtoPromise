﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable RECS0108 // Warns about static fields in generic types
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable RECS0001 // Class is declared partial but has only one part

using System;
using Proto.Utils;

namespace Proto.Promises
{
    partial class Promise
    {
        partial class InternalProtected
        {
            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegatePassthrough : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly bool _isActive;

                public bool IsNull { get { return !_isActive; } }

                public DelegatePassthrough(bool active)
                {
                    _isActive = active;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    owner.ResolveInternal(valueContainer);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    owner.RejectOrCancelInternal(valueContainer);
                }

                public void MaybeUnregisterCancelation() { }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class FinallyDelegate : Internal.ITreeHandleable, Internal.ITraceable
            {
#if PROMISE_DEBUG
                Internal.CausalityTrace Internal.ITraceable.Trace { get; set; }
#endif
                Internal.ITreeHandleable ILinked<Internal.ITreeHandleable>.Next { get; set; }

                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                private Action _onFinally;

                private FinallyDelegate() { }

                static FinallyDelegate()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                public static FinallyDelegate GetOrCreate(Action onFinally)
                {
                    var del = _pool.IsNotEmpty ? (FinallyDelegate) _pool.Pop() : new FinallyDelegate();
                    del._onFinally = onFinally;
                    SetCreatedStacktrace(del, 2);
                    return del;
                }

                private void InvokeAndCatchAndDispose()
                {
                    var callback = _onFinally;
                    SetCurrentInvoker(this);
                    Dispose();
                    try
                    {
                        callback.Invoke();
                    }
                    catch (Exception e)
                    {
                        Internal.AddRejectionToUnhandledStack(e, this);
                    }
                    ClearCurrentInvoker();
                }

                void Dispose()
                {
                    _onFinally = null;
                    if (Config.ObjectPooling != PoolType.None)
                    {
                        _pool.Push(this);
                    }
                }

                void Internal.ITreeHandleable.Handle()
                {
                    InvokeAndCatchAndDispose();
                }

                void Internal.ITreeHandleable.MakeReady(Internal.IValueContainer valueContainer, ref ValueLinkedQueue<Internal.ITreeHandleable> handleQueue)
                {
                    handleQueue.Push(this);
                }

                void Internal.ITreeHandleable.MakeReadyFromSettled(Internal.IValueContainer valueContainer)
                {
                    Internal.AddToHandleQueueBack(this);
                }
            }

            #region Regular Delegates
            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidVoid : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Action _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidVoid(Action callback)
                {
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgVoid<TArg> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Action<TArg> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgVoid(Action<TArg> callback)
                {
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    _callback.Invoke(arg);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidResult<TResult> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidResult(Func<TResult> callback)
                {
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TResult result = _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TResult result = _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgResult<TArg, TResult> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<TArg, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgResult(Func<TArg, TResult> callback)
                {
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    var temp = _callback;
                    TResult result = temp.Invoke(arg);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidPromise : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<Promise> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidPromise(Func<Promise> callback)
                {
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke());
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke());
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgPromise<TArg> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<TArg, Promise> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgPromise(Func<TArg, Promise> callback)
                {
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(arg));
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidPromiseT<TPromise> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<Promise<TPromise>> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidPromiseT(Func<Promise<TPromise>> callback)
                {
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke());
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke());
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgPromiseT<TArg, TPromise> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly Func<TArg, Promise<TPromise>> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgPromiseT(Func<TArg, Promise<TPromise>> callback)
                {
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(arg));
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueVoidVoid : IDelegateContinue
            {
                private readonly Action<ResultContainer> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueVoidVoid(Action<ResultContainer> callback)
                {
                    _callback = callback;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    _callback.Invoke(new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueVoidResult<TResult> : IDelegateContinue<TResult>
            {
                private readonly Func<ResultContainer, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueVoidResult(Func<ResultContainer, TResult> callback)
                {
                    _callback = callback;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    return _callback.Invoke(new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueArgVoid<TArg> : IDelegateContinue
            {
                private readonly Action<Promise<TArg>.ResultContainer> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueArgVoid(Action<Promise<TArg>.ResultContainer> callback)
                {
                    _callback = callback;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    _callback.Invoke(new Promise<TArg>.ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueArgResult<TArg, TResult> : IDelegateContinue<TResult>
            {
                private readonly Func<Promise<TArg>.ResultContainer, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueArgResult(Func<Promise<TArg>.ResultContainer, TResult> callback)
                {
                    _callback = callback;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    return _callback.Invoke(new Promise<TArg>.ResultContainer(valueContainer));
                }
            }
            #endregion

            #region Delegates with capture value
            [System.Diagnostics.DebuggerNonUserCode]
            public sealed class FinallyDelegateCapture<TCapture> : Internal.ITreeHandleable, Internal.ITraceable
            {
#if PROMISE_DEBUG
                Internal.CausalityTrace Internal.ITraceable.Trace { get; set; }
#endif
                Internal.ITreeHandleable ILinked<Internal.ITreeHandleable>.Next { get; set; }

                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                private TCapture _capturedValue;
                private Action<TCapture> _onFinally;

                private FinallyDelegateCapture() { }

                static FinallyDelegateCapture()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                public static FinallyDelegateCapture<TCapture> GetOrCreate(ref TCapture capturedValue, Action<TCapture> onFinally)
                {
                    var del = _pool.IsNotEmpty ? (FinallyDelegateCapture<TCapture>) _pool.Pop() : new FinallyDelegateCapture<TCapture>();
                    del._capturedValue = capturedValue;
                    del._onFinally = onFinally;
                    SetCreatedStacktrace(del, 2);
                    return del;
                }

                private void InvokeAndCatchAndDispose()
                {
                    var value = _capturedValue;
                    var callback = _onFinally;
                    SetCurrentInvoker(this);
                    Dispose();
                    try
                    {
                        callback.Invoke(value);
                    }
                    catch (Exception e)
                    {
                        Internal.AddRejectionToUnhandledStack(e, this);
                    }
                    ClearCurrentInvoker();
                }

                void Dispose()
                {
                    _capturedValue = default(TCapture);
                    _onFinally = null;
                    if (Config.ObjectPooling != PoolType.None)
                    {
                        _pool.Push(this);
                    }
                }

                void Internal.ITreeHandleable.Handle()
                {
                    InvokeAndCatchAndDispose();
                }

                void Internal.ITreeHandleable.MakeReady(Internal.IValueContainer valueContainer, ref ValueLinkedQueue<Internal.ITreeHandleable> handleQueue)
                {
                    handleQueue.Push(this);
                }

                void Internal.ITreeHandleable.MakeReadyFromSettled(Internal.IValueContainer valueContainer)
                {
                    Internal.AddToHandleQueueBack(this);
                }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidVoid<TCapture> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidVoid(ref TCapture capturedValue, Action<TCapture> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgVoid<TCapture, TArg> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, TArg> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgVoid(ref TCapture capturedValue, Action<TCapture, TArg> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    _callback.Invoke(_capturedValue, arg);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidResult<TCapture, TResult> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidResult(ref TCapture capturedValue, Func<TCapture, TResult> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TResult result = _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TResult result = _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgResult<TCapture, TArg, TResult> : IDelegateResolve, IDelegateReject, IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgResult(ref TCapture capturedValue, Func<TCapture, TArg, TResult> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    TResult result = _callback.Invoke(_capturedValue, arg);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidPromise<TCapture> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidPromise(ref TCapture capturedValue, Func<TCapture, Promise> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgPromise<TCapture, TArg> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, Promise> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgPromise(ref TCapture capturedValue, Func<TCapture, TArg, Promise> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(_capturedValue, arg));
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidPromiseT<TCapture, TPromise> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise<TPromise>> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidPromiseT(ref TCapture capturedValue, Func<TCapture, Promise<TPromise>> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void MaybeUnregisterCancelation() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgPromiseT<TCapture, TArg, TPromise> : IDelegateResolvePromise, IDelegateRejectPromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, Promise<TPromise>> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgPromiseT(ref TCapture capturedValue, Func<TCapture, TArg, Promise<TPromise>> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                private void Invoke(TArg arg, Promise owner)
                {
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(_capturedValue, arg));
                }

                void IDelegateResolvePromise.InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value, owner);
                }

                public void InvokeRejecter(Internal.IValueContainer valueContainer, Promise owner)
                {
                    TArg arg;
                    if (Internal.TryConvert(valueContainer, out arg))
                    {
                        Invoke(arg, owner);
                    }
                    else
                    {
                        owner.RejectOrCancelInternal(valueContainer);
                    }
                }

                public void MaybeUnregisterCancelation() { }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureVoidVoid<TCapture> : IDelegateContinue
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, ResultContainer> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureVoidVoid(ref TCapture capturedValue, Action<TCapture, ResultContainer> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    _callback.Invoke(_capturedValue, new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureVoidResult<TCapture, TResult> : IDelegateContinue<TResult>
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, ResultContainer, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureVoidResult(ref TCapture capturedValue, Func<TCapture, ResultContainer, TResult> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    return _callback.Invoke(_capturedValue, new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureArgVoid<TCapture, TArg> : IDelegateContinue
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, Promise<TArg>.ResultContainer> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureArgVoid(ref TCapture capturedValue, Action<TCapture, Promise<TArg>.ResultContainer> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                void IDelegateContinue.Invoke(Internal.IValueContainer valueContainer)
                {
                    _callback.Invoke(_capturedValue, new Promise<TArg>.ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureArgResult<TCapture, TArg, TResult> : IDelegateContinue<TResult>
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise<TArg>.ResultContainer, TResult> _callback;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureArgResult(ref TCapture capturedValue, Func<TCapture, Promise<TArg>.ResultContainer, TResult> callback)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    return _callback.Invoke(_capturedValue, new Promise<TArg>.ResultContainer(valueContainer));
                }
            }
            #endregion

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegatePassthroughCancel : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;
                private readonly bool _isActive;

                public bool IsNull { get { return !_isActive; } }

                public DelegatePassthroughCancel(CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                    _isActive = true;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _cancelationToken.ThrowIfCancelationRequested();
                    owner.ResolveInternal(valueContainer);
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            #region Delegates with cancelation token
            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidVoidCancel : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly Action _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidVoidCancel(Action callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgVoidCancel<TArg> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly Action<TArg> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgVoidCancel(Action<TArg> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidResultCancel<TResult> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly Func<TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidResultCancel(Func<TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TResult result = _callback.Invoke();
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgResultCancel<TArg, TResult> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly Func<TArg, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgResultCancel(Func<TArg, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TResult result = _callback.Invoke(((Internal.ResolveContainer<TArg>) valueContainer).value);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidPromiseCancel : IDelegateResolvePromise
            {
                private readonly Func<Promise> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidPromiseCancel(Func<Promise> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgPromiseCancel<TArg> : IDelegateResolvePromise
            {
                private readonly Func<TArg, Promise> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgPromiseCancel(Func<TArg, Promise> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TArg arg = ((Internal.ResolveContainer<TArg>) valueContainer).value;
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(arg));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateVoidPromiseTCancel<TPromise> : IDelegateResolvePromise
            {
                private readonly Func<Promise<TPromise>> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateVoidPromiseTCancel(Func<Promise<TPromise>> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateArgPromiseTCancel<TArg, TPromise> : IDelegateResolvePromise
            {
                private readonly Func<TArg, Promise<TPromise>> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateArgPromiseTCancel(Func<TArg, Promise<TPromise>> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TArg arg = ((Internal.ResolveContainer<TArg>) valueContainer).value;
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(arg));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueVoidVoidCancel : IDelegateContinue
            {
                private readonly Action<ResultContainer> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueVoidVoidCancel(Action<ResultContainer> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(new ResultContainer(valueContainer));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueVoidResultCancel<TResult> : IDelegateContinue<TResult>
            {
                private readonly Func<ResultContainer, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueVoidResultCancel(Func<ResultContainer, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    return _callback.Invoke(new ResultContainer(valueContainer));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueArgVoidCancel<TArg> : IDelegateContinue
            {
                private readonly Action<Promise<TArg>.ResultContainer> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueArgVoidCancel(Action<Promise<TArg>.ResultContainer> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(new Promise<TArg>.ResultContainer(valueContainer));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueArgResultCancel<TArg, TResult> : IDelegateContinue<TResult>
            {
                private readonly Func<Promise<TArg>.ResultContainer, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueArgResultCancel(Func<Promise<TArg>.ResultContainer, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    return _callback.Invoke(new Promise<TArg>.ResultContainer(valueContainer));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }
            #endregion

            #region Delegates with capture value and cancelation token
            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidVoidCancel<TCapture> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidVoidCancel(ref TCapture capturedValue, Action<TCapture> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgVoidCancel<TCapture, TArg> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, TArg> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgVoidCancel(ref TCapture capturedValue, Action<TCapture, TArg> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(_capturedValue, ((Internal.ResolveContainer<TArg>) valueContainer).value);
                    owner.ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidResultCancel<TCapture, TResult> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidResultCancel(ref TCapture capturedValue, Func<TCapture, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TResult result = _callback.Invoke(_capturedValue);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgResultCancel<TCapture, TArg, TResult> : IDelegateResolve, IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgResultCancel(ref TCapture capturedValue, Func<TCapture, TArg, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TResult result = _callback.Invoke(_capturedValue, ((Internal.ResolveContainer<TArg>) valueContainer).value);
                    owner.ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidPromiseCancel<TCapture> : IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidPromiseCancel(ref TCapture capturedValue, Func<TCapture, Promise> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgPromiseCancel<TCapture, TArg> : IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, Promise> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgPromiseCancel(ref TCapture capturedValue, Func<TCapture, TArg, Promise> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TArg arg = ((Internal.ResolveContainer<TArg>) valueContainer).value;
                    ((PromiseWaitPromise) owner).WaitFor(_callback.Invoke(_capturedValue, arg));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureVoidPromiseTCancel<TCapture, TPromise> : IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise<TPromise>> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureVoidPromiseTCancel(ref TCapture capturedValue, Func<TCapture, Promise<TPromise>> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(_capturedValue));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateCaptureArgPromiseTCancel<TCapture, TArg, TPromise> : IDelegateResolvePromise
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, TArg, Promise<TPromise>> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateCaptureArgPromiseTCancel(ref TCapture capturedValue, Func<TCapture, TArg, Promise<TPromise>> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void InvokeResolver(Internal.IValueContainer valueContainer, Promise owner)
                {
                    MaybeUnregisterCancelation();
                    ReleaseAndMaybeThrow(_cancelationToken);
                    TArg arg = ((Internal.ResolveContainer<TArg>) valueContainer).value;
                    ((PromiseWaitPromise<TPromise>) owner).WaitFor(_callback.Invoke(_capturedValue, arg));
                }

                public void MaybeUnregisterCancelation()
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                }
            }


            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureVoidVoidCancel<TCapture> : IDelegateContinue
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, ResultContainer> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureVoidVoidCancel(ref TCapture capturedValue, Action<TCapture, ResultContainer> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(_capturedValue, new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureVoidResultCancel<TCapture, TResult> : IDelegateContinue<TResult>
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, ResultContainer, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureVoidResultCancel(ref TCapture capturedValue, Func<TCapture, ResultContainer, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                    ReleaseAndMaybeThrow(_cancelationToken);
                    return _callback.Invoke(_capturedValue, new ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureArgVoidCancel<TCapture, TArg> : IDelegateContinue
            {
                private readonly TCapture _capturedValue;
                private readonly Action<TCapture, Promise<TArg>.ResultContainer> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureArgVoidCancel(ref TCapture capturedValue, Action<TCapture, Promise<TArg>.ResultContainer> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public void Invoke(Internal.IValueContainer valueContainer)
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                    ReleaseAndMaybeThrow(_cancelationToken);
                    _callback.Invoke(_capturedValue, new Promise<TArg>.ResultContainer(valueContainer));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal struct DelegateContinueCaptureArgResultCancel<TCapture, TArg, TResult> : IDelegateContinue<TResult>
            {
                private readonly TCapture _capturedValue;
                private readonly Func<TCapture, Promise<TArg>.ResultContainer, TResult> _callback;
                private readonly CancelationToken _cancelationToken;
                private readonly CancelationRegistration _cancelationRegistration;

                public bool IsNull { get { return _callback == null; } }

                public DelegateContinueCaptureArgResultCancel(ref TCapture capturedValue, Func<TCapture, Promise<TArg>.ResultContainer, TResult> callback, CancelationToken cancelationToken, CancelationRegistration cancelationRegistration)
                {
                    _capturedValue = capturedValue;
                    _callback = callback;
                    _cancelationToken = cancelationToken;
                    _cancelationRegistration = cancelationRegistration;
                }

                public TResult Invoke(Internal.IValueContainer valueContainer)
                {
                    if (_cancelationRegistration.IsRegistered)
                    {
                        _cancelationRegistration.Unregister();
                    }
                    ReleaseAndMaybeThrow(_cancelationToken);
                    return _callback.Invoke(_capturedValue, new Promise<TArg>.ResultContainer(valueContainer));
                }
            }
            #endregion
        }
    }
}