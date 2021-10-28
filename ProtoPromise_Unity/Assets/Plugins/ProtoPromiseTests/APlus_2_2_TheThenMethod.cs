﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE1006 // Naming Styles

using NUnit.Framework;
using System;

namespace Proto.Promises.Tests
{
    public class APlus_2_2_TheThenMethod
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

#if PROMISE_DEBUG
        // These will only pass in DEBUG mode.
        public class _2_2_1_BothOnResolveAndOnRejectedAreOptionalArgument
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
            public void _2_2_1_1_IfOnFulfilledIsNull_Throw_void()
            {
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise<int>>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action), () => { });
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action), (string failValue) => { });
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int>), () => default(int));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int>), (string failValue) => default(int));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise>), () => default(Promise));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise>), (string failValue) => default(Promise));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise<int>>), () => default(Promise<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise<int>>), (string failValue) => default(Promise<int>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action), () => default(Promise));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action), (string failValue) => default(Promise));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int>), () => default(Promise<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int>), (string failValue) => default(Promise<int>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise>), () => { });
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise>), (string failValue) => { });
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise<int>>), () => default(int));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<Promise<int>>), (string failValue) => default(int));
                });

                deferred.Resolve();

                promise.Forget();
            }

            [Test]
            public void _2_2_1_1_IfOnFulfilledIsNull_Throw_T()
            {
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise<int>>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action<int>), () => { });
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action<int>), (string failValue) => { });
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, int>), () => default(int));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, int>), (string failValue) => default(int));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise>), () => default(Promise));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise>), (string failValue) => default(Promise));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise<int>>), () => default(Promise<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise<int>>), (string failValue) => default(Promise<int>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action<int>), () => default(Promise));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Action<int>), (string failValue) => default(Promise));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, int>), () => default(Promise<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, int>), (string failValue) => default(Promise<int>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise>), () => { });
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise>), (string failValue) => { });
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise<int>>), () => default(int));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(default(Func<int, Promise<int>>), (string failValue) => default(int));
                });

                deferred.Resolve(0);

                promise.Forget();
            }

            [Test]
            public void _2_2_1_2_IfOnRejectedIsNull_Throw_void()
            {
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Action<string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<string, Promise>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => { }, default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => { }, default(Action<string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise), default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise), default(Func<string, Promise>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => "string", default(Func<string>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => "string", default(Func<Exception, string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise<string>), default(Func<Promise<string>>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise<string>), default(Func<Exception, Promise<string>>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise), default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise), default(Action<string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => { }, default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => { }, default(Func<string, Promise>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise<string>), default(Func<string>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => default(Promise<string>), default(Func<Exception, string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => "string", default(Func<Promise<string>>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then(() => "string", default(Func<Exception, Promise<string>>));
                });

                deferred.Resolve();

                promise.Forget();
            }

            [Test]
            public void _2_2_1_2_IfOnRejectedIsNull_Throw_T()
            {
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<int>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<string, int>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<Promise<int>>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Catch(default(Func<string, Promise<int>>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => { }, default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => { }, default(Action<string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise), default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise), default(Func<string, Promise>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => "string", default(Func<string>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => "string", default(Func<Exception, string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise<string>), default(Func<Promise<string>>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise<string>), default(Func<Exception, Promise<string>>));
                });


                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise), default(Action));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise), default(Action<string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => { }, default(Func<Promise>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => { }, default(Func<string, Promise>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise<string>), default(Func<string>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => default(Promise<string>), default(Func<Exception, string>));
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => "string", default(Func<Promise<string>>));
                });
                Assert.Throws<ArgumentNullException>(() =>
                {
                    promise.Then((int x) => "string", default(Func<Exception, Promise<string>>));
                });

                deferred.Resolve(0);

                promise.Forget();
            }
        }
