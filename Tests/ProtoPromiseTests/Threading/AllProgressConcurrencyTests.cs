﻿#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#else
#undef PROMISE_PROGRESS
#endif

#if CSHARP_7_OR_LATER
#if PROMISE_PROGRESS

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Proto.Promises.Tests.Threading
{
    public class AllProgressConcurrencyTests
    {
        const string rejectValue = "Fail";

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

        [Theory]
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_void0(CompleteType completeType0, CompleteType completeType1)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var allPromise = default(Promise);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_void1(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var deferred2 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var allPromise = default(Promise);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_void2(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)]CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);
            var completer3 = TestHelper.GetCompleterVoid(completeType3, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var deferred2 = default(Promise.Deferred);
            var deferred3 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            var allPromise = default(Promise);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2);
                    deferred3 = TestHelper.GetNewDeferredVoid(completeType3, out cancelationSource3);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise, deferred3.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => completer3(deferred3, cancelationSource3),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () => deferred3.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_void3(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);
            var completer3 = TestHelper.GetCompleterVoid(completeType3, rejectValue);

            Promise.Deferred[] deferreds = null;
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            var allPromise = default(Promise);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferreds = new Promise.Deferred[]
                    {
                        TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0),
                        TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1),
                        TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2),
                        TestHelper.GetNewDeferredVoid(completeType3, out cancelationSource3)
                    };
                    allPromise = Promise.All(deferreds.Select(d => d.Promise).GetEnumerator());
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.IsTrue(continueInvoked);
                },
                // Parallel actions
                () => completer0(deferreds[0], cancelationSource0),
                () => completer1(deferreds[1], cancelationSource1),
                () => completer2(deferreds[2], cancelationSource2),
                () => completer3(deferreds[3], cancelationSource3),
                () => deferreds[0].TryReportProgress(0.5f),
                () => deferreds[1].TryReportProgress(0.5f),
                () => deferreds[2].TryReportProgress(0.5f),
                () => deferreds[3].TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Theory]
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_T0(CompleteType completeType0, CompleteType completeType1)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var allPromise = default(Promise<IList<int>>);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                            }
                        })
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_T1(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var deferred2 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var allPromise = default(Promise<IList<int>>);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                            }
                        })
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_T2(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);
            var completer3 = TestHelper.GetCompleterT(completeType3, 4, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var deferred2 = default(Promise<int>.Deferred);
            var deferred3 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            var allPromise = default(Promise<IList<int>>);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2);
                    deferred3 = TestHelper.GetNewDeferredT<int>(completeType3, out cancelationSource3);
                    allPromise = Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise, deferred3.Promise);
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => completer3(deferred3, cancelationSource3),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () => deferred3.TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                                Assert.AreEqual(4, v[3]);
                            }
                        })
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedWhileProgressIsSubscribedConcurrently_T3(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);
            var completer3 = TestHelper.GetCompleterT(completeType3, 4, rejectValue);

            Promise<int>.Deferred[] deferreds = null;
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            var allPromise = default(Promise<IList<int>>);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferreds = new Promise<int>.Deferred[]
                    {
                        TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0),
                        TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1),
                        TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2),
                        TestHelper.GetNewDeferredT<int>(completeType3, out cancelationSource3)
                    };
                    allPromise = Promise<int>.All(deferreds.Select(d => d.Promise).GetEnumerator());
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.IsTrue(continueInvoked);
                },
                // Parallel actions
                () => completer0(deferreds[0], cancelationSource0),
                () => completer1(deferreds[1], cancelationSource1),
                () => completer2(deferreds[2], cancelationSource2),
                () => completer3(deferreds[3], cancelationSource3),
                () => deferreds[0].TryReportProgress(0.5f),
                () => deferreds[1].TryReportProgress(0.5f),
                () => deferreds[2].TryReportProgress(0.5f),
                () => deferreds[3].TryReportProgress(0.5f),
                () =>
                {
                    allPromise
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                                Assert.AreEqual(4, v[3]);
                            }
                        })
                        .Forget();
                }
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Theory]
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_void0(CompleteType completeType0, CompleteType completeType1)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    Promise.All(deferred0.Promise, deferred1.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_void1(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var deferred2 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2);
                    Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_void2(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);
            var completer3 = TestHelper.GetCompleterVoid(completeType3, rejectValue);

            var deferred0 = default(Promise.Deferred);
            var deferred1 = default(Promise.Deferred);
            var deferred2 = default(Promise.Deferred);
            var deferred3 = default(Promise.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2);
                    deferred3 = TestHelper.GetNewDeferredVoid(completeType3, out cancelationSource3);
                    Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise, deferred3.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => completer3(deferred3, cancelationSource3),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () => deferred3.TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_void3(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterVoid(completeType0, rejectValue);
            var completer1 = TestHelper.GetCompleterVoid(completeType1, rejectValue);
            var completer2 = TestHelper.GetCompleterVoid(completeType2, rejectValue);
            var completer3 = TestHelper.GetCompleterVoid(completeType3, rejectValue);

            Promise.Deferred[] deferreds = null;
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferreds = new Promise.Deferred[]
                    {
                        TestHelper.GetNewDeferredVoid(completeType0, out cancelationSource0),
                        TestHelper.GetNewDeferredVoid(completeType1, out cancelationSource1),
                        TestHelper.GetNewDeferredVoid(completeType2, out cancelationSource2),
                        TestHelper.GetNewDeferredVoid(completeType3, out cancelationSource3)
                    };
                    Promise.All(deferreds.Select(d => d.Promise).GetEnumerator())
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r => continueInvoked = true)
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.IsTrue(continueInvoked);
                },
                // Parallel actions
                () => completer0(deferreds[0], cancelationSource0),
                () => completer1(deferreds[1], cancelationSource1),
                () => completer2(deferreds[2], cancelationSource2),
                () => completer3(deferreds[3], cancelationSource3),
                () => deferreds[0].TryReportProgress(0.5f),
                () => deferreds[1].TryReportProgress(0.5f),
                () => deferreds[2].TryReportProgress(0.5f),
                () => deferreds[3].TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Theory]
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_T0(CompleteType completeType0, CompleteType completeType1)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    Promise.All(deferred0.Promise, deferred1.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                            }
                        })
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_T1(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var deferred2 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            threadHelper.ExecuteParallelActionsWithOffsets(false,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2);
                    Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                            }
                        })
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_T2(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);
            var completer3 = TestHelper.GetCompleterT(completeType3, 4, rejectValue);

            var deferred0 = default(Promise<int>.Deferred);
            var deferred1 = default(Promise<int>.Deferred);
            var deferred2 = default(Promise<int>.Deferred);
            var deferred3 = default(Promise<int>.Deferred);
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferred0 = TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0);
                    deferred1 = TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1);
                    deferred2 = TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2);
                    deferred3 = TestHelper.GetNewDeferredT<int>(completeType3, out cancelationSource3);
                    Promise.All(deferred0.Promise, deferred1.Promise, deferred2.Promise, deferred3.Promise)
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                                Assert.AreEqual(4, v[3]);
                            }
                        })
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.AreEqual(continueInvoked, true);
                },
                // Parallel actions
                () => completer0(deferred0, cancelationSource0),
                () => completer1(deferred1, cancelationSource1),
                () => completer2(deferred2, cancelationSource2),
                () => completer3(deferred3, cancelationSource3),
                () => deferred0.TryReportProgress(0.5f),
                () => deferred1.TryReportProgress(0.5f),
                () => deferred2.TryReportProgress(0.5f),
                () => deferred3.TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }

        [Test] // Only generate up to 2 parameters (more takes too long to test)
        public void DeferredsPassedToAllMayBeCompletedConcurrentlyAfterProgressIsSubscribed_T3(
            [Values] CompleteType completeType0,
            [Values] CompleteType completeType1,
            [Values(CompleteType.Resolve)] CompleteType completeType2,
            [Values(CompleteType.Resolve)] CompleteType completeType3)
        {
            // When 2 or more promises are rejected, the remaining rejects are sent to the UncaughtRejectionHandler.
            // So we need to suppress that here and make sure it's correct.
            var currentHandler = Promise.Config.UncaughtRejectionHandler;
            Promise.Config.UncaughtRejectionHandler = e => Assert.AreEqual(rejectValue, e.Value);

            var completer0 = TestHelper.GetCompleterT(completeType0, 1, rejectValue);
            var completer1 = TestHelper.GetCompleterT(completeType1, 2, rejectValue);
            var completer2 = TestHelper.GetCompleterT(completeType2, 3, rejectValue);
            var completer3 = TestHelper.GetCompleterT(completeType3, 4, rejectValue);

            Promise<int>.Deferred[] deferreds = null;
            var cancelationSource0 = default(CancelationSource);
            var cancelationSource1 = default(CancelationSource);
            var cancelationSource2 = default(CancelationSource);
            var cancelationSource3 = default(CancelationSource);
            bool continueInvoked = false;

            var threadHelper = new ThreadHelper();
            // Don't use WithOffsets because there are too many actions.
            threadHelper.ExecuteParallelActions(ThreadHelper.multiExecutionCount,
                // Setup
                () =>
                {
                    deferreds = new Promise<int>.Deferred[]
                    {
                        TestHelper.GetNewDeferredT<int>(completeType0, out cancelationSource0),
                        TestHelper.GetNewDeferredT<int>(completeType1, out cancelationSource1),
                        TestHelper.GetNewDeferredT<int>(completeType2, out cancelationSource2),
                        TestHelper.GetNewDeferredT<int>(completeType3, out cancelationSource3)
                    };
                    Promise<int>.All(deferreds.Select(d => d.Promise).GetEnumerator())
                        .Progress(v => { }) // Callback might not be invoked if the promise is canceled/rejected.
                        .ContinueWith(r =>
                        {
                            continueInvoked = true;
                            if (r.State == Promise.State.Resolved)
                            {
                                var v = r.Result;
                                Assert.AreEqual(1, v[0]);
                                Assert.AreEqual(2, v[1]);
                                Assert.AreEqual(3, v[2]);
                                Assert.AreEqual(4, v[3]);
                            }
                        })
                        .Forget();
                    continueInvoked = false;
                },
                // Teardown
                () =>
                {
                    cancelationSource0.TryDispose();
                    cancelationSource1.TryDispose();
                    cancelationSource2.TryDispose();
                    cancelationSource3.TryDispose();
                    Promise.Manager.HandleCompletes();
                    Assert.IsTrue(continueInvoked);
                },
                // Parallel actions
                () => completer0(deferreds[0], cancelationSource0),
                () => completer1(deferreds[1], cancelationSource1),
                () => completer2(deferreds[2], cancelationSource2),
                () => completer3(deferreds[3], cancelationSource3),
                () => deferreds[0].TryReportProgress(0.5f),
                () => deferreds[1].TryReportProgress(0.5f),
                () => deferreds[2].TryReportProgress(0.5f),
                () => deferreds[3].TryReportProgress(0.5f)
            );

            Promise.Config.UncaughtRejectionHandler = currentHandler;
        }
    }
}

#endif // PROMISE_PROGRESS
#endif // CSHARP_7_OR_LATER