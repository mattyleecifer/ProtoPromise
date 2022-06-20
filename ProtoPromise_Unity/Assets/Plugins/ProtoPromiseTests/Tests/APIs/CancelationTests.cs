﻿#if UNITY_5_5 || NET_2_0 || NET_2_0_SUBSET
#define NET_LEGACY
#endif

#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0059 // Unnecessary assignment of a value

using NUnit.Framework;
using Proto.Promises;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ProtoPromiseTests.APIs
{
    public class CancelationTests
    {
        public class Source
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
            public void NewCancelationSourceIsNotValid()
            {
                CancelationSource cancelationSource = new CancelationSource();
                Assert.IsFalse(cancelationSource.IsValid);
            }

            [Test]
            public void CancelationSourceInvalidOperations()
            {
                CancelationSource cancelationSource = new CancelationSource();
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => { cancelationSource.Cancel(); });
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => { cancelationSource.Dispose(); });
            }

            [Test]
            public void CancelationSourceNewIsValid()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                Assert.IsTrue(cancelationSource.IsValid);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationSourceIsNotValidAfterDispose()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                cancelationSource.Dispose();
                Assert.IsFalse(cancelationSource.IsValid);
            }

            [Test]
            public void CancelationSourceIsValidAfterCancel()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                cancelationSource.Cancel();
                Assert.IsTrue(cancelationSource.IsValid);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationSourceNoCancelationRequestedBeforeCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                Assert.IsFalse(cancelationSource.IsCancelationRequested);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationSourceCancelationRequestedAfterCanceled_0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                cancelationSource.Cancel();
                Assert.IsTrue(cancelationSource.IsCancelationRequested);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationSource2WithTokenCancelationRequestedAfterToken1Canceled_0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token);
                cancelationSource1.Cancel();
                Assert.IsTrue(cancelationSource2.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSource2WithTokenCancelationRequestedAfterToken1Canceled_1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token);
                Assert.IsTrue(cancelationSource2.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSource1WithTokenNotCancelationRequestedAfterToken2Canceled()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token);
                cancelationSource2.Cancel();
                Assert.IsFalse(cancelationSource1.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSource2IsCanceledWhenToken1IsCanceled_0()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token);
                cancelationSource2.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource1.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSource2IsCanceledWhenToken1IsCanceled_1()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token);
                cancelationSource2.Token.Register(() =>
                {
                    invoked = true;
                });
                Assert.IsTrue(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken1Canceled_0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource1.Cancel();
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken1Canceled_1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken2Canceled_0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource2.Cancel();
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken2Canceled_1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource2.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource1WithTokensNotCancelationRequestedAfterToken2Canceled_0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource2.Cancel();
                Assert.IsFalse(cancelationSource1.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource1And2WithTokensNotCancelationRequestedAfterToken3Canceled_0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource3.Cancel();
                Assert.IsFalse(cancelationSource1.IsCancelationRequested);
                Assert.IsFalse(cancelationSource2.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken1IsCanceled_0()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource1.Cancel();
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource2.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken1IsCanceled_1()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource2.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken2IsCanceled_0()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource2.Cancel();
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource1.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken2IsCanceled_1()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource2.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(cancelationSource1.Token, cancelationSource2.Token);
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource1.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken1Canceled_2()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource1.Cancel();
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken1Canceled_3()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken2Canceled_2()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource2.Cancel();
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3WithTokensCancelationRequestedAfterToken2Canceled_3()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource2.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                Assert.IsTrue(cancelationSource3.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource1WithTokensNotCancelationRequestedAfterToken2Canceled_1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource2.Cancel();
                Assert.IsFalse(cancelationSource1.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource1And2WithTokensNotCancelationRequestedAfterToken3Canceled_1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource3.Cancel();
                Assert.IsFalse(cancelationSource1.IsCancelationRequested);
                Assert.IsFalse(cancelationSource2.IsCancelationRequested);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken1IsCanceled_2()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource1.Cancel();
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource2.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken1IsCanceled_3()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource1.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource2.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken2IsCanceled_2()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource3.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource2.Cancel();
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource1.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSource3IsCanceledWhenToken2IsCanceled_3()
            {
                bool invoked = false;
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New();
                cancelationSource2.Cancel();
                CancelationSource cancelationSource3 = CancelationSource.New(new CancelationToken[] { cancelationSource1.Token, cancelationSource2.Token });
                cancelationSource3.Token.Register(()  =>
                {
                    invoked = true;
                });
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource1.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
                cancelationSource3.Dispose();
            }

            [Test]
            public void CancelationSourceLinkedToToken1TwiceIsCanceledWhenToken1Iscanceled()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token, cancelationSource1.Token);
                bool invoked = false;
                cancelationSource2.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource1.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSourceLinkedToToken1TwiceIsNotCanceledWhenToken1Iscanceled()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token, cancelationSource1.Token);
                bool invoked = false;
                cancelationSource2.Token.Register(() =>
                {
                    invoked = true;
                });
                cancelationSource2.Cancel();
                Assert.IsTrue(invoked);
                invoked = false;
                cancelationSource1.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSourceLinkedToToken1TwiceMayBeDisposedSeparately0()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token, cancelationSource1.Token);
                cancelationSource1.Dispose();
                cancelationSource2.Dispose();
            }

            [Test]
            public void CancelationSourceLinkedToToken1TwiceMayBeDisposedSeparately1()
            {
                CancelationSource cancelationSource1 = CancelationSource.New();
                CancelationSource cancelationSource2 = CancelationSource.New(cancelationSource1.Token, cancelationSource1.Token);
                cancelationSource2.Dispose();
                cancelationSource1.Dispose();
            }
        }

        public class Token
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
            public void CancelationTokenCanceledCanBeCanceled()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                Assert.IsTrue(cancelationToken.CanBeCanceled);
            }

            [Test]
            public void NewCancelationTokenCannotBeCanceled()
            {
                CancelationToken cancelationToken = new CancelationToken();
                Assert.IsFalse(cancelationToken.CanBeCanceled);
            }

            [Test]
            public void CancelationTokenCanceledCancelationIsRequested()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                Assert.IsTrue(cancelationToken.IsCancelationRequested);
            }

            [Test]
            public void NewCancelationTokenNoCancelationRequested()
            {
                CancelationToken cancelationToken = new CancelationToken();
                Assert.IsFalse(cancelationToken.IsCancelationRequested);
            }

            [Test]
            public void CancelationTokenNoCancelationRequestedBeforeCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                Assert.IsFalse(cancelationToken.IsCancelationRequested);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenCancelationRequestedAfterCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                Assert.IsTrue(cancelationToken.IsCancelationRequested);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenInvalidOperations()
            {
                CancelationToken cancelationToken = new CancelationToken();
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => { cancelationToken.Register(() => { }); });
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => { cancelationToken.Register(1, i => { }); });
                Assert.Throws<Proto.Promises.InvalidOperationException>(cancelationToken.Retain);
                Assert.Throws<Proto.Promises.InvalidOperationException>(cancelationToken.Release);
            }

            [Test]
            public void CancelationTokenFromSourceCanBeCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                Assert.IsTrue(cancelationToken.CanBeCanceled);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenFromSourceCannotBeCanceledAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Dispose();
                Assert.IsFalse(cancelationToken.CanBeCanceled);
            }

            [Test]
            public void CancelationTokenFromSourceCancelationIsRequested()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                Assert.IsTrue(cancelationToken.IsCancelationRequested);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenFromSourceCancelationIsNotRequestedAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                cancelationSource.Dispose();
                Assert.IsFalse(cancelationToken.IsCancelationRequested);
            }

            [Test]
            public void CancelationTokenCanceledMaybeBeRetainedAndReleased0()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                cancelationToken.Retain();
                cancelationToken.Release();
            }

            [Test]
            public void CancelationTokenCanceledMaybeBeRetainedAndReleased1()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                Assert.IsTrue(cancelationToken.TryRetain());
                cancelationToken.Release();
            }

            [Test]
            public void RetainedCancelationTokenFromSourceCanBeCanceledAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationToken.Retain();
                cancelationSource.Dispose();
                Assert.IsTrue(cancelationToken.CanBeCanceled);
                cancelationToken.Release();
            }

            [Test]
            public void ReleasedCancelationTokenFromSourceCannotBeCanceledAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationToken.Retain();
                cancelationSource.Dispose();
                cancelationToken.Release();
                Assert.IsFalse(cancelationToken.CanBeCanceled);
            }

            [Test]
            public void RetainedCancelationTokenFromSourceCancelationIsRequestedAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                cancelationToken.Retain();
                cancelationSource.Dispose();
                Assert.IsTrue(cancelationToken.IsCancelationRequested);
                cancelationToken.Release();
            }

            [Test]
            public void ReleasedCancelationTokenFromSourceCancelationIsNotRequestedAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                cancelationToken.Retain();
                cancelationSource.Dispose();
                cancelationToken.Release();
                Assert.IsFalse(cancelationToken.IsCancelationRequested);
            }

            [Test]
            public void CancelationTokenCanceledThrowIfCancelationRequested()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                bool caughtException = false;
                try
                {
                    cancelationToken.ThrowIfCancelationRequested();
                }
                catch (CanceledException)
                {
                    caughtException = true;
                }
                Assert.IsTrue(caughtException);
            }

            [Test]
            public void CancelationTokenFromSourceThrowIfCancelationRequested()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationSource.Cancel();
                bool caughtException = false;
                try
                {
                    cancelationToken.ThrowIfCancelationRequested();
                }
                catch (CanceledException)
                {
                    caughtException = true;
                }
                cancelationSource.Dispose();
                Assert.IsTrue(caughtException);
            }

