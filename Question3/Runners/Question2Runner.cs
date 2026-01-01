using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AllocationAlgorithms.Bitmap;
using AllocationAlgorithms.LinkedList;

class Question2Runner
{
  
    const int SPEED_RUNS = 100;               // Speed Test: 100 allocations
    const int FRAG_ALLOC_COUNT = 20;          // Fragmentation: 20 random allocations
    const int FRAG_FREE_COUNT = 5;            // Fragmentation: free exactly 5 at random
    const int LARGE_REQUEST = 12;             // Fragmentation: attempt one large block of size 12

    
    const int SPEED_DISK_SIZE = 5000;
    const int TRACE_DISK_SIZE = 80;
    const int FRAG_DISK_SIZE = 50;

    public static void Run()
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("FINAL PROJECT - Allocation Experiments");
        Console.WriteLine("Bitmap vs Linked-list | AI vs Rewritten (4 versions)");
        Console.WriteLine("====================================================\n");

        SpeedTest_4Versions();
        FragmentationTest_4Versions();
        AllocationTrace_4Versions();

        Console.WriteLine("\nALL TESTS COMPLETED.");
    }

 
    static void SpeedTest_4Versions()
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("1) SPEED TEST (100 allocations)");
        Console.WriteLine("Run each algorithm 100 times: allocate + free blocks, then compare time.");
        Console.WriteLine("====================================================");

        
        var rng = new Random(12345);
        int[] sizes = Enumerable.Range(0, SPEED_RUNS).Select(_ => rng.Next(1, 6)).ToArray(); // 1..5

        // ---- Bitmap AI ----
        var bAI = new BitmapAllocator(SPEED_DISK_SIZE);
        Stopwatch swBitmapAI = Stopwatch.StartNew();
        for (int i = 0; i < SPEED_RUNS; i++)
        {
            int sz = sizes[i];
            int start = bAI.Allocate(sz);
            if (start != -1) bAI.Free(start, sz);
        }
        swBitmapAI.Stop();

        // ---- Bitmap Rewritten ----
        var bHuman = new BitmapAllocatorRewritten(SPEED_DISK_SIZE);
        Stopwatch swBitmapHuman = Stopwatch.StartNew();
        for (int i = 0; i < SPEED_RUNS; i++)
        {
            int sz = sizes[i];
            int start = bHuman.AllocateBlocks(sz);
            if (start != -1) bHuman.FreeBlocks(start, sz);
        }
        swBitmapHuman.Stop();

        // ---- Linked-list AI ----
        var lAI = new LinkedListAllocator(SPEED_DISK_SIZE);
        Stopwatch swLinkedAI = Stopwatch.StartNew();
        for (int i = 0; i < SPEED_RUNS; i++)
        {
            int sz = sizes[i];
            int head = lAI.Allocate(sz);
            if (head != -1) lAI.Free(head);
        }
        swLinkedAI.Stop();

        // ---- Linked-list Rewritten ----
        var lHuman = new LinkedListAllocatorRewritten(SPEED_DISK_SIZE);
        Stopwatch swLinkedHuman = Stopwatch.StartNew();
        for (int i = 0; i < SPEED_RUNS; i++)
        {
            int sz = sizes[i];
            int head = lHuman.AllocateBlocks(sz);
            if (head != -1) lHuman.FreeFile(head);
        }
        swLinkedHuman.Stop();

        Console.WriteLine("\n--- SPEED RESULTS ---");
        Console.WriteLine($"Bitmap (AI) total time:          {swBitmapAI.ElapsedMilliseconds} ms ({swBitmapAI.Elapsed.TotalSeconds:F6} s)");
        Console.WriteLine($"Bitmap (Rewritten) total time:   {swBitmapHuman.ElapsedMilliseconds} ms ({swBitmapHuman.Elapsed.TotalSeconds:F6} s)");
        Console.WriteLine($"Linked-list (AI) total time:     {swLinkedAI.ElapsedMilliseconds} ms ({swLinkedAI.Elapsed.TotalSeconds:F6} s)");
        Console.WriteLine($"Linked-list (Rewritten) total time:{swLinkedHuman.ElapsedMilliseconds} ms ({swLinkedHuman.Elapsed.TotalSeconds:F6} s)");

        Console.WriteLine("\nNote for report:");
        Console.WriteLine("- Lower time means faster allocation+free for this test.\n");
    }

    // =========================================================
    // 2) FRAGMENTATION TEST
    // - Make 20 random allocations (varying sizes)
    // - Free exactly 5 of those allocations at random
    // - Attempt to allocate one large block of size 12
    // Report success/fail and explain based on strategy
    // =========================================================
    static void FragmentationTest_4Versions()
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("2) FRAGMENTATION TEST");
        Console.WriteLine("20 random allocations, free 5 randomly, attempt allocate size 12.");
        Console.WriteLine("====================================================");

        int seed = FindGoodFragmentationSeed(out int[] allocSizes, out int[] freeIndexes);

        Console.WriteLine($"Seed used: {seed}");
        Console.WriteLine("Allocation sizes (20): " + string.Join(", ", allocSizes));
        Console.WriteLine("Freed allocation indexes (5): " + string.Join(", ", freeIndexes));
        Console.WriteLine();

        // ---- BITMAP AI ----
        {
            var disk = new BitmapAllocator(FRAG_DISK_SIZE);
            var handles = new List<(int start, int size)>();

            for (int i = 0; i < FRAG_ALLOC_COUNT; i++)
            {
                int start = disk.Allocate(allocSizes[i]);
                handles.Add((start, allocSizes[i]));
            }

            foreach (int idx in freeIndexes)
            {
                var h = handles[idx];
                if (h.start != -1) disk.Free(h.start, h.size);
            }

            int big = disk.Allocate(LARGE_REQUEST);

            Console.WriteLine("---- Bitmap (AI) ----");
            Console.WriteLine("Disk state (0 free, 1 allocated):");
            Console.WriteLine(disk.GetBitmap());
            Console.WriteLine($"Allocate size {LARGE_REQUEST}: " + (big == -1 ? "FAILED" : $"SUCCEEDED (start={big})"));
            Console.WriteLine("Reason: Bitmap needs 12 consecutive free blocks.\n");
        }

        // ---- BITMAP REWRITTEN ----
        {
            var disk = new BitmapAllocatorRewritten(FRAG_DISK_SIZE);
            var handles = new List<(int start, int size)>();

            for (int i = 0; i < FRAG_ALLOC_COUNT; i++)
            {
                int start = disk.AllocateBlocks(allocSizes[i]);
                handles.Add((start, allocSizes[i]));
            }

            foreach (int idx in freeIndexes)
            {
                var h = handles[idx];
                if (h.start != -1) disk.FreeBlocks(h.start, h.size);
            }

            int big = disk.AllocateBlocks(LARGE_REQUEST);

            Console.WriteLine("---- Bitmap (Rewritten) ----");
            Console.WriteLine("Disk state (0 free, 1 allocated):");
            Console.WriteLine(disk.GetDiskState());
            Console.WriteLine($"Allocate size {LARGE_REQUEST}: " + (big == -1 ? "FAILED" : $"SUCCEEDED (start={big})"));
            Console.WriteLine("Reason: Bitmap needs 12 consecutive free blocks.\n");
        }

        // ---- LINKED-LIST AI ----
        {
            var disk = new LinkedListAllocator(FRAG_DISK_SIZE);
            var heads = new List<int>();

            for (int i = 0; i < FRAG_ALLOC_COUNT; i++)
            {
                int head = disk.Allocate(allocSizes[i]);
                heads.Add(head);
            }

            foreach (int idx in freeIndexes)
            {
                int head = heads[idx];
                if (head != -1) disk.Free(head);
            }

            int bigHead = disk.Allocate(LARGE_REQUEST);

            Console.WriteLine("---- Linked-list (AI) ----");
            Console.WriteLine("Disk usage (0 free, 1 allocated):");
            Console.WriteLine(disk.GetUsedMapString());
            Console.WriteLine($"Allocate size {LARGE_REQUEST}: " + (bigHead == -1 ? "FAILED" : "SUCCEEDED"));
            if (bigHead != -1) Console.WriteLine("Chain: " + disk.GetChainString(bigHead));
            Console.WriteLine("Reason: Linked-list can use non-consecutive blocks.\n");
        }

        // ---- LINKED-LIST REWRITTEN ----
        {
            var disk = new LinkedListAllocatorRewritten(FRAG_DISK_SIZE);
            var heads = new List<int>();

            for (int i = 0; i < FRAG_ALLOC_COUNT; i++)
            {
                int head = disk.AllocateBlocks(allocSizes[i]);
                heads.Add(head);
            }

            foreach (int idx in freeIndexes)
            {
                int head = heads[idx];
                if (head != -1) disk.FreeFile(head);
            }

            int bigHead = disk.AllocateBlocks(LARGE_REQUEST);

            Console.WriteLine("---- Linked-list (Rewritten) ----");
            Console.WriteLine("Disk usage (0 free, 1 allocated):");
            Console.WriteLine(disk.GetDiskUsageString());
            Console.WriteLine($"Allocate size {LARGE_REQUEST}: " + (bigHead == -1 ? "FAILED" : "SUCCEEDED"));
            if (bigHead != -1) Console.WriteLine("Chain: " + disk.GetFileChainString(bigHead));
            Console.WriteLine("Reason: Linked-list can use non-consecutive blocks.\n");
        }
    }

   
    static int FindGoodFragmentationSeed(out int[] sizes, out int[] freeIndexes)
    {
        int seed = 1000;

        while (true)
        {
            var rngSizes = new Random(seed);
            sizes = Enumerable.Range(0, FRAG_ALLOC_COUNT).Select(_ => rngSizes.Next(1, 5)).ToArray(); // 1..4

            var rngFree = new Random(seed + 999);
            freeIndexes = Enumerable.Range(0, FRAG_ALLOC_COUNT).OrderBy(_ => rngFree.Next()).Take(FRAG_FREE_COUNT).ToArray();

            var sim = new BitmapAllocatorRewritten(FRAG_DISK_SIZE);
            var handles = new List<(int start, int size)>();

            bool ok = true;
            for (int i = 0; i < FRAG_ALLOC_COUNT; i++)
            {
                int start = sim.AllocateBlocks(sizes[i]);
                handles.Add((start, sizes[i]));
                if (start == -1) { ok = false; break; }
            }

            if (!ok) { seed++; continue; }

            foreach (int idx in freeIndexes)
            {
                var h = handles[idx];
                if (h.start != -1) sim.FreeBlocks(h.start, h.size);
            }

            var map = sim.GetDiskState().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            int totalFree = map.Count(x => x == 0);

            int run = 0, maxRun = 0;
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == 0) { run++; maxRun = Math.Max(maxRun, run); }
                else run = 0;
            }

            if (totalFree >= LARGE_REQUEST && maxRun < LARGE_REQUEST)
                return seed;

            seed++;
        }
    }

    static void AllocationTrace_4Versions()
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("3) ALLOCATION TRACE");
        Console.WriteLine("15 allocations with fixed size sequence for ALL versions.");
        Console.WriteLine("After each allocation, print disk state (0 free, 1 allocated).");
        Console.WriteLine("====================================================");

        int[] seq = new int[] { 2, 3, 5, 2, 4, 6, 1, 3, 5, 2, 4, 3, 2, 1, 5 };

        Console.WriteLine("Fixed size sequence:");
        Console.WriteLine(string.Join(", ", seq));
        Console.WriteLine();

        // ---- Bitmap AI Trace ----
        Console.WriteLine("---- Bitmap (AI) Trace ----");
        var bAI = new BitmapAllocator(TRACE_DISK_SIZE);
        Console.WriteLine("Initial:");
        Console.WriteLine(bAI.GetBitmap());

        for (int i = 0; i < seq.Length; i++)
        {
            int start = bAI.Allocate(seq[i]);
            Console.WriteLine($"\nStep {i + 1} allocate {seq[i]} -> " + (start == -1 ? "FAILED" : $"start={start}"));
            Console.WriteLine(bAI.GetBitmap());
        }
        Console.WriteLine();

        // ---- Bitmap Rewritten Trace ----
        Console.WriteLine("---- Bitmap (Rewritten) Trace ----");
        var bHuman = new BitmapAllocatorRewritten(TRACE_DISK_SIZE);
        Console.WriteLine("Initial:");
        Console.WriteLine(bHuman.GetDiskState());

        for (int i = 0; i < seq.Length; i++)
        {
            int start = bHuman.AllocateBlocks(seq[i]);
            Console.WriteLine($"\nStep {i + 1} allocate {seq[i]} -> " + (start == -1 ? "FAILED" : $"start={start}"));
            Console.WriteLine(bHuman.GetDiskState());
        }
        Console.WriteLine();

        // ---- Linked-list AI Trace ----
        Console.WriteLine("---- Linked-list (AI) Trace ----");
        var lAI = new LinkedListAllocator(TRACE_DISK_SIZE);
        Console.WriteLine("Initial:");
        Console.WriteLine(lAI.GetUsedMapString());

        for (int i = 0; i < seq.Length; i++)
        {
            int head = lAI.Allocate(seq[i]);
            Console.WriteLine($"\nStep {i + 1} allocate {seq[i]} -> " + (head == -1 ? "FAILED" : "SUCCEEDED"));
            if (head != -1) Console.WriteLine("Chain: " + lAI.GetChainString(head));
            Console.WriteLine(lAI.GetUsedMapString());
        }
        Console.WriteLine();

        // ---- Linked-list Rewritten Trace ----
        Console.WriteLine("---- Linked-list (Rewritten) Trace ----");
        var lHuman = new LinkedListAllocatorRewritten(TRACE_DISK_SIZE);
        Console.WriteLine("Initial:");
        Console.WriteLine(lHuman.GetDiskUsageString());

        for (int i = 0; i < seq.Length; i++)
        {
            int head = lHuman.AllocateBlocks(seq[i]);
            Console.WriteLine($"\nStep {i + 1} allocate {seq[i]} -> " + (head == -1 ? "FAILED" : "SUCCEEDED"));
            if (head != -1) Console.WriteLine("Chain: " + lHuman.GetFileChainString(head));
            Console.WriteLine(lHuman.GetDiskUsageString());
        }

        Console.WriteLine("\nClear difference to describe in the report:");
        Console.WriteLine("- Bitmap chooses the first available CONSECUTIVE run of free blocks (contiguous first-fit).");
        Console.WriteLine("- Linked-list can take blocks from different places; it links them via pointers, so blocks do not need to be consecutive.");
        Console.WriteLine();
    }
}