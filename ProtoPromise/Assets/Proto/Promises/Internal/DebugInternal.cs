﻿#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#endif
#if !PROTO_PROMISE_CANCEL_DISABLE
#define PROMISE_CANCEL
#endif
#if !PROTO_PROMISE_PROGRESS_DISABLE
#define PROMISE_PROGRESS
#endif

#pragma warning disable RECS0096 // Type parameter is never used
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter

using System;
using Proto.Utils;

namespace Proto.Promises
{
    partial class Promise
    {
        protected static void _SetStackTraceFromCreated(Internal.IStacktraceable stacktraceable, Internal.UnhandledExceptionInternal unhandledException)
        {
            SetStacktraceFromCreated(stacktraceable, unhandledException);
        }

        // Calls to these get compiled away in RELEASE mode
        static partial void ValidateOperation(Promise promise, int skipFrames);
        static partial void ValidateProgress(float progress, int skipFrames);
        static partial void ValidateArgument(object arg, string argName, int skipFrames);
        partial void ValidateReturn(Promise other);
        static partial void ValidateReturn(Delegate other);
        static partial void ValidatePotentialOperation(Internal.IValueContainerOrPrevious valueContainer, int skipFrames);
        static partial void ValidateElementNotNull(Promise promise, string argName, string message, int skipFrames);

        static partial void SetCreatedStacktrace(Internal.IStacktraceable stacktraceable, int skipFrames);
        static partial void SetStacktraceFromCreated(Internal.IStacktraceable stacktraceable, Internal.UnhandledExceptionInternal unhandledException);
        static partial void SetRejectStacktrace(Internal.UnhandledExceptionInternal unhandledException, int skipFrames);
        static partial void SetNotDisposed(ref Internal.IValueContainerOrPrevious valueContainer);
#if PROMISE_DEBUG
        private uint _userRetainCounter;
        private string _createdStackTrace;
        string Internal.IStacktraceable.Stacktrace { get { return _createdStackTrace; } set { _createdStackTrace = value; } }

        private static int idCounter;
        protected readonly int _id;

        private static void SetDisposed(ref Internal.IValueContainerOrPrevious valueContainer)
        {
            valueContainer = Internal.DisposedChecker.instance;
        }

        static partial void SetNotDisposed(ref Internal.IValueContainerOrPrevious valueContainer)
        {
            valueContainer = null;
        }

        partial class Internal
        {
            // This allows us to re-use the reference field without having to add another bool field.
            public sealed class DisposedChecker : IValueContainerOrPrevious
            {
                public static readonly DisposedChecker instance = new DisposedChecker();

                private DisposedChecker() { }

                bool IValueContainerOrPrevious.ContainsType<U>() { throw new System.InvalidOperationException(); }
                bool IValueContainerOrPrevious.TryGetValueAs<U>(out U value) { throw new System.InvalidOperationException(); }
                void IValueContainerOrPrevious.Release() { throw new System.InvalidOperationException(); }
                void IValueContainerOrPrevious.Retain() { throw new System.InvalidOperationException(); }
            }
        }

        static partial void SetCreatedStacktrace(Internal.IStacktraceable stacktraceable, int skipFrames)
        {
            if (Config.DebugStacktraceGenerator == GeneratedStacktrace.All)
            {
                stacktraceable.Stacktrace = GetStackTrace(skipFrames + 1);
            }
        }

        static partial void SetStacktraceFromCreated(Internal.IStacktraceable stacktraceable, Internal.UnhandledExceptionInternal unhandledException)
        {
            unhandledException.SetStackTrace(FormatStackTrace(stacktraceable.Stacktrace));
        }

        static partial void SetRejectStacktrace(Internal.UnhandledExceptionInternal unhandledException, int skipFrames)
        {
            if (Config.DebugStacktraceGenerator != GeneratedStacktrace.None)
            {
                unhandledException.SetStackTrace(GetFormattedStacktrace(skipFrames + 1));
            }
        }

        private static string GetStackTrace(int skipFrames)
        {
            return new System.Diagnostics.StackTrace(skipFrames + 1, true).ToString();
        }

        private static string GetFormattedStacktrace(int skipFrames)
        {
            return FormatStackTrace(GetStackTrace(skipFrames + 1));
        }

        private static System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(128);

        private static string FormatStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
            {
                return stackTrace;
            }

            stringBuilder.Length = 0;
            stringBuilder.Append(stackTrace);

            // Format stacktrace to match "throw exception" so that double-clicking log in Unity console will go to the proper line.
            return stringBuilder.Remove(0, 1)
                .Replace(":line ", ":")
                .Replace("\n ", " \n")
                .Replace("(", " (")
                .Replace(") in", ") [0x00000] in") // Not sure what "[0x00000]" is, but it's necessary for Unity's parsing.
                .Append(" ")
                .ToString();
        }

