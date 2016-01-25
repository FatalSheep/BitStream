using IndexOutOfRangeException = System.IndexOutOfRangeException;
using System.Runtime.CompilerServices;
using SimpleStream.BitHacks;

namespace SimpleStream.Buffer {
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
        }

        /// <summary>Gets the iterator aligned to the specified alignment.</summary>
        /// <param name="iterator">Iterator(position/index) to align.</param>
        /// <param name="alignment">Base value to align to.</param>
        /// <returns>Returns the aligned iterator(position/index).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Align(int iterator, int alignment) =>
            ((iterator + (alignment - 1)) & ~(alignment - 1));

        /// <summary>
        /// Resizes the array by adding the number of bytes according to the ResizeAmount, unless the ResizeAmount is smaller-than-or-equal-to 0.
        /// </summary>
        public void Grow() {
            if(resizeAmount <= 0)
                return;
            capacity += resizeAmount;
            System.Array.Resize(ref memory, Align(iterator, TwiddleBIT.BITSOF_BYTE) / TwiddleBIT.BITSOF_BYTE);
        }

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
        /// Reads a single bit fro mthe stream.
        /// </summary>
        /// <returns>Returns a single bit from the stream at the current iterator bit-index.</returns>
        public bool Read() =>
            (memory[iterator / TwiddleBIT.BITSOF_BYTE] & (0x1 << (iterator % TwiddleBIT.BITSOF_BYTE))) > 0x0;
    }
}