#if !NET_LEGACY || NET40
            [Test]
            public void ToCancellationTokenIsCanceledWhenSourceIsCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                var token = cancelationSource.Token.ToCancellationToken();

                Assert.IsFalse(token.IsCancellationRequested);
                cancelationSource.Cancel();
                Assert.IsTrue(token.IsCancellationRequested);

                token = cancelationSource.Token.ToCancellationToken();
                Assert.IsTrue(token.IsCancellationRequested);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancellationTokenCallbackIsInvokedWhenSourceIsCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                var token = cancelationSource.Token.ToCancellationToken();

                bool invoked = false;
                token.Register(() => invoked = true, false);

                Assert.IsFalse(invoked);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);

                token = cancelationSource.Token.ToCancellationToken();
                invoked = false;
                token.Register(() => invoked = true, false);
                Assert.IsTrue(invoked);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancelationTokenIsCanceledWhenTokenSourceIsCanceled()
            {
                CancellationTokenSource cancelationSource = new CancellationTokenSource();
                var token = cancelationSource.Token.ToCancelationToken();

                Assert.IsFalse(token.IsCancelationRequested);
                cancelationSource.Cancel();
                Assert.IsTrue(token.IsCancelationRequested);

                token = cancelationSource.Token.ToCancelationToken();
                Assert.IsTrue(token.IsCancelationRequested);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancelationTokenCallbackIsInvokedWhenTokenSourceIsCanceled()
            {
                CancellationTokenSource cancelationSource = new CancellationTokenSource();
                var token = cancelationSource.Token.ToCancelationToken();

                bool invoked = false;
                token.Register(() => invoked = true);

                Assert.IsFalse(invoked);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);

                token = cancelationSource.Token.ToCancelationToken();
                invoked = false;
                token.Register(() => invoked = true);
                Assert.IsTrue(invoked);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancellationTokenCircularIsCanceledWhenSourceIsCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                var token = cancelationSource.Token.ToCancellationToken().ToCancelationToken();

                Assert.IsFalse(token.IsCancelationRequested);
                cancelationSource.Cancel();
                Assert.IsTrue(token.IsCancelationRequested);

                token = cancelationSource.Token.ToCancellationToken().ToCancelationToken();
                Assert.IsTrue(token.IsCancelationRequested);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancellationTokenCircularCallbackIsInvokedWhenSourceIsCanceled()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                var token = cancelationSource.Token.ToCancellationToken().ToCancelationToken();

                bool invoked = false;
                token.Register(() => invoked = true);

                Assert.IsFalse(invoked);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);

                token = cancelationSource.Token.ToCancellationToken().ToCancelationToken();
                invoked = false;
                token.Register(() => invoked = true);
                Assert.IsTrue(invoked);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancelationTokenCircularIsCanceledWhenTokenSourceIsCanceled()
            {
                CancellationTokenSource cancelationSource = new CancellationTokenSource();
                var token = cancelationSource.Token.ToCancelationToken().ToCancellationToken();

                Assert.IsFalse(token.IsCancellationRequested);
                cancelationSource.Cancel();
                Assert.IsTrue(token.IsCancellationRequested);

                token = cancelationSource.Token.ToCancelationToken().ToCancellationToken();
                Assert.IsTrue(token.IsCancellationRequested);

                cancelationSource.Dispose();
            }

            [Test]
            public void ToCancelationTokenCircularCallbackIsInvokedWhenTokenSourceIsCanceled()
            {
                CancellationTokenSource cancelationSource = new CancellationTokenSource();
                var token = cancelationSource.Token.ToCancelationToken().ToCancellationToken();

                bool invoked = false;
                token.Register(() => invoked = true, false);

                Assert.IsFalse(invoked);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);

                token = cancelationSource.Token.ToCancelationToken().ToCancellationToken();
                invoked = false;
                token.Register(() => invoked = true, false);
                Assert.IsTrue(invoked);

                cancelationSource.Dispose();
            }

