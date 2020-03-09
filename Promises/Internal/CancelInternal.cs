﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif
#if !PROTO_PROMISE_CANCEL_DISABLE
#define PROMISE_CANCEL
#else
#undef PROMISE_CANCEL
#endif
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#pragma warning disable RECS0108 // Warns about static fields in generic types
#pragma warning disable RECS0001 // Class is declared partial but has only one part
#pragma warning disable RECS0096 // Type parameter is never used
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression

using System;
using Proto.Utils;

namespace Proto.Promises
{
    partial class Promise
    {
        // Calls to this get compiled away when CANCEL is defined.
        static partial void ValidateCancel(int skipFrames);

        static partial void AddToCancelQueueBack(Internal.ITreeHandleable cancelation);
        static partial void AddToCancelQueueFront(Internal.ITreeHandleable cancelation);
        static partial void AddToCancelQueueFront(ref ValueLinkedQueue<Internal.ITreeHandleable> cancelations);
        static partial void AddToCancelQueueBack(ref ValueLinkedQueue<Internal.ITreeHandleable> cancelations);
        static partial void HandleCanceled();
#if PROMISE_CANCEL
        // Cancel promises in a depth-first manner.
        private static ValueLinkedQueue<Internal.ITreeHandleable> _cancelQueue;

        static partial void AddToCancelQueueFront(Internal.ITreeHandleable cancelation)
        {
            _cancelQueue.Push(cancelation);
        }

        static partial void AddToCancelQueueBack(Internal.ITreeHandleable cancelation)
        {
            _cancelQueue.Enqueue(cancelation);
        }

        static partial void AddToCancelQueueFront(ref ValueLinkedQueue<Internal.ITreeHandleable> cancelations)
        {
            _cancelQueue.PushAndClear(ref cancelations);
        }

        static partial void AddToCancelQueueBack(ref ValueLinkedQueue<Internal.ITreeHandleable> cancelations)
        {
            _cancelQueue.EnqueueAndClear(ref cancelations);
        }

        static partial void HandleCanceled()
        {
            while (_cancelQueue.IsNotEmpty)
            {
                _cancelQueue.DequeueRisky().Cancel();
            }
            _cancelQueue.ClearLast();
        }

        void Internal.ITreeHandleable.Cancel()
        {
            if (_state == State.Pending)
            {
                CancelInternal();
            }
        }
#else
        static protected void ThrowCancelException(int skipFrames)
        {
            throw new InvalidOperationException("Cancelations are disabled. Remove PROTO_PROMISE_CANCEL_DISABLE from your compiler symbols to enable cancelations.", GetFormattedStacktrace(skipFrames + 1));
        }

        static partial void ValidateCancel(int skipFrames)
        {
            ThrowCancelException(skipFrames + 1);
        }
#endif

        partial class Internal
        {
            public abstract partial class PotentialCancelation : ITreeHandleable
            {
                ITreeHandleable ILinked<ITreeHandleable>.Next { get; set; }

                protected object _valueContainer;

                public void MakeReady(IValueContainer valueContainer, ref ValueLinkedQueue<ITreeHandleable> handleQueue, ref ValueLinkedQueue<ITreeHandleable> cancelQueue)
                {
                    if (valueContainer.GetState() == State.Canceled)
                    {
                        valueContainer.Retain();
                        _valueContainer = valueContainer;
                    }
                    cancelQueue.Push(this);
                }

                public void MakeReadyFromSettled(IValueContainer valueContainer)
                {
                    if (valueContainer.GetState() == State.Canceled)
                    {
                        valueContainer.Retain();
                        _valueContainer = valueContainer;
                    }
                    AddToCancelQueueFront(this);
                }

                public void Handle()
                {
                    Dispose();
                }

                public abstract void Cancel();

                protected virtual void Dispose()
                {
                    _valueContainer = DisposedObject;
                }

                protected void MakeBranchesReady(ref ValueLinkedStack<ITreeHandleable> branches, IValueContainer valueContainer)
                {
                    ValueLinkedQueue<ITreeHandleable> handleQueue = new ValueLinkedQueue<ITreeHandleable>();
                    ValueLinkedQueue<ITreeHandleable> cancelQueue = new ValueLinkedQueue<ITreeHandleable>();
                    while (branches.IsNotEmpty)
                    {
                        branches.Pop().MakeReady(valueContainer, ref handleQueue, ref cancelQueue);
                    }
                    AddToCancelQueueFront(ref handleQueue);
                }

