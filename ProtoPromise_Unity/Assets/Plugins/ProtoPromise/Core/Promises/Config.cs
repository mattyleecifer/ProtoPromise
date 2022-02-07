﻿// define PROTO_PROMISE_DEBUG_ENABLE to enable debugging options in RELEASE mode. define PROTO_PROMISE_DEBUG_DISABLE to disable debugging options in DEBUG mode.
#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif
// define PROTO_PROMISE_PROGRESS_DISABLE to disable progress reports on promises.
// If Progress is enabled, promises use more memory, and it creates an upper bound to the depth of a promise chain (see Config for details).
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Proto.Promises
{
    partial struct Promise
    {
        public enum TraceLevel : byte
        {
            /// <summary>
            /// Don't track any causality traces.
            /// </summary>
            None,
            /// <summary>
            /// Track causality only when Deferred.Reject is called.
            /// </summary>
            Rejections,
            /// <summary>
            /// Track causality when Deferred.Reject is called and every time a promise is created or a delegate is added to a promise (i.e. with .Then or .Progress).
            /// <para/>
            /// NOTE: This can be extremely expensive, so you should only enable this if you ran into an error and you are not sure where it came from.
            /// </summary>
            All
        }

        [Obsolete("Promise Config now uses a simple boolean for object pooling.")]
        public enum PoolType : byte
        {
            None,
            Internal,
            All
        }

        /// <summary>
        /// Promise configuration. Configuration settings affect the global behaviour of promises.
        /// </summary>
#if !PROTO_PROMISE_DEVELOPER_MODE
        [System.Diagnostics.DebuggerNonUserCode]
#endif
        public static partial class Config
        {
            [Obsolete("Use ProgressPrecision to get the precision of progress reports.")]
            public static readonly int ProgressDecimalBits = Internal.PromiseRef.Fixed32.DecimalBits;

            /// <summary>
            /// The maximum precision of progress reports.
            /// </summary>
#if !PROMISE_PROGRESS
            [Obsolete(Internal.ProgressDisabledMessage, false)]
#endif
            public static readonly float ProgressPrecision = (float) (1d / Math.Pow(2d, Internal.PromiseRef.Fixed32.DecimalBits));

            [Obsolete("Use ObjectPoolingEnabled instead.")]
            public static PoolType ObjectPooling 
            {
                get { return _objectPoolingEnabled ? PoolType.All : PoolType.None; }
                set { _objectPoolingEnabled = value != PoolType.None; }
            }

            volatile private static bool _objectPoolingEnabled = true; // Enabled by default.
            public static bool ObjectPoolingEnabled
            {
                [MethodImpl(Internal.InlineOption)]
                get { return _objectPoolingEnabled; } 
                [MethodImpl(Internal.InlineOption)]
                set { _objectPoolingEnabled = value; } 
            }

            /// <summary>
            /// Set how causality is traced in DEBUG mode. Causality traces are readable from an UnhandledException's Stacktrace property.
            /// </summary>
#if PROMISE_DEBUG
            public static TraceLevel DebugCausalityTracer
            {
                [MethodImpl(Internal.InlineOption)]
                get { return _debugCausalityTracer; }
                [MethodImpl(Internal.InlineOption)]
                set { _debugCausalityTracer = value; }
            }
            volatile private static TraceLevel _debugCausalityTracer = TraceLevel.Rejections;
#else
            public static TraceLevel DebugCausalityTracer
            {
                [MethodImpl(Internal.InlineOption)]
                get { return default(TraceLevel); }
                [MethodImpl(Internal.InlineOption)]
                set { }
            }
#endif

            // Used so that libraries can have a ProtoPromise dependency without forcing progress enabled/disabled on those libraries' users.
            // e.g. a library depends on ProtoPromise v2.0.0 or higher, a user of that library could opt to use ProtoPromise v2.0.0.0 (no progress) or v2.0.0.1 (with progress)
            public static bool IsProgressEnabled
            {
                [MethodImpl(MethodImplOptions.NoInlining)] // Don't allow inlining, otherwise it could break library code that functions depending on if progress is enabled or not.
                get
                {
#if PROMISE_PROGRESS
                    return true;
#else
                    return false;
#endif
                }
            }

            /// <summary>
            /// Uncaught rejections get routed through this delegate.
            /// This must be set to a non-null delegate, otherwise uncaught rejections will continue to pile up without being reported.
            /// </summary>
            public static Action<UnhandledException> UncaughtRejectionHandler
            {
                [MethodImpl(Internal.InlineOption)]
                get { return _uncaughtRejectionHandler; }
                [MethodImpl(Internal.InlineOption)]
                set { _uncaughtRejectionHandler = value; }
            }
            volatile private static Action<UnhandledException> _uncaughtRejectionHandler;

            /// <summary>
            /// The <see cref="SynchronizationContext"/> used to marshal work to the UI thread.
            /// </summary>
            public static SynchronizationContext ForegroundContext
            {
                [MethodImpl(Internal.InlineOption)]
                get { return _foregroundContext; }
                set
                {
                    _foregroundContext = value;
                    Internal._foregroundSynchronizationHandler = new Internal.SynchronizationHandler(value);
                }
            }
            volatile private static SynchronizationContext _foregroundContext;

            /// <summary>
            /// The <see cref="SynchronizationContext"/> used to marshal work to a background thread. If this is null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> is used.
            /// </summary>
            public static SynchronizationContext BackgroundContext
            {
                [MethodImpl(Internal.InlineOption)]
                get { return _backgroundContext; }
                [MethodImpl(Internal.InlineOption)]
                set { _backgroundContext = value; }
            }
            volatile private static SynchronizationContext _backgroundContext;

            [Obsolete]
            public static Action<string> WarningHandler { get; set; }
        }
    }
}