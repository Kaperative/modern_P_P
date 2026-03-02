using System.Threading.Tasks;

namespace Kap_ProjectForTesting
{
    public class CodeConverterFacade
    {
        public string ConvertBase(string number, int fromBase, int toBase)
            => NumberBaseConverter.ConvertNumber(number, fromBase, toBase);

        public string BinaryToGray(string binary)
            => GrayCodeConverter.BinaryToGray(binary);

        public string EncodeHamming74(string message)
            => new Hamming74Encoder().Encode(message);

        public async Task<string> ConvertBaseAsync(string number, int fromBase, int toBase)
        {
            await Task.Delay(10);
            return ConvertBase(number, fromBase, toBase);
        }

        public async Task<string> BinaryToGrayAsync(string binary)
        {
            await Task.Delay(5);
            return BinaryToGray(binary);
        }

        public async Task<string> EncodeHamming74Async(string message)
        {
            await Task.Delay(15);
            return EncodeHamming74(message);
        }
    }
}