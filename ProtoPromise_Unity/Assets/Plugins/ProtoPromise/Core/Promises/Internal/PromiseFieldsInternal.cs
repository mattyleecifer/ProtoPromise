﻿// This file makes it easier to see all the fields that each promise type has, and calculate how much memory they should consume.

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

#pragma warning disable IDE0034 // Simplify 'default' expression

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Proto.Promises
{
    [StructLayout(LayoutKind.Auto)]
    partial struct Promise
    {
        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly Promise<Internal.VoidResult> _target;

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id, int depth)
        {
            _target = new Promise<Internal.VoidResult>(promiseRef, id, depth);
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Promise<Internal.VoidResult> target)
        {
            _target = target;
        }
    }

    partial struct Promise<T>
    {
        // This is used so that _result will be packed efficiently and not padded with extra bytes (only relevant for small, non-primitive struct T types).
        // Otherwise, if all fields are on the same level as _ref, because it is a class type, it will pad T up to IntPtr.Size if T is not primitive, causing the Promise<T> struct to be larger than necessary.
        // This is especially needed for Promise, which has an internal Promise<Internal.VoidResult> field (and sadly, the runtime does not allow 0-sized structs, minimum size is 1 byte).
        // See https://stackoverflow.com/questions/24742325/why-does-struct-alignment-depend-on-whether-a-field-type-is-primitive-or-user-de
        private
#if CSHARP_7_3_OR_NEWER
            readonly
#endif
            struct SmallFields
        {
#if PROMISE_PROGRESS
            internal readonly int _depth;
#endif
            internal readonly short _id;
            internal readonly T _result;

            [MethodImpl(Internal.InlineOption)]
            internal SmallFields(short id, int depth,
#if CSHARP_7_3_OR_NEWER
                in
#endif
                T result)
            {
#if PROMISE_PROGRESS
                _depth = depth;
#endif
                _id = id;
                _result = result;
            }
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly Internal.PromiseRef _ref;
        private readonly SmallFields _smallFields;

        /// <summary>
        /// Internal use.
        /// </summary>
        internal short Id
        {
            [MethodImpl(Internal.InlineOption)]
            get { return _smallFields._id; }
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        internal T Result
        {
            [MethodImpl(Internal.InlineOption)]
            get { return _smallFields._result; }
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        internal int Depth
        {
            [MethodImpl(Internal.InlineOption)]
#if PROMISE_PROGRESS
            get { return _smallFields._depth; }
#else
            get { return 0; }
#endif
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id, int depth)
        {
            _ref = promiseRef;
            _smallFields = new SmallFields(id, depth, default(T));
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id, int depth,
#if CSHARP_7_3_OR_NEWER
                in
#endif
                T value)
        {
            _ref = promiseRef;
            _smallFields = new SmallFields(id, depth, value);
        }
    }

    partial class Internal
    {
        internal struct VoidResult { }

        partial class PromiseRef
        {
#if PROMISE_DEBUG
            CausalityTrace ITraceable.Trace { get; set; }
#endif

            private ITreeHandleable _next;
            volatile private object _valueOrPrevious;
            private IdRetain _idsAndRetains = new IdRetain(1); // Start with Id 1 instead of 0 to reduce risk of false positives.
            private SmallFields _smallFields;

            [StructLayout(LayoutKind.Explicit)]
            private partial struct IdRetain
            {
                [FieldOffset(0)]
                internal short _promiseId;
                [FieldOffset(2)]
                internal short _deferredId;
                [FieldOffset(4)]
                private uint _retains;
                // We can check Id and retain/release atomically.
                [FieldOffset(0)]
                private long _longValue;

                [MethodImpl(InlineOption)]
                internal IdRetain(short initialId)
                {
                    _longValue = 0;
                    _retains = 0;
                    _promiseId = initialId;
                    _deferredId = initialId;
                }
            } // IdRetain

            private partial struct SmallFields
            {
                // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                internal StateAndFlags _stateAndFlags;
#if PROMISE_PROGRESS
                internal Fixed32 _waitDepthAndProgress;
#endif

                [StructLayout(LayoutKind.Explicit)]
                internal partial struct StateAndFlags
                {
                    [FieldOffset(0)]
                    volatile internal Promise.State _state;
                    [FieldOffset(1)]
                    volatile internal bool _suppressRejection;
                    [FieldOffset(2)]
                    volatile internal bool _wasAwaitedOrForgotten;
#if PROMISE_PROGRESS
                    [FieldOffset(3)]
                    volatile private ProgressFlags _progressFlags;
                    // int value with [FieldOffset(0)] allows us to use Interlocked to set the flags without consuming more memory than necessary.
                    [FieldOffset(0)]
                    volatile private int _intValue;
#endif
                } // StateAndFlags
            } // SmallFields

            partial class PromiseSingleAwait : PromiseRef
            {
#if PROMISE_PROGRESS
                volatile protected IProgressListener _progressListener;
#endif
            }

            partial class PromiseBranch : PromiseSingleAwait
            {
                private ITreeHandleable _waiter;
            }

            partial class PromiseMultiAwait
            {
                private ValueLinkedStack<ITreeHandleable> _nextBranches;
                private SpinLocker _branchLocker;

#if PROMISE_PROGRESS
                private struct ProgressAndLocker
                {
                    // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                    // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                    internal Fixed32 _currentProgress;
                    internal SpinLocker _progressCollectionLocker;
                }

                private ValueLinkedQueue<IProgressListener> _progressListeners; // TODO: change to ValueLinkedStack to use less memory. Make sure progress is still invoked in order.
                private ProgressAndLocker _progressAndLocker;

                IProgressListener ILinked<IProgressListener>.Next { get; set; }
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }
#endif
            }

            partial class PromiseWaitPromise : PromiseBranch
            {
#if PROMISE_PROGRESS
                IProgressListener ILinked<IProgressListener>.Next { get; set; }
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }
#endif
            }

            #region Non-cancelable Promises
            partial class PromiseResolve<TArg, TResult, TResolver> : PromiseBranch
                where TResolver : IDelegate<TArg, TResult>
            {
                private TResolver _resolver;
            }

            partial class PromiseResolvePromise<TArg, TResult, TResolver> : PromiseWaitPromise
                where TResolver : IDelegate<TArg, Promise<TResult>>
            {
                private TResolver _resolver;
            }

            partial class PromiseResolveReject<TArgResolve, TResult, TResolver, TArgReject, TRejecter> : PromiseBranch
                where TResolver : IDelegate<TArgResolve, TResult>
                where TRejecter : IDelegate<TArgReject, TResult>
            {
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class PromiseResolveRejectPromise<TArgResolve, TResult, TResolver, TArgReject, TRejecter> : PromiseWaitPromise
                where TResolver : IDelegate<TArgResolve, Promise<TResult>>
                where TRejecter : IDelegate<TArgReject, Promise<TResult>>
            {
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class PromiseContinue<TContinuer> : PromiseBranch
                where TContinuer : IDelegateContinue
            {
                private TContinuer _continuer;
            }

            partial class PromiseContinuePromise<TContinuer> : PromiseWaitPromise
                where TContinuer : IDelegateContinuePromise
            {
                private TContinuer _continuer;
            }

            partial class PromiseFinally<TFinalizer> : PromiseBranch
                where TFinalizer : IDelegateSimple
            {
                private TFinalizer _finalizer;
            }

            partial class PromiseCancel<TCanceler> : PromiseBranch
                where TCanceler : IDelegateSimple
            {
                private TCanceler _canceler;
            }
            #endregion

            #region Cancelable Promises
            partial struct CancelationHelper
            {
                private CancelationRegistration _cancelationRegistration;
                private int _retainAndCanceled; // 17th bit is canceled, lower 16 bits are retains. This allows us to use Interlocked for both.
            }

            partial class DeferredPromiseVoidCancel : DeferredPromiseVoid
            {
                private CancelationRegistration _cancelationRegistration;
            }

            partial class DeferredPromiseCancel<T> : DeferredPromise<T>
            {
                private CancelationRegistration _cancelationRegistration;
            }

            partial class CancelablePromiseResolve<TArg, TResult, TResolver> : PromiseBranch
                where TResolver : IDelegate<TArg, TResult>
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
            }

            partial class CancelablePromiseResolvePromise<TArg, TResult, TResolver> : PromiseWaitPromise
                where TResolver : IDelegate<TArg, Promise<TResult>>
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
            }

            partial class CancelablePromiseResolveReject<TArgResolve, TResult, TResolver, TArgReject, TRejecter> : PromiseBranch
                where TResolver : IDelegate<TArgResolve, TResult>
                where TRejecter : IDelegate<TArgReject, TResult>
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class CancelablePromiseResolveRejectPromise<TArgResolve, TResult, TResolver, TArgReject, TRejecter> : PromiseWaitPromise
                where TResolver : IDelegate<TArgResolve, Promise<TResult>>
                where TRejecter : IDelegate<TArgReject, Promise<TResult>>
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class CancelablePromiseContinue<TContinuer> : PromiseBranch
                where TContinuer : IDelegateContinue
            {
                private CancelationHelper _cancelationHelper;
                private TContinuer _continuer;
            }

            partial class CancelablePromiseContinuePromise<TContinuer> : PromiseWaitPromise
                where TContinuer : IDelegateContinuePromise
            {
                private CancelationHelper _cancelationHelper;
                private TContinuer _continuer;
            }

            partial class CancelablePromiseCancel<TCanceler> : PromiseBranch
                where TCanceler : IDelegateSimple
            {
                private CancelationHelper _cancelationHelper;
                private TCanceler _canceler;
            }
            #endregion

            #region Multi Promises
            partial class MergePromise : PromiseBranch
            {
                private int _waitCount;
#if PROMISE_DEBUG
                private readonly object _locker = new object();
                private ValueLinkedStack<PromisePassThrough> _passThroughs;
#endif

#if PROMISE_PROGRESS
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }

                // These are used to avoid rounding errors when normalizing the progress.
                // Use 64 bits to allow combining many promises with very deep chains.
                private double _progressScaler;
                private UnsignedFixed64 _unscaledProgress;
#endif
            }

            partial class RacePromise : PromiseBranch
            {
                // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                private struct RaceSmallFields
                {
                    internal int _waitCount;
#if PROMISE_PROGRESS
                    internal Fixed32 _currentAmount;
#endif
                }

                private RaceSmallFields _raceSmallFields;
#if PROMISE_DEBUG
                private readonly object _locker = new object();
                private ValueLinkedStack<PromisePassThrough> _passThroughs;
#endif
#if PROMISE_PROGRESS
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }
#endif
            }

            partial class FirstPromise : PromiseBranch
            {
                // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                private struct FirstSmallFields
                {
                    internal int _waitCount;
#if PROMISE_PROGRESS
                    internal Fixed32 _currentAmount;
#endif
                }

                private FirstSmallFields _firstSmallFields;
#if PROMISE_DEBUG
                private readonly object _locker = new object();
                private ValueLinkedStack<PromisePassThrough> _passThroughs;
#endif
#if PROMISE_PROGRESS
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }
#endif
            }

            partial class PromisePassThrough : ITreeHandleable, ILinked<PromisePassThrough>, IProgressListener
            {
                // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                private struct PassThroughSmallFields
                {
                    internal int _index;
                    internal int _retainCounter;
#if PROMISE_PROGRESS
                    internal Fixed32 _currentProgress;
                    internal volatile bool _settingInitialProgress;
                    internal volatile bool _reportingProgress;
#endif
                }

                volatile private PromiseRef _owner;
                volatile private IMultiTreeHandleable _target;
                private PassThroughSmallFields _smallFields;

                ITreeHandleable ILinked<ITreeHandleable>.Next { get; set; }
                PromisePassThrough ILinked<PromisePassThrough>.Next { get; set; }

#if PROMISE_PROGRESS
                IProgressListener ILinked<IProgressListener>.Next { get; set; }
#endif
            }
            #endregion

#if PROMISE_PROGRESS
            partial class PromiseProgress<TProgress> : PromiseBranch, IProgressListener, IProgressInvokable
                where TProgress : IProgress<float>
            {
                // Wrapping struct fields smaller than 64-bits in another struct fixes issue with extra padding
                // (see https://stackoverflow.com/questions/67068942/c-sharp-why-do-class-fields-of-struct-types-take-up-more-space-than-the-size-of).
                private struct ProgressSmallFields
                {
                    internal Fixed32 _currentProgress;
                    volatile internal bool _complete;
                    volatile internal bool _canceled;
                }

                private ProgressSmallFields _smallProgressFields;
                private CancelationRegistration _cancelationRegistration;
                private TProgress _progress;

                IProgressListener ILinked<IProgressListener>.Next { get; set; }
                IProgressInvokable ILinked<IProgressInvokable>.Next { get; set; }
            }
#endif
        } // PromiseRef
    } // Internal
}