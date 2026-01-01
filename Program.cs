/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AllocationAlgorithms.Bitmap;
using AllocationAlgorithms.LinkedList;

class Program
{
  
    const int SPEED_RUNS = 100;               // Speed Test: 100 allocations
    const int FRAG_ALLOC_COUNT = 20;          // Fragmentation: 20 random allocations
    const int FRAG_FREE_COUNT = 5;            // Fragmentation: free exactly 5 at random
    const int LARGE_REQUEST = 12;             // Fragmentation: attempt one large block of size 12

    
    const int SPEED_DISK_SIZE = 5000;
    const int TRACE_DISK_SIZE = 80;
    const int FRAG_DISK_SIZE = 50;

    static void Main()
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
}*/











/* =====================================================================
   QUESTION 3 – PART B : LINKED LIST MEMORY ALLOCATION IMPLEMENTATION
   ---------------------------------------------------------------------
   This section contains the implementation of:
   - Best Fit allocation
   - Worst Fit allocation
   - Next Fit allocation
   - Free with adjacent block merging

   Purpose:
   This code answers Question 3, Part B of the final project.
   It is intentionally commented out so that the instructor can
   clearly distinguish it from previous questions (Bitmap and
   Linked-List allocation examples).

   To run this part:
   - Uncomment this entire block
   - Comment out other Main() methods if necessary
   - Run the program normally (dotnet run)

   ===================================================================== */







    // Question 3 - Part B (Implementation)
    // Memory allocator with a free-list (linked list of [start, length]).
    //
    // What we implement here:
    // - allocate_best_fit(size)
    // - allocate_worst_fit(size)
    // - allocate_next_fit(size)
    // - free(start, size)  + merge adjacent segments
    //
    // Note:
    // This simulates a 100-unit memory: 0..99
    

    /*using System;

namespace AllocationAlgorithms.Question3
{
    public class FreeSegment
    {
        public int Start;
        public int Length;
        public FreeSegment? Next;

        public FreeSegment(int start, int length)
        {
            Start = start;
            Length = length;
            Next = null;
        }

        public int EndExclusive => Start + Length;

        public override string ToString() => $"[{Start}, {Length}]";
    }

    public class MemoryAllocatorLinkedList
    {
        private readonly int memorySize;
        private FreeSegment? freeHead;
        private FreeSegment? nextFitCursor;

        public MemoryAllocatorLinkedList(int totalMemorySize)
        {
            if (totalMemorySize <= 0) throw new ArgumentException("totalMemorySize must be > 0");

            memorySize = totalMemorySize;
            freeHead = new FreeSegment(0, memorySize);
            nextFitCursor = freeHead;
        }

        public int AllocateBestFit(int requestSize)
        {
            // Best Fit chooses the smallest free segment that is still large enough (length >= requestSize).
            // It scans the entire free list, keeps the best candidate found so far, and allocates from that segment.
            if (requestSize <= 0) return -1;

            FreeSegment? best = null;
            FreeSegment? bestPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                if (cur.Length >= requestSize)
                {
                    if (best == null || cur.Length < best.Length)
                    {
                        best = cur;
                        bestPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (best == null) return -1;

            return AllocateFrom(bestPrev, best, requestSize);
        }

        public int AllocateWorstFit(int requestSize)
        {
            // Worst Fit chooses the largest free segment available (among segments with length >= requestSize).
            // It scans the entire free list, tracks the largest candidate, and allocates from that biggest segment.
            if (requestSize <= 0) return -1;

            FreeSegment? worst = null;
            FreeSegment? worstPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                if (cur.Length >= requestSize)
                {
                    if (worst == null || cur.Length > worst.Length)
                    {
                        worst = cur;
                        worstPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (worst == null) return -1;

            return AllocateFrom(worstPrev, worst, requestSize);
        }

        public int AllocateNextFit(int requestSize)
        {
            // Next Fit starts searching from where the last allocation search ended (cursor),
            // not from the head of the list. It picks the first segment it encounters that fits
            // (length >= requestSize). If it reaches the end, it wraps around to the head.
            if (requestSize <= 0) return -1;
            if (freeHead == null) return -1;

            if (nextFitCursor == null) nextFitCursor = freeHead;

            FreeSegment startPoint = nextFitCursor;

            FreeSegment? cur = startPoint;
            FreeSegment? prev = FindPrev(cur);

            while (true)
            {
                if (cur != null && cur.Length >= requestSize)
                {
                    int allocatedStart = AllocateFrom(prev, cur, requestSize);
                    nextFitCursor = (cur.Length > 0) ? cur : (cur.Next ?? freeHead);
                    return allocatedStart;
                }

                if (cur == null || cur.Next == null)
                {
                    cur = freeHead;
                    prev = null;
                }
                else
                {
                    prev = cur;
                    cur = cur.Next;
                }

                if (cur == startPoint) break;
            }

            return -1;
        }

        public void Free(int start, int size)
        {
            if (size <= 0) return;
            if (start < 0 || start + size > memorySize) return;

            FreeSegment freed = new FreeSegment(start, size);

            if (freeHead == null)
            {
                freeHead = freed;
                nextFitCursor = freeHead;
                return;
            }

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null && cur.Start < freed.Start)
            {
                prev = cur;
                cur = cur.Next;
            }

            if (prev == null)
            {
                freed.Next = freeHead;
                freeHead = freed;
            }
            else
            {
                freed.Next = cur;
                prev.Next = freed;
            }

            MergeIfAdjacent(freed);

            if (nextFitCursor == null) nextFitCursor = freeHead;
        }

        private int AllocateFrom(FreeSegment? prev, FreeSegment segment, int requestSize)
        {
            int allocatedStart = segment.Start;

            segment.Start += requestSize;
            segment.Length -= requestSize;

            if (segment.Length == 0)
            {
                if (prev == null)
                    freeHead = segment.Next;
                else
                    prev.Next = segment.Next;

                if (nextFitCursor == segment)
                    nextFitCursor = segment.Next ?? freeHead;
            }

            return allocatedStart;
        }

        private void MergeIfAdjacent(FreeSegment node)
        {
            FreeSegment? prev = FindPrev(node);
            if (prev != null && prev.EndExclusive == node.Start)
            {
                prev.Length += node.Length;
                prev.Next = node.Next;
                node = prev;
            }

            FreeSegment? next = node.Next;
            if (next != null && node.EndExclusive == next.Start)
            {
                node.Length += next.Length;
                node.Next = next.Next;
            }
        }

        private FreeSegment? FindPrev(FreeSegment target)
        {
            if (freeHead == null || freeHead == target) return null;

            FreeSegment? cur = freeHead;
            while (cur != null && cur.Next != null)
            {
                if (cur.Next == target) return cur;
                cur = cur.Next;
            }
            return null;
        }

        public string FreeListToString()
        {
            if (freeHead == null) return "(empty)";

            string text = "";
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                text += cur.ToString();
                if (cur.Next != null) text += " -> ";
                cur = cur.Next;
            }

            return text;
        }
    }

    class Program
    {
        static void Main()
        {
            var mem = new MemoryAllocatorLinkedList(100);

            Console.WriteLine("Initial free list:");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            int a = mem.AllocateBestFit(10);
            Console.WriteLine($"BestFit allocate 10 -> start={a}");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            int b = mem.AllocateWorstFit(20);
            Console.WriteLine($"WorstFit allocate 20 -> start={b}");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            int c = mem.AllocateNextFit(5);
            Console.WriteLine($"NextFit allocate 5 -> start={c}");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            mem.Free(a, 10);
            Console.WriteLine("Free block [0,10] (merge may or may not happen yet):");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            mem.Free(b, 20);
            Console.WriteLine("Free block [10,20] (NOW it should merge with [0,10] into [0,30]):");
            Console.WriteLine(mem.FreeListToString());
            Console.WriteLine();

            mem.Free(c, 5);
            Console.WriteLine("Free block [30,5] (merges again, expected [0,35] -> [35,65]):");
            Console.WriteLine(mem.FreeListToString());
        }
    }
}*/













