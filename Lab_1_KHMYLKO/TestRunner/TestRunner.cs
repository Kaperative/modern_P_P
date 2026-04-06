
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kap_TestLib.Attributes;
using Kap_TestLib.Exceptions;
using System.Diagnostics;

using CustomThreadPoolModule;

namespace TestRunner
{
  
    class Program
    {
        static async Task Main(string[] args)
        {
            string testAssemblyPath = "Kap_Tests.dll";

            var runner = new TestRunner();

            await runner.RunTestsAsync("Kap_Tests.dll", maxDegree: 8);

        }
    }

    public class TestRunner
    {
        private int _total = 0;
        private int _passed = 0;
        private int _failed = 0;
        private int _errored = 0;
        private CustomThreadPool _threadPool;
        private readonly object _lock = new object();

        private static ThreadLocal<Random> _rand = new(() => new Random());
        private void WaitForCompletion()
        {
            while (true)
            {
                lock (_lock)
                {
                    if (_total == (_passed + _failed + _errored))
                        return;
                }

                Thread.Sleep(100);
            }
        }
        public async Task RunTestsAsync(string assemblyPath, int maxDegree = 4)
        {
            _threadPool = new CustomThreadPool(minThreads: 2, maxThreads: maxDegree);

            var assembly = Assembly.LoadFrom(assemblyPath);
            var testClasses = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null);

            foreach (var testClass in testClasses)
            {
                await RunClassTestsAsync(testClass);
            }

            WaitForCompletion();

            PrintSummary();
            _threadPool.Stop();
        }

        private async Task RunClassTestsAsync(Type testClass)
        {
            Console.WriteLine($"\n=== {testClass.Name} ===");

            var beforeAll = GetMethodByAttribute(testClass, typeof(BeforeAll), isStatic: true);
            var afterAll = GetMethodByAttribute(testClass, typeof(AfterAll), isStatic: true);

            var setUp = GetMethodByAttribute(testClass, typeof(TestSetUpAttribute), isStatic: false);
            var tearDown = GetMethodByAttribute(testClass, typeof(TestTearDownAttribute), isStatic: false);

            var testMethods = GetTestMethods(testClass);

            int classTotal = 0;
            int classCompleted = 0;
            object classLock = new object();

            if (beforeAll != null)
            {
                try
                {
                    beforeAll.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  BEFOREALL ERROR: {ex.InnerException?.Message ?? ex.Message}");
                    _errored++;
                    return;
                }
            }


            foreach (var testMethodInfo in testMethods)
            {
                var dataRows = testMethodInfo.Method.GetCustomAttributes<TestDataAttribute>().ToList();

                if (dataRows.Any())
                {
                    int dataIndex = 0;

                    foreach (var dataRow in dataRows)
                    {
                        dataIndex++;
                        string dataSuffix = $" (Data{dataIndex})";
                        classTotal++;
                        _threadPool.Enqueue(() =>
                        {
                            try
                            {
                                RunSingleTest(testClass, testMethodInfo, setUp, tearDown,
                                              dataRow.data, dataSuffix)
                                              .GetAwaiter().GetResult();
                            }
                            finally
                            {
                                lock (classLock)
                                {
                                    classCompleted++;
                                }
                            }

                        });
                    }
                }
                else
                {
                    classTotal++;

                    _threadPool.Enqueue(() =>
                    {
                        try
                        {
                            RunSingleTest(testClass, testMethodInfo, setUp, tearDown,
                                          null, "")
                                          .GetAwaiter().GetResult();
                        }
                        finally
                        {
                            lock (classLock)
                            {
                                classCompleted++; 
                            }
                        }
                    });
                }
            }

                if (afterAll != null)
            {
                try
                {
                    while (true)
                    {
                        lock (classLock)
                        {
                            if (classCompleted == classTotal)
                                break;
                        }
                        Thread.Sleep(50);
                    }
                    afterAll.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  AFTERALL ERROR: {ex.InnerException?.Message ?? ex.Message}");
                    _errored++;
                }
            }
        }


        public async Task SimulateLoad()
        {
            await Task.Delay(2000);

            
            for (int i = 0; i < 30; i++)
            {
                _threadPool.Enqueue(() =>
                {
                    Thread.Sleep(_rand.Value.Next(100, 500));
                });
            }

            await Task.Delay(1000);

            for (int i = 0; i < 5; i++)
            {
                _threadPool.Enqueue(() =>
                {
                    Thread.Sleep(200);
                });
            }
        }

        private async Task RunSingleTest(Type testClass, (MethodInfo Method, bool IsAsync) testInfo,
                                   MethodInfo setUp, MethodInfo tearDown,
                                   object[] parameters, string dataSuffix)
        {
            lock (_lock) { _total++; }

            var timeoutAttr = testInfo.Method.GetCustomAttribute<TimeoutAttribute>();
            var instance = Activator.CreateInstance(testClass);
            string methodName = testInfo.Method.Name;

            string testName = methodName + dataSuffix;

            try
            {
                if (setUp != null) setUp.Invoke(instance, null);

                Task testTask;

                if (testInfo.IsAsync)
                {
                   
                    var cts = timeoutAttr != null
                        ? new CancellationTokenSource(timeoutAttr.Milliseconds)
                        : new CancellationTokenSource();

               
                    var parametersWithToken = parameters ?? Array.Empty<object>();
                    if (testInfo.Method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)))
                    {
                        parametersWithToken = parametersWithToken.Append(cts.Token).ToArray();
                    }

                    testTask = (Task)testInfo.Method.Invoke(instance, parametersWithToken);
                    var completed = await Task.WhenAny(testTask, Task.Delay(Timeout.Infinite, cts.Token));

                    if (completed != testTask)
                        throw new Exception("Test timeout exceeded");

                    await testTask;
                }
                else
                {
            
                    testInfo.Method.Invoke(instance, parameters);
                   
                }

                lock (_lock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  PASSED: {testName}");
                    Console.ResetColor();
                    _passed++;
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException is AssertException)
            {
                lock (_lock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  FAILED: {testName} - {ex.InnerException.Message}");
                    Console.ResetColor();
                    _failed++;
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ERROR: {testName} - {ex.Message}");
                    Console.ResetColor();
                    _errored++;
                }
            }
            finally
            {
                if (tearDown != null)
                {
                    try { tearDown.Invoke(instance, null); }
                    catch (Exception ex)
                    {
                        lock (_lock)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"  TEARDOWN ERROR: {testName} - {ex.Message}");
                            Console.ResetColor();
                            _errored++;
                        }
                    }
                }
            }
        }

        private MethodInfo GetMethodByAttribute(Type type, Type attributeType, bool isStatic)
        {
            return type.GetMethods(BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance))
                .FirstOrDefault(m => m.GetCustomAttribute(attributeType) != null);
        }

        private List<(MethodInfo Method, bool IsAsync)> GetTestMethods(Type type)
        {
            var methods = new List<(MethodInfo, bool)>();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.GetCustomAttribute<TestMethodAttribute>() != null)
                    methods.Add((method, false));
                if (method.GetCustomAttribute<AsyncTestMethodAttribute>() != null)
                    methods.Add((method, true));
            }
            return methods;
        }

        private void PrintSummary()
        {
            lock (_lock)
            {
                Console.WriteLine("\n=====================");
                Console.WriteLine($"Total: {_total}, Passed: {_passed}, Failed: {_failed}, Errors: {_errored}");
                Console.ResetColor();
            }
        }
    }
}