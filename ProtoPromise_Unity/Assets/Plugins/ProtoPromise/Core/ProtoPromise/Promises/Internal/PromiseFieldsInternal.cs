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

using Proto.Utils;
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
        internal readonly Internal.PromiseRef _ref;
        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly short _id;

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id)
        {
            _ref = promiseRef;
            _id = id;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    partial struct Promise<T>
    {
        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly Internal.PromiseRef _ref;
        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly short _id;
        /// <summary>
        /// Internal use.
        /// </summary>
        internal readonly T _result;

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id)
        {
            _ref = promiseRef;
            _id = id;
            _result = default(T);
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        internal Promise(Internal.PromiseRef promiseRef, short id, ref T value)
        {
            _ref = promiseRef;
            _id = id;
            _result = value;
        }
    }

    partial class Internal
    {
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
                    _promiseId = _deferredId = initialId;
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
                    internal bool _suppressRejection;
                    [FieldOffset(2)]
                    internal bool _wasAwaitedOrForgotten;
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
                private readonly object _branchLocker = new object();
                private ValueLinkedStack<ITreeHandleable> _nextBranches;

#if PROMISE_PROGRESS
                internal Fixed32 _currentProgress;
                private readonly object _progressCollectionLocker = new object();
                private ValueLinkedQueue<IProgressListener> _progressListeners; // TODO: change to ValueLinkedStack to use less memory. Make sure progress is still invoked in order.

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
            partial class PromiseResolve<TResolver> : PromiseBranch
                where TResolver : IDelegateResolve
            {
                private TResolver _resolver;
            }

            partial class PromiseResolvePromise<TResolver> : PromiseWaitPromise
                where TResolver : IDelegateResolvePromise
            {
                private TResolver _resolver;
            }

            partial class PromiseResolveReject<TResolver, TRejecter> : PromiseBranch
                where TResolver : IDelegateResolve
                where TRejecter : IDelegateReject
            {
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class PromiseResolveRejectPromise<TResolver, TRejecter> : PromiseWaitPromise
                where TResolver : IDelegateResolvePromise
                where TRejecter : IDelegateRejectPromise
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

            partial class CancelablePromiseResolve<TResolver> : PromiseBranch
                where TResolver : IDelegateResolve
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
            }

            partial class CancelablePromiseResolvePromise<TResolver> : PromiseWaitPromise
                where TResolver : IDelegateResolvePromise
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
            }

            partial class CancelablePromiseResolveReject<TResolver, TRejecter> : PromiseBranch
                where TResolver : IDelegateResolve
                where TRejecter : IDelegateReject
            {
                private CancelationHelper _cancelationHelper;
                private TResolver _resolver;
                private TRejecter _rejecter;
            }

            partial class CancelablePromiseResolveRejectPromise<TResolver, TRejecter> : PromiseWaitPromise
                where TResolver : IDelegateResolvePromise
                where TRejecter : IDelegateRejectPromise
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