/* ============================================================
       Question 3 – Part C
       Experiment 1: Allocation Trace

       Fixed sequence used:
       [10, 5, 20, -5, 12, -10, 8, 6, 7, 3, 10]

       After each request (allocate/free), print the entire free list
       for Best Fit, Worst Fit, and Next Fit.
       ============================================================ */












/*using System;
using System.Collections.Generic;

namespace AllocationAlgorithms.Question3
{
    

    public class FreeSegment
    {
        public int Start;
        public int Length;
        public FreeSegment? Next;

        public FreeSegment(int start, int length)
        {
            Start = start;
            Length = length;
            Next = null;
        }

        public int EndExclusive => Start + Length;

        public override string ToString() => $"[{Start}, {Length}]";
    }

    public class MemoryAllocatorLinkedList
    {
        private readonly int memorySize;
        private FreeSegment? freeHead;
        private FreeSegment? nextFitCursor;

        public MemoryAllocatorLinkedList(int totalMemorySize)
        {
            if (totalMemorySize <= 0) throw new ArgumentException("totalMemorySize must be > 0");
            memorySize = totalMemorySize;
            freeHead = new FreeSegment(0, memorySize);
            nextFitCursor = freeHead;
        }

        public int AllocateBestFit(int requestSize)
        {
            // Best Fit chooses the smallest free segment that can fit the request (length >= requestSize).
            if (requestSize <= 0) return -1;

            FreeSegment? best = null;
            FreeSegment? bestPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                if (cur.Length >= requestSize)
                {
                    if (best == null || cur.Length < best.Length)
                    {
                        best = cur;
                        bestPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (best == null) return -1;
            return AllocateFrom(bestPrev, best, requestSize);
        }

        public int AllocateWorstFit(int requestSize)
        {
            // Worst Fit chooses the largest free segment available (among segments that can fit).
            if (requestSize <= 0) return -1;

            FreeSegment? worst = null;
            FreeSegment? worstPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                if (cur.Length >= requestSize)
                {
                    if (worst == null || cur.Length > worst.Length)
                    {
                        worst = cur;
                        worstPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (worst == null) return -1;
            return AllocateFrom(worstPrev, worst, requestSize);
        }

        public int AllocateNextFit(int requestSize)
        {
            // Next Fit starts searching from the last position (cursor) and picks the first segment that fits.
            // If it reaches the end, it wraps around to the head.
            if (requestSize <= 0) return -1;
            if (freeHead == null) return -1;

            if (nextFitCursor == null) nextFitCursor = freeHead;

            FreeSegment startPoint = nextFitCursor;
            FreeSegment? cur = startPoint;
            FreeSegment? prev = FindPrev(cur);

            while (true)
            {
                if (cur != null && cur.Length >= requestSize)
                {
                    int allocatedStart = AllocateFrom(prev, cur, requestSize);
                    nextFitCursor = (cur.Length > 0) ? cur : (cur.Next ?? freeHead);
                    return allocatedStart;
                }

                if (cur == null || cur.Next == null)
                {
                    cur = freeHead;
                    prev = null;
                }
                else
                {
                    prev = cur;
                    cur = cur.Next;
                }

                if (cur == startPoint) break;
            }

            return -1;
        }

        public void Free(int start, int size)
        {
            if (size <= 0) return;
            if (start < 0 || start + size > memorySize) return;

            FreeSegment freed = new FreeSegment(start, size);

            if (freeHead == null)
            {
                freeHead = freed;
                nextFitCursor = freeHead;
                return;
            }

            FreeSegment? prev = null;
            FreeSegment? cur = freeHead;

            while (cur != null && cur.Start < freed.Start)
            {
                prev = cur;
                cur = cur.Next;
            }

            if (prev == null)
            {
                freed.Next = freeHead;
                freeHead = freed;
            }
            else
            {
                freed.Next = cur;
                prev.Next = freed;
            }

            MergeIfAdjacent(freed);

            if (nextFitCursor == null) nextFitCursor = freeHead;
        }

        private int AllocateFrom(FreeSegment? prev, FreeSegment segment, int requestSize)
        {
            int allocatedStart = segment.Start;

            segment.Start += requestSize;
            segment.Length -= requestSize;

            if (segment.Length == 0)
            {
                if (prev == null) freeHead = segment.Next;
                else prev.Next = segment.Next;

                if (nextFitCursor == segment)
                    nextFitCursor = segment.Next ?? freeHead;
            }

            return allocatedStart;
        }

        private void MergeIfAdjacent(FreeSegment node)
        {
            FreeSegment? prev = FindPrev(node);

            if (prev != null && prev.EndExclusive == node.Start)
            {
                prev.Length += node.Length;
                prev.Next = node.Next;
                node = prev;
            }

            FreeSegment? next = node.Next;

            if (next != null && node.EndExclusive == next.Start)
            {
                node.Length += next.Length;
                node.Next = next.Next;
            }
        }

        private FreeSegment? FindPrev(FreeSegment target)
        {
            if (freeHead == null || freeHead == target) return null;

            FreeSegment? cur = freeHead;

            while (cur != null && cur.Next != null)
            {
                if (cur.Next == target) return cur;
                cur = cur.Next;
            }

            return null;
        }

        public string FreeListToString()
        {
            if (freeHead == null) return "(empty)";

            string text = "";
            FreeSegment? cur = freeHead;

            while (cur != null)
            {
                text += cur.ToString();
                if (cur.Next != null) text += " -> ";
                cur = cur.Next;
            }

            return text;
        }
    }

    public class AllocationTraceRunner
    {
        private readonly MemoryAllocatorLinkedList best;
        private readonly MemoryAllocatorLinkedList worst;
        private readonly MemoryAllocatorLinkedList next;

        private readonly List<(int start, int size)> bestAllocations = new();
        private readonly List<(int start, int size)> worstAllocations = new();
        private readonly List<(int start, int size)> nextAllocations = new();

        public AllocationTraceRunner(int memorySize)
        {
            best = new MemoryAllocatorLinkedList(memorySize);
            worst = new MemoryAllocatorLinkedList(memorySize);
            next = new MemoryAllocatorLinkedList(memorySize);
        }

        public void RunFixedSequence(int[] sequence)
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("Question 3 - Part C | Experiment 1: Allocation Trace");
            Console.WriteLine("====================================================");
            Console.WriteLine("Fixed sequence: " + string.Join(", ", sequence));
            Console.WriteLine();

            for (int step = 0; step < sequence.Length; step++)
            {
                int req = sequence[step];
                Console.WriteLine($"--- Step {step + 1}: Request = {req} ---");

                ApplyRequest(best, bestAllocations, req, "Best Fit");
                ApplyRequest(worst, worstAllocations, req, "Worst Fit");
                ApplyRequest(next, nextAllocations, req, "Next Fit");

                Console.WriteLine();
            }
        }

        private void ApplyRequest(MemoryAllocatorLinkedList allocator, List<(int start, int size)> allocations, int req, string label)
        {
            if (req > 0)
            {
                int start = (label == "Best Fit") ? allocator.AllocateBestFit(req)
                         : (label == "Worst Fit") ? allocator.AllocateWorstFit(req)
                         : allocator.AllocateNextFit(req);

                if (start != -1)
                    allocations.Add((start, req));

                Console.WriteLine($"{label}: allocate {req} -> start={(start == -1 ? "FAILED" : start.ToString())}");
                Console.WriteLine($"{label} Free List: {allocator.FreeListToString()}");
            }
            else
            {
                int sizeToFree = -req;
                int index = allocations.FindIndex(a => a.size == sizeToFree);

                if (index == -1)
                {
                    Console.WriteLine($"{label}: free {sizeToFree} -> NOT FOUND");
                    Console.WriteLine($"{label} Free List: {allocator.FreeListToString()}");
                    return;
                }

                var item = allocations[index];
                allocations.RemoveAt(index);

                allocator.Free(item.start, item.size);

                Console.WriteLine($"{label}: free {sizeToFree} -> start={item.start}");
                Console.WriteLine($"{label} Free List: {allocator.FreeListToString()}");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            int[] sequence = { 10, 5, 20, -5, 12, -10, 8, 6, 7, 3, 10 };

            var runner = new AllocationTraceRunner(memorySize: 100);
            runner.RunFixedSequence(sequence);
        }
    }
}*/











