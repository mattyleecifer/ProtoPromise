﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
# endif

using NUnit.Framework;
using Proto.Promises;
using ProtoPromiseTests.Threading;
using System.Threading;

namespace ProtoPromiseTests.APIs
{
    public class MiscellaneousTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.Setup();
        }

        [TearDown]
        public void Teardown()
        {
            TestHelper.Cleanup();
        }

        [Test]
        public void PromiseIsInvalidAfterAwaited_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise;

            Assert.IsTrue(promise.IsValid);

            promise.Then(() => { }).Forget();

            Assert.IsFalse(promise.IsValid);

#if CSHARP_7_3_OR_NEWER && PROMISE_DEBUG
            Assert.Throws<InvalidOperationException>(() => promise.GetAwaiter());
#endif
            Assert.Throws<InvalidOperationException>(() => promise.Preserve());
            Assert.Throws<InvalidOperationException>(() => promise.Forget());
            Assert.Throws<InvalidOperationException>(() => promise.Duplicate());

#if PROMISE_PROGRESS
            Assert.Throws<InvalidOperationException>(() => promise.Progress(v => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Progress(1, (cv, v) => { }));
#endif
            Assert.Throws<InvalidOperationException>(() => promise.CatchCancelation(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.CatchCancelation(1, cv => { }));

            Assert.Throws<InvalidOperationException>(() => promise.Finally(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Finally(1, cv => { }));

            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => { }));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => 1));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Catch(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(() => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Catch((object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Catch((object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, (int cv, object r) => Promise.Resolved()));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), () => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), 1, cv => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), (object r) => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), 1, (int cv, object r) => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), () => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), 1, cv => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), (object r) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(() => { }, 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => 1, 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(), 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(() => Promise.Resolved(1), 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => { }, 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => 1, 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(), 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, cv => Promise.Resolved(1), 1, (int cv, object r) => Promise.Resolved(1)));

            deferred.Resolve();
        }

        [Test]
        public void PromiseIsInvalidAfterAwaited_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise;

            Assert.IsTrue(promise.IsValid);

            promise.Then(v => { }).Forget();

            Assert.IsFalse(promise.IsValid);

#if CSHARP_7_3_OR_NEWER && PROMISE_DEBUG
            Assert.Throws<InvalidOperationException>(() => promise.GetAwaiter());
#endif
            Assert.Throws<InvalidOperationException>(() => promise.Preserve());
            Assert.Throws<InvalidOperationException>(() => promise.Forget());
            Assert.Throws<InvalidOperationException>(() => promise.Duplicate());

