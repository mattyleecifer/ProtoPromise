﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
# endif

#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0034 // Simplify 'default' expression

using System;
using System.Runtime.CompilerServices;

namespace Proto.Promises
{
    internal static partial class Internal
    {
#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal abstract class ValueContainer<T> : ValueContainer, ITraceable
        {
#if PROMISE_DEBUG
            CausalityTrace ITraceable.Trace { get; set; }
#endif
            private int _retainCounter;
            public T value;

            internal override sealed object Value
            {
                get
                {
                    // Don't throw if in pool, just return either way.
                    // If it's in the pool, it means the promise was canceled, and it will check the cancelation before using the value.
                    //ThrowIfInPool(this);
                    return value;
                }
            }

            internal override sealed Type ValueType
            {
                get
                {
                    // Don't throw if in pool, just return either way.
                    // If it's in the pool, it means the promise was canceled, and it will check the cancelation before using the value.
                    //ThrowIfInPool(this);
                    Type type = typeof(T);
                    if (type.IsValueType)
                    {
                        return type;
                    }
                    object temp = value;
                    return temp == null ? type : temp.GetType();
                }
            }

#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
            ~ValueContainer()
            {
                if (_retainCounter != 0)
                {
                    // For debugging. This should never happen.
                    string message = "A " + GetType() + " was garbage collected without it being released. _retainCounter: " + _retainCounter + ", value: " + value;
                    AddRejectionToUnhandledStack(new UnreleasedObjectException(message), this);
                }
            }
#endif

            internal override void Retain()
            {
                ThrowIfInPool(this);
                int _;
                // Don't let counter wrap around past 0.
                if (!InterlockedAddIfNotEqual(ref _retainCounter, 1, -1, out _))
                {
                    throw new OverflowException();
                }
            }

            protected bool TryReleaseComplete()
            {
                ThrowIfInPool(this);
                int newValue;
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                // Don't let counter go below 0.
                if (!InterlockedAddIfNotEqual(ref _retainCounter, -1, 0, out newValue))
                {
                    throw new OverflowException(); // This should never happen, but checking just in case.
                }
#else
                newValue = System.Threading.Interlocked.Decrement(ref _retainCounter);
#endif
                return newValue == 0;
            }

            protected void Reset(int retainCount)
            {
                _retainCounter = retainCount;
                SetCreatedStacktrace(this, 2);
            }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal sealed class RejectionContainer<T> : ValueContainer<T>, ILinked<RejectionContainer<T>>, IRejectValueContainer, IRejectionToContainer, ICantHandleException
        {
            RejectionContainer<T> ILinked<RejectionContainer<T>>.Next { get; set; }

#if PROMISE_DEBUG
            System.Diagnostics.StackTrace _rejectedStackTrace;
            // Stack traces of recursive callbacks.
            private CausalityTrace _stackTraces;

            public void SetCreatedAndRejectedStacktrace(System.Diagnostics.StackTrace rejectedStacktrace, CausalityTrace createdStacktraces)
            {
                ThrowIfInPool(this);
                _rejectedStackTrace = rejectedStacktrace;
                _stackTraces = createdStacktraces;
            }
#endif

            private RejectionContainer() { }

            internal static RejectionContainer<T> GetOrCreate(
#if CSHARP_7_3_OR_NEWER
                in
#endif
                T value, int retainCount)
            {
                var container = ObjectPool<RejectionContainer<T>>.TryTake<RejectionContainer<T>>()
                    ?? new RejectionContainer<T>();
                container.value = value;
                container.Reset(retainCount);
                return container;
            }

            internal override Promise.State GetState()
            {
                return Promise.State.Rejected;
            }

            internal override void Release()
            {
                ThrowIfInPool(this);
                if (TryReleaseComplete())
                {
                    Dispose();
                }
            }

            internal override void ReleaseAndMaybeAddToUnhandledStack(bool shouldAdd)
            {
                if (shouldAdd)
                {
                    AddUnhandledException(ToException());
                }
                Release();
            }

            private void Dispose()
            {
#if PROMISE_DEBUG
                _rejectedStackTrace = null;
                _stackTraces = null;
#endif
                value = default(T);
                ObjectPool<RejectionContainer<T>>.MaybeRepool(this);
            }

            private UnhandledException ToException()
            {
                ThrowIfInPool(this);
#if PROMISE_DEBUG
                string innerStacktrace = _rejectedStackTrace == null ? null : FormatStackTrace(new System.Diagnostics.StackTrace[1] { _rejectedStackTrace });
#else
                string innerStacktrace = null;
#endif
                string message = null;
                Type type = Value.GetType();
                Exception innerException = Value as Exception;
                if (innerException != null)
                {
#if PROMISE_DEBUG
                    if (_rejectedStackTrace != null)
                    {
                        innerException = new RejectionException(message, innerStacktrace, innerException);
                    }
#endif
                    message = "An exception was not handled.";
                }
                else
                {
                    message = "A rejected value was not handled, type: " + type + ", value: " + Value.ToString();
                    innerException = new RejectionException(message, innerStacktrace, null);
                }
#if PROMISE_DEBUG
                string outerStacktrace = _stackTraces.ToString();
#else
                string outerStacktrace = null;
#endif
                return new UnhandledExceptionInternal(Value, type, message + CausalityTraceMessage, outerStacktrace, innerException);
            }

            Exception IThrowable.GetException()
            {
                ThrowIfInPool(this);
                return ToException();
            }

            ValueContainer IRejectionToContainer.ToContainer(ITraceable traceable)
            {
                ThrowIfInPool(this);
                return this;
            }

            void ICantHandleException.AddToUnhandledStack(ITraceable traceable)
            {
                ThrowIfInPool(this);
                AddUnhandledException(ToException());
            }
        }

        internal interface IConstructor<T>
        {
            T Construct();
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal abstract class SingletonValueContainer<TValueContainer, TConstructor> : ValueContainer<VoidResult>
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
            , ILinked<TValueContainer>
#endif
            where TValueContainer : SingletonValueContainer<TValueContainer, TConstructor>
            where TConstructor : struct, IConstructor<TValueContainer>
        {
            // This is to help with internal debugging. When not debugging, a single instance can be reused for efficiency.

#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
            TValueContainer ILinked<TValueContainer>.Next { get; set; }
#else
            // We can reuse the same object.
            private static readonly TValueContainer _instance = default(TConstructor).Construct();
#endif

            protected static TValueContainer GetOrCreateBase(int retainCount)
            {
#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
                var container = ObjectPool<TValueContainer>.TryTake<TValueContainer>()
                    ?? default(TConstructor).Construct();
                container.Reset(retainCount);
                return container;
#else
                return _instance;
#endif
            }

#if PROMISE_DEBUG || PROTO_PROMISE_DEVELOPER_MODE
            internal override void Release()
            {
                if (TryReleaseComplete())
                {
                    ObjectPool<TValueContainer>.MaybeRepool((TValueContainer) this);
                }
            }

            internal override void ReleaseAndMaybeAddToUnhandledStack(bool shouldAdd)
            {
                Release();
            }
#else
            internal override void Release() { }
            internal override void ReleaseAndMaybeAddToUnhandledStack(bool shouldAdd) { }
#endif
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal sealed class CancelContainerVoid : SingletonValueContainer<CancelContainerVoid, CancelContainerVoid.Constructor>, IThrowable
        {
            internal struct Constructor : IConstructor<CancelContainerVoid>
            {
                [MethodImpl(InlineOption)]
                public CancelContainerVoid Construct()
                {
                    return new CancelContainerVoid();
                }
            }

            [MethodImpl(InlineOption)]
            internal static CancelContainerVoid GetOrCreate(int retainCount)
            {
                return GetOrCreateBase(retainCount);
            }

            internal override Promise.State GetState()
            {
                return Promise.State.Canceled;
            }

            Exception IThrowable.GetException()
            {
                return CanceledExceptionInternal.GetOrCreate();
            }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal sealed class ResolveContainer<T> : ValueContainer<T>, ILinked<ResolveContainer<T>>
        {
            ResolveContainer<T> ILinked<ResolveContainer<T>>.Next { get; set; }

            private ResolveContainer() { }

            internal static ResolveContainer<T> GetOrCreate(
#if CSHARP_7_3_OR_NEWER
                in
#endif
                T value, int retainCount)
            {
                var container = ObjectPool<ResolveContainer<T>>.TryTake<ResolveContainer<T>>()
                    ?? new ResolveContainer<T>();
                container.value = value;
                container.Reset(retainCount);
                return container;
            }

            internal override Promise.State GetState()
            {
                return Promise.State.Resolved;
            }

            internal override void Release()
            {
                ThrowIfInPool(this);
                if (TryReleaseComplete())
                {
                    Dispose();
                }
            }

            internal override void ReleaseAndMaybeAddToUnhandledStack(bool shouldAdd)
            {
                Release();
            }

            private void Dispose()
            {
                value = default(T);
                ObjectPool<ResolveContainer<T>>.MaybeRepool(this);
            }
        }

#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        internal sealed class ResolveContainerVoid : SingletonValueContainer<ResolveContainerVoid, ResolveContainerVoid.Constructor>
        {
            internal struct Constructor : IConstructor<ResolveContainerVoid>
            {
                [MethodImpl(InlineOption)]
                public ResolveContainerVoid Construct()
                {
                    return new ResolveContainerVoid();
                }
            }

            [MethodImpl(InlineOption)]
            internal static ResolveContainerVoid GetOrCreate(int retainCount)
            {
                return GetOrCreateBase(retainCount);
            }

            internal override Promise.State GetState()
            {
                return Promise.State.Resolved;
            }
        }
    }
}