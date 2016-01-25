using System.Runtime.CompilerServices;

namespace SimpleStream.BitHacks {
    public static class TwiddleBIT {
        /// <summary>
        /// Gets the number of bits in an unsigned 32-bit integer.
        /// </summary>
        public static int BITSOF_UINT { get; private set; }
        /// <summary>
        /// Gets the number of bits in an unsigned 8-bit integer.
        /// </summary>
        public static int BITSOF_BYTE { get; private set; }

        /// <summary>
        /// Initiates the BITSOF_* static properties.
        /// </summary>
        static TwiddleBIT() {
            BITSOF_UINT = sizeof(uint) * 8;
            BITSOF_BYTE = sizeof(byte) * 8;
        }

        /// <summary>Gets the iterator aligned to the specified alignment.</summary>
        /// <param name="iterator">Iterator(position/index) to align.</param>
        /// <param name="alignment">Base value to align to.</param>
        /// <returns>Returns the aligned iterator(position/index).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Align(int iterator, int alignment) =>
            ((iterator + (alignment - 1)) & ~(alignment - 1));

        /// <summary>Gets the number of bits used for this value.</summary>
        /// <param name="val">Value to count bits used of.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitCount(uint val) {
            byte bits = 1;
            for(; (val = val >> 1) != 0; bits++)
                ;
            return bits;
        }

