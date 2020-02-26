using System;
using System.Diagnostics;
using System.Threading;
using static FnvBrute.FnvHasher;

namespace FnvBrute
{
    class Program
    {
        private static Stopwatch _stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            Console.WriteLine("FnvBrute FNV-1 (32-bit) hash collider, https://github.com/xyx0826/FnvBrute");
            Console.WriteLine("Usage: FnvBrute.exe {hash} {maxLength}");
            Console.WriteLine("Hash should be a uint32_t in either decimal or hexadecimal form");
            Console.WriteLine("MaxLength should be equal or greater than 2");
            Console.WriteLine("Example: FnvBrute.exe 0x1234abcd 12\n");

            // Parse hash param
            uint match;
            try
            {
                match = Convert.ToUInt32(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading first argument (hash): " + e.Message);
                return;
            }

            // Parse length param
            int maxLength;
            try
            {
                maxLength = Convert.ToInt32(args[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading second argument (maxLength): " + e.Message);
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
