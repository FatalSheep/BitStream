using System.Runtime.CompilerServices;
using Array = System.Array;

namespace BasicStream {
    class BitStream {
        private byte[] memory;
        public byte[] Memory { get { return memory; } }
        public int Capacity { get { return memory.Length; } }
        public uint Length { get; private set; }
        public uint Iterator { get; private set; }
        public uint BlockSize { get; private set; }

        public BitStream(uint length, uint blockSize = 0x0) {
            memory = new byte[Align(length, (uint)BitTwiddler.BITSOF_BYTE) / (uint)BitTwiddler.BITSOF_BYTE];
            BlockSize = (blockSize >= 0x0) ? blockSize : 0x0;
            Length = length;
            Iterator = 0x0;
        }

        public BitStream(byte[] memory, uint length, uint blockSize = 0x0) {
            this.memory = memory;
            Length = length;
            BlockSize = blockSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Align(uint iterator, uint alignment) =>
            ((iterator + (alignment - 0x1u)) & ~(alignment - 0x1u));

        #region Memory Configuration
        private void Grow() =>
            Resize(Align(Length + BlockSize, (uint)BitTwiddler.BITSOF_BYTE) / (uint)BitTwiddler.BITSOF_BYTE);

        public void Clear() =>
            Array.Clear(memory, 0x0, Memory.Length);

        public BitStream Clone() {
            BitStream stream = new BitStream(Length, BlockSize);
            Array.Copy(memory, stream.Memory, Capacity);
            stream.Iterator = Iterator;
            return stream;
        }

        public void Resize(uint length) =>
            Array.Resize(ref memory, (int)(Align(Length = length, (uint)BitTwiddler.BITSOF_BYTE) / (uint)BitTwiddler.BITSOF_BYTE));
        #endregion

        #region Read/Write Bit
        public void Write(bool bit) =>
            BitTwiddler.FillBits(memory, ((bit) ? 0x1u : 0x0u), 0x1, (int)(Iterator / BitTwiddler.BITSOF_BYTE), (int)(Iterator++ % BitTwiddler.BITSOF_BYTE));

        public void Read(out bool val) =>
            val = (memory[Iterator / BitTwiddler.BITSOF_BYTE] & (0x1 << (int)(Iterator++ % BitTwiddler.BITSOF_BYTE))) > 0x0;
        #endregion

        #region Read/Write Byte
        public void Write(byte val) =>
            BitTwiddler.FillBits(memory, val, BitTwiddler.BITSOF_BYTE, (int)(Iterator / BitTwiddler.BITSOF_BYTE), (int)(Iterator++ % BitTwiddler.BITSOF_BYTE));

        public void Read(out byte val) =>
            val = (byte)BitTwiddler.GetBits(memory, BitTwiddler.BITSOF_BYTE, (int)(Iterator / BitTwiddler.BITSOF_BYTE), (int)(Iterator++ % BitTwiddler.BITSOF_BYTE));
        #endregion

        #region Read/Write Bytes
        public void Write(byte[] bytes, uint bits = 0x0) {
            int bitsOfByte = BitTwiddler.BITSOF_BYTE;

            if(bits == 0x0)
                bits = (uint)(bytes.Length * bitsOfByte);

            foreach(byte val in bytes) {
                BitTwiddler.FillBits(memory, (uint)val, (bits >= bitsOfByte) ? bitsOfByte : (int)bits, (int)(Iterator / bitsOfByte), (int)(Iterator % bitsOfByte));
                Iterator += (uint)bitsOfByte;
                bits -= (uint)bitsOfByte;
            }
        }

        public byte[] Read(uint bits) {
            int bitsOfByte = BitTwiddler.BITSOF_BYTE;
            int length = (int)(bits / bitsOfByte);
            byte[] bytes = new byte[length];

            for(uint i = 0x0; i < length; i++) {
                bytes[i] = (byte)BitTwiddler.GetBits(memory, bitsOfByte, (int)(Iterator / bitsOfByte), (int)(Iterator % bitsOfByte));
                Iterator += (uint)bitsOfByte;
            }

            int leftOver, indLeftOver;
            leftOver = (int)(Iterator % bitsOfByte);
            indLeftOver = bitsOfByte - leftOver;

            if (leftOver > 0x0)
                bytes[length - 0x1] = (byte)BitTwiddler.ZeroBits(bytes[length - 0x1], leftOver, indLeftOver);
            return bytes;
        }
        #endregion
    }
}