                protected void DisposeBranches(ref ValueLinkedStack<ITreeHandleable> branches)
                {
                    while (branches.IsNotEmpty)
                    {
                        AddToHandleQueueFront(branches.Pop());
                    }
                }
            }

            public sealed class CancelDelegateAny : PotentialCancelation
            {
                private static ValueLinkedStack<ITreeHandleable> _pool;

                private Action _onCanceled;

                private CancelDelegateAny() { }

                static CancelDelegateAny()
                {
                    OnClearPool += () => _pool.Clear();
                }

                public static CancelDelegateAny GetOrCreate(Action onCanceled, int skipFrames)
                {
                    var del = _pool.IsNotEmpty ? (CancelDelegateAny) _pool.Pop() : new CancelDelegateAny();
                    del._onCanceled = onCanceled;
                    del._valueContainer = null;
                    SetCreatedStacktrace(del, skipFrames + 1);
                    return del;
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    _onCanceled = null;
                    if (Config.ObjectPooling != PoolType.None)
                    {
                        _pool.Push(this);
                    }
                }

                public override void Cancel()
                {
                    var callback = _onCanceled;
                    ((IValueContainer) _valueContainer).Release();
                    Dispose();
                    try
                    {
                        callback.Invoke();
                    }
                    catch (Exception e)
                    {
                        AddRejectionToUnhandledStack(e, this);
                    }
                }
            }

            public sealed class CancelDelegate<T> : PotentialCancelation, IPotentialCancelation
            {
                private static ValueLinkedStack<ITreeHandleable> _pool;

                private ValueLinkedStack<ITreeHandleable> _nextBranches;
                private Action<T> _onCanceled;
                private ushort _retainCounter;

                private CancelDelegate() { }

                static CancelDelegate()
                {
                    OnClearPool += () => _pool.Clear();
                }

                ~CancelDelegate()
                {
                    if (_retainCounter > 0)
                    {
                        // Delegate wasn't released.
                        AddRejectionToUnhandledStack(UnreleasedObjectException.instance, this);
                    }
                }

                public static CancelDelegate<T> GetOrCreate(Action<T> onCanceled, int skipFrames)
                {
                    var del = _pool.IsNotEmpty ? (CancelDelegate<T>) _pool.Pop() : new CancelDelegate<T>();
                    del._onCanceled = onCanceled;
                    del._valueContainer = null;
                    SetCreatedStacktrace(del, skipFrames + 1);
                    return del;
                }

                protected override void Dispose()
                {
                    if (_retainCounter == 0 & _onCanceled == null)
                    {
                        base.Dispose();
                        if (Config.ObjectPooling == PoolType.All)
                        {
                            _pool.Push(this);
                        }
                    }
                }

                public override void Cancel()
                {
                    var callback = _onCanceled;
                    _onCanceled = null;
                    var branches = _nextBranches;
                    _nextBranches.Clear();
                    IValueContainer valueContainer = (IValueContainer) _valueContainer;
                    Dispose();
                    T arg;
                    if (valueContainer.TryGetValueAs(out arg))
                    {
                        valueContainer.Release();
                        try
                        {
                            callback.Invoke(arg);
                        }
                        catch (Exception e)
                        {
                            AddRejectionToUnhandledStack(e, this);
                        }
                        DisposeBranches(ref branches);
                    }
                    else
                    {
                        MakeBranchesReady(ref branches, valueContainer);
                        valueContainer.Release();
                    }
                }

                void IPotentialCancelation.CatchCancelation(Action onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled != null)
                    {
                        _nextBranches.Push(CancelDelegateAny.GetOrCreate(onCanceled, 1));
                    }
                }

                IPotentialCancelation IPotentialCancelation.CatchCancelation<TCancel>(Action<TCancel> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled == null)
                    {
                        return this;
                    }
                    var cancelation = CancelDelegate<TCancel>.GetOrCreate(onCanceled, 1);
                    _nextBranches.Push(cancelation);
                    return cancelation;
                }

