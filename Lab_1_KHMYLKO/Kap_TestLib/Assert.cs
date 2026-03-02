using Kap_TestLib.Exceptions;

namespace Kap_TestLib
{
        public static class Assert
        {
            public static void IsEqual<T>(T expected, T actual, string message = "")
            {
                if (!Equals(expected, actual))
                    throw new AssertException($"Expected: {expected}. Actual: {actual}. {message}");
            }

            public static void IsNotEqual<T>(T expected, T actual, string message = "")
            {
                if (Equals(expected, actual))
                    throw new AssertException($"Expected: {expected}. Actual: {actual}. {message}");
            }

            public static void IsTrue(bool condition, string message = "")
            {
                if (!condition)
                    throw new AssertException($"Expected: True. Actual: False. {message}");
            }

            public static void IsFalse(bool condition, string message = "")
            {
                if (condition)
                    throw new AssertException($"Expected: False. Actual: True. {message}");
            }

            public static void IsNull(object obj, string message = "")
            {
                if (obj != null)
                    throw new AssertException($"Expected: NULL. Actual: {obj}. {message}");
            }

            public static void IsNotNull(object obj, string message = "")
            {
                if (obj == null)
                    throw new AssertException($"Expected: {obj}. Actual: NULL. {message}");
            }

            public static void ThrowsException<TException>(Action action, string message = "") where TException : Exception
            {
                try
                {
                    action();
                }
                catch (TException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    throw new AssertException($"Expected exception {typeof(TException).Name}, but got {ex.GetType().Name}. {message}");
                }
                throw new AssertException($"Expected exception {typeof(TException).Name}, but no exception was thrown. {message}");
            }

            public static async Task ThrowsExceptionAsync<TException>(Func<Task> action, string message = "") where TException : Exception
            {
                try
                {
                    await action();
                }
                catch (TException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    throw new AssertException($"Expected exception {typeof(TException).Name}, but got {ex.GetType().Name}. {message}");
                }
                throw new AssertException($"Expected exception {typeof(TException).Name}, but no exception was thrown. {message}");
            }

            public static void DoesNotThrow(Action action, string message = "")
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    throw new AssertException($"Expected: No exception. Actual: {ex.GetType().Name}. {message}");
                }
            }

            public static void IsInstanceOfType(object obj, Type expectedType, string message = "")
            {
                if (obj == null || !expectedType.IsAssignableFrom(obj.GetType()))
                    throw new AssertException($"Expected object of type {expectedType.Name}, but got {obj?.GetType().Name ?? "null"}. {message}");
            }

            
    }
    }

