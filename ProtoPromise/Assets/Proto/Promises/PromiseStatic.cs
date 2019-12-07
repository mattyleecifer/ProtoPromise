﻿#if !PROTO_PROMISE_CANCEL_DISABLE
#define PROMISE_CANCEL
#endif

using System;
using System.Collections.Generic;
using Proto.Utils;

namespace Proto.Promises
{
    public partial class Promise
    {
        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
		public static Promise All(params Promise[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.AllPromise0.GetOrCreate(new ArrayEnumerator<Promise>(promises), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(IEnumerable<Promise> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.AllPromise0.GetOrCreate(promises.GetEnumerator(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise AllNonAlloc<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.AllPromise0.GetOrCreate(promises, 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve with a list of values in the same order as <paramref name="promises"/> when they have all resolved.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<IList<T>> All<T>(params Promise<T>[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.AllPromise<T>.GetOrCreate(new ArrayEnumerator<Promise<T>>(promises), new List<T>(promises.Length), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve with a list of values in the same order as <paramref name="promises"/>s when they have all resolved.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<IList<T>> All<T>(IEnumerable<Promise<T>> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.AllPromise<T>.GetOrCreate(promises.GetEnumerator(), new List<T>(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve with <paramref name="valueContainer"/> in the same order as <paramref name="promises"/> when they have all resolved.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<IList<T>> AllNonAlloc<T, TEnumerator>(TEnumerator promises, IList<T> valueContainer) where TEnumerator : IEnumerator<Promise<T>>
        {
            ValidateArgument(promises, "promises", 1);
            ValidateArgument(valueContainer, "valueContainer", 1);
            return Internal.AllPromise<T>.GetOrCreate(promises, valueContainer, 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(params Promise[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise0.GetOrCreate(new ArrayEnumerator<Promise>(promises), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(IEnumerable<Promise> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise0.GetOrCreate(promises.GetEnumerator(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise RaceNonAlloc<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise0.GetOrCreate(promises, 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<T> Race<T>(params Promise<T>[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise<T>.GetOrCreate(new ArrayEnumerator<Promise<T>>(promises), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<T> Race<T>(IEnumerable<Promise<T>> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise<T>.GetOrCreate(promises.GetEnumerator(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If any <see cref="Promise{T}"/> is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<T> RaceNonAlloc<T, TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise<T>>
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.RacePromise<T>.GetOrCreate(promises, 1);
        }

        /// <summary>
        /// Runs <paramref name="funcs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Sequence(params Func<Promise>[] funcs)
        {
            ValidateArgument(funcs, "funcs", 1);
            return Internal.SequencePromise0.GetOrCreate(new ArrayEnumerator<Func<Promise>>(funcs), 1);
        }

        /// <summary>
        /// Runs <paramref name="funcs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Sequence(IEnumerable<Func<Promise>> funcs)
        {
            ValidateArgument(funcs, "funcs", 1);
            return Internal.SequencePromise0.GetOrCreate(funcs.GetEnumerator(), 1);
        }

        /// <summary>
        /// Runs <paramref name="funcs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any <see cref="Promise"/> is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise SequenceNonAlloc<TEnumerator>(TEnumerator funcs) where TEnumerator : IEnumerator<Func<Promise>>
        {
            ValidateArgument(funcs, "funcs", 1);
            return Internal.SequencePromise0.GetOrCreate(funcs, 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(params Promise[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise0.GetOrCreate(new ArrayEnumerator<Promise>(promises), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(IEnumerable<Promise> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise0.GetOrCreate(promises.GetEnumerator(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise FirstNonAlloc<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise0.GetOrCreate(promises, 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise{T}"/> that is rejected or canceled.
        /// </summary>
        public static Promise<T> First<T>(params Promise<T>[] promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise<T>.GetOrCreate(new ArrayEnumerator<Promise<T>>(promises), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise{T}"/> that is rejected or canceled.
        /// </summary>
        public static Promise<T> First<T>(IEnumerable<Promise<T>> promises)
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise<T>.GetOrCreate(promises.GetEnumerator(), 1);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve when the first of the <paramref name="promises"/> has resolved with the same value as that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise{T}"/> that is rejected or canceled.
        /// </summary>
        public static Promise<T> FirstNonAlloc<T, TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise<T>>
        {
            ValidateArgument(promises, "promises", 1);
            return Internal.FirstPromise<T>.GetOrCreate(promises, 1);
        }

        // TODO
        //public static Promise<T1> Merge<T1>(Promise<T1> promise1, Promise promise2)
        //{

        //}
        //public static Promise<ValueTuple<T1, T2>> Merge<T1, T2>(Promise<T1> promise1, Promise<T2> promise2)
        //{

        //}

        /// <summary>
        /// Returns a new <see cref="Promise"/>. <paramref name="resolver"/> is invoked immediately with a <see cref="Deferred"/> that controls the state of the promise.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be rejected with that <see cref="Exception"/>.
        /// </summary>
		public static Promise New(Action<Deferred> resolver)
        {
            var promise = Internal.DeferredPromise0.GetOrCreate(1);
            try
            {
                resolver.Invoke(promise.Deferred);
            }
            catch (Exception e)
            {
                var deferred = promise.Deferred;
                if (deferred.State == State.Pending)
                {
                    deferred.Reject(e);
                }
                else
                {
                    var rejectValue = Internal.UnhandledExceptionException.GetOrCreate(e);
                    _SetStackTraceFromCreated(promise, rejectValue);
                    AddRejectionToUnhandledStack(rejectValue);
                }
            }
            return promise;
        }

        /// <summary>
        /// Returns a new <see cref="Promise{T}"/>. <paramref name="resolver"/> is invoked immediately with a <see cref="Deferred"/> that controls the state of the promise.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be rejected with that <see cref="Exception"/>.
        /// </summary>
		public static Promise<T> New<T>(Action<Promise<T>.Deferred> resolver)
        {
            var promise = Internal.DeferredPromise<T>.GetOrCreate(1);
            try
            {
                resolver.Invoke(promise.Deferred);
            }
            catch (Exception e)
            {
                var deferred = promise.Deferred;
                if (deferred.State == State.Pending)
                {
                    deferred.Reject(e);
                }
                else
                {
                    var rejectValue = Internal.UnhandledExceptionException.GetOrCreate(e);
                    _SetStackTraceFromCreated(promise, rejectValue);
                    AddRejectionToUnhandledStack(rejectValue);
                }
            }
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already resolved.
        /// </summary>
		public static Promise Resolved()
        {
            // Reuse a single resolved instance.
            return Internal.ResolvedPromise.instance;
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that is already resolved with <paramref name="value"/>.
        /// </summary>
		public static Promise<T> Resolved<T>(T value)
        {
            var promise = Internal.LitePromise<T>.GetOrCreate(1);
            promise.ResolveDirect(value);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already rejected without a reason.
        /// </summary>
        public static Promise Rejected()
        {
            var promise = Internal.LitePromise0.GetOrCreate(1);
            var rejection = CreateRejection(1);
            promise.RejectDirect(rejection);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already rejected with <paramref name="reason"/>.
        /// </summary>
        public static Promise Rejected<TReject>(TReject reason)
        {
            var promise = Internal.LitePromise0.GetOrCreate(1);
            var rejection = CreateRejection(reason, 1);
            promise.RejectDirect(rejection);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that is already rejected without a reason.
        /// </summary>
        public static Promise<T> Rejected<T>()
        {
            var promise = Internal.LitePromise<T>.GetOrCreate(1);
            var rejection = CreateRejection(1);
            promise.RejectDirect(rejection);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that is already rejected with <paramref name="reason"/>.
        /// </summary>
        public static Promise<T> Rejected<T, TReject>(TReject reason)
        {
            var promise = Internal.LitePromise<T>.GetOrCreate(1);
            var rejection = CreateRejection(reason, 1);
            promise.RejectDirect(rejection);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already canceled without a reason.
        /// </summary>
#if !PROMISE_CANCEL
            [Obsolete("Cancelations are disabled. Remove PROTO_PROMISE_CANCEL_DISABLE from your compiler symbols to enable cancelations.", true)]
#endif
        public static Promise Canceled()
        {
            ValidateCancel(1);

            var promise = Internal.LitePromise0.GetOrCreate(1);
            promise.Cancel();
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already canceled with <paramref name="reason"/>.
        /// </summary>
#if !PROMISE_CANCEL
        [Obsolete("Cancelations are disabled. Remove PROTO_PROMISE_CANCEL_DISABLE from your compiler symbols to enable cancelations.", true)]
#endif
        public static Promise Canceled<TCancel>(TCancel reason)
        {
            ValidateCancel(1);

            var promise = Internal.LitePromise0.GetOrCreate(1);
            promise.Cancel(reason);
            return promise;
        }

        /// <summary>
        /// Returns a new <see cref="Promise{T}"/> that will be canceled without a reason.
        /// </summary>
#if !PROMISE_CANCEL
        [Obsolete("Cancelations are disabled. Remove PROTO_PROMISE_CANCEL_DISABLE from your compiler symbols to enable cancelations.", true)]
#endif
        public static Promise<T> Canceled<T>()
        {
            ValidateCancel(1);

            var promise = Internal.LitePromise<T>.GetOrCreate(1);
            promise.Cancel();
            return promise;
        }

        /// <summary>
        /// Returns a new <see cref="Promise{T}"/> that will be canceled with <paramref name="reason"/>.
        /// </summary>
#if !PROMISE_CANCEL
        [Obsolete("Cancelations are disabled. Remove PROTO_PROMISE_CANCEL_DISABLE from your compiler symbols to enable cancelations.", true)]
#endif
        public static Promise<T> Canceled<T, TCancel>(TCancel reason)
        {
            ValidateCancel(1);

            var promise = Internal.LitePromise<T>.GetOrCreate(1);
            promise.Cancel(reason);
            return promise;
        }

        /// <summary>
        /// Returns a <see cref="Deferred"/> object that is linked to and controls the state of a new <see cref="Promise"/>.
        /// </summary>
		public static Deferred NewDeferred()
        {
            return Internal.DeferredPromise0.GetOrCreate(1).Deferred;
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}.Deferred"/> object that is linked to and controls the state of a new <see cref="Promise{T}"/>.
        /// </summary>
        public static Promise<T>.Deferred NewDeferred<T>()
        {
            return Internal.DeferredPromise<T>.GetOrCreate(1).Deferred;
        }
    }
}