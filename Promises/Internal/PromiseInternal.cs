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

#pragma warning disable RECS0108 // Warns about static fields in generic types
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable RECS0001 // Class is declared partial but has only one part
#pragma warning disable IDE0041 // Use 'is null' check

using System;
using System.Collections.Generic;
using Proto.Utils;

namespace Proto.Promises
{
    partial class Promise : Internal.ITreeHandleable, Internal.ITraceable
    {
        private ValueLinkedStack<Internal.ITreeHandleable> _nextBranches;
        protected object _valueOrPrevious;
        private ushort _retainCounter;
        protected State _state;
        protected bool _wasWaitedOn;

        Internal.ITreeHandleable ILinked<Internal.ITreeHandleable>.Next { get; set; }

        ~Promise()
        {
            if (_retainCounter > 0 & _state != State.Pending)
            {
                if (_wasWaitedOn)
                {
                    ((Internal.IValueContainer) _valueOrPrevious).Release();
                }
                else
                {
                    // Rejection maybe wasn't caught.
                    ((Internal.IValueContainer) _valueOrPrevious).ReleaseAndAddToUnhandledStack();
                }
                // Promise wasn't released.
                string message = "A Promise object was garbage collected that was not released. You must release all IRetainable objects that you have retained.";
                Internal.AddRejectionToUnhandledStack(new UnreleasedObjectException(message), this);
            }
        }

        void Internal.ITreeHandleable.MakeReady(Internal.IValueContainer valueContainer, ref ValueLinkedQueue<Internal.ITreeHandleable> handleQueue)
        {
            ((Promise) _valueOrPrevious)._wasWaitedOn = true;
            valueContainer.Retain();
            _valueOrPrevious = valueContainer;
            handleQueue.Push(this);
        }

        void Internal.ITreeHandleable.MakeReadyFromSettled(Internal.IValueContainer valueContainer)
        {
            ((Promise) _valueOrPrevious)._wasWaitedOn = true;
            valueContainer.Retain();
            _valueOrPrevious = valueContainer;
            Internal.AddToHandleQueueBack(this);
        }

