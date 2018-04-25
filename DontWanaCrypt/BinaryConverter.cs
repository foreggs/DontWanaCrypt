using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontWanaCrypt
{
    public static class BinaryConverter
    {
        public static BitArray ToBinary(this int numeral) => new BitArray(new[] { numeral });
        public static BitArray ToBinary(this byte numeral) => new BitArray(new[] { numeral });

        public static int ToNumeral(this BitArray binary)
        {
            if (binary == null)
                throw new ArgumentNullException("binary");
            if (binary.Length > 32)
                throw new ArgumentException("must be at most 32 bits long");

            var result = new int[1];
            binary.CopyTo(result, 0);
            return result[0];
        }
    }
}
