
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kap_TestLib.Attributes;
using Kap_TestLib.Exceptions;
using System.Diagnostics;

namespace TestRunner
{
  
    class Program
    {
        static async Task Main(string[] args)
        {
            string testAssemblyPath = "Kap_Tests.dll";

            var runner = new TestRunner();


            var sw = Stopwatch.StartNew();

            await runner.RunTestsAsync(testAssemblyPath, maxDegree: 1);

            sw.Stop();
            Console.WriteLine($"\nSequential: {sw.ElapsedMilliseconds} ms");


            sw.Restart();

            await runner.RunTestsAsync(testAssemblyPath, maxDegree: 4);

            sw.Stop();
            Console.WriteLine($"Parallel: {sw.ElapsedMilliseconds} ms");
        }
    }

    public class TestRunner
    {
        private int _total = 0;
        private int _passed = 0;
        private int _failed = 0;
        private int _errored = 0;
        private SemaphoreSlim _semaphore;
        private readonly object _lock = new object();

        public async Task RunTestsAsync(string assemblyPath, int maxDegree = 4)
        {
            _semaphore = new SemaphoreSlim(maxDegree);
            var assembly = Assembly.LoadFrom(assemblyPath);
            var testClasses = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null);

            foreach (var testClass in testClasses)
            {
                await RunClassTestsAsync(testClass);
            }

            PrintSummary();
        }

        private async Task RunClassTestsAsync(Type testClass)
        {
            Console.WriteLine($"\n=== {testClass.Name} ===");

            var beforeAll = GetMethodByAttribute(testClass, typeof(BeforeAll), isStatic: true);
            var afterAll = GetMethodByAttribute(testClass, typeof(AfterAll), isStatic: true);

            var setUp = GetMethodByAttribute(testClass, typeof(TestSetUpAttribute), isStatic: false);
            var tearDown = GetMethodByAttribute(testClass, typeof(TestTearDownAttribute), isStatic: false);

            var testMethods = GetTestMethods(testClass);

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
            var tasks = new List<Task>();

            foreach (var testMethodInfo in testMethods)
            {
                var dataRows = testMethodInfo.Method.GetCustomAttributes<TestDataAttribute>().ToList();

                if (dataRows.Any())
                {
                    int dataIndex = 0;
                    foreach (var dataRow in dataRows)
                    {
                        dataIndex++;
                        string dataSuffix = $" (Data{dataIndex}: {string.Join(",", dataRow.data)})";

                        tasks.Add(RunTestWithSemaphore(
                            testClass, testMethodInfo, setUp, tearDown,
                            dataRow.data, dataSuffix));
                    }
                }
                else
                {
                    tasks.Add(RunTestWithSemaphore(
                        testClass, testMethodInfo, setUp, tearDown,
                        null, ""));
                }
            }

            await Task.WhenAll(tasks);

            if (afterAll != null)
            {
                try
                {
                    afterAll.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  AFTERALL ERROR: {ex.InnerException?.Message ?? ex.Message}");
                    _errored++;
                }
            }
        }


        private async Task RunTestWithSemaphore(
    Type testClass,
    (MethodInfo Method, bool IsAsync) testInfo,
    MethodInfo setUp,
    MethodInfo tearDown,
    object[] parameters,
    string dataSuffix)
        {
            await _semaphore.WaitAsync();

            try
            {
                await RunSingleTest(testClass, testInfo, setUp, tearDown, parameters, dataSuffix);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private async Task RunSingleTest(Type testClass, (MethodInfo Method, bool IsAsync) testInfo,
                                          MethodInfo setUp, MethodInfo tearDown,
                                          object[] parameters, string dataSuffix)
        {
            lock (_lock)
            {
                _total++;
            }
            var timeoutAttr = testInfo.Method.GetCustomAttribute<TimeoutAttribute>();
            var instance = Activator.CreateInstance(testClass);
            string methodName = testInfo.Method.Name;
            var testAttr = testInfo.Method.GetCustomAttribute<TestMethodAttribute>();


            if (testAttr != null && !string.IsNullOrEmpty(testAttr.NameOfTesting))
            {
                methodName = testAttr.NameOfTesting+"\\\\"+testInfo.Method.Name; 
            }
            string testName = methodName + dataSuffix;
            try
            {
                if (setUp != null)
                {
                    setUp.Invoke(instance, null);
                }
                Task testTask;

                if (testInfo.IsAsync)
                {
                    testTask = (Task)testInfo.Method.Invoke(instance, parameters);
                }
                else
                {
                    testTask = Task.Run(() => testInfo.Method.Invoke(instance, parameters));
                }

                if (timeoutAttr != null)
                {
                    var completed = await Task.WhenAny(testTask, Task.Delay(timeoutAttr.Milliseconds));

                    if (completed != testTask)
                    {
                        throw new Exception("Test timeout exceeded");
                    }
                }
                else
                {
                    await testTask;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  PASSED: {testName}");
                lock(_lock)
                { 
                    _passed++;
                }
                
            }
            catch (TargetInvocationException ex) when (ex.InnerException is AssertException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  FAILED: {testName} - {ex.InnerException.Message}");
                lock (_lock)
                {
                    _failed++;
                }
                
            }
            catch (TargetInvocationException ex)
            {
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ERROR: {testName} - {ex.InnerException?.Message ?? ex.Message}");
                lock (_lock)
                {
                    _errored++;
                }
               
            }
            catch (Exception ex)
            {
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ERROR: {testName} - {ex.Message}");
                lock (_lock)
                {
                    _errored++;
                }
            }
            finally
            {
                Console.ResetColor();
                if (tearDown != null)
                {
                    try
                    {
                        tearDown.Invoke(instance, null);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  TEARDOWN ERROR: {testName} - {ex.Message}");
                        lock (_lock)
                        {
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