using System;
using System.Text;

namespace FnvBrute
{
    class FnvHasher
    {
        private const uint OffsetBasis = 2166136261;

        private const uint Prime = 16777619;

        // digits are 0x30 to 0x39
        // lowercase alphabet is 0x61 to 0x7a
        // underscore is 0x5f

        /// <summary>
        /// An array of bytes in the plain text.
        /// Its length is always one less than the specified plain text length,
        /// since the last byte is handled inline.
        /// </summary>
        private byte[] _bytes;

        /// <summary>
        /// An array of hashes of _bytes at different length.
        /// Serves as a cache to prevent re-calculation.
        /// </summary>
        private uint[] _hashes;

        /// <summary>
        /// Creates and initializes arrays.
        /// </summary>
        /// <param name="length">The length of plain text.</param>
        private void Initialize(int length)
        {
            _bytes = new byte[length - 1];
            _hashes = new uint[length - 1];

            // set up the first byte to get going after an Increment()
            _bytes[0] = 0x60;

            for (var i = 1; i < _bytes.Length; i++)
            {
                // set up every other byte to chain Increment() for the whole array
                _bytes[i] = 0x5f;
            }
        }

        /// <summary>
        /// Bruuuuuuuuuteforce!
        /// </summary>
        /// <param name="length">The length of plain text.</param>
        /// <param name="match">The hash to look for.</param>
        /// <param name="callback">An <see cref="OnMatchFound"/> callback that is called when a match is found.</param>
        public void Bruteforce(int length, uint match, OnMatchFound callback)
        {
            Initialize(length);
            while (true)
            {
                var depth = _bytes.Length - 1;
                while (_bytes.Increment(depth))
                {
                    depth--;
                    if (depth == -1) return;    // all permutations at this length are done
                }

                _hashes.ZeroFrom(depth);    // throw away stale hash caches

                byte lastByte = 0x2f;
                uint tempHash = Hash(_bytes, _hashes, _bytes.Length) * Prime;
                while (!lastByte.Increment(out var nextByte))
                {
                    lastByte = nextByte;
                    uint result = tempHash;
                    result ^= lastByte;

                    if (result == match)
                    {
                        callback(length, Encoding.ASCII.GetString(_bytes) + Convert.ToChar(lastByte));
                    }
                }
            }
        }

        private static uint Hash(byte[] array, uint[] hashes, int length)
        {
            if (length > 1)
            {
                // Return cached hash, or build upon last hash?
                uint hash = hashes[length - 1];
                if (hash > 0)
                {
                    // Cached
                    return hash;
                }
                else
                {
                    // Build
                    hash = Hash(array, hashes, length - 1) * Prime;
                    hash ^= array[length - 1];
                    hashes[length - 1] = hash;
                    return hash;
                }
            }
            else
            {
                // Build first hash, nothing to build upon on
                uint hash = OffsetBasis;
                hash *= Prime;
                hash ^= array[0];
                hashes[0] = hash;
                return hash;
            }
        }

        public delegate void OnMatchFound(int length, string match);
    }
}
