namespace Algorithms.Utility
{
    public static class BitFunctions
    {
        public static bool IsBitSet(long value, int location)
        {
            long mask = GetBitMask(location);
            return (value & mask) == mask;
        }

        public static long SetBit(long value, int location)
        {
            return value | (GetBitMask(location));
        }

        public static long ClearBit(long value, int location)
        {
            return value & (~(GetBitMask(location)));
        }

        public static long SetOrClearBit(long value, int location, bool isSet)
        {
            if (isSet)
                return SetBit(value, location);
            return ClearBit(value, location);
        }

        public static int NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (int)((((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
        }


        public static long GetBitMask(int index)
        {
            return (long)1 << index;
        }
    }
}