#if !PROMISE_DEBUG && !PROTO_PROMISE_DEVELOPER_MODE // Nulled out fields aren't garbage collected in Debug mode until the end of the scope.
            [MethodImpl(MethodImplOptions.NoInlining)]
            void ConvertToken(CancellationTokenSource source)
            {
                source.Token.ToCancelationToken();
            }

            [Test]
            [MethodImpl((MethodImplOptions) 512)] // AggressiveOptimization. This is necessary in Core runtime so the source will actually be collected.
            public void ToCancelationToken_SourceIsCollectedWhenReferenceIsDropped()
            {
                var cancelationSource = new CancellationTokenSource();
                var weakReference = new WeakReference(cancelationSource, true);

                ConvertToken(cancelationSource);

                Assert.IsTrue(weakReference.IsAlive);
                
                cancelationSource = null;
                TestHelper.GcCollectAndWaitForFinalizers();

                Assert.IsFalse(weakReference.IsAlive);
            }

            // TODO: test cancelationSource.Dispose().

#if NET6_0_OR_GREATER
            [Test]
            [MethodImpl(MethodImplOptions.AggressiveOptimization)] // AggressiveOptimization is necessary in Core runtime so the source will actually be collected.
            public void ToCancelationToken_SourceIsCollectedWhenSourceIsResetThenReferenceIsDropped()
            {
                bool canceled = false;

                var cancelationSource = new CancellationTokenSource();
                var weakReference = new WeakReference(cancelationSource, true);

                ConvertTokenAndRegister(cancelationSource);

                Assert.IsFalse(canceled);
                Assert.IsTrue(weakReference.IsAlive);

                cancelationSource.TryReset();
                cancelationSource = null;
                TestHelper.GcCollectAndWaitForFinalizers();
                Assert.IsFalse(weakReference.IsAlive);
                Assert.IsFalse(canceled);

                [MethodImpl(MethodImplOptions.NoInlining)]
                void ConvertTokenAndRegister(CancellationTokenSource source)
                {
                    source.Token.ToCancelationToken().Register(() => canceled = true);
                }
            }

            [Test]
            [MethodImpl(MethodImplOptions.AggressiveOptimization)] // AggressiveOptimization is necessary in Core runtime so the source will actually be collected.
            public void ToCancelationTokenIsCanceled_AndSourceIsCollected_WhenSourceIsResetThenCanceledThenReferenceIsDropped()
            {
                bool canceled = false;

                var cancelationSource = new CancellationTokenSource();
                var weakReference = new WeakReference(cancelationSource, true);

                ConvertTokenAndRegister(cancelationSource);

                Assert.IsFalse(canceled);
                Assert.IsTrue(weakReference.IsAlive);

                cancelationSource.TryReset();
                cancelationSource.Cancel();
                Assert.IsFalse(canceled);

                ConvertTokenAndRegister(cancelationSource);
                Assert.IsTrue(canceled);

                cancelationSource = null;
                TestHelper.GcCollectAndWaitForFinalizers();
                Assert.IsFalse(weakReference.IsAlive);

                [MethodImpl(MethodImplOptions.NoInlining)]
                void ConvertTokenAndRegister(CancellationTokenSource source)
                {
                    source.Token.ToCancelationToken().Register(() => canceled = true);
                }
            }