// ====================================================
// QUESTION 3 - PART D
// Experiment 2: Fragmentation Test
// This section implements ONLY Part D as requested.
// ====================================================




/*using System;
using System.Collections.Generic;
using System.Linq;

namespace AllocationAlgorithms.Question3
{
 
   

    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("Question 3 - Part D | Experiment 2: Fragmentation Test");
            Console.WriteLine("====================================================");

            const int totalMemory = 100;

            // D part requirements
            const int allocationCount = 12; // 12 random allocations (size 3–12)
            const int freeCount = 4;        // free exactly 4 previous blocks
            const int largeRequest = 25;    // attempt one large allocation of 25

            // Fixed seeds = reproducible experiment
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

            // Keep successful allocations so we can randomly free 4 of them later
            var successful = new List<(int start, int size)>();

            // 1) Perform 12 random allocations
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

            // 2) Free exactly 4 previously allocated blocks at random
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

            // 3) Attempt one large allocation of size 25
            int bigStart = allocator.Allocate(largeRequest);

            if (bigStart == -1)
                Console.WriteLine($"Allocate size {largeRequest}: FAILED");
            else
                Console.WriteLine($"Allocate size {largeRequest}: SUCCEEDED -> start={bigStart}");

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

            return picked.ToList();
        }
    }

  

    internal class FreeSegment
    {
        public int Start;
        public int Length;
        public FreeSegment? Next;

        public FreeSegment(int start, int length)
        {
            Start = start;
            Length = length;
            Next = null;
        }

        public int EndExclusive => Start + Length;

        public override string ToString() => $"[{Start}, {Length}]";
    }

    internal interface IAllocator
    {
        int Allocate(int size);
        void Free(int start, int size);
        string FreeListString();
    }

    internal abstract class AllocatorBase : IAllocator
    {
        protected readonly int memorySize;
        protected FreeSegment? head;

        protected AllocatorBase(int totalMemory)
        {
            memorySize = totalMemory;
            head = new FreeSegment(0, totalMemory);
        }

        public abstract int Allocate(int size);

        public void Free(int start, int size)
        {
            if (size <= 0) return;
            if (start < 0 || start + size > memorySize) return;

            var freed = new FreeSegment(start, size);

            // Insert sorted by Start
            if (head == null || start < head.Start)
            {
                freed.Next = head;
                head = freed;
                MergeAround(freed);
                return;
            }

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null && cur.Start < start)
            {
                prev = cur;
                cur = cur.Next;
            }

            freed.Next = cur;
            prev!.Next = freed;

            MergeAround(freed);
        }

        public string FreeListString()
        {
            if (head == null) return "(empty)";

            var parts = new List<string>();
            var cur = head;

            while (cur != null)
            {
                parts.Add(cur.ToString());
                cur = cur.Next;
            }

            return string.Join(" -> ", parts);
        }

        protected int AllocateFrom(FreeSegment? prev, FreeSegment seg, int size)
        {
            int allocatedStart = seg.Start;

            // allocate from the beginning of the segment
            seg.Start += size;
            seg.Length -= size;

            // remove if fully consumed
            if (seg.Length == 0)
            {
                if (prev == null) head = seg.Next;
                else prev.Next = seg.Next;
            }

            return allocatedStart;
        }

        protected FreeSegment? FindPrev(FreeSegment target)
        {
            if (head == null || head == target) return null;

            FreeSegment? cur = head;
            while (cur != null && cur.Next != null)
            {
                if (cur.Next == target) return cur;
                cur = cur.Next;
            }
            return null;
        }

        private void MergeAround(FreeSegment node)
        {
            // merge with previous if adjacent
            var prev = FindPrev(node);
            if (prev != null && prev.EndExclusive == node.Start)
            {
                prev.Length += node.Length;
                prev.Next = node.Next;
                node = prev;
            }

            // merge with next if adjacent
            var next = node.Next;
            if (next != null && node.EndExclusive == next.Start)
            {
                node.Length += next.Length;
                node.Next = next.Next;
            }
        }
    }

    internal class BestFitAllocator : AllocatorBase
    {
        public BestFitAllocator(int totalMemory) : base(totalMemory) { }

        public override int Allocate(int size)
        {
            // Best Fit: scans the whole list, picks the SMALLEST segment that still fits (Length >= size)
            if (size <= 0) return -1;

            FreeSegment? best = null;
            FreeSegment? bestPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    if (best == null || cur.Length < best.Length)
                    {
                        best = cur;
                        bestPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (best == null) return -1;
            return AllocateFrom(bestPrev, best, size);
        }
    }

    internal class WorstFitAllocator : AllocatorBase
    {
        public WorstFitAllocator(int totalMemory) : base(totalMemory) { }

        public override int Allocate(int size)
        {
            // Worst Fit: scans the whole list, picks the LARGEST segment that fits (Length >= size)
            if (size <= 0) return -1;

            FreeSegment? worst = null;
            FreeSegment? worstPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    if (worst == null || cur.Length > worst.Length)
                    {
                        worst = cur;
                        worstPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (worst == null) return -1;
            return AllocateFrom(worstPrev, worst, size);
        }
    }

    internal class NextFitAllocator : AllocatorBase
    {
        private FreeSegment? cursor;

        public NextFitAllocator(int totalMemory) : base(totalMemory)
        {
            cursor = head;
        }

        public override int Allocate(int size)
        {
            // Next Fit: starts searching from the last cursor position, picks the FIRST segment that fits.
            // If it reaches the end, it wraps back to head.
            if (size <= 0) return -1;
            if (head == null) return -1;

            if (cursor == null) cursor = head;

            FreeSegment startPoint = cursor;

            FreeSegment? cur = cursor;
            FreeSegment? prev = FindPrev(cur);

            while (true)
            {
                if (cur != null && cur.Length >= size)
                {
                    int allocatedStart = AllocateFrom(prev, cur, size);

                    // move cursor forward (if current was removed, try next, otherwise stay on current)
                    cursor = (cur.Length > 0) ? cur : (cur.Next ?? head);

                    return allocatedStart;
                }

                // go next, wrap if needed
                if (cur == null || cur.Next == null)
                {
                    cur = head;
                    prev = null;
                }
                else
                {
                    prev = cur;
                    cur = cur.Next;
                }

                if (cur == startPoint) break;
            }

            return -1;
        }
    }
}*/






 // ============================================================
    // QUESTION 3 - PART E
    // Experiment 3: Speed Test (200 iterations)
    //
    // Repeat 200 times:
    // 1) allocate a random block (size 1–10)
    // 2) free one previously allocated block
    //
    // Measure total time for Best Fit, Worst Fit, Next Fit.
    // ============================================================







