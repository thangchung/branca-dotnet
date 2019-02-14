using System;
using System.Linq;
using Branca.Internal;
using Xunit;

namespace Branca.Tests
{
    public class Base62Test
    {
        [Fact]
        public void CanEncodeAndDecode()
        {
            byte[] x = { 128, 128, 128, 128, 128, 128 };
            string s = x.ToBase62();
            byte[] x2 = s.FromBase62();
            Assert.True(x.SequenceEqual(x2));
        }
    }
}
