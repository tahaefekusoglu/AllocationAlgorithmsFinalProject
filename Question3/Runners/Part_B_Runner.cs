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
    

    using System;

namespace AllocationAlgorithms.Question3.Runners.PartB
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
        public static void Run()
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
}