        /// <summary>Gets the max value that can be stored in the max number of bits used in the value passed.</summary>
        /// <param name="val">Value to get the max value from bits of.</param>
        /// <returns>Returns the max value that can be stored in the max number of bits used in the value passed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MaxValue(uint val) {
            val--;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            return val;
        }

        /// <summary>Gets the value with the specified bits zero-ed out.</summary>
        /// <param name="val">Value to zero bits from.</param>
        /// <param name="len">Number of bits to zero out.</param>
        /// <param name="ind">Index to start zeroing bits from.</param>
        /// <returns>Returns the value with the specified bits zero-ed out.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ZeroBits(uint val, int len, int ind) =>
            val & ~((uint.MaxValue >> (BITSOF_UINT - len)) << ind);

        /// <summary>Gets the value with the specified bits zero-ed out.</summary>
        /// <param name="val">Value to zero bits from.</param>
        /// <param name="len">Number of bits to zero out.</param>
        /// <param name="ind">Index to start zeroing bits from.</param>
        /// <returns>Returns the value with the specified bits zero-ed out.</returns>
        public static void ZeroBits(byte[] stream, int len, int byteInd, int bitInd) {
            int bytes = Align(bitInd + len, BITSOF_BYTE) / BITSOF_BYTE;
            uint value = GetBits(stream, bytes * BITSOF_BYTE, byteInd, 0);
            value = ZeroBits(value, len, bitInd);

            byteInd = byteInd + Align(bitInd, BITSOF_BYTE) % BITSOF_BYTE;
            for(int i = (bytes - 1); i > -1; i--) {
                stream[byteInd + i] = (byte)(value >> (i * BITSOF_BYTE));
            }
        }

        /// <summary>Gets the value with the specified bits set(1).</summary>
        /// <param name="val">Value to set bits to.</param>
        /// <param name="len">Number of bits to set.</param>
        /// <param name="ind">Index to start setting bits at.</param>
        /// <returns>Returns the value passed with the specified bits set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBits(uint val, int len, int ind) =>
            val | ((uint.MaxValue >> (BITSOF_UINT - len)) << ind);

        /// <summary>Gets the value with the specified bits set(1) to it.</summary>
        /// <param name="val">Value to set bits to.</param>
        /// <param name="len">Number of bits to set.</param>
        /// <param name="ind">Index to start setting bits at.</param>
        /// <returns>Returns the value passed with the specified bits set.</returns>
        public static void SetBits(byte[] stream, int len, int byteInd, int bitInd) {
            int bytes = Align(bitInd + len, BITSOF_BYTE) / BITSOF_BYTE;
            uint value = GetBits(stream, bytes * BITSOF_BYTE, byteInd, 0);
            value = SetBits(value, len, bitInd);

            byteInd = byteInd + Align(bitInd, BITSOF_BYTE) % BITSOF_BYTE;
            for(int i = (bytes - 1); i > -1; i--) {
                stream[byteInd + i] = (byte)(value >> (i * BITSOF_BYTE));
            }
        }

        /// <summary>Gets the value with the specified bits flipped/toggled.</summary>
        /// <param name="val">Value to flip/toggle bits in.</param>
        /// <param name="len">Number of bits to flip/toggle.</param>
        /// <param name="ind">Index to start flipping bits at.</param>
        /// <returns>Returns the value passed with the specified bits flipped/toggled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FlipBits(uint val, int len, int ind) =>
            val ^ ((uint.MaxValue >> (BITSOF_UINT - len)) << ind);

        /// <summary>Gets the value with the specified bits flipped/toggled.</summary>
        /// <param name="val">Value to flip/toggle bits in.</param>
        /// <param name="len">Number of bits to flip/toggle.</param>
        /// <param name="ind">Index to start flipping bits at.</param>
        /// <returns>Returns the value passed with the specified bits flipped/toggled.</returns>
        public static void FlipBits(byte[] stream, int len, int byteInd, int bitInd) {
            int bytes = Align(bitInd + len, BITSOF_BYTE) / BITSOF_BYTE;
            uint value = GetBits(stream, bytes * BITSOF_BYTE, byteInd, 0);
            value = FlipBits(value, len, bitInd);

            byteInd = byteInd + Align(bitInd, BITSOF_BYTE) % BITSOF_BYTE;
            for(int i = (bytes - 1); i > -1; i--) {
                stream[byteInd + i] = (byte)(value >> (i * BITSOF_BYTE));
            }
        }

        /// <summary>Gets the value with the specified bits filled.</summary>
        /// <param name="val">Value to fill bits in.</param>
        /// <param name="bits">Bits to fill into the passed value.</param>
        /// <param name="len">Number of bits to fill.</param>
        /// <param name="ind">Index to start filling bits in.</param>
        /// <returns>Returns the value passed with the specified bits filled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FillBits(uint val, uint bits, int len, int ind) {
            val = ZeroBits(val, len, ind);
            return val | (bits << ind);
        }

        /// <summary>Gets the value with the specified bits filled.</summary>
        /// <param name="val">Value to fill bits in.</param>
        /// <param name="bits">Bits to fill into the passed value.</param>
        /// <param name="len">Number of bits to fill.</param>
        /// <param name="ind">Index to start filling bits in.</param>
        /// <returns>Returns the value passed with the specified bits filled.</returns>
        public static void FillBits(byte[] stream, uint val, int len, int byteInd, int bitInd) {
            int bytes = Align(bitInd + len, BITSOF_BYTE) / BITSOF_BYTE;
            uint value = GetBits(stream, bytes * BITSOF_BYTE, byteInd, 0);
            value = FillBits(value, val, len, bitInd);

            byteInd = byteInd + Align(bitInd, BITSOF_BYTE) % BITSOF_BYTE;
            for(int i = (bytes - 1); i > -1; i--) {
                stream[byteInd + i] = (byte)(value >> (i * BITSOF_BYTE));
            }
        }

        /// <summary>Gets the specified bits from the value passed.</summary>
        /// <param name="val">Value to fill bits in.</param>
        /// <param name="len">Number of bits to get.</param>
        /// <param name="ind">Index to start getting bits from.</param>
        /// <returns>Returns the specified bits from the value passed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBits(uint val, int len, int ind) =>
            (val >> ind) & (uint.MaxValue >> (BITSOF_UINT - len));

        /// <summary>Gets the specified bits from the value passed.</summary>
        /// <param name="val">Value to fill bits in.</param>
        /// <param name="len">Number of bits to get.</param>
        /// <param name="ind">Index to start getting bits from.</param>
        /// <returns>Returns the specified bits from the value passed.</returns>
        public static uint GetBits(byte[] stream, int len, int byteInd, int bitInd) {
            uint val = 0;
            int bytes = Align(bitInd + len, BITSOF_BYTE) / BITSOF_BYTE;

            for(int i = (bytes - 1); i > -1; i--) {
                val |= (uint)stream[byteInd + i] << (i * BITSOF_BYTE);
            }

            val = val >> bitInd;
            val = val & (uint.MaxValue >> (BITSOF_UINT - len));
            return val;
        }
    }
}
