﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable CA1507 // Use nameof to express symbol names
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Proto.Promises
{
    public partial struct Promise
    {
        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(Promise promise1, Promise promise2)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, promise2.Depth);

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            if (promise1._ref == null | promise2._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));

            var promise = Internal.PromiseRefBase.RacePromise<Internal.VoidResult>.GetOrCreate(passThroughs, 2, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(Promise promise1, Promise promise2, Promise promise3)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, promise3.Depth));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));

            var promise = Internal.PromiseRefBase.RacePromise<Internal.VoidResult>.GetOrCreate(passThroughs, 3, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(Promise promise1, Promise promise2, Promise promise3, Promise promise4)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, Math.Min(promise3.Depth, promise4.Depth)));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            ValidateArgument(promise4, "promise4", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null | promise4._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise4._ref, promise4._id, false);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise4, 3));

            var promise = Internal.PromiseRefBase.RacePromise<Internal.VoidResult>.GetOrCreate(passThroughs, 4, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(params Promise[] promises)
        {
            return Race(promises.GetGenericEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race(IEnumerable<Promise> promises)
        {
            return Race(promises.GetEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Race<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);

            try
            {
                if (!promises.MoveNext())
                {
                    throw new EmptyArgumentException("promises", "You must provide at least one element to Race.", Internal.GetFormattedStacktrace(1));
                }
                var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
                int pendingCount = 0;
                ushort minDepth = ushort.MaxValue;

                int index = -1; // Index isn't necessary for Race, but might help with debugging.
                do
                {
                    var p = promises.Current;
                    ValidateElement(p, "promises", 1);
                    if (!Internal.TryPrepareForRace(p, ref passThroughs, ++index, ref minDepth))
                    {
                        // Validate and release remaining elements.
                        while (promises.MoveNext())
                        {
                            p = promises.Current;
                            ValidateElement(p, "promises", 1);
                            Internal.MaybeMarkAwaitedAndDispose(p._ref, p._id, false);
                            minDepth = Math.Min(minDepth, p.Depth);
                        }
                        // Repool any created passthroughs.
                        foreach (var passthrough in passThroughs)
                        {
                            passthrough.Dispose();
                        }
                        return Internal.CreateResolved(minDepth);
                    }
                    ++pendingCount;
                } while (promises.MoveNext());

                var promise = Internal.PromiseRefBase.RacePromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, minDepth);
                return new Promise(promise, promise.Id, minDepth);
            }
            finally
            {
                promises.Dispose();
            }
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex(Promise promise1, Promise promise2)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, promise2.Depth);

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            if (promise1._ref == null | promise2._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                int resolveIndex = promise1._ref == null ? 0 : 1;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));

            var promise = Internal.PromiseRefBase.RacePromiseWithIndexVoid.GetOrCreate(passThroughs, 2, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex(Promise promise1, Promise promise2, Promise promise3)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, promise3.Depth));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                int resolveIndex = promise1._ref == null ? 0
                    : promise2._ref == null ? 1
                    : 2;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));

            var promise = Internal.PromiseRefBase.RacePromiseWithIndexVoid.GetOrCreate(passThroughs, 3, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex(Promise promise1, Promise promise2, Promise promise3, Promise promise4)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, Math.Min(promise3.Depth, promise4.Depth)));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            ValidateArgument(promise4, "promise4", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null | promise4._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise4._ref, promise4._id, false);
                int resolveIndex = promise1._ref == null ? 0
                    : promise2._ref == null ? 1
                    : promise3._ref == null ? 2
                    : 3;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise4, 3));

            var promise = Internal.PromiseRefBase.RacePromiseWithIndexVoid.GetOrCreate(passThroughs, 4, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex(params Promise[] promises)
        {
            return RaceWithIndex(promises.GetGenericEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex(IEnumerable<Promise> promises)
        {
            return RaceWithIndex(promises.GetEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<int> RaceWithIndex<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);

            try
            {
                if (!promises.MoveNext())
                {
                    throw new EmptyArgumentException("promises", "You must provide at least one element to RaceWithIndex.", Internal.GetFormattedStacktrace(1));
                }
                var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
                int pendingCount = 0;
                ushort minDepth = ushort.MaxValue;

                int index = -1;
                do
                {
                    var p = promises.Current;
                    ValidateElement(p, "promises", 1);
                    if (!Internal.TryPrepareForRace(p, ref passThroughs, ++index, ref minDepth))
                    {
                        // Validate and release remaining elements.
                        while (promises.MoveNext())
                        {
                            p = promises.Current;
                            ValidateElement(p, "promises", 1);
                            Internal.MaybeMarkAwaitedAndDispose(p._ref, p._id, false);
                            minDepth = Math.Min(minDepth, p.Depth);
                        }
                        // Repool any created passthroughs.
                        foreach (var passthrough in passThroughs)
                        {
                            passthrough.Dispose();
                        }
                        return Internal.CreateResolved(index, minDepth);
                    }
                    ++pendingCount;
                } while (promises.MoveNext());

                var promise = Internal.PromiseRefBase.RacePromiseWithIndexVoid.GetOrCreate(passThroughs, pendingCount, minDepth);
                return new Promise<int>(promise, promise.Id, minDepth);
            }
            finally
            {
                promises.Dispose();
            }
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T>(Promise<T> promise1, Promise<T> promise2)
        {
            return Promise<T>.Race(promise1, promise2);
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3)
        {
            return Promise<T>.Race(promise1, promise2, promise3);
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3, Promise<T> promise4)
        {
            return Promise<T>.Race(promise1, promise2, promise3, promise4);
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T>(params Promise<T>[] promises)
        {
            return Promise<T>.Race(promises);
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T>(IEnumerable<Promise<T>> promises)
        {
            return Promise<T>.Race(promises);
        }

        [Obsolete("Prefer Promise<T>.Race()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Race<T, TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise<T>>
        {
            return Promise<T>.Race(promises);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(Promise promise1, Promise promise2)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, promise2.Depth);

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            if (promise1._ref == null | promise2._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, true);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));

            var promise = Internal.PromiseRefBase.FirstPromise<Internal.VoidResult>.GetOrCreate(passThroughs, 2, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(Promise promise1, Promise promise2, Promise promise3)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, promise3.Depth));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, true);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));

            var promise = Internal.PromiseRefBase.FirstPromise<Internal.VoidResult>.GetOrCreate(passThroughs, 3, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the promises has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(Promise promise1, Promise promise2, Promise promise3, Promise promise4)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, Math.Min(promise3.Depth, promise4.Depth)));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            ValidateArgument(promise4, "promise4", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null | promise4._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, true);
                Internal.MaybeMarkAwaitedAndDispose(promise4._ref, promise4._id, true);
                return Internal.CreateResolved(depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise4, 3));

            var promise = Internal.PromiseRefBase.FirstPromise<Internal.VoidResult>.GetOrCreate(passThroughs, 4, depth);
            return new Promise(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(params Promise[] promises)
        {
            return First(promises.GetGenericEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First(IEnumerable<Promise> promises)
        {
            return First(promises.GetEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise First<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);

            try
            {
                if (!promises.MoveNext())
                {
                    throw new EmptyArgumentException("promises", "You must provide at least one element to First.", Internal.GetFormattedStacktrace(1));
                }
                var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
                int pendingCount = 0;
                ushort minDepth = ushort.MaxValue;

                int index = -1; // Index isn't necessary for First, but might help with debugging.
                do
                {
                    var p = promises.Current;
                    ValidateElement(p, "promises", 1);
                    if (!Internal.TryPrepareForRace(p, ref passThroughs, ++index, ref minDepth))
                    {
                        // Validate and release remaining elements.
                        while (promises.MoveNext())
                        {
                            p = promises.Current;
                            ValidateElement(p, "promises", 1);
                            Internal.MaybeMarkAwaitedAndDispose(p._ref, p._id, true);
                            minDepth = Math.Min(minDepth, p.Depth);
                        }
                        // Repool any created passthroughs.
                        foreach (var passthrough in passThroughs)
                        {
                            passthrough.Dispose();
                        }
                        return Internal.CreateResolved(minDepth);
                    }
                    ++pendingCount;
                } while (promises.MoveNext());

                var promise = Internal.PromiseRefBase.FirstPromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, minDepth);
                return new Promise(promise, promise.Id, minDepth);
            }
            finally
            {
                promises.Dispose();
            }
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex(Promise promise1, Promise promise2)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, promise2.Depth);

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            if (promise1._ref == null | promise2._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                int resolveIndex = promise1._ref == null ? 0 : 1;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));

            var promise = Internal.PromiseRefBase.FirstPromiseWithIndexVoid.GetOrCreate(passThroughs, 2, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex(Promise promise1, Promise promise2, Promise promise3)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, promise3.Depth));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                int resolveIndex = promise1._ref == null ? 0
                    : promise2._ref == null ? 1
                    : 2;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));

            var promise = Internal.PromiseRefBase.FirstPromiseWithIndexVoid.GetOrCreate(passThroughs, 3, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex(Promise promise1, Promise promise2, Promise promise3, Promise promise4)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();

            ushort depth = Math.Min(promise1.Depth, Math.Min(promise2.Depth, Math.Min(promise3.Depth, promise4.Depth)));

            ValidateArgument(promise1, "promise1", 1);
            ValidateArgument(promise2, "promise2", 1);
            ValidateArgument(promise3, "promise3", 1);
            ValidateArgument(promise4, "promise4", 1);
            if (promise1._ref == null | promise2._ref == null | promise3._ref == null | promise4._ref == null)
            {
                Internal.MaybeMarkAwaitedAndDispose(promise1._ref, promise1._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise2._ref, promise2._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise3._ref, promise3._id, false);
                Internal.MaybeMarkAwaitedAndDispose(promise4._ref, promise4._id, false);
                int resolveIndex = promise1._ref == null ? 0
                    : promise2._ref == null ? 1
                    : promise3._ref == null ? 2
                    : 3;
                return Internal.CreateResolved(resolveIndex, depth);
            }
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise1, 0));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise2, 1));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise3, 2));
            passThroughs.Push(Internal.PromiseRefBase.PromisePassThrough.GetOrCreate(promise4, 3));

            var promise = Internal.PromiseRefBase.FirstPromiseWithIndexVoid.GetOrCreate(passThroughs, 4, depth);
            return new Promise<int>(promise, promise.Id, depth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex(params Promise[] promises)
        {
            return FirstWithIndex(promises.GetGenericEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when the first of the <paramref name="promises"/> has resolved.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex(IEnumerable<Promise> promises)
        {
            return FirstWithIndex(promises.GetEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="int"/> that will resolve when the first of the promises has resolved with the index of that promise.
        /// If all promises are rejected or canceled, the returned <see cref="Promise{T}"/> will be rejected or canceled with the same reason as the last <see cref="Promise"/> that is rejected or canceled.
        /// </summary>
        public static Promise<int> FirstWithIndex<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);

            try
            {
                if (!promises.MoveNext())
                {
                    throw new EmptyArgumentException("promises", "You must provide at least one element to FirstWithIndex.", Internal.GetFormattedStacktrace(1));
                }
                var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
                int pendingCount = 0;
                ushort minDepth = ushort.MaxValue;

                int index = -1;
                do
                {
                    var p = promises.Current;
                    ValidateElement(p, "promises", 1);
                    if (!Internal.TryPrepareForRace(p, ref passThroughs, ++index, ref minDepth))
                    {
                        // Validate and release remaining elements.
                        while (promises.MoveNext())
                        {
                            p = promises.Current;
                            ValidateElement(p, "promises", 1);
                            Internal.MaybeMarkAwaitedAndDispose(p._ref, p._id, false);
                            minDepth = Math.Min(minDepth, p.Depth);
                        }
                        // Repool any created passthroughs.
                        foreach (var passthrough in passThroughs)
                        {
                            passthrough.Dispose();
                        }
                        return Internal.CreateResolved(index, minDepth);
                    }
                    ++pendingCount;
                } while (promises.MoveNext());

                var promise = Internal.PromiseRefBase.FirstPromiseWithIndexVoid.GetOrCreate(passThroughs, pendingCount, minDepth);
                return new Promise<int>(promise, promise.Id, minDepth);
            }
            finally
            {
                promises.Dispose();
            }
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T>(Promise<T> promise1, Promise<T> promise2)
        {
            return Promise<T>.First(promise1, promise2);
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3)
        {
            return Promise<T>.First(promise1, promise2, promise3);
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3, Promise<T> promise4)
        {
            return Promise<T>.First(promise1, promise2, promise3, promise4);
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T>(params Promise<T>[] promises)
        {
            return Promise<T>.First(promises);
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T>(IEnumerable<Promise<T>> promises)
        {
            return Promise<T>.First(promises);
        }

        [Obsolete("Prefer Promise<T>.First()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> First<T, TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise<T>>
        {
            return Promise<T>.First(promises);
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Sequence(params Func<Promise>[] promiseFuncs)
        {
            return Sequence(default(CancelationToken), promiseFuncs.GetGenericEnumerator());
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled before all of the <paramref name="promiseFuncs"/> have been invoked,
        /// the <paramref name="promiseFuncs"/> will stop being invoked, and the returned <see cref="Promise"/> will be canceled with its reason.
        /// </summary>
        public static Promise Sequence(CancelationToken cancelationToken, params Func<Promise>[] promiseFuncs)
        {
            return Sequence(cancelationToken, promiseFuncs.GetGenericEnumerator());
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Sequence(IEnumerable<Func<Promise>> promiseFuncs)
        {
            return Sequence(default(CancelationToken), promiseFuncs.GetEnumerator());
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled before all of the <paramref name="promiseFuncs"/> have been invoked,
        /// the <paramref name="promiseFuncs"/> will stop being invoked, and the returned <see cref="Promise"/> will be canceled with its reason.
        /// </summary>
        public static Promise Sequence(CancelationToken cancelationToken, IEnumerable<Func<Promise>> promiseFuncs)
        {
            return Sequence(cancelationToken, promiseFuncs.GetEnumerator());
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise Sequence<TEnumerator>(TEnumerator promiseFuncs) where TEnumerator : IEnumerator<Func<Promise>>
        {
            return Sequence(default(CancelationToken), promiseFuncs);
        }

        /// <summary>
        /// Runs <paramref name="promiseFuncs"/> in sequence, returning a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// 
        /// <para/>If the <paramref name="cancelationToken"/> is canceled before all of the <paramref name="promiseFuncs"/> have been invoked,
        /// the <paramref name="promiseFuncs"/> will stop being invoked, and the returned <see cref="Promise"/> will be canceled with its reason.
        /// </summary>
        public static Promise Sequence<TEnumerator>(CancelationToken cancelationToken, TEnumerator promiseFuncs) where TEnumerator : IEnumerator<Func<Promise>>
        {
            ValidateArgument(promiseFuncs, "promiseFuncs", 2);

            try
            {
                if (!promiseFuncs.MoveNext())
                {
                    return Internal.CreateResolved(0);
                }

                // Invoke funcs and normalize the progress.
                var promise = new Promise(null, 0, Internal.NegativeOneDepth);
                do
                {
                    promise = promise.Then(promiseFuncs.Current, cancelationToken);
                } while (promiseFuncs.MoveNext());
                return promise;
            }
            finally
            {
                promiseFuncs.Dispose();
            }
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(Promise promise1, Promise promise2)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(Promise promise1, Promise promise2, Promise promise3)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(Promise promise1, Promise promise2, Promise promise3, Promise promise4)
        {
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(params Promise[] promises)
        {
            return All(promises.GetGenericEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All(IEnumerable<Promise> promises)
        {
            return All(promises.GetEnumerator());
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that will resolve when all <paramref name="promises"/> have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise All<TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise>
        {
            ValidateArgument(promises, "promises", 1);

            try
            {
                var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
                int pendingCount = 0;
                ulong completedProgress = 0;
                ulong totalProgress = 0;
                ushort maxDepth = 0;

                int index = -1;
                while (promises.MoveNext())
                {
                    var p = promises.Current;
                    ValidateElement(p, "promises", 1);
                    Internal.PrepareForMerge(p, ref passThroughs, ++index, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
                }

                if (pendingCount == 0)
                {
                    return Internal.CreateResolved(maxDepth);
                }
                var promise = Internal.PromiseRefBase.MergePromise<Internal.VoidResult>.GetOrCreate(passThroughs, pendingCount, completedProgress, totalProgress, maxDepth);
                return new Promise(promise, promise.Id, maxDepth);
            }
            finally
            {
                promises.Dispose();
            }
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T>(Promise<T> promise1, Promise<T> promise2, IList<T> valueContainer = null)
        {
            return Promise<T>.All(promise1, promise2, valueContainer);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3, IList<T> valueContainer = null)
        {
            return Promise<T>.All(promise1, promise2, promise3, valueContainer);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T>(Promise<T> promise1, Promise<T> promise2, Promise<T> promise3, Promise<T> promise4, IList<T> valueContainer = null)
        {
            return Promise<T>.All(promise1, promise2, promise3, promise4, valueContainer);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T>(params Promise<T>[] promises)
        {
            return Promise<T>.All(promises);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T>(IEnumerable<Promise<T>> promises)
        {
            return Promise<T>.All(promises);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> All<T, TEnumerator>(TEnumerator promises) where TEnumerator : IEnumerator<Promise<T>>
        {
            return Promise<T>.All(promises);
        }

        [Obsolete("Prefer Promise<T>.All()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<IList<T>> AllNonAlloc<T, TEnumerator>(TEnumerator promises, IList<T> valueContainer) where TEnumerator : IEnumerator<Promise<T>>
        {
            return Promise<T>.AllNonAlloc(promises, valueContainer);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that will resolve with the value of <paramref name="promise1"/> when both promises have resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<T1> Merge<T1>(Promise<T1> promise1, Promise promise2)
        {
            T1 value = default(T1);
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<T1>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref T1 target, int index) =>
            {
                if (index == 0)
                {
                    target = feed.GetResult<T1>();
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<T1>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2>> Merge<T1, T2>(Promise<T1> promise1, Promise<T2> promise2)
        {
            var value = new ValueTuple<T1, T2>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2> target, int index) =>
            {
                if (index == 0)
                {
                    target.Item1 = feed.GetResult<T1>();
                }
                else
                {
                    target.Item2 = feed.GetResult<T2>();
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2>> Merge<T1, T2>(Promise<T1> promise1, Promise<T2> promise2, Promise promise3)
        {
            var value = new ValueTuple<T1, T2>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3>> Merge<T1, T2, T3>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3)
        {
            var value = new ValueTuple<T1, T2, T3>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3>> Merge<T1, T2, T3>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise promise4)
        {
            var value = new ValueTuple<T1, T2, T3>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4>> Merge<T1, T2, T3, T4>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4)
        {
            var value = new ValueTuple<T1, T2, T3, T4>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4>> Merge<T1, T2, T3, T4>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise promise5)
        {
            var value = new ValueTuple<T1, T2, T3, T4>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5>> Merge<T1, T2, T3, T4, T5>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5>> Merge<T1, T2, T3, T4, T5>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5, Promise promise6)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise6, "promise6", 1);
            Internal.PrepareForMerge(promise6, ref passThroughs, 5, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5, T6>> Merge<T1, T2, T3, T4, T5, T6>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5, Promise<T6> promise6)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5, T6>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise6, "promise6", 1);
            Internal.PrepareForMerge(promise6, ref value.Item6, ref passThroughs, 5, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5, T6>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5, T6> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                    case 5:
                        target.Item6 = feed.GetResult<T6>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5, T6>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5, T6>> Merge<T1, T2, T3, T4, T5, T6>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5, Promise<T6> promise6, Promise promise7)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5, T6>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise6, "promise6", 1);
            Internal.PrepareForMerge(promise6, ref value.Item6, ref passThroughs, 5, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise7, "promise7", 1);
            Internal.PrepareForMerge(promise7, ref passThroughs, 6, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5, T6>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5, T6> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                    case 5:
                        target.Item6 = feed.GetResult<T6>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5, T6>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> Merge<T1, T2, T3, T4, T5, T6, T7>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5, Promise<T6> promise6, Promise<T7> promise7)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5, T6, T7>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise6, "promise6", 1);
            Internal.PrepareForMerge(promise6, ref value.Item6, ref passThroughs, 5, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise7, "promise7", 1);
            Internal.PrepareForMerge(promise7, ref value.Item7, ref passThroughs, 6, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                    case 5:
                        target.Item6 = feed.GetResult<T6>();
                        break;
                    case 6:
                        target.Item7 = feed.GetResult<T7>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>(promise, promise.Id, maxDepth);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> that will resolve with the values of the promises when they have all resolved.
        /// If any promise is rejected or canceled, the returned <see cref="Promise{T}"/> will immediately be rejected or canceled with the same reason.
        /// </summary>
        public static Promise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> Merge<T1, T2, T3, T4, T5, T6, T7>(Promise<T1> promise1, Promise<T2> promise2, Promise<T3> promise3, Promise<T4> promise4, Promise<T5> promise5, Promise<T6> promise6, Promise<T7> promise7, Promise promise8)
        {
            var value = new ValueTuple<T1, T2, T3, T4, T5, T6, T7>();
            var passThroughs = new Internal.ValueLinkedStack<Internal.PromiseRefBase.PromisePassThrough>();
            int pendingCount = 0;
            ulong completedProgress = 0;
            ulong totalProgress = 0;
            ushort maxDepth = 0;

            ValidateArgument(promise1, "promise1", 1);
            Internal.PrepareForMerge(promise1, ref value.Item1, ref passThroughs, 0, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise2, "promise2", 1);
            Internal.PrepareForMerge(promise2, ref value.Item2, ref passThroughs, 1, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise3, "promise3", 1);
            Internal.PrepareForMerge(promise3, ref value.Item3, ref passThroughs, 2, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise4, "promise4", 1);
            Internal.PrepareForMerge(promise4, ref value.Item4, ref passThroughs, 3, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise5, "promise5", 1);
            Internal.PrepareForMerge(promise5, ref value.Item5, ref passThroughs, 4, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise6, "promise6", 1);
            Internal.PrepareForMerge(promise6, ref value.Item6, ref passThroughs, 5, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise7, "promise7", 1);
            Internal.PrepareForMerge(promise7, ref value.Item7, ref passThroughs, 6, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);
            ValidateArgument(promise8, "promise8", 1);
            Internal.PrepareForMerge(promise8, ref passThroughs, 7, ref pendingCount, ref completedProgress, ref totalProgress, ref maxDepth);

            if (pendingCount == 0)
            {
                return Internal.CreateResolved(value, maxDepth);
            }
            var promise = Internal.PromiseRefBase.MergePromise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>.GetOrCreate(passThroughs, value, (Internal.PromiseRefBase feed, ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> target, int index) =>
            {
                switch (index)
                {
                    case 0:
                        target.Item1 = feed.GetResult<T1>();
                        break;
                    case 1:
                        target.Item2 = feed.GetResult<T2>();
                        break;
                    case 2:
                        target.Item3 = feed.GetResult<T3>();
                        break;
                    case 3:
                        target.Item4 = feed.GetResult<T4>();
                        break;
                    case 4:
                        target.Item5 = feed.GetResult<T5>();
                        break;
                    case 5:
                        target.Item6 = feed.GetResult<T6>();
                        break;
                    case 6:
                        target.Item7 = feed.GetResult<T7>();
                        break;
                }
            }, pendingCount, completedProgress, totalProgress, maxDepth);
            return new Promise<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>(promise, promise.Id, maxDepth);
        }

        private static Promise SwitchToContext(SynchronizationOption synchronizationOption, bool forceAsync)
        {
            return Internal.CreateResolved(0)
                .WaitAsync(synchronizationOption, forceAsync);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/> that will resolve on the foreground context.
        /// </summary>
        /// <param name="forceAsync">If true, forces the context switch to happen asynchronously.</param>
        public static Promise SwitchToForeground(bool forceAsync = false)
        {
            return SwitchToContext(SynchronizationOption.Foreground, forceAsync);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/> that will resolve on the background context.
        /// </summary>
        /// <param name="forceAsync">If true, forces the context switch to happen asynchronously.</param>
        public static Promise SwitchToBackground(bool forceAsync = false)
        {
            return SwitchToContext(SynchronizationOption.Background, forceAsync);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/> that will resolve on the provided <paramref name="synchronizationContext"/>
        /// </summary>
        /// <param name="synchronizationContext">The context to switch to. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the context switch to happen asynchronously.</param>
        public static Promise SwitchToContext(SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            return Internal.CreateResolved(0)
                .WaitAsync(synchronizationContext, forceAsync);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/>. <paramref name="resolver"/> is invoked with a <see cref="Deferred"/> that controls the state of the new <see cref="Promise"/>.
        /// You may provide a <paramref name="synchronizationOption"/> to control the context on which the <paramref name="resolver"/> is invoked.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/> and the <see cref="Deferred"/> is still pending, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>,
        /// or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate that will control the completion of the returned <see cref="Promise"/> via the passed in <see cref="Deferred"/>.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="resolver"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the <paramref name="resolver"/> to be invoked asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise New(Action<Deferred> resolver, SynchronizationOption synchronizationOption = SynchronizationOption.Synchronous, bool forceAsync = false)
        {
            ValidateArgument(resolver, "resolver", 1);

            Deferred deferred = Deferred.New();
            Run(ValueTuple.Create(deferred, resolver), cv =>
            {
                Deferred def = cv.Item1;
                try
                {
                    cv.Item2.Invoke(def);
                }
                catch (OperationCanceledException)
                {
                    def.TryCancel(); // Don't rethrow cancelation.
                }
                catch (Exception e)
                {
                    if (!def.TryReject(e)) throw;
                }
            }, synchronizationOption, forceAsync)
                .Forget();
            return deferred.Promise;
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/>. <paramref name="resolver"/> is invoked with a <see cref="Deferred"/> that controls the state of the new <see cref="Promise"/> on the provided <paramref name="synchronizationContext"/>.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/> and the <see cref="Deferred"/> is still pending, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>,
        /// or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate that will control the completion of the returned <see cref="Promise"/> via the passed in <see cref="Deferred"/>.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="resolver"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the <paramref name="resolver"/> to be invoked asynchronously.</param>
		public static Promise New(Action<Deferred> resolver, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(resolver, "resolver", 1);

            Deferred deferred = Deferred.New();
            Run(ValueTuple.Create(deferred, resolver), cv =>
            {
                Deferred def = cv.Item1;
                try
                {
                    cv.Item2.Invoke(def);
                }
                catch (OperationCanceledException)
                {
                    def.TryCancel(); // Don't rethrow cancelation.
                }
                catch (Exception e)
                {
                    if (!def.TryReject(e)) throw;
                }
            }, synchronizationContext, forceAsync)
                .Forget();
            return deferred.Promise;
        }

        [Obsolete("Prefer Promise<T>.New()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> New<T>(Action<Promise<T>.Deferred> resolver, SynchronizationOption synchronizationOption = SynchronizationOption.Synchronous, bool forceAsync = false)
        {
            return Promise<T>.New(resolver, synchronizationOption, forceAsync);
        }

        [Obsolete("Prefer Promise<T>.New()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> New<T>(Action<Promise<T>.Deferred> resolver, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            return Promise<T>.New(resolver, synchronizationContext, forceAsync);
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/>. <paramref name="resolver"/> is invoked with <paramref name="captureValue"/> and a <see cref="Deferred"/> that controls the state of the <see cref="Promise"/>.
        /// You may provide a <paramref name="synchronizationOption"/> to control the context on which the <paramref name="resolver"/> is invoked.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/> and the <see cref="Deferred"/> is still pending, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>,
        /// or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="resolver"/>.</param>
        /// <param name="resolver">The resolver delegate that will control the completion of the returned <see cref="Promise"/> via the passed in <see cref="Deferred"/>.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="resolver"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the <paramref name="resolver"/> to be invoked asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
        public static Promise New<TCapture>(TCapture captureValue, Action<TCapture, Deferred> resolver, SynchronizationOption synchronizationOption = SynchronizationOption.Synchronous, bool forceAsync = false)
        {
            ValidateArgument(resolver, "resolver", 1);

            Deferred deferred = Deferred.New();
            Run(ValueTuple.Create(deferred, resolver, captureValue), cv =>
            {
                Deferred def = cv.Item1;
                try
                {
                    cv.Item2.Invoke(cv.Item3, def);
                }
                catch (OperationCanceledException)
                {
                    def.TryCancel(); // Don't rethrow cancelation.
                }
                catch (Exception e)
                {
                    if (!def.TryReject(e)) throw;
                }
            }, synchronizationOption, forceAsync)
                .Forget();
            return deferred.Promise;
        }

        /// <summary>
        /// Returns a new <see cref="Promise"/>. <paramref name="resolver"/> is invoked with <paramref name="captureValue"/> and a <see cref="Deferred"/> that controls the state of the <see cref="Promise"/> on the provided <paramref name="synchronizationContext"/>.
        /// <para/>If <paramref name="resolver"/> throws an <see cref="Exception"/> and the <see cref="Deferred"/> is still pending, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>,
        /// or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="resolver"/>.</param>
        /// <param name="resolver">The resolver delegate that will control the completion of the returned <see cref="Promise"/> via the passed in <see cref="Deferred"/>.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="resolver"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the <paramref name="resolver"/> to be invoked asynchronously.</param>
        public static Promise New<TCapture>(TCapture captureValue, Action<TCapture, Deferred> resolver, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(resolver, "resolver", 1);

            Deferred deferred = Deferred.New();
            Run(ValueTuple.Create(deferred, resolver, captureValue), cv =>
            {
                Deferred def = cv.Item1;
                try
                {
                    cv.Item2.Invoke(cv.Item3, def);
                }
                catch (OperationCanceledException)
                {
                    def.TryCancel(); // Don't rethrow cancelation.
                }
                catch (Exception e)
                {
                    if (!def.TryReject(e)) throw;
                }
            }, synchronizationContext, forceAsync)
                .Forget();
            return deferred.Promise;
        }

        [Obsolete("Prefer Promise<T>.New()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> New<TCapture, T>(TCapture captureValue, Action<TCapture, Promise<T>.Deferred> resolver, SynchronizationOption synchronizationOption = SynchronizationOption.Synchronous, bool forceAsync = false)
        {
            return Promise<T>.New(captureValue, resolver, synchronizationOption, forceAsync);
        }

        [Obsolete("Prefer Promise<T>.New()"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> New<TCapture, T>(TCapture captureValue, Action<TCapture, Promise<T>.Deferred> resolver, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            return Promise<T>.New(captureValue, resolver, synchronizationContext, forceAsync);
        }

        /// <summary>
        /// Run the <paramref name="action"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise"/> that will be resolved when the <paramref name="action"/> returns successfully.
        /// <para/>If the <paramref name="action"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="action">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="action"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise Run(Action action, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(action, "action", 1);

            return SwitchToContext(synchronizationOption, forceAsync)
                .Finally(action);
        }

        /// <summary>
        /// Run the <paramref name="action"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise"/> that will be resolved when the <paramref name="action"/> returns successfully.
        /// <para/>If the <paramref name="action"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="action"/>.</param>
        /// <param name="action">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="action"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise Run<TCapture>(TCapture captureValue, Action<TCapture> action, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(action, "action", 1);

            return SwitchToContext(synchronizationOption, forceAsync)
                .Finally(captureValue, action);
        }

        /// <summary>
        /// Run the <paramref name="action"/>  on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise"/> that will be resolved when the <paramref name="action"/> returns successfully.
        /// <para/>If the <paramref name="action"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="action">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="action"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise Run(Action action, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(action, "action", 1);

            return SwitchToContext(synchronizationContext, forceAsync)
                .Finally(action);
        }

        /// <summary>
        /// Run the <paramref name="action"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise"/> that will be resolved when the <paramref name="action"/> returns successfully.
        /// <para/>If the <paramref name="action"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="action"/>.</param>
        /// <param name="action">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="action"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise Run<TCapture>(TCapture captureValue, Action<TCapture> action, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(action, "action", 1);

            return SwitchToContext(synchronizationContext, forceAsync)
                .Finally(captureValue, action);
        }

        /// <summary>
        /// Run the <paramref name="function"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise{T}"/> that will be resolved with the value returned by the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise<T> Run<T>(Func<T> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            return SwitchToContext(synchronizationOption, forceAsync)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise{T}"/> that will be resolved with the value returned by the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise<T> Run<TCapture, T>(TCapture captureValue, Func<TCapture, T> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            return SwitchToContext(synchronizationOption, forceAsync)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Run the <paramref name="function"/>  on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise{T}"/> that will be resolved with the value returned by the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise<T> Run<T>(Func<T> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            return SwitchToContext(synchronizationContext, forceAsync)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise{T}"/> that will be resolved with the value returned by the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise<T> Run<TCapture, T>(TCapture captureValue, Func<TCapture, T> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            return SwitchToContext(synchronizationContext, forceAsync)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise"/> that will adopt the state of the <see cref="Promise"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise Run(Func<Promise> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationOption, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise"/> that will adopt the state of the <see cref="Promise"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise Run<TCapture>(TCapture captureValue, Func<TCapture, Promise> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationOption, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Run the <paramref name="function"/>  on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise"/> that will adopt the state of the <see cref="Promise"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise Run(Func<Promise> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationContext, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise"/> that will adopt the state of the <see cref="Promise"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise Run<TCapture>(TCapture captureValue, Func<TCapture, Promise> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationContext, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise{T}"/> that will adopt the state of the <see cref="Promise{T}"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise<T> Run<T>(Func<Promise<T>> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationOption, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationOption"/> context. Returns a new <see cref="Promise{T}"/> that will adopt the state of the <see cref="Promise{T}"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationOption">Indicates on which context the <paramref name="function"/> will be invoked.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously. If <paramref name="synchronizationOption"/> is <see cref="SynchronizationOption.Synchronous"/>, this value will be ignored.</param>
		public static Promise<T> Run<TCapture, T>(TCapture captureValue, Func<TCapture, Promise<T>> function, SynchronizationOption synchronizationOption = SynchronizationOption.Background, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationOption, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Run the <paramref name="function"/>  on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise{T}"/> that will adopt the state of the <see cref="Promise{T}"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise<T> Run<T>(Func<Promise<T>> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationContext, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(function);
        }

        /// <summary>
        /// Run the <paramref name="function"/> with <paramref name="captureValue"/> on the provided <paramref name="synchronizationContext"/>. Returns a new <see cref="Promise{T}"/> that will adopt the state of the <see cref="Promise{T}"/> returned from the <paramref name="function"/>.
        /// <para/>If the <paramref name="function"/> throws an <see cref="Exception"/>, the new <see cref="Promise{T}"/> will be canceled if it is an <see cref="OperationCanceledException"/>, or rejected with that <see cref="Exception"/>.
        /// </summary>
        /// <param name="captureValue">The value that will be passed to <paramref name="function"/>.</param>
        /// <param name="function">The delegate that will invoked.</param>
        /// <param name="synchronizationContext">The context on which the <paramref name="function"/> will be invoked. If null, <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)"/> will be used.</param>
        /// <param name="forceAsync">If true, forces the invoke to happen asynchronously.</param>
		public static Promise<T> Run<TCapture, T>(TCapture captureValue, Func<TCapture, Promise<T>> function, SynchronizationContext synchronizationContext, bool forceAsync = false)
        {
            ValidateArgument(function, "function", 1);

            Promise promise = SwitchToContext(synchronizationContext, forceAsync);
            // Depth -1 to properly normalize the progress from the returned promise.
            return new Promise(promise._ref, promise._id, Internal.NegativeOneDepth)
                .Then(captureValue, function);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already resolved.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public static Promise Resolved()
        {
#if PROMISE_DEBUG
            return Internal.CreateResolved(0);
#else
            return new Promise();
#endif
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that is already resolved with <paramref name="value"/>.
        /// </summary>
        [MethodImpl(Internal.InlineOption)]
        public static Promise<T> Resolved<T>(T value)
        {
#if PROMISE_DEBUG
            return Internal.CreateResolved(value, 0);
#else
            return new Promise<T>(value);
#endif
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already rejected with <paramref name="reason"/>.
        /// </summary>
        public static Promise Rejected<TReject>(TReject reason)
        {
            var deferred = NewDeferred();
            deferred.Reject(reason);
            return deferred.Promise;
        }

        [Obsolete("Prefer Promise<T>.Rejected<TReject>(TReject reason)"), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Rejected<T, TReject>(TReject reason)
        {
            return Promise<T>.Rejected(reason);
        }

        /// <summary>
        /// Returns a <see cref="Promise"/> that is already canceled.
        /// </summary>
        public static Promise Canceled()
        {
            var deferred = NewDeferred();
            deferred.Cancel();
            return deferred.Promise;
        }

        [Obsolete("Cancelation reasons are no longer supported. Use Cancel() instead.", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise Canceled<TCancel>(TCancel reason)
        {
            throw new InvalidOperationException("Cancelation reasons are no longer supported. Use Canceled() instead.", Internal.GetFormattedStacktrace(1));
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}"/> that is already canceled.
        /// </summary>
        public static Promise<T> Canceled<T>()
        {
            return Promise<T>.Canceled();
        }

        [Obsolete("Cancelation reasons are no longer supported. Use Cancel() instead.", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static Promise<T> Canceled<T, TCancel>(TCancel reason)
        {
            return Promise<T>.Canceled(reason);
        }

        /// <summary>
        /// Returns a new <see cref="Deferred"/> instance that is linked to and controls the state of a new <see cref="Promise"/>.
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while the <see cref="Deferred"/> is pending, it and the <see cref="Promise"/> will be canceled.
        /// </summary>
        public static Deferred NewDeferred(CancelationToken cancelationToken = default(CancelationToken))
        {
            return Deferred.New(cancelationToken);
        }

        /// <summary>
        /// Returns a <see cref="Promise{T}.Deferred"/> object that is linked to and controls the state of a new <see cref="Promise{T}"/>.
        /// <para/>If the <paramref name="cancelationToken"/> is canceled while the <see cref="Promise{T}.Deferred"/> is pending, it and the <see cref="Promise{T}"/> will be canceled.
        /// </summary>
        public static Promise<T>.Deferred NewDeferred<T>(CancelationToken cancelationToken = default(CancelationToken))
        {
            return Promise<T>.NewDeferred(cancelationToken);
        }

        /// <summary>
        /// Get a <see cref="RethrowException"/> that can be thrown inside an onRejected callback to rethrow the caught rejection, preserving the stack trace.
        /// This should be used as "throw Promise.Rethrow;"
        /// This is similar to "throw;" in a synchronous catch clause.
        /// </summary>
        public static RethrowException Rethrow
        {
            get
            {
                return RethrowException.GetOrCreate();
            }
        }

        /// <summary>
        /// Get a <see cref="CanceledException"/> that can be thrown to cancel the promise from an onResolved or onRejected callback, or in an async Promise function.
        /// This should be used as "throw Promise.CancelException();"
        /// </summary>
        public static CanceledException CancelException()
        {
            return Internal.CanceledExceptionInternal.GetOrCreate();
        }

        [Obsolete("Cancelation reasons are no longer supported. Use CancelException() instead.", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static CanceledException CancelException<T>(T value)
        {
            throw new InvalidOperationException("Cancelation reasons are no longer supported. Use CancelException() instead.", Internal.GetFormattedStacktrace(1));
        }

        /// <summary>
        /// Get a <see cref="Promises.RejectException"/> that can be thrown to reject the promise from an onResolved or onRejected callback, or in an async Promise function.
        /// This should be used as "throw Promise.RejectException(value);"
        /// </summary>
        public static RejectException RejectException<T>(T value)
        {
            return new Internal.RejectExceptionInternal<T>(value);
        }
    }
}