                void IPotentialCancelation.CatchCancelation<TCapture>(TCapture captureValue, Action<TCapture> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled != null)
                    {
                        _nextBranches.Push(CancelDelegateAnyCapture<TCapture>.GetOrCreate(captureValue, onCanceled, 1));
                    }
                }

                IPotentialCancelation IPotentialCancelation.CatchCancelation<TCapture, TCancel>(TCapture captureValue, Action<TCapture, TCancel> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled == null)
                    {
                        return this;
                    }
                    var cancelation = CancelDelegateCapture<TCapture, TCancel>.GetOrCreate(captureValue, onCanceled, 1);
                    _nextBranches.Push(cancelation);
                    return cancelation;
                }

                public void Retain()
                {
#if PROMISE_DEBUG
                    checked
#endif
                    {
                        ++_retainCounter;
                    }
                }

                public void Release()
                {
#if PROMISE_DEBUG
                    checked
#endif
                    {
                        --_retainCounter;
                        Dispose();
                    }
                }
            }

            public sealed class CancelDelegateAnyCapture<TCapture> : PotentialCancelation
            {
                private static ValueLinkedStack<ITreeHandleable> _pool;

                private TCapture _capturedValue;
                private Action<TCapture> _onCanceled;

                private CancelDelegateAnyCapture() { }

                static CancelDelegateAnyCapture()
                {
                    OnClearPool += () => _pool.Clear();
                }

                public static CancelDelegateAnyCapture<TCapture> GetOrCreate(TCapture capturedValue, Action<TCapture> onCanceled, int skipFrames)
                {
                    var del = _pool.IsNotEmpty ? (CancelDelegateAnyCapture<TCapture>) _pool.Pop() : new CancelDelegateAnyCapture<TCapture>();
                    del._capturedValue = capturedValue;
                    del._onCanceled = onCanceled;
                    del._valueContainer = null;
                    SetCreatedStacktrace(del, skipFrames + 1);
                    return del;
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    _capturedValue = default(TCapture);
                    _onCanceled = null;
                    if (Config.ObjectPooling != PoolType.None)
                    {
                        _pool.Push(this);
                    }
                }

                public override void Cancel()
                {
                    var value = _capturedValue;
                    var callback = _onCanceled;
                    ((IValueContainer) _valueContainer).Release();
                    Dispose();
                    try
                    {
                        callback.Invoke(value);
                    }
                    catch (Exception e)
                    {
                        AddRejectionToUnhandledStack(e, this);
                    }
                }
            }

            public sealed class CancelDelegateCapture<TCapture, T> : PotentialCancelation, IPotentialCancelation
            {
                private static ValueLinkedStack<ITreeHandleable> _pool;

                private ValueLinkedStack<ITreeHandleable> _nextBranches;
                private TCapture _capturedValue;
                private Action<TCapture, T> _onCanceled;
                private ushort _retainCounter;

                private CancelDelegateCapture() { }

                static CancelDelegateCapture()
                {
                    OnClearPool += () => _pool.Clear();
                }

                ~CancelDelegateCapture()
                {
                    if (_retainCounter > 0)
                    {
                        // Delegate wasn't released.
                        AddRejectionToUnhandledStack(UnreleasedObjectException.instance, this);
                    }
                }

                public static CancelDelegateCapture<TCapture, T> GetOrCreate(TCapture capturedValue, Action<TCapture, T> onCanceled, int skipFrames)
                {
                    var del = _pool.IsNotEmpty ? (CancelDelegateCapture<TCapture, T>) _pool.Pop() : new CancelDelegateCapture<TCapture, T>();
                    del._capturedValue = capturedValue;
                    del._onCanceled = onCanceled;
                    del._valueContainer = null;
                    SetCreatedStacktrace(del, skipFrames + 1);
                    return del;
                }

                protected override void Dispose()
                {
                    if (_retainCounter == 0 & _onCanceled == null)
                    {
                        base.Dispose();
                        if (Config.ObjectPooling == PoolType.All)
                        {
                            _pool.Push(this);
                        }
                    }
                }

                public override void Cancel()
                {
                    var value = _capturedValue;
                    _capturedValue = default(TCapture);
                    var callback = _onCanceled;
                    _onCanceled = null;
                    var branches = _nextBranches;
                    _nextBranches.Clear();
                    IValueContainer valueContainer = (IValueContainer) _valueContainer;
                    Dispose();
                    T arg;
                    if (valueContainer.TryGetValueAs(out arg))
                    {
                        valueContainer.Release();
                        try
                        {
                            callback.Invoke(value, arg);
                        }
                        catch (Exception e)
                        {
                            AddRejectionToUnhandledStack(e, this);
                        }
                        DisposeBranches(ref branches);
                    }
                    else
                    {
                        MakeBranchesReady(ref branches, valueContainer);
                        valueContainer.Release();
                    }
                }

                void IPotentialCancelation.CatchCancelation(Action onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled != null)
                    {
                        _nextBranches.Push(CancelDelegateAny.GetOrCreate(onCanceled, 1));
                    }
                }

                IPotentialCancelation IPotentialCancelation.CatchCancelation<TCancel>(Action<TCancel> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled == null)
                    {
                        return this;
                    }
                    var cancelation = CancelDelegate<TCancel>.GetOrCreate(onCanceled, 1);
                    _nextBranches.Push(cancelation);
                    return cancelation;
                }

                void IPotentialCancelation.CatchCancelation<TCapture1>(TCapture1 captureValue, Action<TCapture1> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled != null)
                    {
                        _nextBranches.Push(CancelDelegateAnyCapture<TCapture1>.GetOrCreate(captureValue, onCanceled, 1));
                    }
                }

                IPotentialCancelation IPotentialCancelation.CatchCancelation<TCapture1, TCancel>(TCapture1 captureValue, Action<TCapture1, TCancel> onCanceled)
                {
                    ValidatePotentialOperation(_valueContainer, 1);
                    ValidateArgument(onCanceled, "onCanceled", 1);

                    if (_onCanceled == null)
                    {
                        return this;
                    }
                    var cancelation = CancelDelegateCapture<TCapture1, TCancel>.GetOrCreate(captureValue, onCanceled, 1);
                    _nextBranches.Push(cancelation);
                    return cancelation;
                }

                public void Retain()
                {
#if PROMISE_DEBUG
                    checked
#endif
                    {
                        ++_retainCounter;
                    }
                }

                public void Release()
                {
#if PROMISE_DEBUG
                    checked
#endif
                    {
                        --_retainCounter;
                        Dispose();
                    }
                }
            }
        }

        private void ResolveDirectIfNotCanceled()
        {
#if PROMISE_CANCEL
            if (_state != State.Canceled)
#endif
            {
                _state = State.Resolved;
                var resolveValue = Internal.ResolveContainerVoid.GetOrCreate();
                _valueOrPrevious = resolveValue;
                AddBranchesToHandleQueueBack(resolveValue);
                ResolveProgressListeners();
                AddToHandleQueueFront(this);
            }
        }

        protected void RejectDirectIfNotCanceled<TReject>(TReject reason, bool generateStacktrace)
        {
#if PROMISE_CANCEL
            if (_state == State.Canceled)
            {
                AddRejectionToUnhandledStack(reason, generateStacktrace ? null : this);
            }
            else
#endif
            {
                RejectDirect(reason, generateStacktrace);
            }
        }

        protected void ResolveInternalIfNotCanceled()
        {
#if PROMISE_CANCEL
            if (_state == State.Canceled)
            {
                MaybeDispose();
            }
            else
#endif
            {
                ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
            }
        }

        protected void ResolveInternalIfNotCanceled<T>(T value)
        {
#if PROMISE_CANCEL
            if (_state == State.Canceled)
            {
                MaybeDispose();
            }
            else
#endif
            {
                ResolveInternal(Internal.ResolveContainer<T>.GetOrCreate(value));
            }
        }
    }

    partial class Promise<T>
    {
        // Calls to this get compiled away when CANCEL is defined.
        static partial void ValidateCancel(int skipFrames);
#if !PROMISE_CANCEL
        static partial void ValidateCancel(int skipFrames)
        {
            ThrowCancelException(skipFrames + 1);
        }
#endif
    }
}