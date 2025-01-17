﻿#if UNITY_5_5 || NET_2_0 || NET_2_0_SUBSET
#define NET_LEGACY
#endif

#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0074 // Use compound assignment
#pragma warning disable IDE0250 // Make struct 'readonly'
#pragma warning disable 0420 // A reference to a volatile field will not be treated as volatile

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Proto.Promises
{
    internal static partial class Internal
    {
#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal struct CancelDelegateTokenVoid : ICancelable
        {
            private readonly Action _callback;

            [MethodImpl(InlineOption)]
            internal CancelDelegateTokenVoid(Action callback)
            {
                _callback = callback;
            }

            [MethodImpl(InlineOption)]
            public void Cancel()
            {
                _callback.Invoke();
            }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal struct CancelDelegateToken<TCapture> : ICancelable
        {
            private readonly TCapture _capturedValue;
            private readonly Action<TCapture> _callback;

            [MethodImpl(InlineOption)]
            internal CancelDelegateToken(
#if CSHARP_7_3_OR_NEWER
                in
#endif
                TCapture capturedValue, Action<TCapture> callback)
            {
                _capturedValue = capturedValue;
                _callback = callback;
            }

            [MethodImpl(InlineOption)]
            public void Cancel()
            {
                _callback.Invoke(_capturedValue);
            }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [DebuggerNonUserCode, StackTraceHidden]
#endif
        internal sealed partial class CancelationRef : HandleablePromiseBase, ICancelable, ITraceable
        {
            internal static readonly CancelationRef s_canceledSentinel;

#if PROMISE_DEBUG
            CausalityTrace ITraceable.Trace { get; set; }
#endif

            static CancelationRef()
            {
                // Set _userRetainIncrementor to 0 so _userRetainCounter will never overflow.
                s_canceledSentinel = new CancelationRef(0) { _state = State.CanceledComplete, _internalRetainCounter = 1, _tokenId = -1 };
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
                // If we don't suppress, the finalizer can run when the AppDomain is unloaded, causing a NullReferenceException. This happens in Unity when switching between editmode and playmode.
                GC.SuppressFinalize(s_canceledSentinel);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
            }

            private CancelationRef() : this(1) { }

            private CancelationRef(byte userRetainIncrementor)
            {
                _userRetainIncrementor = userRetainIncrementor;
            }

            ~CancelationRef()
            {
                try
                {
                    if (_userRetainCounter > 0)
                    {
                        // CancelationToken wasn't released.
                        string message = "A CancelationToken's resources were garbage collected without being released. You must release all IRetainable objects that you have retained.";
                        ReportRejection(new UnreleasedObjectException(message), this);
                    }
                    // We don't check the disposed state if this was linked to a System.Threading.CancellationToken.
                    if (!_linkedToBclToken & _state != State.Disposed)
                    {
                        // CancelationSource wasn't disposed.
                        ReportRejection(new UnreleasedObjectException("CancelationSource's resources were garbage collected without being disposed."), this);
                    }
                }
                catch (Exception e)
                {
                    // This should never happen.
                    ReportRejection(e, this);
                }
            }

            internal enum State : byte
            {
                Pending,
                Disposed,
                Canceled,
                CanceledComplete,
            }

            // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
            // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
            internal struct SmallFields
            {
                // Semi-unique instance id helps prevent accidents in case a CancelationRegistration is torn. This doesn't need to be fool-proof.
                private static int s_idCounter;

                // Must not be readonly.
                internal SpinLocker _locker;
                internal readonly int _instanceId;

                private SmallFields(int instanceId)
                {
                    _locker = new SpinLocker();
                    _instanceId = instanceId;
                }

                internal static SmallFields Create()
                {
                    return new SmallFields(Interlocked.Increment(ref s_idCounter));
                }
            }

#if NET6_0_OR_GREATER
            // Used to prevent a deadlock from synchronous invoke.
            [ThreadStatic]
            private static bool ts_isLinkingToBclToken;
#endif
            internal Thread _executingThread;
            private ValueLinkedStackZeroGC<CancelationRegistration> _links = ValueLinkedStackZeroGC<CancelationRegistration>.Create();
            // Use a sentinel for the linked list so we don't need to null check.
            private readonly CancelationCallbackNode _registeredCallbacksHead = CancelationCallbackNode.CreateLinkedListSentinel();
            // This must not be readonly since the SpinLocker is mutable.
            internal SmallFields _smallFields = SmallFields.Create();
            // Start with Id 1 instead of 0 to reduce risk of false positives.
            volatile private int _sourceId = 1;
            volatile private int _tokenId = 1;
            private uint _userRetainCounter;
            private readonly byte _userRetainIncrementor; // 0 for s_canceledSentinel, 1 for all others.
            private byte _internalRetainCounter;
            internal bool _linkedToBclToken;
            volatile internal State _state;

            internal int SourceId
            {
                [MethodImpl(InlineOption)]
                get { return _sourceId; }
            }
            internal int TokenId
            {
                [MethodImpl(InlineOption)]
                get { return _tokenId; }
            }

            [MethodImpl(InlineOption)]
            private void Initialize(bool linkedToBclToken)
            {
                _internalRetainCounter = 1; // 1 for Dispose.
                _linkedToBclToken = linkedToBclToken;
                _state = State.Pending;
                SetCreatedStacktrace(this, 2);
            }

            [MethodImpl(InlineOption)]
            private static CancelationRef GetFromPoolOrCreate()
            {
                var obj = ObjectPool.TryTakeOrInvalid<CancelationRef>();
                return obj == PromiseRefBase.InvalidAwaitSentinel.s_instance
                    ? new CancelationRef()
                    : obj.UnsafeAs<CancelationRef>();
            }

            [MethodImpl(InlineOption)]
            internal static CancelationRef GetOrCreate()
            {
                var cancelRef = GetFromPoolOrCreate();
                cancelRef._next = null;
                cancelRef.Initialize(false);
                return cancelRef;
            }

            [MethodImpl(InlineOption)]
            internal static bool IsValidSource(CancelationRef _this, int sourceId)
            {
                return _this != null && _this.SourceId == sourceId;
            }

            [MethodImpl(InlineOption)]
            internal static bool IsSourceCanceled(CancelationRef _this, int sourceId)
            {
                return _this != null && _this.IsSourceCanceled(sourceId);
            }

            [MethodImpl(InlineOption)]
            private bool IsSourceCanceled(int sourceId)
            {
                return sourceId == SourceId & _state >= State.Canceled;
            }

            [MethodImpl(InlineOption)]
            internal static bool CanTokenBeCanceled(CancelationRef _this, int tokenId)
            {
                return _this != null && (_this.TokenId == tokenId & _this._state != State.Disposed);
            }

            [MethodImpl(InlineOption)]
            internal static bool IsTokenCanceled(CancelationRef _this, int tokenId)
            {
                return _this != null && _this.IsTokenCanceled(tokenId);
            }

            [MethodImpl(InlineOption)]
            private bool IsTokenCanceled(int tokenId)
            {
                return tokenId == TokenId & (_state >= State.Canceled
                    // TODO: Unity hasn't adopted .Net 6+ yet, and they usually use different compilation symbols than .Net SDK, so we'll have to update the compilation symbols here once Unity finally does adopt it.
#if NET6_0_OR_GREATER
                    // This is only necessary in .Net 6 or later, since `CancellationTokenSource.TryReset()` was added.
                    | (_linkedToBclToken && _bclSource.IsCancellationRequested)
#endif
                    );
            }

            internal void MaybeLinkToken(CancelationToken token)
            {
                CancelationRegistration linkedRegistration;
                if (token.TryRegister(this, out linkedRegistration))
                {
                    _smallFields._locker.Enter();
                    // Register may have invoked Cancel synchronously or on another thread, so we check the state here before adding the registration for later unlinking.
                    if (_state == State.Pending)
                    {
                        _links.Push(linkedRegistration);
                    }
                    _smallFields._locker.Exit();
                }
            }

            [MethodImpl(InlineOption)]
            internal static bool TryRegister<TCancelable>(CancelationRef _this, int tokenId,
#if CSHARP_7_3_OR_NEWER
                in
#endif
                TCancelable cancelable, out CancelationRegistration registration) where TCancelable : ICancelable
            {
                if (_this == null)
                {
                    registration = default(CancelationRegistration);
                    return false;
                }
                return _this.TryRegister(cancelable, tokenId, out registration);
            }

            [MethodImpl(InlineOption)]
            private bool TryRegister<TCancelable>(
#if CSHARP_7_3_OR_NEWER
                in
#endif
                TCancelable cancelable, int tokenId, out CancelationRegistration registration) where TCancelable : ICancelable
            {
                _smallFields._locker.Enter();
                State state = _state;
                bool isTokenMatched = tokenId == TokenId;
                if (!isTokenMatched | state != State.Pending)
                {
                    _smallFields._locker.Exit();
                    registration = default(CancelationRegistration);
                    if (isTokenMatched & state >= State.Canceled)
                    {
                        cancelable.Cancel();
                        return true;
                    }
                    return false;
                }

                // TODO: Unity hasn't adopted .Net 6+ yet, and they usually use different compilation symbols than .Net SDK, so we'll have to update the compilation symbols here once Unity finally does adopt it.
#if NET6_0_OR_GREATER
                // This is only necessary in .Net 6 or later, since `CancellationTokenSource.TryReset()` was added.
                if (_linkedToBclToken)
                {
                    System.Threading.CancellationToken token;
                    // If the source was disposed, the Token property will throw ObjectDisposedException. Unfortunately, this is the only way to check if it's disposed.
                    try
                    {
                        token = _bclSource.Token;
                    }
                    catch (ObjectDisposedException)
                    {
                        int sourceId = SourceId;
                        bool isCanceled = _bclSource.IsCancellationRequested;
                        if (isCanceled)
                        {
                            _smallFields._locker.Exit();
                            cancelable.Cancel();
                        }
                        else
                        {
                            UnregisterAll();
                            _smallFields._locker.Exit();
                        }
                        registration = default(CancelationRegistration);
                        return isCanceled;
                    }

                    // If we are unable to unregister, it means the source had TryReset() called on it, or the token was canceled on another thread (and the other thread may be waiting on the lock).
                    if (!_bclRegistration.Unregister())
                    {
                        if (token.IsCancellationRequested)
                        {
                            _smallFields._locker.Exit();
                            cancelable.Cancel();
                            registration = default(CancelationRegistration);
                            return true;
                        }
                        UnregisterAll();
                    }
                    // Callback could be invoked synchronously if the token is canceled on another thread,
                    // so we set a flag to prevent a deadlock, then check the flag again after the hookup to see if it was invoked.
                    ts_isLinkingToBclToken = true;
                    _bclRegistration = token.Register(state =>
                    {
                        // This could be invoked synchronously if the token is canceled, so we check the flag to prevent a deadlock.
                        if (ts_isLinkingToBclToken)
                        {
                            // Reset the flag so that we can tell that this was invoked synchronously.
                            ts_isLinkingToBclToken = false;
                            return;
                        }
                        state.UnsafeAs<CancelationRef>().Cancel();
                    }, this, false);

                    if (!ts_isLinkingToBclToken)
                    {
                        // Hook up the node instead of invoking since it might throw, and we need all registered callbacks to be invoked.
                        var node = CallbackNodeImpl<TCancelable>.GetOrCreate(cancelable, this);
                        int oldNodeId = node.NodeId;
                        _registeredCallbacksHead.InsertPrevious(node);

                        InvokeCallbacksAlreadyLocked();
                        registration = new CancelationRegistration(this, node, oldNodeId, tokenId);
                        return true;
                    }
                    ts_isLinkingToBclToken = false;
                }
#endif

                {
                    var node = CallbackNodeImpl<TCancelable>.GetOrCreate(cancelable, this);
                    int oldNodeId = node.NodeId;
                    _registeredCallbacksHead.InsertPrevious(node);
                    _smallFields._locker.Exit();
                    registration = new CancelationRegistration(this, node, oldNodeId, tokenId);
                    return true;
                }
            }

            [MethodImpl(InlineOption)]
            internal static bool TrySetCanceled(CancelationRef _this, int sourceId)
            {
                return _this != null && _this.TrySetCanceled(sourceId);
            }

            [MethodImpl(InlineOption)]
            private bool TrySetCanceled(int sourceId)
            {
                _smallFields._locker.Enter();
                if (sourceId != SourceId | _state != State.Pending)
                {
                    _smallFields._locker.Exit();
                    return false;
                }
                InvokeCallbacksAlreadyLocked();
                return true;
            }

            private void InvokeCallbacksAlreadyLocked()
            {
                ThrowIfInPool(this);

                _executingThread = Thread.CurrentThread;
                _state = State.Canceled;
                ++_internalRetainCounter;
                _smallFields._locker.Exit();

                Unlink();

                // We call the delegates in LIFO order so that callbacks fire 'deepest first'.
                // This is intended to help with nesting scenarios so that child enlisters cancel before their parents.

                List<Exception> exceptions = null;
                while (true)
                {
                    _smallFields._locker.Enter();
                    // If the sentinel's previous points to itself, no more registrations exist.
                    var current = _registeredCallbacksHead._previous;
                    if (current == _registeredCallbacksHead)
                    {
                        _smallFields._locker.Exit();
                        break;
                    }
                    current.RemoveFromLinkedList();
                    _smallFields._locker.Exit();

                    try
                    {
                        current.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(e);
                    }
                }

                _executingThread = null;
                _state = State.CanceledComplete;
                MaybeResetAndRepool();
                if (exceptions != null)
                {
                    // Propagate exceptions to caller as aggregate.
                    throw new AggregateException(exceptions);
                }
            }

            [MethodImpl(InlineOption)]
            internal static bool TryDispose(CancelationRef _this, int sourceId)
            {
                return _this != null && _this.TryDispose(sourceId);
            }

            [MethodImpl(InlineOption)]
            internal bool TryDispose(int sourceId)
            {
                _smallFields._locker.Enter();
                if (sourceId != SourceId)
                {
                    _smallFields._locker.Exit();
                    return false;
                }

                unchecked
                {
                    ++_sourceId;
                }
                if (_state != State.Pending)
                {
                    MaybeResetAndRepoolAlreadyLocked();
                    return true;
                }

                ThrowIfInPool(this);
                _state = State.Disposed;
                Unlink();
                UnregisterAll();

                MaybeResetAndRepoolAlreadyLocked();
                return true;
            }

            private void UnregisterAll()
            {
                var previous = _registeredCallbacksHead._previous;
                // If the previous references itself, then it is the sentinel and no registrations exist.
                if (previous == _registeredCallbacksHead)
                {
                    return;
                }
                // Set the last node's previous to null since null check is faster than reference comparison.
                _registeredCallbacksHead._next.UnsafeAs<CancelationCallbackNode>()._previous = null;
                _registeredCallbacksHead.ResetSentinel();
                do
                {
                    var current = previous;
                    previous = current._previous;
                    current._previous = null;
                    current.Dispose();
                } while (previous != null);
            }

            private void Unlink()
            {
                while (_links.IsNotEmpty)
                {
                    _links.Pop().TryUnregister();
                }
            }

            [MethodImpl(InlineOption)]
            internal static bool TryRetainUser(CancelationRef _this, int tokenId)
            {
                return _this != null && _this.TryRetainUser(tokenId);
            }

            [MethodImpl(InlineOption)]
            private bool TryRetainUser(int tokenId)
            {
                _smallFields._locker.Enter();
                if (tokenId != TokenId | _state == State.Disposed)
                {
                    _smallFields._locker.Exit();
                    return false;
                }

                ThrowIfInPool(this);
                checked
                {
                    _userRetainCounter += _userRetainIncrementor;
                }
                _smallFields._locker.Exit();
                return true;
            }

            [MethodImpl(InlineOption)]
            internal static bool TryReleaseUser(CancelationRef _this, int tokenId)
            {
                return _this != null && _this.TryReleaseUser(tokenId);
            }

            [MethodImpl(InlineOption)]
            private bool TryReleaseUser(int tokenId)
            {
                _smallFields._locker.Enter();
                if (tokenId != TokenId)
                {
                    _smallFields._locker.Exit();
                    return false;
                }
                checked
                {
                    if ((_userRetainCounter -= _userRetainIncrementor) == 0 & _internalRetainCounter == 0)
                    {
                        unchecked
                        {
                            ++_tokenId;
                        }
                        _smallFields._locker.Exit();
                        ResetAndRepool();
                        return true;
                    }
                }
                _smallFields._locker.Exit();
                return true;
            }

            private void MaybeResetAndRepool()
            {
                _smallFields._locker.Enter();
                MaybeResetAndRepoolAlreadyLocked();
            }

            private void MaybeResetAndRepoolAlreadyLocked()
            {
                if (--_internalRetainCounter == 0 & _userRetainCounter == 0)
                {
#if !NET_LEGACY || NET40
                    if (_bclSource != null)
                    {
                        CancelationConverter.DetachCancelationRef(_bclSource);
                        // We should only dispose the source if we were the one that created it.
                        if (!_linkedToBclToken)
                        {
                            // TODO: We can call _cancellationTokenSource.TryReset() in .Net 6+ instead of always creating a new one.
                            // But this should only be done if we add a TryReset() API to our own CancelationSource, because if a user still holds an old token after this is reused, it could have cancelations triggered unexpectedly.
                            _bclSource.Dispose();
                        }
                        _bclSource = null;
                        Thread.MemoryBarrier();
                    }
#endif
                    unchecked
                    {
                        ++_tokenId;
                    }
                    _smallFields._locker.Exit();
                    ResetAndRepool();
                    return;
                }
                _smallFields._locker.Exit();
            }

            [MethodImpl(InlineOption)]
            private void ResetAndRepool()
            {
                ThrowIfInPool(this);
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                if (_registeredCallbacksHead._next != _registeredCallbacksHead || _registeredCallbacksHead._previous != _registeredCallbacksHead)
                {
                    throw new System.InvalidOperationException("CancelationToken callbacks have not been unregistered.");
                }
#endif
                _state = State.Disposed;
                ObjectPool.MaybeRepool(this);
            }

            public void Cancel()
            {
                TrySetCanceled(SourceId);
            }

#if !PROTO_PROMISE_DEVELOPER_MODE
            [DebuggerNonUserCode, StackTraceHidden]
#endif
            private sealed class CallbackNodeImpl<TCancelable> : CancelationCallbackNode, ITraceable
                where TCancelable : ICancelable
            {
#if PROMISE_DEBUG
                CausalityTrace ITraceable.Trace { get; set; }
#endif

                private TCancelable _cancelable;

                private CallbackNodeImpl() { }

#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                volatile private bool _disposed;

                ~CallbackNodeImpl()
                {
                    try
                    {
                        if (!_disposed)
                        {
                            // For debugging. This should never happen.
                            string message = "A " + GetType() + " was garbage collected without it being disposed.";
                            ReportRejection(new UnreleasedObjectException(message), this);
                        }
                    }
                    catch (Exception e)
                    {
                        // This should never happen.
                        ReportRejection(e, this);
                    }
                }
#endif

                [MethodImpl(InlineOption)]
                private static CallbackNodeImpl<TCancelable> GetOrCreate()
                {
                    var obj = ObjectPool.TryTakeOrInvalid<CallbackNodeImpl<TCancelable>>();
                    return obj == PromiseRefBase.InvalidAwaitSentinel.s_instance
                        ? new CallbackNodeImpl<TCancelable>()
                        : obj.UnsafeAs<CallbackNodeImpl<TCancelable>>();
                }

                [MethodImpl(InlineOption)]
                internal static CallbackNodeImpl<TCancelable> GetOrCreate(
#if CSHARP_7_3_OR_NEWER
                    in
#endif
                    TCancelable cancelable, CancelationRef parent)
                {
                    var del = GetOrCreate();
                    del._parentId = parent._smallFields._instanceId;
                    del._cancelable = cancelable;
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                    // If the CancelationRef was attached to a BCL token, it is possible this will not be disposed, so we won't check for it.
                    del._disposed = parent._linkedToBclToken;
#endif
                    SetCreatedStacktrace(del, 2);
                    return del;
                }

                internal override void Invoke(CancelationRef parent)
                {
                    ThrowIfInPool(this);
                    SetCurrentInvoker(this);
                    try
                    {
                        _cancelable.Cancel();
                    }
                    finally
                    {
                        Dispose();
                        ClearCurrentInvoker();
                    }
                }

                [MethodImpl(InlineOption)]
                internal override void Dispose()
                {
                    ThrowIfInPool(this);
                    unchecked
                    {
                        ++_nodeId;
                    }
                    _cancelable = default(TCancelable);
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                    _disposed = true;
#endif
                    ObjectPool.MaybeRepool(this);
                }
            }
        } // class CancelationRef

        internal class CancelationCallbackNode : HandleablePromiseBase
        {
            internal CancelationCallbackNode _previous; // _next is HandleablePromiseBase which we just unsafe cast to CallbackNode.
            volatile protected int _nodeId = 1; // Start with id 1 instead of 0 to reduce risk of false positives.
            protected int _parentId; // In case the CancelationRegistration is torn from threads.

            internal int NodeId
            {
                [MethodImpl(InlineOption)]
                get { return _nodeId; }
            }

            protected CancelationCallbackNode() { }

            internal static CancelationCallbackNode CreateLinkedListSentinel()
            {
                var sentinel = new CancelationCallbackNode();
                sentinel._next = sentinel;
                sentinel._previous = sentinel;
                return sentinel;
            }

            internal void ResetSentinel()
            {
                _next = this;
                _previous = this;
            }

            internal void RemoveFromLinkedList()
            {
                _previous._next = _next;
                _next.UnsafeAs<CancelationCallbackNode>()._previous = _previous;
                _previous = null;
            }

            internal void InsertPrevious(CancelationCallbackNode node)
            {
                node._previous = _previous;
                node._next = this;
                _previous._next = node;
                _previous = node;
            }

            internal virtual void Invoke(CancelationRef parent) { throw new System.InvalidOperationException(); }
            internal virtual void Dispose() { throw new System.InvalidOperationException(); }

            [MethodImpl(InlineOption)]
            private bool GetIsRegistered(CancelationRef parent, int nodeId, int tokenId)
            {
                return parent._smallFields._instanceId == _parentId & parent.TokenId == tokenId
                    & _nodeId == nodeId & _previous != null;
            }

            [MethodImpl(InlineOption)]
            internal static bool GetIsRegistered(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId)
            {
                if (_this == null | parent == null)
                {
                    return false;
                }
                return _this.GetIsRegistered(parent, nodeId, tokenId);
            }

            [MethodImpl(InlineOption)]
            internal static bool TryUnregister(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId)
            {
                if (_this == null | parent == null)
                {
                    return false;
                }

                parent._smallFields._locker.Enter();
                if (!_this.GetIsRegistered(parent, nodeId, tokenId))
                {
                    parent._smallFields._locker.Exit();
                    return false;
                }

                _this.RemoveFromLinkedList();
                parent._smallFields._locker.Exit();
                _this.Dispose();
                return true;
            }

            [MethodImpl(InlineOption)]
            private bool GetIsRegisteredAndIsCanceled(CancelationRef parent, int nodeId, int tokenId, out bool isCanceled)
            {
                bool canceled = parent._state >= CancelationRef.State.Canceled;
                bool tokenIdMatches = parent._smallFields._instanceId == _parentId & parent.TokenId == tokenId;
                bool isRegistered = tokenIdMatches & _nodeId == nodeId & _previous != null;
                isCanceled = canceled & tokenIdMatches;
                return isRegistered;
            }

            [MethodImpl(InlineOption)]
            internal static bool GetIsRegisteredAndIsCanceled(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId, out bool isCanceled)
            {
                if (_this == null | parent == null)
                {
                    isCanceled = false;
                    return false;
                }
                parent._smallFields._locker.Enter();
                bool isRegistered = _this.GetIsRegisteredAndIsCanceled(parent, nodeId, tokenId, out isCanceled);
                parent._smallFields._locker.Exit();
                return isRegistered;
            }

            [MethodImpl(InlineOption)]
            internal static bool TryUnregister(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId, out bool isCanceled)
            {
                if (_this == null | parent == null)
                {
                    isCanceled = false;
                    return false;
                }

                parent._smallFields._locker.Enter();
                if (!_this.GetIsRegisteredAndIsCanceled(parent, nodeId, tokenId, out isCanceled))
                {
                    parent._smallFields._locker.Exit();
                    return false;
                }

                _this.RemoveFromLinkedList();
                parent._smallFields._locker.Exit();
                _this.Dispose();
                return true;
            }

            [MethodImpl(InlineOption)]
            internal static void TryUnregisterOrWaitForCallbackToComplete(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId)
            {
                if (_this == null | parent == null)
                {
                    return;
                }

                parent._smallFields._locker.Enter();
                bool idsMatch = parent._smallFields._instanceId == _this._parentId
                    & tokenId == parent.TokenId
                    & nodeId == _this._nodeId;
                if (idsMatch & _this._previous != null)
                {
                    _this.RemoveFromLinkedList();
                    parent._smallFields._locker.Exit();
                    _this.Dispose();
                    return;
                }

                bool parentIsCanceling = parent._state == CancelationRef.State.Canceled;
                parent._smallFields._locker.Exit();
                // If the source is executing callbacks on another thread, we must wait until this callback is complete.
                if (idsMatch & parentIsCanceling
                    & parent._executingThread != Thread.CurrentThread)
                {
                    var spinner = new SpinWait();
                    // _this._nodeId will be incremented when the callback is complete and this is disposed.
                    // parent.TokenId will be incremented when all callbacks are complete and it is disposed.
                    // We really only need to compare the nodeId, the tokenId comparison is just for a little extra safety in case of thread starvation and node re-use.
                    while (nodeId == _this._nodeId & tokenId == parent.TokenId)
                    {
                        spinner.SpinOnce(); // Spin, as we assume callback execution is fast and that this situation is rare.
                    }
                }
            }

            [MethodImpl(InlineOption)]
            internal static Promise TryUnregisterOrWaitForCallbackToCompleteAsync(CancelationRef parent, CancelationCallbackNode _this, int nodeId, int tokenId)
            {
                if (_this == null | parent == null)
                {
                    return new Promise();
                }

                parent._smallFields._locker.Enter();
                bool idsMatch = parent._smallFields._instanceId == _this._parentId
                    & tokenId == parent.TokenId
                    & nodeId == _this._nodeId;
                if (idsMatch & _this._previous != null)
                {
                    _this.RemoveFromLinkedList();
                    parent._smallFields._locker.Exit();
                    _this.Dispose();
                    return new Promise();
                }

                bool parentIsCanceling = parent._state == CancelationRef.State.Canceled;
                parent._smallFields._locker.Exit();
                // If the source is executing callbacks on another thread, we must wait until this callback is complete.
                if (idsMatch & parentIsCanceling
                    & parent._executingThread != Thread.CurrentThread)
                {
                    // The specified callback is actually running: queue an async loop that'll poll for the currently executing
                    // callback to complete. While such polling isn't ideal, we expect this to be a rare case (disposing while
                    // the associated callback is running), and brief when it happens (so the polling will be minimal), and making
                    // this work with a callback mechanism will add additional cost to other more common cases.
                    var deferred = Promise.NewDeferred();
                    WaitForInvokeComplete(parent, _this, nodeId, tokenId, deferred);
                    return deferred.Promise;
                }
                return new Promise();
            }

            private static void WaitForInvokeComplete(CancelationRef parent, CancelationCallbackNode node, int nodeId, int tokenId, Promise.Deferred deferred)
            {
                // node._nodeId will be incremented when the callback is complete and it is disposed.
                // parent.TokenId will be incremented when all callbacks are complete and it is disposed.
                // We really only need to compare the nodeId, the tokenId comparison is just for a little extra safety in case of thread starvation and node re-use.
                if (nodeId == node._nodeId & tokenId == parent.TokenId)
                {
                    // Queue the check to happen again on a background thread.
                    // Force async so the current thread will be yielded if this is already being executed on a background thread.
                    // This is recursive, but it's done so asynchronously so it will never cause StackOverflowException.
                    Promise.Run(ValueTuple.Create(parent, node, nodeId, tokenId, deferred),
                        cv => WaitForInvokeComplete(cv.Item1, cv.Item2, cv.Item3, cv.Item4, cv.Item5),
                        Promise.Config.BackgroundContext, forceAsync: true)
                        .Forget();
                }
                else
                {
                    deferred.Resolve();
                }
            }
        } // class CancelationCallbackNode

        partial class CancelationRef
        {
#if !NET_LEGACY || NET40
            // A separate class so that static data won't need to be created if it is never used.
            internal static class CancelationConverter
            {
                private static readonly bool s_canExtractSource = GetCanExtractSource();
                // Cache so if ToCancelationToken() is called multiple times on the same token, we don't need to allocate for every call.
                // ConditionalWeakTable so we aren't extending the lifetime of any sources beyond what the user is using them for.
                private static readonly ConditionalWeakTable<CancellationTokenSource, CancelationRef> s_tokenCache = new ConditionalWeakTable<CancellationTokenSource, CancelationRef>();

                private static bool GetCanExtractSource()
                {
                    // This assumes the CancellationToken is implemented like this, and will return false if it's different.
                    // public struct CancellationToken
                    // {
                    //     private CancellationTokenSource m_source;
                    //     ...
                    // }
                    var fields = typeof(System.Threading.CancellationToken).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    return fields.Length == 1 && typeof(CancellationTokenSource).IsAssignableFrom(fields[0].FieldType);
                }

                // Implementation detail, the token wraps the source, so we can retrieve it by placing it in this explicit layout struct and reading the source.
                // This is equivalent to `Unsafe.As`, but also works in older runtimes that don't support Unsafe. It's also more efficient than using reflection (and some runtimes don't support TypedReference).
                // I think it is very unlikely, but the internal implementation of CancellationToken could change in the future (or different runtime) to break this code, which is why we have the s_canExtractSource check.
                [StructLayout(LayoutKind.Explicit)]
                private struct TokenSourceExtractor
                {
                    [FieldOffset(0)]
                    internal System.Threading.CancellationToken _token;
                    [FieldOffset(0)]
                    internal CancellationTokenSource _source;
                }

                internal static void AttachCancelationRef(CancellationTokenSource source, CancelationRef _ref)
                {
                    s_tokenCache.Add(source, _ref);
                }

                internal static void DetachCancelationRef(CancellationTokenSource source)
                {
                    s_tokenCache.Remove(source);
                }

                internal static CancelationToken Convert(System.Threading.CancellationToken token)
                {
                    if (!s_canExtractSource)
                    {
                        throw new System.Reflection.TargetException("Cannot convert System.Threading.CancellationToken to Proto.Promises.CancelationToken due to an implementation change. Please notify the developer.");
                    }

                    if (!token.CanBeCanceled)
                    {
                        return default(CancelationToken);
                    }
                    if (token.IsCancellationRequested)
                    {
                        return CancelationToken.Canceled();
                    }

                    // This relies on internal implementation details. If the implementation changes, the s_canExtractSource check should catch it.
                    var source = new TokenSourceExtractor() { _token = token }._source;

                    if (source == null)
                    {
                        // Source should never be null if token.CanBeCanceled returned true.
                        throw new System.Reflection.TargetException("The token's internal source was null.");
                    }

                    // If the source was disposed, the Token property will throw ObjectDisposedException.
                    // Unfortunately, this is the only way to check if it's disposed, since token.CanBeCanceled may still return true after it's disposed in .Net Core.
                    try
                    {
                        token = source.Token;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Check canceled state again in case of a race condition.
                        return source.IsCancellationRequested ? CancelationToken.Canceled() : default(CancelationToken);
                    }

                    CancelationRef cancelationRef;
                    if (s_tokenCache.TryGetValue(source, out cancelationRef))
                    {
                        var tokenId = cancelationRef.TokenId;
                        Thread.MemoryBarrier();
                        return cancelationRef._bclSource != source // In case of race condition on another thread.
                            ? default(CancelationToken)
                            : new CancelationToken(cancelationRef, tokenId);
                    }

                    // Lock instead of AddOrUpdate so multiple refs won't be created on separate threads.
                    lock (s_tokenCache)
                    {
                        if (!s_tokenCache.TryGetValue(source, out cancelationRef))
                        {
                            cancelationRef = GetOrCreateForBclTokenConvert(source);
                            s_tokenCache.Add(source, cancelationRef);
                        }
                    }
                    {
                        var tokenId = cancelationRef.TokenId;
                        cancelationRef.HookupBclCancelation(token);
                        return new CancelationToken(cancelationRef, tokenId);
                    }
                }
            } // class CancelationConverter

            private CancellationTokenSource _bclSource;
            private CancellationTokenRegistration _bclRegistration;

            internal static CancelationRef GetOrCreateForBclTokenConvert(CancellationTokenSource source)
            {
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                // Don't take from the object pool, just create new (this is so the object pool's tracker won't report this since Dispose is never called on it).
                var cancelRef = new CancelationRef();
#else
                var cancelRef = GetFromPoolOrCreate();
                cancelRef._next = null;
#endif
                cancelRef.Initialize(true);
                cancelRef._bclSource = source;
                return cancelRef;
            }

            private void HookupBclCancelation(System.Threading.CancellationToken token)
            {
                // We don't need the synchronous invoke check when this is created.
                _bclRegistration = token.Register(state => state.UnsafeAs<CancelationRef>().Cancel(), this, false);
            }

            internal static CancellationToken GetCancellationToken(CancelationRef _this, int tokenId)
            {
                return _this == null ? default(CancellationToken) : _this.GetCancellationToken(tokenId);
            }

            private CancellationToken GetCancellationToken(int tokenId)
            {
                _smallFields._locker.Enter();
                try
                {
                    var state = _state;
                    if (tokenId != TokenId | state == State.Disposed)
                    {
                        return default(CancellationToken);
                    }
                    if (state >= State.Canceled)
                    {
                        return new CancellationToken(true);
                    }
                    if (_bclSource == null)
                    {
                        _bclSource = new CancellationTokenSource();
                        CancelationConverter.AttachCancelationRef(_bclSource, this);
                        var del = new CancelDelegateToken<CancellationTokenSource>(_bclSource, source => source.Cancel(false));
                        var node = CallbackNodeImpl<CancelDelegateToken<CancellationTokenSource>>.GetOrCreate(del, this);
                        _registeredCallbacksHead.InsertPrevious(node);
                    }
                    return _bclSource.Token;
                }
                // The original source may be disposed, in which case the Token property will throw ObjectDisposedException.
                catch (ObjectDisposedException)
                {
                    return _bclSource.IsCancellationRequested ? new CancellationToken(true) : default(CancellationToken);
                }
                finally
                {
                    _smallFields._locker.Exit();
                }
            }
#endif // !NET_LEGACY || NET40
        } // class CancelationRef
    } // class Internal
} // namespace Proto.Promises