#endif

        public class IfOnFulfilledIsAFunction_2_2_2
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
            public void _2_2_2_1_ItMustBeCalledAfterPromiseIsFulfilledWithPromisesValueAsItsFirstArgument()
            {
                var promisedValue = 100;
                var resolved = false;
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                    onResolve: v =>
                    {
                        Assert.AreEqual(promisedValue, v);
                        resolved = true;
                    }
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    onResolve: v =>
                    {
                        Assert.AreEqual(promisedValue, v);
                        resolved = true;
                    }
                );
                deferred.Resolve(promisedValue);

                Assert.True(resolved);

                promise.Forget();
            }

            [Test]
            public void _2_2_2_2_ItMustNotBeCalledBeforePromiseIsFulfilled_void()
            {
                var resolved = false;
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                TestHelper.AddResolveCallbacks<bool, string>(promise,
                    () => resolved = true
                );
                TestHelper.AddCallbacks<bool, object, string>(promise,
                    () => resolved = true,
                    s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    () => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                Assert.False(resolved);

                deferred.Resolve();

                Assert.True(resolved);

                promise.Forget();
            }

            [Test]
            public void _2_2_2_2_ItMustNotBeCalledBeforePromiseIsFulfilled_T()
            {
                var resolved = false;
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                    v => resolved = true
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    v => resolved = true,
                    s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    () => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                Assert.False(resolved);

                deferred.Resolve(100);

                Assert.True(resolved);

                promise.Forget();
            }

            [Test]
            public void _2_2_2_3_ItMustNotBeCalledMoreThanOnce_void()
            {
                var resolveCount = 0;
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                TestHelper.AddResolveCallbacks<bool, string>(promise,
                    () => ++resolveCount
                );
                TestHelper.AddCallbacks<bool, object, string>(promise,
                    () => ++resolveCount,
                    s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    () => Assert.Fail("Promise was rejected when it should have been resolved.")
                );
                deferred.Resolve();

                Assert.IsFalse(deferred.TryResolve());
                Assert.Throws<InvalidOperationException>(() => deferred.Resolve());

                Assert.AreEqual(
                    (TestHelper.resolveVoidVoidCallbacks + TestHelper.resolveVoidConvertCallbacks +
                    TestHelper.resolveVoidPromiseVoidCallbacks + TestHelper.resolveVoidPromiseConvertCallbacks) * 2,
                    resolveCount
                );

                promise.Forget();
            }

            [Test]
            public void _2_2_2_3_ItMustNotBeCalledMoreThanOnce_T()
            {
                var resolveCount = 0;
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                    x => ++resolveCount
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    x => ++resolveCount,
                    s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    () => Assert.Fail("Promise was rejected when it should have been resolved.")
                );
                deferred.Resolve(1);

                Assert.IsFalse(deferred.TryResolve(1));
                Assert.Throws<InvalidOperationException>(() => deferred.Resolve(100));

                Assert.AreEqual(
                    (TestHelper.resolveTVoidCallbacks + TestHelper.resolveTConvertCallbacks +
                    TestHelper.resolveTPromiseVoidCallbacks + TestHelper.resolveTPromiseConvertCallbacks) * 2,
                    resolveCount
                );

                promise.Forget();
            }
        }

        public class _2_2_3_IfOnRejectedIsAFunction
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
            public void _2_2_3_1_ItMustBeCalledAfterPromiseIsRejected_WithPromisesReasonAsItsFirstArgument_void()
            {
                var rejectReason = "Fail value";
                var errored = false;
                var deferred = Promise.NewDeferred<int>();

                TestHelper.AddCallbacks<int, bool, string, string>(deferred.Promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    reason =>
                    {
                        Assert.AreEqual(rejectReason, reason);
                        errored = true;
                    }
                );
                deferred.Reject(rejectReason);

                Assert.True(errored);
            }

            [Test]
            public void _2_2_3_1_ItMustBeCalledAfterPromiseIsRejected_WithPromisesReasonAsItsFirstArgument_T()
            {
                var rejectReason = "Fail value";
                var errored = false;
                var deferred = Promise.NewDeferred();

                TestHelper.AddCallbacks<bool, string, string>(deferred.Promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    reason =>
                    {
                        Assert.AreEqual(rejectReason, reason);
                        errored = true;
                    }
                );
                deferred.Reject(rejectReason);

                Assert.True(errored);
            }

            [Test]
            public void _2_2_3_2_ItMustNotBeCalledBeforePromiseIsRejected_void()
            {
                var errored = false;
                var deferred = Promise.NewDeferred<int>();

                TestHelper.AddCallbacks<int, bool, string, string>(deferred.Promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    reason => errored = true
                );

                Assert.False(errored);

                deferred.Reject("Fail value");

                Assert.True(errored);
            }

            [Test]
            public void _2_2_3_2_ItMustNotBeCalledBeforePromiseIsRejected_T()
            {
                var errored = false;
                var deferred = Promise.NewDeferred();

                TestHelper.AddCallbacks<bool, string, string>(deferred.Promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    reason => errored = true
                );

                Assert.False(errored);

                deferred.Reject("Fail value");

                Assert.True(errored);
            }

            [Test]
            public void _2_2_3_3_ItMustNotBeCalledMoreThanOnce_void()
            {
                var errorCount = 0;
                var deferred = Promise.NewDeferred();

                TestHelper.AddCallbacks<bool, object, string>(deferred.Promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    x => ++errorCount,
                    () => ++errorCount
                );
                deferred.Reject("Fail value");

                Assert.IsFalse(deferred.TryReject("Fail value"));
                Assert.Throws<InvalidOperationException>(() => deferred.Reject("Fail value"));

                Assert.AreEqual(
                    (TestHelper.rejectVoidVoidCallbacks + TestHelper.rejectVoidConvertCallbacks +
                    TestHelper.rejectVoidPromiseVoidCallbacks + TestHelper.rejectVoidPromiseConvertCallbacks) * 2,
                    errorCount
                );
            }

            [Test]
            public void _2_2_3_3_ItMustNotBeCalledMoreThanOnce_T()
            {
                var errorCount = 0;
                var deferred = Promise.NewDeferred<int>();

                TestHelper.AddCallbacks<int, bool, object, string>(deferred.Promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    x => ++errorCount,
                    () => ++errorCount
                );
                deferred.Reject("Fail value");

                Assert.IsFalse(deferred.TryReject("Fail value"));
                Assert.Throws<InvalidOperationException>(() => deferred.Reject("Fail value"));

                Assert.AreEqual(
                    (TestHelper.rejectTVoidCallbacks + TestHelper.rejectTConvertCallbacks + TestHelper.rejectTTCallbacks +
                    TestHelper.rejectTPromiseVoidCallbacks + TestHelper.rejectTPromiseConvertCallbacks + TestHelper.rejectTPromiseTCallbacks) * 2,
                    errorCount
                );
            }
        }

        // This is implemented in C# via a SynchronizationContext set in the Config.
        // Unit tests here directly invoke the callbacks sent to the SynchronizationContext, but different applications may handle it differently.
        // (In Unity, it executes all callbacks on the main thread every frame).
        [Test]
        public void _2_2_4_OnFulfilledMustNotBeCalledUntilTheExecutionContextStackContainsOnlyPlatformCode_void()
        {
            bool resolved = false;
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise
                .WaitAsync(SynchronizationOption.Foreground)
                .Preserve();

            TestHelper.AddResolveCallbacks<bool, string>(promise,
                () => resolved = true
            );
            TestHelper.AddCallbacks<bool, object, string>(promise,
                () => resolved = true,
                s => Assert.Fail("Promise was rejected when it should have been resolved.")
            );
            deferred.Resolve();
            Assert.False(resolved);

            TestHelper.ExecuteForegroundCallbacks();
            Assert.True(resolved);

            promise.Forget();
        }

        [Test]
        public void _2_2_4_OnFulfilledMustNotBeCalledUntilTheExecutionContextStackContainsOnlyPlatformCode_T()
        {
            bool resolved = false;
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise
                .WaitAsync(SynchronizationOption.Foreground)
                .Preserve();

            TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                v => resolved = true
            );
            TestHelper.AddCallbacks<int, bool, object, string>(promise,
                v => resolved = true,
                s => Assert.Fail("Promise was rejected when it should have been resolved.")
            );
            deferred.Resolve(1);
            Assert.False(resolved);

            TestHelper.ExecuteForegroundCallbacks();
            Assert.True(resolved);

            promise.Forget();
        }

        [Test]
        public void _2_2_4_OnRejectedMustNotBeCalledUntilTheExecutionContextStackContainsOnlyPlatformCode_void()
        {
            bool errored = false;
            var deferred = Promise.NewDeferred();

            TestHelper.AddCallbacks<bool, object, string>(deferred.Promise.WaitAsync(SynchronizationOption.Foreground),
                () => Assert.Fail("Promise was resolved when it should have been rejected."),
                s => errored = true
            );
            deferred.Reject("Fail value");
            Assert.False(errored);

            TestHelper.ExecuteForegroundCallbacks();
            Assert.True(errored);
        }

        [Test]
        public void _2_2_4_OnRejectedMustNotBeCalledUntilTheExecutionContextStackContainsOnlyPlatformCode_T()
        {
            bool errored = false;
            var deferred = Promise.NewDeferred<int>();

            TestHelper.AddCallbacks<int, bool, object, string>(deferred.Promise.WaitAsync(SynchronizationOption.Foreground),
                v => Assert.Fail("Promise was resolved when it should have been rejected."),
                s => errored = true
            );
            deferred.Reject("Fail value");
            Assert.False(errored);

            TestHelper.ExecuteForegroundCallbacks();
            Assert.True(errored);
        }

        // Not relevant for C#
        // 2.2.5 onFulfilled and onRejected must be called as functions (i.e. with no this value)

        public class _2_2_6_ThenMayBeCalledMultipleTimesOnTheSamePromise
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
            public void _2_2_6_1_IfWhenPromiseIsFulfilled_AllRespectiveOnFulfilledCallbacksMustExecuteInTheOrderOfTheirOriginatingCallsToThen_void()
            {
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                int order = 0;
                int counter = 0;

                Action<int> callback = expected =>
                {
                    Assert.AreEqual(expected, order);
                    if (++counter == TestHelper.resolveVoidCallbacks * 2)
                    {
                        counter = 0;
                        ++order;
                    }
                };

                TestHelper.AddResolveCallbacks<bool, string>(promise, () => callback(0));
                TestHelper.AddCallbacks<bool, object, string>(promise, () => callback(0), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                TestHelper.AddResolveCallbacks<bool, string>(promise, () => callback(1));
                TestHelper.AddCallbacks<bool, object, string>(promise, () => callback(1), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                TestHelper.AddResolveCallbacks<bool, string>(promise, () => callback(2));
                TestHelper.AddCallbacks<bool, object, string>(promise, () => callback(2), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                deferred.Resolve();

                Assert.AreEqual(3, order);

                promise.Forget();
            }

            [Test]
            public void _2_2_6_1_IfWhenPromiseIsFulfilled_AllRespectiveOnFulfilledCallbacksMustExecuteInTheOrderOfTheirOriginatingCallsToThen_T()
            {
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                int orderT = 0;
                int counterT = 0;

                Action<int> callbackT = expected =>
                {
                    Assert.AreEqual(expected, orderT);
                    if (++counterT == TestHelper.resolveTCallbacks * 2)
                    {
                        counterT = 0;
                        ++orderT;
                    }
                };

                TestHelper.AddResolveCallbacks<int, bool, string>(promise, v => callbackT(0));
                TestHelper.AddCallbacks<int, bool, object, string>(promise, v => callbackT(0), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                TestHelper.AddResolveCallbacks<int, bool, string>(promise, v => callbackT(1));
                TestHelper.AddCallbacks<int, bool, object, string>(promise, v => callbackT(1), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                TestHelper.AddResolveCallbacks<int, bool, string>(promise, v => callbackT(2));
                TestHelper.AddCallbacks<int, bool, object, string>(promise, v => callbackT(2), s => Assert.Fail("Promise was rejected when it should have been resolved."));

                deferred.Resolve(100);

                Assert.AreEqual(3, orderT);

                promise.Forget();
            }

            [Test]
            public void _2_2_6_2_IfWhenPromiseIsRejected_AllRespectiveOnRejectedCallbacksMustExecuteInTheOrderOfTheirOriginatingCallsToThenOrCatch_void()
            {
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                int order = 0;
                int counter = 0;

                Action<int> callback = expected =>
                {
                    Assert.AreEqual(expected, order);
                    if (++counter == TestHelper.rejectVoidCallbacks * 2)
                    {
                        counter = 0;
                        ++order;
                    }
                };

                TestHelper.AddCallbacks<bool, object, string>(promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callback(0),
                    () => callback(0)
                );

                TestHelper.AddCallbacks<bool, object, string>(promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callback(1),
                    () => callback(1)
                );

                TestHelper.AddCallbacks<bool, object, string>(promise,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callback(2),
                    () => callback(2)
                );

                deferred.Reject("Fail value");

                Assert.AreEqual(3, order);

                promise.Forget();
            }

            [Test]
            public void _2_2_6_2_IfWhenPromiseIsRejected_AllRespectiveOnRejectedCallbacksMustExecuteInTheOrderOfTheirOriginatingCallsToThenOrCatch_T()
            {
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                int orderT = 0;
                int counterT = 0;

                Action<int> callbackT = expected =>
                {
                    Assert.AreEqual(expected, orderT);
                    if (++counterT == TestHelper.rejectTCallbacks * 2)
                    {
                        counterT = 0;
                        ++orderT;
                    }
                };

                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callbackT(0),
                    () => callbackT(0)
                );

                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callbackT(1),
                    () => callbackT(1)
                );

                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    _ => callbackT(2),
                    () => callbackT(2)
                );

                deferred.Reject("Fail value");

                Assert.AreEqual(3, orderT);

                promise.Forget();
            }
        }

        public class ThenMustReturnAPromise_2_2_7
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

            // 2.2.7.1 Promise Resolution Procedure in 2.3

            [Test]
            public void _2_2_7_2_IfOnFulfilledThrowsAnExceptionE_Promise2MustBeRejectedWithEAsTheReason_void()
            {
                var deferred = Promise.NewDeferred();
                var promise = deferred.Promise.Preserve();

                int exceptionCount = 0;
                Exception expected = new Exception("Fail value");

                Action<Promise> catchCallback = p =>
                    p.Catch((Exception e) =>
                    {
                        Assert.AreEqual(expected, e);
                        ++exceptionCount;
                    }).Forget();

                TestHelper.AddResolveCallbacks<bool, string>(promise,
                    onResolve: () => { throw expected; },
                    onResolveCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p)
                );
                TestHelper.AddCallbacks<bool, object, string>(promise,
                    onReject: s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    onUnknownRejection: () => Assert.Fail("Promise was rejected when it should have been resolved."),
                    onResolve: () => { throw expected; },
                    onResolveCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p)
                );

                deferred.Resolve();

                Assert.AreEqual(
                    (TestHelper.resolveVoidVoidCallbacks + TestHelper.resolveVoidConvertCallbacks +
                    TestHelper.resolveVoidPromiseVoidCallbacks + TestHelper.resolveVoidPromiseConvertCallbacks) * 2,
                    exceptionCount
                );

                promise.Forget();
            }

            [Test]
            public void _2_2_7_2_IfOnFulfilledThrowsAnExceptionE_Promise2MustBeRejectedWithEAsTheReason_T()
            {
                var deferred = Promise.NewDeferred<int>();
                var promise = deferred.Promise.Preserve();

                int exceptionCount = 0;
                Exception expected = new Exception("Fail value");

                Action<Promise> catchCallback = p =>
                    p.Catch((Exception e) =>
                    {
                        Assert.AreEqual(expected, e);
                        ++exceptionCount;
                    }).Forget();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                    onResolve: v => { throw expected; },
                    onResolveCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p)
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise,
                    onReject: s => Assert.Fail("Promise was rejected when it should have been resolved."),
                    onUnknownRejection: () => Assert.Fail("Promise was rejected when it should have been resolved."),
                    onResolve: v => { throw expected; },
                    onResolveCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p),
                    onCallbackAddedT: (ref Promise<int> p) => catchCallback(p)
                );

                deferred.Resolve(100);

                Assert.AreEqual(
                    (TestHelper.resolveTVoidCallbacks + TestHelper.resolveTConvertCallbacks +
                    TestHelper.resolveTPromiseVoidCallbacks + TestHelper.resolveTPromiseConvertCallbacks) * 2,
                    exceptionCount
                );

                promise.Forget();
            }

            [Test]
            public void _2_2_7_2_IfOnRejectedThrowsAnExceptionE_Promise2MustBeRejectedWithEAsTheReason_void()
            {
                var deferred = Promise.NewDeferred();

                int exceptionCount = 0;
                Exception expected = new Exception("Fail value");

                Action<Promise> catchCallback = p =>
                    p.Catch((Exception e) =>
                    {
                        Assert.AreEqual(expected, e);
                        ++exceptionCount;
                    }).Forget();

                TestHelper.AddCallbacks<bool, object, string>(deferred.Promise,
                    onResolve: () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    onReject: _ => { throw expected; },
                    onRejectCapture: _ => { throw expected; },
                    onUnknownRejection: () => { throw expected; },
                    onUnknownRejectionCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p)
                );

                deferred.Reject("Fail value");

                Assert.AreEqual(
                    (TestHelper.rejectVoidVoidCallbacks + TestHelper.rejectVoidConvertCallbacks +
                    TestHelper.rejectVoidPromiseVoidCallbacks + TestHelper.rejectVoidPromiseConvertCallbacks) * 2,
                    exceptionCount
                );
            }

            [Test]
            public void _2_2_7_2_IfOnRejectedThrowsAnExceptionE_Promise2MustBeRejectedWithEAsTheReason_T()
            {
                var deferred = Promise.NewDeferred<int>();

                int exceptionCount = 0;
                Exception expected = new Exception("Fail value");

                Action<Promise> catchCallback = p =>
                    p.Catch((Exception e) =>
                    {
                        Assert.AreEqual(expected, e);
                        ++exceptionCount;
                    }).Forget();

                TestHelper.AddCallbacks<int, bool, object, string>(deferred.Promise,
                    onResolve: _ => Assert.Fail("Promise was resolved when it should have been rejected."),
                    onReject: _ => { throw expected; },
                    onRejectCapture: _ => { throw expected; },
                    onUnknownRejection: () => { throw expected; },
                    onUnknownRejectionCapture: _ => { throw expected; },
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<bool> p) => catchCallback(p),
                    onCallbackAddedT: (ref Promise<int> p) => catchCallback(p)
                );

                deferred.Reject("Fail value");

                Assert.AreEqual(
                    (TestHelper.rejectTVoidCallbacks + TestHelper.rejectTConvertCallbacks + TestHelper.rejectTTCallbacks +
                    TestHelper.rejectTPromiseVoidCallbacks + TestHelper.rejectTPromiseConvertCallbacks + TestHelper.rejectTPromiseTCallbacks) * 2,
                    exceptionCount
                );
            }

            [Test]
            public void _2_2_7_3_IfOnFulfilledIsNotAFunctionAndPromise1IsFulfilled_Promise2MustBeFulfilledWithTheSameValueAsPromise1_void_0()
            {
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Catch(() => { Assert.Fail("Promise was rejected when it should have been resolved."); return; })
                    .Preserve();

                TestHelper.AddResolveCallbacks<bool, string>(promise2,
                    () => ++counter
                );
                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => ++counter,
                    s => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                deferred.Resolve();

                Assert.AreEqual(
                    TestHelper.resolveVoidCallbacks * 2,
                    counter
                );

                promise2.Forget();
            }

            [Test]
            public void _2_2_7_3_IfOnFulfilledIsNotAFunctionAndPromise1IsFulfilled_Promise2MustBeFulfilledWithTheSameValueAsPromise1_void_1()
            {
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Catch(() => { Assert.Fail("Promise was rejected when it should have been resolved."); return Promise.Resolved(); })
                    .Preserve();

                TestHelper.AddResolveCallbacks<bool, string>(promise2,
                    () => ++counter
                );
                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => ++counter,
                    s => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                deferred.Resolve();

                Assert.AreEqual(
                    TestHelper.resolveVoidCallbacks * 2,
                    counter
                );

                promise2.Forget();
            }

            [Test]
            public void _2_2_7_3_IfOnFulfilledIsNotAFunctionAndPromise1IsFulfilled_Promise2MustBeFulfilledWithTheSameValueAsPromise1_T_0()
            {
                int expected = 100;
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Catch(() => { Assert.Fail("Promise was rejected when it should have been resolved."); return 50; })
                    .Preserve();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise2,
                    v => { Assert.AreEqual(expected, v); ++counter; }
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => { Assert.AreEqual(expected, v); ++counter; },
                    s => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                deferred.Resolve(expected);

                Assert.AreEqual(
                    TestHelper.resolveTCallbacks * 2,
                    counter
                );

                promise2.Forget();
            }

            [Test]
            public void _2_2_7_3_IfOnFulfilledIsNotAFunctionAndPromise1IsFulfilled_Promise2MustBeFulfilledWithTheSameValueAsPromise1_T_1()
            {
                int expected = 100;
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Catch(() => { Assert.Fail("Promise was rejected when it should have been resolved."); return Promise.Resolved(50); })
                    .Preserve();

                TestHelper.AddResolveCallbacks<int, bool, string>(promise2,
                    v => { Assert.AreEqual(expected, v); ++counter; }
                );
                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => { Assert.AreEqual(expected, v); ++counter; },
                    s => Assert.Fail("Promise was rejected when it should have been resolved.")
                );

                deferred.Resolve(expected);

                Assert.AreEqual(
                    TestHelper.resolveTCallbacks * 2,
                    counter
                );

                promise2.Forget();
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_void_0()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(() => { Assert.Fail("Promise was resolved when it should have been rejected."); return; });

                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectVoidCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_void_1()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(() => { Assert.Fail("Promise was resolved when it should have been rejected."); return Promise.Resolved(); });

                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectVoidCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_void_2()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(() => { Assert.Fail("Promise was resolved when it should have been rejected."); return 50; });

                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectVoidCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_void_3()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(() => { Assert.Fail("Promise was resolved when it should have been rejected."); return Promise.Resolved(50); });

                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectVoidCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_T_0()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(v => { Assert.Fail("Promise was resolved when it should have been rejected."); return 50; });

                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectTCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_T_1()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(v => { Assert.Fail("Promise was resolved when it should have been rejected."); return Promise.Resolved(50); });

                TestHelper.AddCallbacks<int, bool, object, string>(promise2,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectTCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_T_2()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(v => { Assert.Fail("Promise was resolved when it should have been rejected."); return; });

                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectTCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void _2_2_7_4_IfOnRejectedIsNotAFunctionAndPromise1IsRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_T_3()
            {
                string expected = "Fail value";
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;
                var promise2 = promise1
                    .Then(v => { Assert.Fail("Promise was resolved when it should have been rejected."); return Promise.Resolved(); });

                TestHelper.AddCallbacks<bool, object, string>(promise2,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { Assert.AreEqual(expected, e); ++counter; },
                    () => ++counter
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    TestHelper.rejectTCallbacks * 2,
                    counter
                );
            }

            [Test]
            public void IfPromise1IsRejectedAndItsReasonIsNotCompatibleWithOnRejected_ItMustNotBeInvoked_void()
            {
                int counter = 0;
                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;

                TestHelper.AddCallbacks<bool, string, string>(promise1,
                    () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { ++counter; Assert.Fail("OnRejected was invoked with a string when the promise was rejected with an integer."); },
                    onCallbackAdded: (ref Promise p) => p.Catch((int _) => { }).Forget(),
                    onCallbackAddedConvert: (ref Promise<bool> p) => p.Catch((int _) => { }).Forget()
                );

                deferred.Reject(100);

                Assert.AreEqual(0, counter);
            }

            [Test]
            public void IfPromise1IsRejectedAndItsReasonIsNotCompatibleWithOnRejected_ItMustNotBeInvoked_T()
            {
                int counter = 0;
                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;

                TestHelper.AddCallbacks<int, bool, string, string>(promise1,
                    v => Assert.Fail("Promise was resolved when it should have been rejected."),
                    e => { ++counter; Assert.Fail("OnRejected was invoked with a string when the promise was rejected with an integer."); },
                    onCallbackAdded: (ref Promise p) => p.Catch((int _) => { }).Forget(),
                    onCallbackAddedConvert: (ref Promise<bool> p) => p.Catch((int _) => { }).Forget(),
                    onCallbackAddedT: (ref Promise<int> p) => p.Catch((int _) => { }).Forget()
                );

                deferred.Reject(100);

                Assert.AreEqual(0, counter);
            }

            [Test]
            public void IfPromise1IsRejectedAndItsReasonIsNotCompatibleWithOnRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_void()
            {
                int expected = 100;
                int counter = 0;

                var deferred = Promise.NewDeferred();
                var promise1 = deferred.Promise;

                Action<Promise> catchCallback = p =>
                    p.Catch((int i) =>
                    {
                        Assert.AreEqual(expected, i);
                        ++counter;
                    }).Forget();

                TestHelper.AddCallbacks<int, string, string>(promise1,
                    onResolve: () => Assert.Fail("Promise was resolved when it should have been rejected."),
                    onReject: (string cancelString) => Assert.Fail("OnRejected was invoked with a string when the promise was rejected with an integer."),
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<int> p) => catchCallback(p)
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    (TestHelper.rejectVoidVoidCallbacks + TestHelper.rejectVoidConvertCallbacks +
                    TestHelper.rejectVoidPromiseVoidCallbacks + TestHelper.rejectVoidPromiseConvertCallbacks
                    - TestHelper.rejectVoidKnownCallbacks) * 2,
                    counter
                );
            }

            [Test]
            public void IfPromise1IsRejectedAndItsReasonIsNotCompatibleWithOnRejected_Promise2MustBeRejectedWithTheSameReasonAsPromise1_T()
            {
                int expected = 100;
                int counter = 0;

                var deferred = Promise.NewDeferred<int>();
                var promise1 = deferred.Promise;

                Action<Promise> catchCallback = p =>
                    p.Catch((int i) =>
                    {
                        Assert.AreEqual(expected, i);
                        ++counter;
                    }).Forget();

                TestHelper.AddCallbacks<int, int, string, string>(promise1,
                    onResolve: _ => Assert.Fail("Promise was resolved when it should have been rejected."),
                    onReject: (string cancelString) => Assert.Fail("OnRejected was invoked with a string when the promise was rejected with an integer."),
                    onCallbackAdded: (ref Promise p) => catchCallback(p),
                    onCallbackAddedConvert: (ref Promise<int> p) => catchCallback(p),
                    onCallbackAddedT: (ref Promise<int> p) => catchCallback(p)
                );

                deferred.Reject(expected);

                Assert.AreEqual(
                    (TestHelper.rejectTVoidCallbacks + TestHelper.rejectTConvertCallbacks + TestHelper.rejectTTCallbacks +
                    TestHelper.rejectTPromiseVoidCallbacks + TestHelper.rejectTPromiseConvertCallbacks + TestHelper.rejectTPromiseTCallbacks
                    - TestHelper.rejectTKnownCallbacks) * 2,
                    counter
                );
            }
        }
    }
}