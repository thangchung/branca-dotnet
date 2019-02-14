using Xunit;

namespace Branca.Tests
{
    public class BrancaTest
    {
        [Fact]
        public void CanEncode()
        {
            var payload = "iwanttoencode";
            var branca = new Branca("supersecretkeyyoushouldnotcommit");
            var encode = branca.Encode(payload);
            var decode = branca.Decode(encode);
            Assert.True(payload == decode);
        }
    }
}
