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

using System;
using NUnit.Framework;

namespace Proto.Promises.Tests
{
    public class CaptureTests
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

#if PROMISE_PROGRESS
        [Test]
        public void IfOnProgressIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Progress(100, default(Action<int, float>));
            });

            deferred.Resolve();
            promise.Forget();
        }

        [Test]
        public void IfOnProgressIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Progress(100, default(Action<int, float>));
            });

            deferred.Resolve(1);
            promise.Forget();
        }
#endif

        [Test]
        public void IfOnCanceledIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.CatchCancelation(100, default(Promise.CanceledAction<int>));
            });

            deferred.Resolve();
            promise.Forget();
        }

        [Test]
        public void IfOnCanceledIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.CatchCancelation(100, default(Promise.CanceledAction<int>));
            });

            deferred.Resolve(1);
            promise.Forget();
        }

        [Test]
        public void IfOnFinallyIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Finally(100, default(Action<int>));
            });

            deferred.Resolve();

            promise.Forget();
        }

        [Test]
        public void IfOnFinallyIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Finally(100, default(Action<int>));
            });

            deferred.Resolve(1);

            promise.Forget();
        }

        [Test]
        public void IfOnContinueIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise.ContinueAction<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise.ContinueFunc<int, bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise.ContinueFunc<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise.ContinueFunc<int, Promise<bool>>));
            });

            deferred.Resolve();

            promise.Forget();
        }

        [Test]
        public void IfOnContinueIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise<int>.ContinueAction<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise<int>.ContinueFunc<int, bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise<int>.ContinueFunc<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.ContinueWith(100, default(Promise<int>.ContinueFunc<int, Promise<bool>>));
            });

            deferred.Resolve(1);

            promise.Forget();
        }

        [Test]
        public void IfOnFulfilledIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Action<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise<bool>>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Action<int>), () => { });
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Action<int>), (string failValue) => { });
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, bool>), () => default(bool));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, bool>), (string failValue) => default(bool));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise>), () => default(Promise));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise>), (string failValue) => default(Promise));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise<bool>>), () => default(Promise<bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise<bool>>), (string failValue) => default(Promise<bool>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Action<int>), () => default(Promise));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Action<int>), (string failValue) => default(Promise));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, bool>), () => default(Promise<bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, bool>), (string failValue) => default(Promise<bool>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise>), () => { });
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise>), (string failValue) => { });
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise<bool>>), () => default(bool));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(100, default(Func<int, Promise<bool>>), (string failValue) => default(bool));
            });

            deferred.Resolve();
            promise.Forget();
        }

        [Test]
        public void IfOnFulfilledIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Action<bool, int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise<int>>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Action<bool, int>), () => { });
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Action<bool, int>), (string failValue) => { });
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, int>), () => default(int));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, int>), (string failValue) => default(int));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise>), () => default(Promise));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise>), (string failValue) => default(Promise));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise<int>>), () => default(Promise<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise<int>>), (string failValue) => default(Promise<int>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Action<bool, int>), () => default(Promise));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Action<bool, int>), (string failValue) => default(Promise));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, int>), () => default(Promise<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, int>), (string failValue) => default(Promise<int>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise>), () => { });
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise>), (string failValue) => { });
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise<int>>), () => default(int));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(true, default(Func<bool, int, Promise<int>>), (string failValue) => default(int));
            });

            deferred.Resolve(1);
            promise.Forget();
        }

        [Test]
        public void IfOnRejectedIsNullThrow_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(100, default(Action<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(100, default(Action<int, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(100, default(Func<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(100, default(Func<int, string, Promise>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => { }, 100, default(Action<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => { }, 100, default(Action<int, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise), 100, default(Func<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise), 100, default(Func<int, string, Promise>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => "string", 100, default(Func<int, string>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => "string", 100, default(Func<int, Exception, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise<string>), 100, default(Func<int, Promise<string>>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise<string>), 100, default(Func<int, Exception, Promise<string>>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise), 100, default(Action<int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise), 100, default(Action<int, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => { }, 100, default(Func<int, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => { }, 100, default(Func<int, string, Promise>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise<string>), 100, default(Func<int, string>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => default(Promise<string>), 100, default(Func<int, Exception, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => "string", 100, default(Func<int, Promise<string>>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then(() => "string", 100, default(Func<int, Exception, Promise<string>>));
            });

            deferred.Resolve();
            promise.Forget();
        }

        [Test]
        public void IfOnRejectedIsNullThrow_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(true, default(Func<bool, int>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(true, default(Func<bool, string, int>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(true, default(Func<bool, Promise<int>>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Catch(true, default(Func<bool, string, Promise<int>>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => { }, true, default(Action<bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => { }, true, default(Action<bool, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise), true, default(Func<bool, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise), true, default(Func<bool, string, Promise>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => "string", true, default(Func<bool, string>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => "string", true, default(Func<bool, Exception, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise<string>), true, default(Func<bool, Promise<string>>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise<string>), true, default(Func<bool, Exception, Promise<string>>));
            });


            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise), true, default(Action<bool>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise), true, default(Action<bool, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => { }, true, default(Func<bool, Promise>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => { }, true, default(Func<bool, string, Promise>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise<string>), true, default(Func<bool, string>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => default(Promise<string>), true, default(Func<bool, Exception, string>));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => "string", true, default(Func<bool, Promise<string>>));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                promise.Then((int x) => "string", true, default(Func<bool, Exception, Promise<string>>));
            });

            deferred.Resolve(1);
            promise.Forget();
        }
#endif

#if PROMISE_PROGRESS
        [Test]
        public void OnProgressWillBeInvokedWithCapturedValue_void()
        {
            string expected = "expected";
            bool invoked = false;

            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            promise
                .Progress(expected, (cv, progress) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                });

            deferred.ReportProgress(0.5f);
            Promise.Manager.HandleCompletesAndProgress();
            deferred.Resolve();
            Promise.Manager.HandleCompletesAndProgress();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnProgressWillBeInvokedWithCapturedValue_T()
        {
            string expected = "expected";
            bool invoked = false;

            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            promise
                .Progress(expected, (cv, progress) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                });

            deferred.ReportProgress(0.5f);
            Promise.Manager.HandleCompletesAndProgress();
            deferred.Resolve(1);
            Promise.Manager.HandleCompletesAndProgress();

            Assert.IsTrue(invoked);

            promise.Forget();
        }
#endif

        [Test]
        public void OnCanceledWillBeInvokedWithCapturedValue_void0()
        {
            string expected = "expected";
            bool invoked = false;

            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred(cancelationSource.Token);

            deferred.Promise
                .CatchCancelation(expected, (cv, reason) =>
                {
                    Assert.AreEqual(expected, cv);
                    Assert.IsNull(reason.ValueType);
                    invoked = true;
                })
                .Forget();

            cancelationSource.Cancel();
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
        }

        [Test]
        public void OnCanceledWillBeInvokedWithCapturedValue_void1()
        {
            string expected = "expected";
            int cancelValue = 50;
            bool invoked = false;

            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred(cancelationSource.Token);

            deferred.Promise
                .CatchCancelation(expected, (cv, reason) =>
                {
                    Assert.AreEqual(expected, cv);
                    Assert.AreEqual(cancelValue, reason.Value);
                    invoked = true;
                })
                .Forget();

            cancelationSource.Cancel(cancelValue);
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
        }

        [Test]
        public void OnCanceledWillBeInvokedWithCapturedValue_T0()
        {
            string expected = "expected";
            bool invoked = false;

            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred<int>(cancelationSource.Token);

            deferred.Promise
                .CatchCancelation(expected, (cv, reason) =>
                {
                    Assert.AreEqual(expected, cv);
                    Assert.IsNull(reason.ValueType);
                    invoked = true;
                })
                .Forget();

            cancelationSource.Cancel();
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
        }

        [Test]
        public void OnCanceledWillBeInvokedWithCapturedValue_T1()
        {
            string expected = "expected";
            int cancelValue = 50;
            bool invoked = false;

            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred<int>(cancelationSource.Token);

            deferred.Promise
                .CatchCancelation(expected, (cv, reason) =>
                {
                    Assert.AreEqual(expected, cv);
                    Assert.AreEqual(cancelValue, reason.Value);
                    invoked = true;
                })
                .Forget();

            cancelationSource.Cancel(cancelValue);
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_resolved_void()
        {
            var deferred = Promise.NewDeferred();

            string expected = "expected";
            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Forget();

            deferred.Resolve();

            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_resolved_T()
        {
            var deferred = Promise.NewDeferred<int>();

            string expected = "expected";
            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Forget();

            deferred.Resolve(1);

            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_rejected_void()
        {
            var deferred = Promise.NewDeferred();

            string expected = "expected";
            string rejection = "Reject";
            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Catch((string e) => Assert.AreEqual(rejection, e))
                .Forget();

            deferred.Reject(rejection);

            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_rejected_T()
        {
            var deferred = Promise.NewDeferred<int>();

            string expected = "expected";
            string rejection = "Reject";
            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Catch((string e) => Assert.AreEqual(rejection, e))
                .Forget();

            deferred.Reject(rejection);

            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_canceled_void()
        {
            string expected = "expected";
            bool repeat = true;
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred(cancelationSource.Token);

            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Forget();

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }
            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void OnFinallyWillBeInvokedWithCapturedValue_canceled_T()
        {
            string expected = "expected";
            bool repeat = true;
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred<int>(cancelationSource.Token);

            bool invoked = false;

            deferred.Promise
                .Finally(expected, cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                })
                .Forget();

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }
            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_resolve_void()
        {
            var deferred = Promise.NewDeferred();

            bool invoked = false;
            Exception expected = new Exception();

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            deferred.Resolve();

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_resolve_T()
        {
            var deferred = Promise.NewDeferred<int>();

            bool invoked = false;
            Exception expected = new Exception();

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            deferred.Resolve(1);

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_reject_void()
        {
            var deferred = Promise.NewDeferred();

            bool invoked = false;
            string rejectValue = "Reject";
            Exception expected = new Exception();

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            // When the exception thrown in onFinally overwrites the current rejection, the current rejection gets sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it actually gets sent to it.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            bool uncaughtHandled = false;
            Promise.Config.UncaughtRejectionHandler = e =>
            {
                Assert.AreEqual(rejectValue, e.Value);
                uncaughtHandled = true;
            };

            deferred.Reject(rejectValue);

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);
            Assert.IsTrue(uncaughtHandled);
            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_reject_T()
        {
            var deferred = Promise.NewDeferred<int>();

            bool invoked = false;
            string rejectValue = "Reject";
            Exception expected = new Exception();

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            // When the exception thrown in onFinally overwrites the current rejection, the current rejection gets sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it actually gets sent to it.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            bool uncaughtHandled = false;
            Promise.Config.UncaughtRejectionHandler = e =>
            {
                Assert.AreEqual(rejectValue, e.Value);
                uncaughtHandled = true;
            };

            deferred.Reject(rejectValue);

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);
            Assert.IsTrue(uncaughtHandled);
            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_cancel_void()
        {
            bool repeat = true;
            Exception expected = new Exception();
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred(cancelationSource.Token);

            bool invoked = false;

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);

            cancelationSource.Dispose();

            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void PromiseIsRejectedWithThrownExceptionWhenOnFinallyWithCapturedValueThrows_cancel_T()
        {
            bool repeat = true;
            Exception expected = new Exception();
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred<int>(cancelationSource.Token);

            bool invoked = false;

            deferred.Promise
                .Finally(100, cv => { throw expected; })
                .Catch((Exception e) =>
                {
                    Assert.AreEqual(expected, e);
                    invoked = true;
                })
                .Forget();

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }

            Promise.Manager.HandleCompletes();
            Assert.IsTrue(invoked);

            cancelationSource.Dispose();

            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_resolved_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddContinueCallbacks<int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Resolve();
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_resolved_T()
        {
            string expected = "expected";
            bool invoked = false;

            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Resolve(50);
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_rejected_void()
        {
            string expected = "expected";
            bool invoked = false;

            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            TestHelper.AddContinueCallbacks<int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Reject("Reject");
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_rejected_T()
        {
            string expected = "expected";
            bool invoked = false;

            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Reject("Reject");
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_canceled_void()
        {
            string expected = "expected";
            bool repeat = true;
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred(cancelationSource.Token);
            var promise = deferred.Promise.Preserve();

            bool invoked = false;

            TestHelper.AddContinueCallbacks<int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
            promise.Forget();
            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void OnContinueWillBeInvokedWithCapturedValue_canceled_T()
        {
            bool repeat = true;
        Repeat:
            CancelationSource cancelationSource = CancelationSource.New();
            var deferred = Promise.NewDeferred<int>(cancelationSource.Token);
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddContinueCallbacks<int, int, string>(promise,
                captureValue: expected,
                onContinueCapture: (cv, r) =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            if (repeat)
            {
                cancelationSource.Cancel();
            }
            else
            {
                cancelationSource.Cancel("Cancel");
            }
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            cancelationSource.Dispose();
            promise.Forget();
            if (repeat)
            {
                repeat = false;
                goto Repeat;
            }
        }

        [Test]
        public void OnResolvedWillBeInvokedWithCapturedValue_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddResolveCallbacks<int, string>(promise,
                captureValue: expected,
                onResolveCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );
            TestHelper.AddCallbacks<int, object, string>(promise,
                captureValue: expected,
                onResolveCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Resolve();
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnResolvedWillBeInvokedWithCapturedValue_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddResolveCallbacks<int, bool, string>(promise,
                captureValue: expected,
                onResolveCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );
            TestHelper.AddCallbacks<int, bool, object, string>(promise,
                captureValue: expected,
                onResolveCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Resolve(50);
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnRejectedWillBeInvokedWithCapturedValue_void()
        {
            var deferred = Promise.NewDeferred();
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddCallbacks<int, object, string>(promise,
                captureValue: expected,
                onRejectCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                },
                onUnknownRejectionCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Reject("Reject");
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }

        [Test]
        public void OnRejectedWillBeInvokedWithCapturedValue_T()
        {
            var deferred = Promise.NewDeferred<int>();
            var promise = deferred.Promise.Preserve();

            string expected = "expected";
            bool invoked = false;

            TestHelper.AddCallbacks<int, bool, object, string>(promise,
                captureValue: expected,
                onResolveCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                },
                onUnknownRejectionCapture: cv =>
                {
                    Assert.AreEqual(expected, cv);
                    invoked = true;
                }
            );

            deferred.Reject("Reject");
            Promise.Manager.HandleCompletes();

            Assert.IsTrue(invoked);

            promise.Forget();
        }
    }
}