        partial void ValidateReturn(Promise other)
        {
            if (other == null)
            {
                // Returning a null from the callback is not allowed.
                throw new InvalidReturnException("A null promise was returned.");
            }

            // Validate returned promise as not disposed.
            if (IsDisposed(other._rejectedOrCanceledValueOrPrevious))
            {
                throw new InvalidReturnException("A disposed promise was returned.");
            }

            // A promise cannot wait on itself.

            // This allows us to check AllPromises and RacePromises iteratively.
            ValueLinkedStack<Internal.PromisePassThrough> passThroughs = new ValueLinkedStack<Internal.PromisePassThrough>();
            var prev = other;
        Repeat:
            for (; prev != null; prev = prev._rejectedOrCanceledValueOrPrevious as Promise)
            {
                if (prev == this)
                {
                    throw new InvalidReturnException("Circular Promise chain detected.", other._createdStackTrace);
                }
                prev.BorrowPassthroughs(ref passThroughs);
            }

            if (passThroughs.IsNotEmpty)
            {
                // passThroughs are removed from their targets before adding to passThroughs. Add them back here.
                var passThrough = passThroughs.Pop();
                prev = passThrough.Owner;
                passThrough.Target.ReAdd(passThrough);
                goto Repeat;
            }
        }

        static partial void ValidateReturn(Delegate other)
        {
            if (other == null)
            {
                // Returning a null from the callback is not allowed.
                throw new InvalidReturnException("A null delegate was returned.");
            }
        }

        protected static void ValidateProgressValue(float value, int skipFrames)
        {
            const string argName = "progress";
            if (value < 0f || value > 1f || float.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(argName, "Must be between 0 and 1.");
            }
        }

        private static bool IsDisposed(Internal.IValueContainerOrPrevious valueContainer)
        {
            return ReferenceEquals(valueContainer, Internal.DisposedChecker.instance);
        }

        static protected void ValidateNotDisposed(Internal.IValueContainerOrPrevious valueContainer, int skipFrames)
        {
            if (IsDisposed(valueContainer))
            {
                throw new PromiseDisposedException("Always nullify your references when you are finished with them!" +
                    " Call Retain() if you want to perform operations after the object has finished. Remember to call Release() when you are finished with it!"
                    , GetFormattedStacktrace(skipFrames + 1));
            }
        }

        static partial void ValidatePotentialOperation(Internal.IValueContainerOrPrevious valueContainer, int skipFrames)
        {
            ValidateNotDisposed(valueContainer, skipFrames + 1);
        }

        static partial void ValidateOperation(Promise promise, int skipFrames)
        {
            ValidateNotDisposed(promise._rejectedOrCanceledValueOrPrevious, skipFrames + 1);
        }

        static partial void ValidateProgress(float progress, int skipFrames)
        {
            ValidateProgressValue(progress, skipFrames + 1);
        }

        static protected void ValidateArg(object del, string argName, int skipFrames)
        {
            if (del == null)
            {
                throw new ArgumentNullException(argName, null, GetFormattedStacktrace(skipFrames + 1));
            }
        }

        static partial void ValidateArgument(object arg, string argName, int skipFrames)
        {
            ValidateArg(arg, argName, skipFrames + 1);
        }

        static partial void ValidateElementNotNull(Promise promise, string argName, string message, int skipFrames)
        {
            if (promise == null)
            {
                throw new ElementNullException(argName, message, GetFormattedStacktrace(skipFrames + 1));
            }
        }

        public override string ToString()
        {
            return string.Format("Type: Promise, Id: {0}, State: {1}", _id, _state);
        }
#else
        private static string GetFormattedStacktrace(int skipFrames)
        {
            return null;
        }

        private static void SetDisposed(ref Internal.IValueContainerOrPrevious valueContainer)
        {
            // Allow GC to clean up the object if necessary.
            valueContainer = null;
        }

        public override string ToString()
        {
            return string.Format("Type: Promise, State: {0}", _state);
        }
#endif

        partial class Internal
        {
            public interface IStacktraceable
            {
#if PROMISE_DEBUG
                string Stacktrace { get; set; }
#endif
            }

            partial class FinallyDelegate : IStacktraceable
            {
#if PROMISE_DEBUG
                string IStacktraceable.Stacktrace { get; set; }
#endif
            }

            partial class PotentialCancelation : IStacktraceable
            {
#if PROMISE_DEBUG
                string IStacktraceable.Stacktrace { get; set; }
#endif
            }
        }
    }

    partial class Promise<T>
    {
        // Calls to these get compiled away in RELEASE mode
        static partial void ValidateOperation(Promise<T> promise, int skipFrames);
        static partial void ValidateArgument(object arg, string argName, int skipFrames);
        static partial void ValidateProgress(float progress, int skipFrames);
#if PROMISE_DEBUG
        static partial void ValidateProgress(float progress, int skipFrames)
        {
            ValidateProgressValue(progress, skipFrames + 1);
        }

        static partial void ValidateOperation(Promise<T> promise, int skipFrames)
        {
            ValidateNotDisposed(promise._rejectedOrCanceledValueOrPrevious, skipFrames + 1);
        }

        static partial void ValidateArgument(object arg, string argName, int skipFrames)
        {
            ValidateArg(arg, argName, skipFrames + 1);
        }

        public override string ToString()
        {
            return string.Format("Type: Promise<{0}>, Id: {1}, State: {2}", typeof(T), _id, _state);
        }
#else
        public override string ToString()
        {
            return string.Format("Type: Promise<{0}>, State: {1}", typeof(T), _state);
        }
#endif
    }
}