#if PROMISE_PROGRESS
            Assert.Throws<InvalidOperationException>(() => promise.Progress(v => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Progress(1, (cv, v) => { }));
#endif
            Assert.Throws<InvalidOperationException>(() => promise.CatchCancelation(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.CatchCancelation(1, cv => { }));

            Assert.Throws<InvalidOperationException>(() => promise.Finally(() => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Finally(1, cv => { }));

            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => { }));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => 1));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(r => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.ContinueWith(1, (cv, r) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Catch(() => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(() => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Catch((object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Catch((object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Catch(1, (int cv, object r) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, () => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), () => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), () => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, 1, cv => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), 1, cv => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), 1, cv => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, (object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), (object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), (object r) => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, 1, (int cv, object r) => 1));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), 1, (int cv, object r) => { }));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), 1, (int cv, object r) => 1));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, () => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), () => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), () => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, 1, cv => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), 1, cv => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), 1, cv => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, (object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), (object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), (object r) => Promise.Resolved(1)));

            Assert.Throws<InvalidOperationException>(() => promise.Then(v => { }, 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => 1, 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(), 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(v => Promise.Resolved(1), 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => { }, 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => 1, 1, (int cv, object r) => Promise.Resolved(1)));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(), 1, (int cv, object r) => Promise.Resolved()));
            Assert.Throws<InvalidOperationException>(() => promise.Then(1, (cv, v) => Promise.Resolved(1), 1, (int cv, object r) => Promise.Resolved(1)));

            deferred.Resolve(1);
        }

        [Test]
        public void ThrowingRejectExceptionInOnResolvedRejectsThePromiseWithTheGivenValue_void()
        {
            var promise = Promise.Resolved().Preserve();

            int rejectCount = 0;
            string expected = "Reject!";

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
            {
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            };

            TestHelper.AddResolveCallbacks<int, string>(promise,
                onResolve: () => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddCallbacks<int, object, string>(promise,
                onResolve: () => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, string>(promise,
                onContinue: _ => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.resolveVoidCallbacks + TestHelper.continueVoidCallbacks) * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRejectExceptionInOnResolvedRejectsThePromiseWithTheGivenValue_T()
        {
            var promise = Promise.Resolved(100).Preserve();

            int rejectCount = 0;
            string expected = "Reject!";

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();

            TestHelper.AddResolveCallbacks<int, int, string>(promise,
                onResolve: v => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddCallbacks<int, int, object, string>(promise,
                onResolve: v => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                onContinue: _ => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.resolveTCallbacks + TestHelper.continueTCallbacks) * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRejectExceptionInOnRejectedRejectsThePromiseWithTheGivenValue_void()
        {
            var promise = Promise.Rejected("A different reject value.").Preserve();

            int rejectCount = 0;
            string expected = "Reject!";

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();

            TestHelper.AddCallbacks<int, object, string>(promise,
                onReject: v => { throw Promise.RejectException(expected); },
                onUnknownRejection: () => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, string>(promise,
                onContinue: _ => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.rejectVoidCallbacks + TestHelper.continueVoidCallbacks) * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRejectExceptionInOnRejectedRejectsThePromiseWithTheGivenValue_T()
        {
            var promise = Promise<int>.Rejected("A different reject value.").Preserve();

            int rejectCount = 0;
            string expected = "Reject!";

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();

            TestHelper.AddCallbacks<int, int, object, string>(promise,
                onReject: v => { throw Promise.RejectException(expected); },
                onUnknownRejection: () => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert,
                onCallbackAddedT: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                onContinue: _ => { throw Promise.RejectException(expected); },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.rejectTCallbacks + TestHelper.continueTCallbacks) * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingCancelExceptionInOnResolvedCancelsThePromiseWithTheGivenValue_void()
        {
            var promise = Promise.Resolved().Preserve();

            int cancelCount = 0;

            System.Action onCancel = () =>
            {
                ++cancelCount;
            };

            TestHelper.AddResolveCallbacks<int, string>(promise,
                onResolve: () => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddCallbacks<int, object, string>(promise,
                onResolve: () => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddContinueCallbacks<int, string>(promise,
                onContinue: _ => { throw Promise.CancelException(); },
                onCancel: onCancel
            );

            Assert.AreEqual(
                (TestHelper.resolveVoidCallbacks + TestHelper.continueVoidCallbacks) * 2,
                cancelCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingCancelExceptionInOnResolvedCancelsThePromiseWithTheGivenValue_T()
        {
            var promise = Promise.Resolved(100).Preserve();

            int cancelCount = 0;

            System.Action onCancel = () =>
            {
                ++cancelCount;
            };

            TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                onResolve: v => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddCallbacks<int, bool, object, string>(promise,
                onResolve: v => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddContinueCallbacks<int, bool, string>(promise,
                onContinue: _ => { throw Promise.CancelException(); },
                onCancel: onCancel
            );

            Assert.AreEqual(
                (TestHelper.resolveTCallbacks + TestHelper.continueTCallbacks) * 2,
                cancelCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingCancelExceptionInOnRejectedCancelsThePromiseWithTheGivenValue_void()
        {
            var promise = Promise.Rejected("Rejected").Preserve();

            int cancelCount = 0;

            System.Action onCancel = () =>
            {
                ++cancelCount;
            };

            TestHelper.AddCallbacks<int, object, string>(promise,
                onReject: v => { throw Promise.CancelException(); },
                onUnknownRejection: () => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddContinueCallbacks<int, string>(promise,
                onContinue: _ => { throw Promise.CancelException(); },
                onCancel: onCancel
            );

            Assert.AreEqual(
                (TestHelper.rejectVoidCallbacks + TestHelper.continueVoidCallbacks) * 2,
                cancelCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingCancelExceptionInOnRejectedCancelsThePromiseWithTheGivenValue_T()
        {
            var promise = Promise<int>.Rejected("Rejected").Preserve();

            int cancelCount = 0;

            System.Action onCancel = () =>
            {
                ++cancelCount;
            };

            TestHelper.AddCallbacks<int, bool, object, string>(promise,
                onReject: v => { throw Promise.CancelException(); },
                onUnknownRejection: () => { throw Promise.CancelException(); },
                onCancel: onCancel
            );
            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                onContinue: _ => { throw Promise.CancelException(); },
                onCancel: onCancel
            );

            Assert.AreEqual(
                (TestHelper.rejectTCallbacks + TestHelper.continueTCallbacks) * 2,
                cancelCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRethrowInOnResolvedRejectsThePromiseWithInvalidOperationException_void()
        {
            var promise = Promise.Resolved().Preserve();

            int errorCount = 0;

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((object e) =>
                {
                    Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                    ++errorCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((object e) =>
                {
                    Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                    ++errorCount;
                }).Forget();

            TestHelper.AddResolveCallbacks<int, string>(promise,
                onResolve: () => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddCallbacks<int, object, string>(promise,
                onResolve: () => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, string>(promise,
                onContinue: _ => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.resolveVoidCallbacks + TestHelper.continueVoidCallbacks) * 2,
                errorCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRethrowInOnResolvedRejectsThePromiseWithInvalidOperationException_T()
        {
            var promise = Promise.Resolved(100).Preserve();

            int errorCount = 0;

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((object e) =>
                {
                    Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                    ++errorCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((object e) =>
                {
                    Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                    ++errorCount;
                }).Forget();

            TestHelper.AddResolveCallbacks<int, int, string>(promise,
                onResolve: v => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddCallbacks<int, int, object, string>(promise,
                onResolve: v => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );
            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                onContinue: _ => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                (TestHelper.resolveTCallbacks + TestHelper.continueTCallbacks) * 2,
                errorCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRethrowInOnRejectedRejectsThePromiseWithTheSameReason_void()
        {
            string expected = "Reject!";

            var promise = Promise.Rejected(expected).Preserve();

            int rejectCount = 0;

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();

            TestHelper.AddCallbacks<int, object, string>(promise,
                onReject: _ => { throw Promise.Rethrow; },
                onUnknownRejection: () => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert
            );

            Assert.AreEqual(
                TestHelper.rejectVoidCallbacks * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void ThrowingRethrowInOnRejectedRejectsThePromiseWithTheSameReason_T()
        {
            string expected = "Reject!";

            var promise = Promise<int>.Rejected(expected).Preserve();

            int rejectCount = 0;

            TestAction<Promise> onCallbackAdded = (ref Promise p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();
            TestAction<Promise<int>> onCallbackAddedConvert = (ref Promise<int> p) =>
                p.Catch((string e) =>
                {
                    Assert.AreEqual(expected, e);
                    ++rejectCount;
                }).Forget();

            TestHelper.AddCallbacks<int, int, object, string>(promise,
                onReject: _ => { throw Promise.Rethrow; },
                onUnknownRejection: () => { throw Promise.Rethrow; },
                onCallbackAdded: onCallbackAdded,
                onCallbackAddedConvert: onCallbackAddedConvert,
                onCallbackAddedT: onCallbackAddedConvert
            );

            Assert.AreEqual(
                TestHelper.rejectTCallbacks * 2,
                rejectCount
            );

            promise.Forget();
        }

        [Test]
        public void PromiseResolvedIsResolved()
        {
            bool resolved = false;

            Promise.Resolved()
                .CatchCancelation(() => Assert.Fail("Promise was canceled when it should have been resolved"))
                .Then(() => resolved = true, () => Assert.Fail("Promise was rejected when it should have been resolved"))
                .Forget();

            Assert.IsTrue(resolved);
        }

        [Test]
        public void PromiseResolvedIsResolvedWithTheGivenValue()
        {
            int expected = 100;
            bool resolved = false;

            Promise.Resolved(expected)
                .Then(val =>
                {
                    Assert.AreEqual(expected, val);
                    resolved = true;
                }, () => Assert.Fail("Promise was rejected when it should have been resolved"))
                .CatchCancelation(() => Assert.Fail("Promise was canceled when it should have been resolved"))
                .Forget();

            Assert.IsTrue(resolved);
        }

        [Test]
        public void PromiseRejectedIsRejectedWithTheGivenReason_void()
        {
            string expected = "Reject";
            bool rejected = false;

            Promise.Rejected(expected)
                .CatchCancelation(() => Assert.Fail("Promise was canceled when it should have been rejected"))
                .Then(() => Assert.Fail("Promise was resolved when it should have been rejected"),
                (object reason) =>
                {
                    Assert.AreEqual(expected, reason);
                    rejected = true;
                })
                .Forget();

            Assert.IsTrue(rejected);
        }

        [Test]
        public void PromiseRejectedIsRejectedWithTheGivenReason_T()
        {
            string expected = "Reject";
            bool rejected = false;

            Promise<int>.Rejected(expected)
                .CatchCancelation(() => Assert.Fail("Promise was canceled when it should have been rejected"))
                .Then(() => Assert.Fail("Promise was resolved when it should have been rejected"),
                (object reason) =>
                {
                    Assert.AreEqual(expected, reason);
                    rejected = true;
                })
                .Forget();

            Assert.IsTrue(rejected);
        }

        [Test]
        public void PromiseCanceledIsCanceled_void()
        {
            bool canceled = false;

            Promise.Canceled()
                .CatchCancelation(() =>
                {
                    canceled = true;
                })
                .Forget();

            Assert.IsTrue(canceled);
        }

        [Test]
        public void PromiseCanceledIsCanceled_T()
        {
            bool canceled = false;

            Promise<int>.Canceled()
                .CatchCancelation(() =>
                {
                    canceled = true;
                })
                .Forget();

            Assert.IsTrue(canceled);
        }

        [Test]
        public void PromiseSwitchToContextWorksProperly_Then(
            [Values(SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
                SynchronizationType.Explicit)] SynchronizationType synchronizationType,
            [Values(SynchronizationType.Foreground
#if !UNITY_WEBGL
                , SynchronizationType.Background
#endif
                )] SynchronizationType invokeContext)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;

            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                Promise promise = synchronizationType == SynchronizationType.Foreground
                    ? Promise.SwitchToForeground()
                    : synchronizationType == TestHelper.backgroundType
                    ? Promise.SwitchToBackground()
                    : Promise.SwitchToContext(TestHelper._foregroundContext);

                promise
                    .Then(() =>
                    {
                        TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                        invoked = true;
                    })
                    .Forget();
            }, invokeContext == SynchronizationType.Foreground);

            TestHelper.ExecuteForegroundCallbacks();
            if (synchronizationType != TestHelper.backgroundType)
            {
                Assert.True(invoked);
            }
            else
            {
                if (!SpinWait.SpinUntil(() => invoked, System.TimeSpan.FromSeconds(1)))
                {
                    throw new System.TimeoutException();
                }
            }
        }

#if CSHARP_7_3_OR_NEWER
        [Test]
        public void PromiseSwitchToContextWorksProperly_Await(
            [Values(SynchronizationType.Foreground,
#if !UNITY_WEBGL
                SynchronizationType.Background,
#endif
                SynchronizationType.Explicit)] SynchronizationType synchronizationType,
            [Values(SynchronizationType.Foreground
#if !UNITY_WEBGL
                , SynchronizationType.Background
#endif
                )] SynchronizationType invokeContext)
        {
            Thread foregroundThread = Thread.CurrentThread;
            bool invoked = false;

            new ThreadHelper().ExecuteSynchronousOrOnThread(() =>
            {
                Await();

                async void Await()
                {
                    Promise promise = synchronizationType == SynchronizationType.Foreground
                        ? Promise.SwitchToForeground()
                        : synchronizationType == TestHelper.backgroundType
                        ? Promise.SwitchToBackground()
                        : Promise.SwitchToContext(TestHelper._foregroundContext);
                    
                    await promise;

                    TestHelper.AssertCallbackContext(synchronizationType, invokeContext, foregroundThread);
                    invoked = true;
                }
            }, invokeContext == SynchronizationType.Foreground);

            TestHelper.ExecuteForegroundCallbacks();
            if (synchronizationType != TestHelper.backgroundType)
            {
                Assert.True(invoked);
            }
            else
            {
                if (!SpinWait.SpinUntil(() => invoked, System.TimeSpan.FromSeconds(1)))
                {
                    throw new System.TimeoutException();
                }
            }
        }
#endif

        [Test]
        public void PromiseMayBeResolvedWithNullable(
            [Values(10, 0, null)] int? expected)
        {
            var deferred = Promise.NewDeferred<int?>();
            deferred.Promise
                .Then(v => Assert.AreEqual(expected, v))
                .Forget();
            deferred.Resolve(expected);
        }
    }
}