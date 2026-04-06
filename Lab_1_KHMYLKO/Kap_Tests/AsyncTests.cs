using Kap_TestLib;
using Kap_TestLib.Attributes;
using Kap_ProjectForTesting;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class AsyncTests
    {
        private CodeConverterFacade _facade;

        [TestSetUp]
        public void Setup() => _facade = new CodeConverterFacade();

        [AsyncTestMethod]
        [Timeout(10)]
        public async Task ConvertBaseAsync_ReturnsCorrect(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(200);
            }
            ct.ThrowIfCancellationRequested();
            string result = await _facade.ConvertBaseAsync("FF", 16, 10);
            Assert.IsEqual("255", result);
        }

        [AsyncTestMethod]
        public async Task BinaryToGrayAsync_ReturnsCorrect()
        {
            string result = await _facade.BinaryToGrayAsync("1011");
            Assert.IsEqual("1110", result);
        }

        [AsyncTestMethod]
        public async Task EncodeHamming74Async_ReturnsCorrect()
        {
            string result = await _facade.EncodeHamming74Async("1011");
            Assert.IsEqual("1011010", result);
        }

        [AsyncTestMethod]
        public async Task ThrowsExceptionAsync_Test()
        {
            await Assert.ThrowsExceptionAsync<System.Exception>(async () =>
            {
                await Task.Delay(1);
                throw new System.Exception("Test");
            });
        }
    }
}