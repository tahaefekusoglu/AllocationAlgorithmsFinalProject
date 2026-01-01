using System;
using System.Collections.Generic;
using System.Linq;
using AllocationAlgorithms.Question3.Core;

namespace AllocationAlgorithms.Question3.Runners
{
    internal static class PartDRunner
    {
        public static void Run()
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("Question 3 - Part D | Experiment 2: Fragmentation Test");
            Console.WriteLine("====================================================");

            const int totalMemory = 100;

            const int allocationCount = 12; // sizes 3–12
            const int freeCount = 4;
            const int largeRequest = 25;

            int seedSizes = 2025;
            int seedFrees = 777;

            Console.WriteLine($"Seed used for allocation sizes: {seedSizes}");
            Console.WriteLine($"Seed used for freeing blocks: {seedFrees}");
            Console.WriteLine();

            int[] allocationSizes = GenerateRandomSizes(allocationCount, 3, 12, seedSizes);

            Console.WriteLine("Random allocation sizes (12 allocations, size range 3–12):");
            Console.WriteLine(string.Join(", ", allocationSizes));
            Console.WriteLine();

            RunFragmentationTest("Best Fit", new BestFitAllocator(totalMemory), allocationSizes, freeCount, largeRequest, seedFrees);
            Console.WriteLine();

            RunFragmentationTest("Worst Fit", new WorstFitAllocator(totalMemory), allocationSizes, freeCount, largeRequest, seedFrees);
            Console.WriteLine();

            RunFragmentationTest("Next Fit", new NextFitAllocator(totalMemory), allocationSizes, freeCount, largeRequest, seedFrees);
        }

        private static void RunFragmentationTest(
            string title,
            IAllocator allocator,
            int[] sizes,
            int freeExactly,
            int largeRequest,
            int seedFrees)
        {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"---- {title} ----");
            Console.WriteLine("----------------------------------------------------");

            var successful = new List<(int start, int size)>();

            // 1) 12 allocations
            foreach (int size in sizes)
            {
                int start = allocator.Allocate(size);

                if (start != -1)
                {
                    successful.Add((start, size));
                    Console.WriteLine($"Allocate {size} -> start={start}");
                }
                else
                {
                    Console.WriteLine($"Allocate {size} -> FAILED");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Free List after 12 allocations:");
            Console.WriteLine(allocator.FreeListString());
            Console.WriteLine();

            // 2) free exactly 4 allocations randomly
            int howMany = Math.Min(freeExactly, successful.Count);

            if (howMany == 0)
            {
                Console.WriteLine("No successful allocations, nothing to free.");
            }
            else
            {
                var rnd = new Random(seedFrees);
                var indexes = PickUniqueIndexes(howMany, successful.Count, rnd);

                Console.WriteLine($"Freed allocation indexes ({howMany}): " + string.Join(", ", indexes));
                Console.WriteLine();

                foreach (int idx in indexes)
                {
                    var item = successful[idx];
                    allocator.Free(item.start, item.size);
                    Console.WriteLine($"Free block [{item.start},{item.size}]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Free List after freeing blocks (merge applied):");
            Console.WriteLine(allocator.FreeListString());
            Console.WriteLine();

            // 3) allocate size 25
            int bigStart = allocator.Allocate(largeRequest);

            Console.WriteLine(bigStart == -1
                ? $"Allocate size {largeRequest}: FAILED"
                : $"Allocate size {largeRequest}: SUCCEEDED -> start={bigStart}");

            Console.WriteLine();
            Console.WriteLine("Final Free List:");
            Console.WriteLine(allocator.FreeListString());
        }

        private static int[] GenerateRandomSizes(int count, int minInclusive, int maxInclusive, int seed)
        {
            var rnd = new Random(seed);
            int[] arr = new int[count];

            for (int i = 0; i < count; i++)
                arr[i] = rnd.Next(minInclusive, maxInclusive + 1);

            return arr;
        }

        private static List<int> PickUniqueIndexes(int howMany, int maxCount, Random rnd)
        {
            var picked = new HashSet<int>();
            while (picked.Count < howMany)
                picked.Add(rnd.Next(0, maxCount));

            return picked.OrderBy(x => x).ToList();
        }
    }
}
