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

#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Proto.Promises
{
    /// <summary>
    /// A <see cref="Promise{T}"/> represents the eventual result of an asynchronous operation.
    /// The primary way of interacting with a <see cref="Promise{T}"/> is through its then method,
    /// which registers callbacks to be invoked with its resolve value when the <see cref="Promise{T}"/> is resolved,
    /// or the reason why the <see cref="Promise{T}"/> cannot be resolved.
    /// </summary>
    public
#if CSHARP_7_3_OR_NEWER
        readonly
#endif
        partial struct Promise<T> : IEquatable<Promise<T>>
    {
        public bool IsValid
        {
            get
            {
                var _this = GetVoidCopy();
                return _this._id == (_this._ref == null ? Internal.ValidIdFromApi : _this._ref.Id);
            }
        }

        /// <summary>
        /// Cast to <see cref="Promise"/>.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise AsPromise()
        {
            return new Promise(_ref, _id);
        }

        /// <summary>
        /// Cast to <see cref="Promise"/>.
        /// </summary>
        public static implicit operator Promise(Promise<T> rhs)
        {
            return rhs.AsPromise();
        }

        public override string ToString()
        {
            var _this = GetVoidCopy();
            string state =
                !_this.IsValid ? "Invalid"
                : _this._ref != null ? _this._ref.State.ToString()
                : Promise.State.Resolved.ToString();
            return string.Format("Type: Promise<{0}>, State: {1}", typeof(T), state);
        }

        /// <summary>
        /// Mark <see cref="this"/> as awaited and get a new <see cref="Promise{T}"/> of <typeparamref name="T"/> that inherits the state of <see cref="this"/> and can be awaited multiple times until <see cref="Forget"/> is called on it.
        /// <para/><see cref="Forget"/> must be called when you are finished with it.
        /// <para/>NOTE: You should not return a preserved <see cref="Promise{T}"/> from a public API. Use <see cref="Duplicate"/> to get a <see cref="Promise{T}"/> that is publicly safe.
        /// </summary>
        public Promise<T> Preserve()
        {
            ValidateOperation(1);
            var _this = GetVoidCopy();
            if (_this._ref != null)
            {
                var newPromise = _this._ref.GetPreserved(_this._id);
                return new Promise<T>(newPromise, newPromise.Id);
            }
            return this;
        }

        /// <summary>
        /// Mark <see cref="this"/> as awaited and prevent any further awaits or callbacks on <see cref="this"/>.
        /// <para/>NOTE: It is imperative to terminate your promise chains with Forget so that any uncaught rejections will be reported and objects repooled (if pooling is enabled).
        /// </summary>
        public void Forget()
        {
            ValidateOperation(1);
            var _this = GetVoidCopy();
            if (_this._ref != null)
            {
                _this._ref.Forget(_this._id);
            }
        }


        /// <summary>
        /// Mark <see cref="this"/> as awaited and get a new <see cref="Promise{T}"/> of <typeparamref name="T"/> that inherits the state of <see cref="this"/> and can be awaited once.
        /// <para/>Preserved promises are unsafe to return from public APIs. Use <see cref="Duplicate"/> to get a <see cref="Promise{T}"/> that is publicly safe.
        /// <para/><see cref="Duplicate"/> is safe to call even if you are unsure if <see cref="this"/> is preserved.
        /// </summary>
        public Promise<T> Duplicate()
        {
            ValidateOperation(1);
            var _this = GetVoidCopy();
            if (_ref != null)
            {
                var newPromise = _this._ref.GetDuplicate(_this._id);
                return new Promise<T>(newPromise, newPromise.Id);
            }
            return this;
        }

        /// <summary>
        /// Add a progress listener. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/><paramref name="progressListener"/> will be reported with progress that is normalized between 0 and 1 from this and all previous waiting promises in the chain.
        /// 
        /// <para/>If/when this is resolved, <paramref name="onProgress"/> will be invoked with <paramref name="progressCaptureValue"/> and 1.0, then the new <see cref="Promise"/> will be resolved when it returns.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, progress will stop being reported.
        /// </summary>
#if !PROMISE_PROGRESS
        [Obsolete("Progress is disabled, progress will not be reported. Remove PROTO_PROMISE_PROGRESS_DISABLE from your compiler symbols to enable progress reports.", false)]
#endif
        public Promise<T> Progress<TProgress>(TProgress progressListener, CancelationToken cancelationToken = default(CancelationToken)) where TProgress : IProgress<float>
        {
            ValidateArgument(progressListener, "progressListener", 1);

#if !PROMISE_PROGRESS
            return Duplicate();
#else
            ValidateOperation(1);

            return Internal.PromiseRef.CallbackHelper.AddProgress(this, progressListener, cancelationToken);
#endif
        }

        /// <summary>
        /// Add a progress listener. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/><paramref name="onProgress"/> will be invoked with <paramref name="progressCaptureValue"/> and progress that is normalized between 0 and 1 from this and all previous waiting promises in the chain.
        /// 
        /// <para/>If/when this is resolved, <paramref name="onProgress"/> will be invoked with <paramref name="progressCaptureValue"/> and 1.0, then the new <see cref="Promise"/> will be resolved when it returns.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, <paramref name="onProgress"/> will stop being invoked.
        /// </summary>
#if !PROMISE_PROGRESS
        [Obsolete("Progress is disabled, onProgress will not be invoked. Remove PROTO_PROMISE_PROGRESS_DISABLE from your compiler symbols to enable progress reports.", false)]
#endif
        [MethodImpl(Internal.InlineOption)]
        public Promise<T> Progress(Action<float> onProgress, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateArgument(onProgress, "onProgress", 1);

            return Progress(new Internal.PromiseRef.DelegateProgress(onProgress), cancelationToken);
        }

        /// <summary>
        /// Add a cancel callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/> that inherits the state of <see cref="this"/> and can be awaited once.
        /// <para/>If/when this instance is canceled, <paramref name="onCanceled"/> will be invoked with the cancelation reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise<T> CatchCancelation(Promise.CanceledAction onCanceled, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, "onCanceled", 1);

            return Internal.PromiseRef.CallbackHelper.AddCancel(this, new Internal.PromiseRef.DelegateCancel(onCanceled), cancelationToken);
        }

        /// <summary>
        /// Add a finally callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked.
        /// <para/>If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise{T}"/> will be rejected with that exception,
        /// otherwise it will be resolved, rejected, or canceled with the same value or reason as this.
        /// </summary>
        public Promise<T> Finally(Action onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, "onFinally", 1);

            return Internal.PromiseRef.CallbackHelper.AddFinally(this, new Internal.PromiseRef.DelegateFinally(onFinally));
        }

        #region Resolve Callbacks
        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then(Action<T> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolve(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, TResult> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolve(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<T, Promise> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, Promise<TResult>> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch(Func<T> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.CreatePassthrough(), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TReject>(Func<TReject, T> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.CreatePassthrough(), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch(Func<Promise<T>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.CreatePassthrough(), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is canceled or rejected with any other reason or no reason, the new <see cref="Promise{T}"/> will be canceled or rejected with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TReject>(Func<TReject, Promise<T>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.CreatePassthrough(), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Action<T> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Action<T> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<T, TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<T, Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Func<T, Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<T, Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Action<T> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Action<T> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<T, TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then(Func<T, Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TReject>(Func<T, Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult>(Func<T, Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TResult, TReject>(Func<T, Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this, Internal.PromiseRef.DelegateWrapper.Create(onResolved), Internal.PromiseRef.DelegateWrapper.Create(onRejected), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Continue Callbacks
        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith(ContinueAction onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinue(this, new Internal.PromiseRef.DelegateContinueArgVoid<T>(onContinue), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TResult>(ContinueFunc<TResult> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinue(this, new Internal.PromiseRef.DelegateContinueArgResult<T, TResult>(onContinue), cancelationToken, out newPromise);
            return newPromise;
        }


        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith(ContinueFunc<Promise> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinueWait(this, new Internal.PromiseRef.DelegateContinueArgPromise<T>(onContinue), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TResult>(ContinueFunc<Promise<TResult>> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinueWait(this, new Internal.PromiseRef.DelegateContinueArgPromiseT<T, TResult>(onContinue), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        // Capture values below.

        /// <summary>
        /// Capture a value and add a progress listener. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/><paramref name="onProgress"/> will be invoked with <paramref name="progressCaptureValue"/> and progress that is normalized between 0 and 1 from this and all previous waiting promises in the chain.
        /// 
        /// <para/>If/when this is resolved, <paramref name="onProgress"/> will be invoked with <paramref name="progressCaptureValue"/> and 1.0, then the new <see cref="Promise"/> will be resolved when it returns.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, <paramref name="onProgress"/> will stop being invoked.
        /// </summary>
#if !PROMISE_PROGRESS
        [Obsolete("Progress is disabled, onProgress will not be invoked. Remove PROTO_PROMISE_PROGRESS_DISABLE from your compiler symbols to enable progress reports.", false)]
#endif
        [MethodImpl(Internal.InlineOption)]
        public Promise<T> Progress<TCaptureProgress>(TCaptureProgress progressCaptureValue, Action<TCaptureProgress, float> onProgress, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateArgument(onProgress, "onProgress", 1);

            return Progress(new Internal.PromiseRef.DelegateCaptureProgress<TCaptureProgress>(ref progressCaptureValue, onProgress), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a cancel callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/> that inherits the state of <see cref="this"/> and can be awaited once.
        /// <para/>If/when this instance is canceled, <paramref name="onCanceled"/> will be invoked with <paramref name="cancelCaptureValue"/> and the cancelation reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, <paramref name="onCanceled"/> will not be invoked.
        /// </summary>
        public Promise<T> CatchCancelation<TCaptureCancel>(TCaptureCancel cancelCaptureValue, Promise.CanceledAction<TCaptureCancel> onCanceled, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onCanceled, "onCanceled", 1);

            return Internal.PromiseRef.CallbackHelper.AddCancel(this, new Internal.PromiseRef.DelegateCaptureCancel<TCaptureCancel>(ref cancelCaptureValue, onCanceled), cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a finally callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onFinally"/> will be invoked with <paramref name="finallyCaptureValue"/>.
        /// <para/>If <paramref name="onFinally"/> throws an exception, the new <see cref="Promise{T}"/> will be rejected with that exception,
        /// otherwise it will be resolved, rejected, or canceled with the same value or reason as this.
        /// </summary>
        public Promise<T> Finally<TCaptureFinally>(TCaptureFinally finallyCaptureValue, Action<TCaptureFinally> onFinally)
        {
            ValidateOperation(1);
            ValidateArgument(onFinally, "onFinally", 1);

            return Internal.PromiseRef.CallbackHelper.AddFinally(this, new Internal.PromiseRef.DelegateCaptureFinally<TCaptureFinally>(ref finallyCaptureValue, onFinally));
        }

        #region Resolve Callbacks
        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolve(this, Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolve(this, Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveWait(this, Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveWait(this, Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, T> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, T> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<T>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="T"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise{T}"/> will be resolved with the resolve value.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<T> Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<T>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<T> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.CreatePassthrough(),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Action<T> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Action<T> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveReject(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Func<T, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Func<T, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Action<T> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Action<T> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve, T> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject>(Func<T, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureReject, TReject>(Func<T, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/> and the resolve value, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, T, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onResolved, "onResolved", 1);
            ValidateArgument(onRejected, "onRejected", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddResolveRejectWait(this,
                Internal.PromiseRef.DelegateWrapper.Create(ref resolveCaptureValue, onResolved),
                Internal.PromiseRef.DelegateWrapper.Create(ref rejectCaptureValue, onRejected),
                cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        #region Continue Callbacks
        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith<TCapture>(TCapture continueCaptureValue, ContinueAction<TCapture> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinue(this, new Internal.PromiseRef.DelegateContinueCaptureArgVoid<TCapture, T>(ref continueCaptureValue, onContinue), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TCapture, TResult>(TCapture continueCaptureValue, ContinueFunc<TCapture, TResult> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinue(this, new Internal.PromiseRef.DelegateContinueCaptureArgResult<TCapture, T, TResult>(ref continueCaptureValue, onContinue), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Capture a value and add a continuation callback. Returns a new <see cref="Promise"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise ContinueWith<TCapture>(TCapture continueCaptureValue, ContinueFunc<TCapture, Promise> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinueWait(this, new Internal.PromiseRef.DelegateContinueCaptureArgPromise<TCapture, T>(ref continueCaptureValue, onContinue), cancelationToken, out newPromise);
            return newPromise;
        }

        /// <summary>
        /// Add a continuation callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>When this is resolved, rejected, or canceled, <paramref name="onContinue"/> will be invoked with <paramref name="continueCaptureValue"/> and the <see cref="ResultContainer"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onContinue"/> will not be invoked.
        /// </summary>
        public Promise<TResult> ContinueWith<TCapture, TResult>(TCapture continueCaptureValue, ContinueFunc<TCapture, Promise<TResult>> onContinue, CancelationToken cancelationToken = default(CancelationToken))
        {
            ValidateOperation(1);
            ValidateArgument(onContinue, "onContinue", 1);

            Promise<TResult> newPromise;
            Internal.PromiseRef.CallbackHelper.AddContinueWait(this, new Internal.PromiseRef.DelegateContinueCaptureArgPromiseT<TCapture, T, TResult>(ref continueCaptureValue, onContinue), cancelationToken, out newPromise);
            return newPromise;
        }
        #endregion

        /// <summary>
        /// Returns typeof(<typeparamref name="T"/>).
        /// </summary>
        [Obsolete]
        public Type ResultType { get { return typeof(T); } }

        [Obsolete("Retain is no longer valid, use Preserve instead.", true)]
        public void Retain()
        {
            throw new InvalidOperationException("Retain is no longer valid, use Preserve instead.", Internal.GetFormattedStacktrace(1));
        }

        [Obsolete("Release is no longer valid, use Forget instead.", true)]
        public void Release()
        {
            throw new InvalidOperationException("Release is no longer valid, use Preserve instead.", Internal.GetFormattedStacktrace(1));
        }
    }

    // Inherited from Promise (must copy since structs cannot inherit).
    // Did not copy Progress, CatchCancelation, Finally, or ContinueWith.
    partial struct Promise<T>
    {
        #region Resolve Callbacks
        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Action onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Func<Promise> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, cancelationToken);
        }

        /// <summary>
        /// Add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, cancelationToken);
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch(Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TReject>(Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch(Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TReject>(Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(onRejected, cancelationToken);
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Action onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TReject>(Action onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult, TReject>(Func<TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Func<Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TReject>(Func<Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult, TReject>(Func<Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Action onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TReject>(Action onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult, TReject>(Func<TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then(Func<Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TReject>(Func<Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult>(Func<Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If if throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>, unless it is a Special Exception (see README).
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TResult, TReject>(Func<Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, onRejected, cancelationToken);
        }
        #endregion

        // Capture values below.

        #region Resolve Callbacks
        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, cancelationToken);
        }
        #endregion

        #region Reject Callbacks
        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TCaptureReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, the new <see cref="Promise"/> will be resolved.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Catch<TCaptureReject, TReject>(TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Catch(rejectCaptureValue, onRejected, cancelationToken);
        }
        #endregion

        #region Resolve or Reject Callbacks
        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject>(Action onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject, TReject>(Action onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject, TReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Func<Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject>(Action onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, Func<TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject, TReject>(Action onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Action<TCaptureResolve> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, Func<TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, TResult> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, Promise<TResult>> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Action onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, Action<TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureReject, TReject>(Func<Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise"/> will adopt the state of the returned <see cref="Promise"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise"/> will be resolved when it returns.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise Then<TCaptureResolve, TCaptureReject, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise> onResolved, TCaptureReject rejectCaptureValue, Action<TCaptureReject, TReject> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/>, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, Func<TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture a value and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureReject, TResult, TReject>(Func<Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }

        /// <summary>
        /// Capture 2 values and add a resolve and a reject callback. Returns a new <see cref="Promise{T}"/> of <typeparamref name="TResult"/>.
        /// <para/>If/when this is resolved, <paramref name="onResolved"/> will be invoked with <paramref name="resolveCaptureValue"/>, and the new <see cref="Promise{T}"/> will adopt the state of the returned <see cref="Promise{T}"/>.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// <para/>If/when this is rejected with any reason that is convertible to <typeparamref name="TReject"/>, <paramref name="onRejected"/> will be invoked with <paramref name="rejectCaptureValue"/> and that reason, and the new <see cref="Promise{T}"/> will be resolved with the returned value.
        /// If it throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// If this is rejected with any other reason, the new <see cref="Promise{T}"/> will be rejected with the same reason.
        /// <para/>If/when this is canceled with any reason or no reason, the new <see cref="Promise{T}"/> will be canceled with the same reason.
        ///
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while this is pending, the new <see cref="Promise{T}"/> will be canceled with its reason, and <paramref name="onResolved"/> and <paramref name="onRejected"/> will not be invoked.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public Promise<TResult> Then<TCaptureResolve, TCaptureReject, TResult, TReject>(TCaptureResolve resolveCaptureValue, Func<TCaptureResolve, Promise<TResult>> onResolved, TCaptureReject rejectCaptureValue, Func<TCaptureReject, TReject, TResult> onRejected, CancelationToken cancelationToken = default(CancelationToken))
        {
            return AsPromise().Then(resolveCaptureValue, onResolved, rejectCaptureValue, onRejected, cancelationToken);
        }
        #endregion

        [MethodImpl(Internal.InlineOption)]
        public bool Equals(Promise<T> other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
#if CSHARP_7_3_OR_NEWER
            return obj is Promise<T> promise && Equals(promise);
#else
            return obj is Promise<T> && Equals((Promise<T>) obj);
#endif
        }

        // Promises really shouldn't be used for lookups, but GetHashCode is overridden to complement ==.
        public override int GetHashCode()
        {
            unchecked
            {
                Promise<T> _this = this;
                int hash = 17;
                hash = hash * 31 + _id.GetHashCode();
                if (_this._ref != null)
                {
                    hash = hash * 31 + _this._ref.GetHashCode();
                }
                else if (_this._result != null)
                {
                    hash = hash * 31 + EqualityComparer<T>.Default.GetHashCode(_this._result);
                }
                hash = hash * 31 + typeof(T).TypeHandle.GetHashCode(); // Hashcode variance for different T types.
                return hash;
            }
        }

        public static bool operator ==(Promise<T> lhs, Promise<T> rhs)
        {
            return lhs._ref == rhs._ref
                & lhs._id == rhs._id
                & EqualityComparer<T>.Default.Equals(lhs._result, rhs._result);
        }

        [MethodImpl(Internal.InlineOption)]
        public static bool operator !=(Promise<T> lhs, Promise<T> rhs)
        {
            return !(lhs == rhs);
        }

        // Defensive copy for thread safety purposes.
        [MethodImpl(Internal.InlineOption)]
        private Promise<Internal.VoidResult> GetVoidCopy()
        {
            return new Promise<Internal.VoidResult>(_ref, _id);
        }
    }
}