using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AllocationAlgorithms.Question3
{
    

    internal class Program
    {
        static void Main()
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

        private static SpeedResult RunSpeedTestOneAllocator(string title, IAllocator allocator, int[] requestSizes, int seedForFreeChoice)
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
    }

    internal readonly record struct SpeedResult(string Title, long ElapsedTicks, long ElapsedMilliseconds, double ElapsedSeconds);

    // ============================================================
    // Linked-list free segments infrastructure
    // ============================================================

    internal class FreeSegment
    {
        public int Start;
        public int Length;
        public FreeSegment? Next;

        public FreeSegment(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int EndExclusive => Start + Length;

        public override string ToString() => $"[{Start}, {Length}]";
    }

    internal interface IAllocator
    {
        int Allocate(int size);
        void Free(int start, int size);
    }

    internal abstract class AllocatorBase : IAllocator
    {
        protected readonly int memorySize;
        protected FreeSegment? head;

        protected AllocatorBase(int memorySize)
        {
            this.memorySize = memorySize;
            head = new FreeSegment(0, memorySize);
        }

        public abstract int Allocate(int size);

        public void Free(int start, int size)
        {
            if (size <= 0) return;
            if (start < 0 || start + size > memorySize) return;

            
            var freed = new FreeSegment(start, size);

            if (head == null || start < head.Start)
            {
                freed.Next = head;
                head = freed;
                MergeAround(freed);
                return;
            }

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null && cur.Start < start)
            {
                prev = cur;
                cur = cur.Next;
            }

            freed.Next = cur;
            prev!.Next = freed;

            MergeAround(freed);
        }

        protected int AllocateFrom(FreeSegment? prev, FreeSegment seg, int size)
        {
            int allocatedStart = seg.Start;

            
            seg.Start += size;
            seg.Length -= size;

            
            if (seg.Length == 0)
            {
                if (prev == null) head = seg.Next;
                else prev.Next = seg.Next;
            }

            return allocatedStart;
        }

        protected FreeSegment? FindPrev(FreeSegment target)
        {
            if (head == null || head == target) return null;

            FreeSegment? cur = head;
            while (cur != null && cur.Next != null)
            {
                if (cur.Next == target) return cur;
                cur = cur.Next;
            }
            return null;
        }

        private void MergeAround(FreeSegment node)
        {
            
            var prev = FindPrev(node);
            if (prev != null && prev.EndExclusive == node.Start)
            {
                prev.Length += node.Length;
                prev.Next = node.Next;
                node = prev;
            }

           
            var next = node.Next;
            if (next != null && node.EndExclusive == next.Start)
            {
                node.Length += next.Length;
                node.Next = next.Next;
            }
        }
    }

    internal class BestFitAllocator : AllocatorBase
    {
        public BestFitAllocator(int memorySize) : base(memorySize) { }

        public override int Allocate(int size)
        {
            // Best Fit: scan the entire free list and pick the smallest segment that can fit.
            if (size <= 0) return -1;

            FreeSegment? best = null;
            FreeSegment? bestPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    if (best == null || cur.Length < best.Length)
                    {
                        best = cur;
                        bestPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (best == null) return -1;
            return AllocateFrom(bestPrev, best, size);
        }
    }

    internal class WorstFitAllocator : AllocatorBase
    {
        public WorstFitAllocator(int memorySize) : base(memorySize) { }

        public override int Allocate(int size)
        {
            // Worst Fit: scan the entire free list and pick the largest segment that can fit.
            if (size <= 0) return -1;

            FreeSegment? worst = null;
            FreeSegment? worstPrev = null;

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    if (worst == null || cur.Length > worst.Length)
                    {
                        worst = cur;
                        worstPrev = prev;
                    }
                }

                prev = cur;
                cur = cur.Next;
            }

            if (worst == null) return -1;
            return AllocateFrom(worstPrev, worst, size);
        }
    }

    internal class NextFitAllocator : AllocatorBase
    {
        private FreeSegment? cursor;

        public NextFitAllocator(int memorySize) : base(memorySize)
        {
            cursor = head;
        }

        public override int Allocate(int size)
        {
            // Next Fit: start searching from the last cursor position,
            // pick the first segment that fits, and scan at most one full loop.
            if (size <= 0) return -1;
            if (head == null) return -1;

            if (cursor == null)
                cursor = head;

            FreeSegment? cur = cursor;
            FreeSegment? prev = FindPrev(cur);

            bool wrapped = false;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    int start = AllocateFrom(prev, cur, size);

                    
                    cursor = (cur.Length > 0) ? cur : (cur.Next ?? head);

                    return start;
                }

                if (cur.Next != null)
                {
                    prev = cur;
                    cur = cur.Next;
                }
                else
                {
                    
                    if (wrapped) break;
                    wrapped = true;

                    cur = head;
                    prev = null;
                }
            }

            
            return -1;
        }
    }
}