#endif // NET6_0_OR_GREATER

#endif // !PROMISE_DEBUG && !PROTO_PROMISE_DEVELOPER_MODE

#endif // !NET_LEGACY || NET40
        }

        public class Registration
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
            public void NewCancelationRegistrationIsNotRegistered()
            {
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                Assert.IsFalse(cancelationRegistration.IsRegistered);
            }

            [Test]
            public void RegistrationFromCancelationTokenIsRegistered0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(() => { });
                Assert.IsTrue(cancelationRegistration.IsRegistered);
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenIsRegistered1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(0, i => { });
                Assert.IsTrue(cancelationRegistration.IsRegistered);
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterInvoked0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(() => { });
                cancelationSource.Cancel();
                Assert.IsFalse(cancelationRegistration.IsRegistered);
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterInvoked1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(0, i => { });
                cancelationSource.Cancel();
                Assert.IsFalse(cancelationRegistration.IsRegistered);
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterInvoked2()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                cancelationRegistration = cancelationToken.Register(() => Assert.IsFalse(cancelationRegistration.IsRegistered));
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterInvoked3()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                cancelationRegistration = cancelationToken.Register(0, i => Assert.IsFalse(cancelationRegistration.IsRegistered));
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void RegistrationFromCancelationTokenCanceledIsNotRegisteredAfterInvoked0()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                CancelationRegistration cancelationRegistration = cancelationToken.Register(() => { });
                Assert.IsFalse(cancelationRegistration.IsRegistered);
            }

            [Test]
            public void RegistrationFromCancelationTokenCanceledIsNotRegisteredAfterInvoked1()
            {
                CancelationToken cancelationToken = CancelationToken.Canceled();
                CancelationRegistration cancelationRegistration = cancelationToken.Register(0, i => { });
                Assert.IsFalse(cancelationRegistration.IsRegistered);
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterSourceIsDisposed0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(() => { });
                cancelationSource.Dispose();
                Assert.IsFalse(cancelationRegistration.IsRegistered);
            }

            [Test]
            public void RegistrationFromCancelationTokenIsNotRegisteredAfterSourceIsDisposed1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(0, i => { });
                cancelationSource.Dispose();
                Assert.IsFalse(cancelationRegistration.IsRegistered);
            }

            [Test]
            public void CancelationTokenRegisterCallbackIsInvoked0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                cancelationToken.Register(() => invoked = true);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenRegisterCallbackIsInvoked1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                cancelationToken.Register(0, i => invoked = true);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenRegisterCallbackIsInvoked2()
            {
                bool invoked = false;
                CancelationToken.Canceled().Register(() => invoked = true);
                Assert.IsTrue(invoked);
            }

            [Test]
            public void CancelationTokenRegisterCallbackIsInvoked3()
            {
                bool invoked = false;
                CancelationToken.Canceled().Register(0, i => invoked = true);
                Assert.IsTrue(invoked);
            }

            [Test]
            public void CancelationRegistrationTryUnregisterCallbackIsInvoked2()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                // Can't unregister cancelation after token is canceled.
                cancelationToken.Register(() => Assert.IsFalse(cancelationRegistration.TryUnregister()));
                cancelationRegistration = cancelationToken.Register(() => invoked = true);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationTryUnregisterCallbackIsInvoked3()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                // Can't unregister cancelation after token is canceled.
                cancelationToken.Register(() => Assert.IsFalse(cancelationRegistration.TryUnregister()));
                cancelationRegistration = cancelationToken.Register(0, i => invoked = true);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationUnregisterCallbackIsNotInvoked0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(() => invoked = true);
                cancelationRegistration.Unregister();
                cancelationSource.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationUnregisterCallbackIsNotInvoked1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                CancelationRegistration cancelationRegistration = cancelationToken.Register(0, i => invoked = true);
                cancelationRegistration.Unregister();
                cancelationSource.Cancel();
                Assert.IsFalse(invoked);
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationTokenRegisterCaptureVariableMatches()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                string expected = "Captured";
                cancelationToken.Register(expected, cv => Assert.AreEqual(expected, cv));
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void RegisteredCallbacksAreInvokedAfterSourceIsDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                bool invoked = false;
                // This should never be done in practice!
                cancelationToken.Register(() => cancelationSource.Dispose());
                cancelationToken.Register(() => invoked = true);
                cancelationSource.Cancel();
                Assert.IsTrue(invoked);
            }

            [Test]
            public void RegisteredCallbackIsNotRegisteredDuringInvocation()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = new CancelationRegistration();
                cancelationToken.Register(() => Assert.IsFalse(cancelationRegistration.IsRegistered));
                cancelationRegistration = cancelationToken.Register(() => { });
                cancelationSource.Cancel();
                    cancelationSource.Dispose();
            }

            [Test]
            public void RegisteredCallbackExceptionPropagatesToCancelCaller()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationToken.Register(() =>
                {
                    throw new Exception();
                });
                Assert.Throws<Proto.Promises.AggregateException>(cancelationSource.Cancel);
                cancelationSource.Dispose();
            }

            [Test]
            public void RegisteredCallbacksAreInvokedEvenWhenAnExceptionOccurs()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                int callbackCount = 0;
                cancelationToken.Register(() => ++callbackCount);
                cancelationToken.Register(() =>
                {
                    ++callbackCount;
                    throw new Exception();
                });
                cancelationToken.Register(() => ++callbackCount);
                try
                {
                    cancelationSource.Cancel();
                }
                catch (Exception) { }
                cancelationSource.Dispose();
                Assert.AreEqual(3, callbackCount);
            }

            [Test]
            public void RetainedCancelationTokenMayBeRegisteredToAfterCancelationSourceIsCanceledAndDisposed()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationToken.Retain();
                cancelationSource.Cancel();
                cancelationSource.Dispose();
                cancelationToken.Register(() => { });
                cancelationToken.Register(1, cv => { });
                CancelationRegistration cancelationRegistration;
                Assert.IsTrue(cancelationToken.TryRegister(() => { }, out cancelationRegistration));
                Assert.IsTrue(cancelationToken.TryRegister(1, cv => { }, out cancelationRegistration));
                cancelationToken.Release();
            }

            [Test]
            public void RetainedCancelationTokenMayNotBeRegisteredToAfterCancelationSourceIsDisposedWithoutCancel()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                cancelationToken.Retain();
                cancelationSource.Dispose();
                CancelationRegistration cancelationRegistration;
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => cancelationToken.Register(() => { }));
                Assert.Throws<Proto.Promises.InvalidOperationException>(() => cancelationToken.Register(1, cv => { }));
                Assert.IsFalse(cancelationToken.TryRegister(() => { }, out cancelationRegistration));
                Assert.IsFalse(cancelationToken.TryRegister(1, cv => { }, out cancelationRegistration));
                cancelationToken.Release();
            }

            [Test]
            public void CancelationRegistrationCancelationIsRequestedDuringInvocation0()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = default(CancelationRegistration);
                cancelationRegistration = cancelationToken.Register(() =>
                {
                    bool isRegistered, isCancelationRequested;
                    cancelationRegistration.GetIsRegisteredAndIsCancelationRequested(out isRegistered, out isCancelationRequested);
                    Assert.IsFalse(isRegistered);
                    Assert.IsTrue(isCancelationRequested);
                });
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationCancelationIsRequestedDuringInvocation1()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = default(CancelationRegistration);
                cancelationRegistration = cancelationToken.Register(1, cv =>
                {
                    bool isRegistered, isCancelationRequested;
                    cancelationRegistration.GetIsRegisteredAndIsCancelationRequested(out isRegistered, out isCancelationRequested);
                    Assert.IsFalse(isRegistered);
                    Assert.IsTrue(isCancelationRequested);
                });
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationCancelationIsRequestedDuringInvocation2()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = default(CancelationRegistration);
                cancelationRegistration = cancelationToken.Register(() =>
                {
                    bool isCancelationRequested;
                    Assert.IsFalse(cancelationRegistration.TryUnregister(out isCancelationRequested));
                    Assert.IsTrue(isCancelationRequested);
                });
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }

            [Test]
            public void CancelationRegistrationCancelationIsRequestedDuringInvocation3()
            {
                CancelationSource cancelationSource = CancelationSource.New();
                CancelationToken cancelationToken = cancelationSource.Token;
                CancelationRegistration cancelationRegistration = default(CancelationRegistration);
                cancelationRegistration = cancelationToken.Register(1, cv =>
                {
                    bool isCancelationRequested;
                    Assert.IsFalse(cancelationRegistration.TryUnregister(out isCancelationRequested));
                    Assert.IsTrue(isCancelationRequested);
                });
                cancelationSource.Cancel();
                cancelationSource.Dispose();
            }
        }
    }
}