        protected virtual void Reset()
        {
            _state = State.Pending;
            _retainCounter = 1;
            SetNotDisposed();
            SetCreatedStacktrace(this, 3);
        }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
        private protected void AddWaiter(Internal.ITreeHandleable waiter)
        {
#else
        protected void AddWaiter(object _waiter)
        {
            Internal.ITreeHandleable waiter = (Internal.ITreeHandleable) _waiter;
#endif
            if (_state == State.Pending)
            {
                _nextBranches.Push(waiter);
            }
            else
            {
                waiter.MakeReadyFromSettled((Internal.IValueContainer) _valueOrPrevious);
            }
        }

        protected void RetainInternal()
        {
#if PROMISE_DEBUG
            // If this fails, change _retainCounter to uint or ulong.
            // Have to directly check ushort since C# compiler doesn't check integer types smaller than Int32...
            if (_retainCounter == ushort.MaxValue)
            {
                throw new OverflowException();
            }
            checked
#endif
            {
                ++_retainCounter;
            }
        }

        protected void ReleaseInternal()
        {
            if (ReleaseWithoutDisposeCheck() == 0)
            {
                Dispose();
            }
        }

        private ushort ReleaseWithoutDisposeCheck()
        {
#if PROMISE_DEBUG
            // This should never fail, but check in debug mode just in case.
            // Have to directly check ushort since C# compiler doesn't check integer types smaller than Int32...
            if (_retainCounter == 0)
            {
                throw new OverflowException();
            }
            checked
#endif
            {
                return --_retainCounter;
            }
        }

        protected virtual void Dispose()
        {
            if (_valueOrPrevious != null)
            {
                if (_wasWaitedOn)
                {
                    ((Internal.IValueContainer) _valueOrPrevious).Release();
                }
                else
                {
                    // Rejection maybe wasn't caught.
                    ((Internal.IValueContainer) _valueOrPrevious).ReleaseAndAddToUnhandledStack();
                }
            }
            _valueOrPrevious = disposedObject;
        }

        private void ResolveInternal(Internal.IValueContainer container)
        {
            _state = State.Resolved;
            container.Retain();
            _valueOrPrevious = container;
            HandleBranches();
            ResolveProgressListeners();

            ReleaseInternal();
        }

        private void RejectOrCancelInternal(Internal.IValueContainer container)
        {
            _state = container.GetState();
            container.Retain();
            _valueOrPrevious = container;
            HandleBranches();
            CancelProgressListeners();

            ReleaseInternal();
        }

        protected void MaybeHookupNewPromise(Promise newPromise)
        {
            // This is called from a Then/Catch/ContinueWith with a valid cancelationToken, which could have been fed an already canceled token.
            if (newPromise._valueOrPrevious == null)
            {
                HookupNewPromise(newPromise);
            }
            else
            {
                Internal.AddToHandleQueueBack(newPromise);
            }
        }

        protected void HookupNewPromise(Promise newPromise)
        {
            newPromise._valueOrPrevious = this;
            SetDepth(newPromise);
            AddWaiter(newPromise);
        }

        void Internal.ITreeHandleable.Handle()
        {
            Internal.IValueContainer container = (Internal.IValueContainer) _valueOrPrevious;
            _valueOrPrevious = null;
            SetCurrentInvoker(this);
            try
            {
                Execute(container);
                container.Release();
            }
            catch (RethrowException)
            {
                _state = container.GetState();
                _valueOrPrevious = container;
                HandleBranches();
                CancelProgressListeners();
                ReleaseInternal();
            }
            catch (OperationCanceledException e)
            {
                container.Release();
                RejectOrCancelInternal(Internal.CreateCancelContainer(ref e));
            }
            catch (Exception e)
            {
                container.Release();
                RejectOrCancelInternal(Internal.CreateRejectContainer(ref e, int.MinValue, this));
            }
            finally
            {
                Internal.invokingResolved = false;
                Internal.invokingRejected = false;
                ClearCurrentInvoker();
            }
        }

        private void ResolveDirect()
        {
            _state = State.Resolved;
            var resolveValue = Internal.ResolveContainerVoid.GetOrCreate();
            _valueOrPrevious = resolveValue;
            AddBranchesToHandleQueueBack(resolveValue);
            ResolveProgressListeners();
            Internal.AddToHandleQueueFront(this);
        }

        protected void ResolveDirect<T>(ref T value)
        {
            _state = State.Resolved;
            var resolveValue = Internal.ResolveContainer<T>.GetOrCreate(ref value);
            resolveValue.Retain();
            _valueOrPrevious = resolveValue;
            AddBranchesToHandleQueueBack(resolveValue);
            ResolveProgressListeners();
            Internal.AddToHandleQueueFront(this);
        }

        private void RejectDirect<TReject>(ref TReject reason, int rejectSkipFrames)
        {
            _state = State.Rejected;
            var rejection = Internal.CreateRejectContainer(ref reason, rejectSkipFrames + 1, this);
            rejection.Retain();
            _valueOrPrevious = rejection;
            AddBranchesToHandleQueueBack(rejection);
            CancelProgressListeners();
            Internal.AddToHandleQueueFront(this);
        }

        private void HandleSelf(Internal.IValueContainer valueContainer)
        {
            _state = valueContainer.GetState();
            valueContainer.Retain();
            _valueOrPrevious = valueContainer;

            HandleBranches();
            if (_state == State.Resolved)
            {
                ResolveProgressListeners();
            }
            else
            {
                CancelProgressListeners();
            }

            ReleaseInternal();
        }

        // Annoyingly necessary since private protected isn't available in old C# versions.
#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
        private protected virtual void Execute(Internal.IValueContainer valueContainer) { }
#else
        protected virtual void Execute(object valueContainer) { }
#endif

        private void HandleBranches()
        {
            var valueContainer = (Internal.IValueContainer) _valueOrPrevious;
            ValueLinkedQueue<Internal.ITreeHandleable> handleQueue = new ValueLinkedQueue<Internal.ITreeHandleable>();
            while (_nextBranches.IsNotEmpty)
            {
                _nextBranches.Pop().MakeReady(valueContainer, ref handleQueue);
            }
            Internal.AddToHandleQueueFront(ref handleQueue);
        }

        private void AddBranchesToHandleQueueBack(Internal.IValueContainer valueContainer)
        {
            ValueLinkedQueue<Internal.ITreeHandleable> handleQueue = new ValueLinkedQueue<Internal.ITreeHandleable>();
            while (_nextBranches.IsNotEmpty)
            {
                _nextBranches.Pop().MakeReady(valueContainer, ref handleQueue);
            }
            Internal.AddToHandleQueueBack(ref handleQueue);
        }

        protected static partial class InternalProtected
        {
            // PromiseIntermediate is annoyingly necessary since private protected isn't available in old C# versions.
            [System.Diagnostics.DebuggerNonUserCode]
            internal abstract partial class PromiseIntermediate : Promise
            {
#if !CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                protected override sealed void Execute(object valueContainer)
                {
                    Execute((Internal.IValueContainer) valueContainer);
                }

                protected abstract void Execute(Internal.IValueContainer valueContainer);
#endif
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal abstract partial class PromiseIntermediate<T> : Promise<T>
            {
#if !CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                protected override sealed void Execute(object valueContainer)
                {
                    Execute((Internal.IValueContainer) valueContainer);
                }

                protected abstract void Execute(Internal.IValueContainer valueContainer);
#endif
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal abstract partial class PromiseWaitPromise : PromiseIntermediate
            {
                public void WaitFor(Promise other)
                {
                    ValidateReturn(other);
                    _valueOrPrevious = other;
#if PROMISE_PROGRESS
                    _secondPrevious = true;
                    if (_progressListeners.IsNotEmpty)
                    {
                        SubscribeProgressToBranchesAndRoots(other, this);
                    }
#endif
                    other.AddWaiter(this);
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal abstract partial class PromiseWaitPromise<T> : PromiseIntermediate<T>
            {
                public void WaitFor(Promise<T> other)
                {
                    ValidateReturn(other);
                    _valueOrPrevious = other;
#if PROMISE_PROGRESS
                    _secondPrevious = true;
                    if (_progressListeners.IsNotEmpty)
                    {
                        SubscribeProgressToBranchesAndRoots(other, this);
                    }
#endif
                    other.AddWaiter(this);
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed partial class DeferredPromise0 : Promise, Internal.ITreeHandleable
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static DeferredPromise0()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public readonly DeferredInternal0 deferred;

                private DeferredPromise0()
                {
                    deferred = new DeferredInternal0(this);
                }

                public static DeferredPromise0 GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (DeferredPromise0) _pool.Pop() : new DeferredPromise0();
                    promise.Reset();
                    promise.ResetDepth();
                    return promise;
                }

                void Internal.ITreeHandleable.Handle()
                {
                    ReleaseInternal();
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed partial class DeferredPromise<T> : Promise<T>, Internal.ITreeHandleable
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static DeferredPromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public readonly DeferredInternal<T> deferred;

                private DeferredPromise()
                {
                    deferred = new DeferredInternal<T>(this);
                }

                public static DeferredPromise<T> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (DeferredPromise<T>) _pool.Pop() : new DeferredPromise<T>();
                    promise.Reset();
                    promise.ResetDepth();
                    return promise;
                }

                void Internal.ITreeHandleable.Handle()
                {
                    ReleaseInternal();
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class SettledPromise : Promise
            {
                private SettledPromise() { }

#if PROMISE_DEBUG
                public static Promise GetOrCreateResolved()
                {
                    // Create new because stack trace can be different.
                    var promise = LitePromise0.GetOrCreate();
                    promise.ResolveDirectFromSettled();
                    return promise;
                }

                public static Promise GetOrCreateCanceled()
                {
                    // Create new because stack trace can be different.
                    var promise = LitePromise0.GetOrCreate();
                    promise.CancelDirect();
                    return promise;
                }
#else
                private static readonly SettledPromise _resolved = new SettledPromise()
                {
                    _state = State.Resolved,
                    _valueOrPrevious = Internal.ResolveContainerVoid.GetOrCreate()
                };

                private static readonly SettledPromise _canceled = new SettledPromise()
                {
                    _state = State.Canceled,
                    _valueOrPrevious = Internal.CancelContainerVoid.GetOrCreate()
                };

                public static Promise GetOrCreateResolved()
                {
                    // Reuse a single resolved instance.
                    return _resolved;
                }

                public static Promise GetOrCreateCanceled()
                {
                    // Reuse a single canceled instance.
                    return _canceled;
                }
#endif

                protected override void Dispose() { }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class LitePromise0 : Promise, Internal.ITreeHandleable
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static LitePromise0()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                private LitePromise0() { }

                public static LitePromise0 GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (LitePromise0) _pool.Pop() : new LitePromise0();
                    promise.Reset();
                    promise.ResetDepth();
                    return promise;
                }

#if PROMISE_DEBUG
                public void ResolveDirectFromSettled()
                {
                    _state = State.Resolved;
                    _valueOrPrevious = Internal.ResolveContainerVoid.GetOrCreate();
                    Internal.AddToHandleQueueFront(this);
                }
#endif

                void Internal.ITreeHandleable.Handle()
                {
                    ReleaseInternal();
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class LitePromise<T> : Promise<T>, Internal.ITreeHandleable
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static LitePromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                private LitePromise() { }

                public static LitePromise<T> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (LitePromise<T>) _pool.Pop() : new LitePromise<T>();
                    promise.Reset();
                    promise.ResetDepth();
                    return promise;
                }

                public void ResolveDirect(ref T value)
                {
                    _state = State.Resolved;
                    var val = Internal.ResolveContainer<T>.GetOrCreate(ref value);
                    val.Retain();
                    _valueOrPrevious = val;
                    Internal.AddToHandleQueueFront(this);
                }

                void Internal.ITreeHandleable.Handle()
                {
                    ReleaseInternal();
                }
            }

            #region Resolve Promises
            // IDelegate to reduce the amount of classes I would have to write(Composition Over Inheritance).
            // Using generics with constraints allows us to use structs to get composition for "free"
            // (no extra object allocation or extra memory overhead, and the compiler will generate the Promise classes for us).
            // The only downside is that more classes are created than if we just used straight interfaces (not a problem with JIT, but makes the code size larger with AOT).

            // Resolve types for more common .Then(onResolved) calls to be more efficient.
            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolve<TResolver> : PromiseIntermediate where TResolver : IDelegateResolve
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolve()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;

                private PromiseResolve() { }

                public static PromiseResolve<TResolver> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolve<TResolver>) _pool.Pop() : new PromiseResolve<TResolver>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    if (valueContainer.GetState() == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                    }
                    else
                    {
                        resolveCallback.MaybeUnregisterCancelation();
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolve<T, TResolver> : PromiseIntermediate<T> where TResolver : IDelegateResolve
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolve()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;

                private PromiseResolve() { }

                public static PromiseResolve<T, TResolver> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolve<T, TResolver>) _pool.Pop() : new PromiseResolve<T, TResolver>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    if (valueContainer.GetState() == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                    }
                    else
                    {
                        resolveCallback.MaybeUnregisterCancelation();
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolvePromise<TResolver> : PromiseWaitPromise where TResolver : IDelegateResolvePromise
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolvePromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;

                private PromiseResolvePromise() { }

                public static PromiseResolvePromise<TResolver> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolvePromise<TResolver>) _pool.Pop() : new PromiseResolvePromise<TResolver>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (resolver.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    if (valueContainer.GetState() == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                    }
                    else
                    {
                        resolver.MaybeUnregisterCancelation();
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolvePromise<T, TResolver> : PromiseWaitPromise<T> where TResolver : IDelegateResolvePromise
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolvePromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;

                private PromiseResolvePromise() { }

                public static PromiseResolvePromise<T, TResolver> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolvePromise<T, TResolver>) _pool.Pop() : new PromiseResolvePromise<T, TResolver>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (resolver.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    if (valueContainer.GetState() == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                    }
                    else
                    {
                        resolver.MaybeUnregisterCancelation();
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }
            #endregion

            #region Resolve or Reject Promises
            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolveReject<TResolver, TRejecter> : PromiseIntermediate where TResolver : IDelegateResolve where TRejecter : IDelegateReject
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolveReject()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;
                public TRejecter rejecter;

                private PromiseResolveReject() { }

                public static PromiseResolveReject<TResolver, TRejecter> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolveReject<TResolver, TRejecter>) _pool.Pop() : new PromiseResolveReject<TResolver, TRejecter>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    var rejectCallback = rejecter;
                    rejecter = default(TRejecter);
                    State state = valueContainer.GetState();
                    if (state == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                        return;
                    }
                    resolveCallback.MaybeUnregisterCancelation();
                    if (state == State.Rejected)
                    {
                        Internal.invokingRejected = true;
                        rejectCallback.InvokeRejecter(valueContainer, this);
                    }
                    else
                    {
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolveReject<T, TResolver, TRejecter> : PromiseIntermediate<T> where TResolver : IDelegateResolve where TRejecter : IDelegateReject
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolveReject()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;
                public TRejecter rejecter;

                private PromiseResolveReject() { }

                public static PromiseResolveReject<T, TResolver, TRejecter> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolveReject<T, TResolver, TRejecter>) _pool.Pop() : new PromiseResolveReject<T, TResolver, TRejecter>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    var rejectCallback = rejecter;
                    rejecter = default(TRejecter);
                    State state = valueContainer.GetState();
                    if (state == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                        return;
                    }
                    resolveCallback.MaybeUnregisterCancelation();
                    if (state == State.Rejected)
                    {
                        Internal.invokingRejected = true;
                        rejectCallback.InvokeRejecter(valueContainer, this);
                    }
                    else
                    {
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolveRejectPromise<TResolver, TRejecter> : PromiseWaitPromise where TResolver : IDelegateResolvePromise where TRejecter : IDelegateRejectPromise
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolveRejectPromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;
                public TRejecter rejecter;

                private PromiseResolveRejectPromise() { }

                public static PromiseResolveRejectPromise<TResolver, TRejecter> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolveRejectPromise<TResolver, TRejecter>) _pool.Pop() : new PromiseResolveRejectPromise<TResolver, TRejecter>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (resolver.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    var rejectCallback = rejecter;
                    rejecter = default(TRejecter);
                    State state = valueContainer.GetState();
                    if (state == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                        return;
                    }
                    resolveCallback.MaybeUnregisterCancelation();
#if PROMISE_PROGRESS
                    _suspended = true;
#endif
                    if (state == State.Rejected)
                    {
                        Internal.invokingRejected = true;
                        rejectCallback.InvokeRejecter(valueContainer, this);
                    }
                    else
                    {
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseResolveRejectPromise<TPromise, TResolver, TRejecter> : PromiseWaitPromise<TPromise> where TResolver : IDelegateResolvePromise where TRejecter : IDelegateRejectPromise
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseResolveRejectPromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TResolver resolver;
                public TRejecter rejecter;

                private PromiseResolveRejectPromise() { }

                public static PromiseResolveRejectPromise<TPromise, TResolver, TRejecter> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseResolveRejectPromise<TPromise, TResolver, TRejecter>) _pool.Pop() : new PromiseResolveRejectPromise<TPromise, TResolver, TRejecter>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (resolver.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var resolveCallback = resolver;
                    resolver = default(TResolver);
                    var rejectCallback = rejecter;
                    rejecter = default(TRejecter);
                    State state = valueContainer.GetState();
                    if (state == State.Resolved)
                    {
                        Internal.invokingResolved = true;
                        resolveCallback.InvokeResolver(valueContainer, this);
                        return;
                    }
                    resolveCallback.MaybeUnregisterCancelation();
#if PROMISE_PROGRESS
                    _suspended = true;
#endif
                    if (state == State.Rejected)
                    {
                        Internal.invokingRejected = true;
                        rejectCallback.InvokeRejecter(valueContainer, this);
                    }
                    else
                    {
                        RejectOrCancelInternal(valueContainer);
                    }
                }
            }
            #endregion

            #region Continue Promises
            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseContinue<TContinuer> : PromiseIntermediate where TContinuer : IDelegateContinue
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseContinue()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TContinuer continuer;

                private PromiseContinue() { }

                public static PromiseContinue<TContinuer> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseContinue<TContinuer>) _pool.Pop() : new PromiseContinue<TContinuer>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var callback = continuer;
                    continuer = default(TContinuer);
                    Internal.invokingResolved = true;
                    callback.Invoke(valueContainer);
                    ResolveInternal(Internal.ResolveContainerVoid.GetOrCreate());
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseContinue<TResult, TContinuer> : PromiseIntermediate<TResult> where TContinuer : IDelegateContinue<TResult>
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseContinue()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TContinuer continuer;

                private PromiseContinue() { }

                public static PromiseContinue<TResult, TContinuer> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseContinue<TResult, TContinuer>) _pool.Pop() : new PromiseContinue<TResult, TContinuer>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    var callback = continuer;
                    continuer = default(TContinuer);
                    Internal.invokingResolved = true;
                    TResult result = callback.Invoke(valueContainer);
                    ResolveInternal(Internal.ResolveContainer<TResult>.GetOrCreate(ref result));
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseContinuePromise<TContinuer> : PromiseWaitPromise where TContinuer : IDelegateContinue<Promise>
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseContinuePromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TContinuer continuer;

                private PromiseContinuePromise() { }

                public static PromiseContinuePromise<TContinuer> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseContinuePromise<TContinuer>) _pool.Pop() : new PromiseContinuePromise<TContinuer>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (continuer.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var callback = continuer;
                    continuer = default(TContinuer);
                    Internal.invokingResolved = true;
                    Promise result = callback.Invoke(valueContainer);
                    WaitFor(result);
                }
            }

            [System.Diagnostics.DebuggerNonUserCode]
            internal sealed class PromiseContinuePromise<TPromise, TContinuer> : PromiseWaitPromise<TPromise> where TContinuer : IDelegateContinue<Promise<TPromise>>
            {
                private static ValueLinkedStack<Internal.ITreeHandleable> _pool;

                static PromiseContinuePromise()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                protected override void Dispose()
                {
                    base.Dispose();
                    if (Config.ObjectPooling == PoolType.All)
                    {
                        _pool.Push(this);
                    }
                }

                public TContinuer continuer;

                private PromiseContinuePromise() { }

                public static PromiseContinuePromise<TPromise, TContinuer> GetOrCreate()
                {
                    var promise = _pool.IsNotEmpty ? (PromiseContinuePromise<TPromise, TContinuer>) _pool.Pop() : new PromiseContinuePromise<TPromise, TContinuer>();
                    promise.Reset();
                    return promise;
                }

#if CSHARP_7_3_OR_NEWER // Really C# 7.2 but this is the closest symbol Unity offers.
                private
#endif
                protected override void Execute(Internal.IValueContainer valueContainer)
                {
                    if (continuer.IsNull)
                    {
                        // The returned promise is handling this.
                        HandleSelf(valueContainer);
                        return;
                    }

                    var callback = continuer;
                    continuer = default(TContinuer);
                    Internal.invokingResolved = true;
                    Promise<TPromise> result = callback.Invoke(valueContainer);
                    WaitFor(result);
                }
            }
            #endregion

            [System.Diagnostics.DebuggerNonUserCode]
            public sealed partial class PromisePassThrough : Internal.ITreeHandleable, IRetainable, ILinked<PromisePassThrough>
            {
                private static ValueLinkedStack<PromisePassThrough> _pool;

                static PromisePassThrough()
                {
                    Internal.OnClearPool += () => _pool.Clear();
                }

                Internal.ITreeHandleable ILinked<Internal.ITreeHandleable>.Next { get; set; }
                PromisePassThrough ILinked<PromisePassThrough>.Next { get; set; }

                public Promise Owner { get; private set; }
                internal IMultiTreeHandleable Target { get; private set; }

                private int _index;
                private uint _retainCounter;

                public static PromisePassThrough GetOrCreate(Promise owner, int index)
                {
                    ValidateElementNotNull(owner, "promises", "A promise was null", 2);
                    ValidateOperation(owner, 2);

                    var passThrough = _pool.IsNotEmpty ? _pool.Pop() : new PromisePassThrough();
                    passThrough.Owner = owner;
                    passThrough._index = index;
                    passThrough._retainCounter = 1u;
                    return passThrough;
                }

                private PromisePassThrough() { }

                internal void SetTargetAndAddToOwner(IMultiTreeHandleable target)
                {
                    Target = target;
                    Owner.AddWaiter(this);
                }

                void Internal.ITreeHandleable.MakeReady(Internal.IValueContainer valueContainer, ref ValueLinkedQueue<Internal.ITreeHandleable> handleQueue)
                {
                    var temp = Target;
                    if (temp.Handle(valueContainer, Owner, _index))
                    {
                        handleQueue.Push(temp);
                    }
                }

                void Internal.ITreeHandleable.MakeReadyFromSettled(Internal.IValueContainer valueContainer)
                {
                    var temp = Target;
                    if (temp.Handle(valueContainer, Owner, _index))
                    {
                        Internal.AddToHandleQueueBack(temp);
                    }
                }

                public void Retain()
                {
                    ++_retainCounter;
                }

                public void Release()
                {
#if PROMISE_DEBUG
                    checked
#endif
                    {
                        if (--_retainCounter == 0)
                        {
                            Owner = null;
                            Target = null;
                            if (Config.ObjectPooling != PoolType.None)
                            {
                                _pool.Push(this);
                            }
                        }
                    }
                }

                void Internal.ITreeHandleable.Handle() { throw new System.InvalidOperationException(); }
            }

            internal static ValueLinkedStack<PromisePassThrough> WrapInPassThroughs<TEnumerator>(TEnumerator promises, out int count) where TEnumerator : IEnumerator<Promise>
            {
                // Assumes promises.MoveNext() was already called once before this.
                int index = 0;
                var passThroughs = new ValueLinkedStack<PromisePassThrough>(PromisePassThrough.GetOrCreate(promises.Current, index));
                while (promises.MoveNext())
                {
                    passThroughs.Push(PromisePassThrough.GetOrCreate(promises.Current, ++index));
                }
                count = index + 1;
                return passThroughs;
            }

#pragma warning disable RECS0096 // Type parameter is never used
            internal static ValueLinkedStack<PromisePassThrough> WrapInPassThroughs<T, TEnumerator>(TEnumerator promises, out int count) where TEnumerator : IEnumerator<Promise<T>>
#pragma warning restore RECS0096 // Type parameter is never used
            {
                // Assumes promises.MoveNext() was already called once before this.
                int index = 0;
                var passThroughs = new ValueLinkedStack<PromisePassThrough>(PromisePassThrough.GetOrCreate(promises.Current, index));
                while (promises.MoveNext())
                {
                    passThroughs.Push(PromisePassThrough.GetOrCreate(promises.Current, ++index));
                }
                count = index + 1;
                return passThroughs;
            }
        }
    }
}