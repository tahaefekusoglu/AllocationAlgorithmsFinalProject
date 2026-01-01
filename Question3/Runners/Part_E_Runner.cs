using System;
using System.Collections.Generic;
using System.Diagnostics;
using AllocationAlgorithms.Question3.Core;

namespace AllocationAlgorithms.Question3.Runners
{
    internal static class PartERunner
    {
        public static void Run()
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("Question 3 - Part E | Experiment 3: Speed Test");
            Console.WriteLine("====================================================");

            const int memorySize = 100;
            const int iterations = 200;
            const int minSize = 1;
            const int maxSize = 10;

            const int seed = 2026;

            Console.WriteLine($"Memory size: {memorySize}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine($"Allocation size range: {minSize}–{maxSize}");
            Console.WriteLine($"Seed: {seed}");
            Console.WriteLine();

            int[] requestSizes = CreateRandomRequests(iterations, minSize, maxSize, seed);

            var best = RunSpeedTestOneAllocator("Best Fit", new BestFitAllocator(memorySize), requestSizes, seed + 100);
            var worst = RunSpeedTestOneAllocator("Worst Fit", new WorstFitAllocator(memorySize), requestSizes, seed + 200);
            var next = RunSpeedTestOneAllocator("Next Fit", new NextFitAllocator(memorySize), requestSizes, seed + 300);

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("--- SPEED RESULTS (Total time for 200 iterations) ---");
            Console.WriteLine($"Best Fit :  {best.ElapsedMilliseconds} ms ({best.ElapsedSeconds:F6} s)");
            Console.WriteLine($"Worst Fit:  {worst.ElapsedMilliseconds} ms ({worst.ElapsedSeconds:F6} s)");
            Console.WriteLine($"Next Fit :  {next.ElapsedMilliseconds} ms ({next.ElapsedSeconds:F6} s)");
            Console.WriteLine();

            SpeedResult fastest = best;
            if (worst.ElapsedTicks < fastest.ElapsedTicks) fastest = worst;
            if (next.ElapsedTicks < fastest.ElapsedTicks) fastest = next;

            SpeedResult slowest = best;
            if (worst.ElapsedTicks > slowest.ElapsedTicks) slowest = worst;
            if (next.ElapsedTicks > slowest.ElapsedTicks) slowest = next;

            Console.WriteLine($"Fastest: {fastest.Title}");
            Console.WriteLine($"Slowest: {slowest.Title}");
            Console.WriteLine();
            Console.WriteLine("Note: Small timing differences are normal on PCs (background processes etc.).");
            Console.WriteLine("The main reason for speed differences is how much of the linked list each algorithm scans.");
        }

        private static SpeedResult RunSpeedTestOneAllocator(
            string title,
            IAllocator allocator,
            int[] requestSizes,
            int seedForFreeChoice)
        {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"---- {title} ----");
            Console.WriteLine("----------------------------------------------------");

            var allocatedBlocks = new List<(int start, int size)>();
            var rndFree = new Random(seedForFreeChoice);

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < requestSizes.Length; i++)
            {
                int size = requestSizes[i];

                int start = allocator.Allocate(size);
                if (start != -1)
                    allocatedBlocks.Add((start, size));

                // free one previously allocated block (if any)
                if (allocatedBlocks.Count > 0)
                {
                    int victimIndex = rndFree.Next(0, allocatedBlocks.Count);
                    var victim = allocatedBlocks[victimIndex];

                    allocator.Free(victim.start, victim.size);
                    allocatedBlocks.RemoveAt(victimIndex);
                }
            }

            sw.Stop();

            Console.WriteLine($"Completed: {requestSizes.Length} iterations (allocate + free each loop)");
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalSeconds:F6} s)");
            Console.WriteLine();

            return new SpeedResult(title, sw.ElapsedTicks, sw.ElapsedMilliseconds, sw.Elapsed.TotalSeconds);
        }

        private static int[] CreateRandomRequests(int count, int minInclusive, int maxInclusive, int seed)
        {
            var rnd = new Random(seed);
            int[] arr = new int[count];

            for (int i = 0; i < count; i++)
                arr[i] = rnd.Next(minInclusive, maxInclusive + 1);

            return arr;
        }

        private readonly record struct SpeedResult(
            string Title,
            long ElapsedTicks,
            long ElapsedMilliseconds,
            double ElapsedSeconds);
    }
}
