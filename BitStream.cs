using IndexOutOfRangeException = System.IndexOutOfRangeException;
using Array = System.Array;
using System.Runtime.CompilerServices;

namespace SimpleStream {
    class BitStream {
        private int iterator, capacity, resizeAmount;
        private byte[] memory;
    
        /// <summary>
        /// Gets the underlying array of memory for this BitStream.
        /// </summary>
        public byte[] Memory { get { return memory; } }
        /// <summary>
        /// Gets the length of the buffer in bytes.
        /// </summary>
        public int Capacity { get { return capacity; } }
        /// <summary>
        /// Gets the current iterator(read/write position) for this BufferStream.
        /// </summary>
        public int Iterator { get { return iterator; } }
        /// <summary>
        /// The amount of bytes to add to the end of the stream when we've reached the end of the stream.
        /// </summary>
        public int ResizeAmount { get { return ResizeAmount; } }
        /// <summary>
        /// Whether the stream can grow when we've reached capacity.
        /// </summary>
        public bool CanGrow { get { return resizeAmount > 0; } }
    
        /// <summary>
        /// Constructs a bitstream with the specified number of bits allocated.
        /// </summary>
        /// <param name="capacity">New length of this stream.</param>
        /// <param name="resizeAmount">Amount of bytes to add to the end of the stream when the stream is resized.</param>
        public BitStream(int capacity, int resizeAmount) {
            memory = new byte[Align(iterator, TwiddleBIT.BITSOF_BYTE) / TwiddleBIT.BITSOF_BYTE];
            this.capacity = capacity;
            this.resizeAmount = resizeAmount;
        }
    
        /// <summary>Gets the iterator aligned to the specified alignment.</summary>
        /// <param name="iterator">Iterator(position/index) to align.</param>
        /// <param name="alignment">Base value to align to.</param>
        /// <returns>Returns the aligned iterator(position/index).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Align(int iterator, int alignment) =>
            ((iterator + (alignment - 1)) & ~(alignment - 1));
    
        /// <summary>
        /// Sets all bits in the stream to zero.
        /// </summary>
        public void Clear() {
            for(int i = 0; i < capacity; i++)
                memory[i] = 0x0;
        }
    
        /// <summary>
        /// Resizes the array by adding the number of bytes according to the ResizeAmount, unless the ResizeAmount is smaller-than-or-equal-to 0.
        /// </summary>
        public void Grow() {
            if(resizeAmount <= 0)
                return;
            capacity += resizeAmount;
            System.Array.Resize(ref memory, Align(iterator + resizeAmount, TwiddleBIT.BITSOF_BYTE) / TwiddleBIT.BITSOF_BYTE);
        }
    
        /// <summary>
        /// Shrinks the stream to the number of bytes required for the current iterator position.
        /// </summary>
        public void Shrink() =>
            Array.Resize(ref memory, Align(iterator - resizeAmount, TwiddleBIT.BITSOF_BYTE) / TwiddleBIT.BITSOF_BYTE);
    
        /// <summary>
        /// Copies the bits as a UINT to the bitstream.
        /// </summary>
        /// <param name="bits">Bits in the form of a UINT to write.</param>
        /// <param name="count">Number of bits to actually write.</param>
        /// <param name="startIndex">Starting bit-index in the bits to begin copying from.</param>
        public void Write(uint bits, int count, int startIndex) {
            if(iterator + bits >= capacity)
                if(CanGrow) {
                    Grow();
                } else {
                    throw new IndexOutOfRangeException();
                }
    
            TwiddleBIT.FillBits(memory, bits, (count >> startIndex), iterator / TwiddleBIT.BITSOF_BYTE, iterator++ % TwiddleBIT.BITSOF_BYTE);
        }
    
        /// <summary>
        /// Copies the bit as a BOOL to the bitstream.
        /// </summary>
        /// <param name="bit">The bit in the form of a BOOL to write.</param>
        public void Write(bool bit) {
            if(iterator + ((bit)? 0x1u : 0x0u) >= capacity)
                if(CanGrow) {
                    Grow();
                } else {
                    throw new IndexOutOfRangeException();
                }
    
            TwiddleBIT.FillBits(memory, ((bit) ? 0x1u : 0x0u), 1, iterator / TwiddleBIT.BITSOF_BYTE, iterator++ % TwiddleBIT.BITSOF_BYTE);
        }
    
        /// <summary>
        /// Reads the specified number of bits from the stream.
        /// </summary>
        /// <param name="count">Number of bits to read.</param>
        /// <returns>Returns the specified number of bits from the stream at the current iterator bit-index.</returns>
        public uint Read(int count) =>
            TwiddleBIT.GetBits(memory, count, iterator / TwiddleBIT.BITSOF_BYTE, iterator++ % TwiddleBIT.BITSOF_BYTE);
    
        /// <summary>
        /// Reads a single bit from the stream.
        /// </summary>
        /// <returns>Returns a single bit from the stream at the current iterator bit-index.</returns>
        public bool Read() =>
            (memory[iterator / TwiddleBIT.BITSOF_BYTE] & (0x1 << (iterator++ % TwiddleBIT.BITSOF_BYTE))) > 0x0;
    }
    
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
