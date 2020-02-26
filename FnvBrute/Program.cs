using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using static FnvBrute.FnvHasher;

namespace FnvBrute
{
    class Program
    {
        private static Stopwatch _stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            uint match = 0;
            int maxLength = 0;

            if (args.Length == 2)
            {
                // Parse hash param
                try
                {
                    match = (uint)new UInt32Converter().ConvertFromString(args[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading first argument (hash): " + e.Message);
                }

                // Parse length param
                try
                {
                    maxLength = (int)new Int32Converter().ConvertFromString(args[1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading second argument (maxLength): " + e.Message);
                }
            }

            if (args.Length < 2 || match == 0 || maxLength < 2)
            {
                Console.WriteLine("\nFnvBrute FNV-1 (32-bit) hash collider, https://github.com/xyx0826/FnvBrute");
                Console.WriteLine("Usage: FnvBrute.exe {hash} {maxLength}");
                Console.WriteLine("Hash should be a uint32_t in either decimal or hexadecimal form");
                Console.WriteLine("MaxLength should be equal or greater than 2");
                Console.WriteLine("Example: FnvBrute.exe 0x1234abcd 12\n");
                return;
            }

            Console.WriteLine($"Hash: {match}, max plaintext length: {maxLength}");
            _stopwatch.Start();

            for (int length = 2; length <= maxLength; length++)
            {
                CreateFnvThread(length, match);
            }

            var exitEvent = new ManualResetEventSlim();
            exitEvent.Wait();
        }

        static void CreateFnvThread(int length, uint match)
        {
            var thread = new Thread(() =>
            {
                Console.WriteLine("Creating hasher for length " + length);
                StartFnv(length, match);
                Console.WriteLine($"Hasher for length {length} finished in {_stopwatch.ElapsedMilliseconds / 1000}s");
            });

            thread.Start();
        }

        static void StartFnv(int length, uint match)
        {
            var hasher = new FnvHasher();
            hasher.Bruteforce(length, match, _onMatchFound);
        }

        static OnMatchFound _onMatchFound = new OnMatchFound((length, match) =>
        {
            Console.WriteLine($"Length {length} match: >> {match} << in {_stopwatch.ElapsedMilliseconds / 1000}s");
        });
    }
}
