/* ============================================================
       Question 3 – Part C
       Experiment 1: Allocation Trace

       Fixed sequence used:
       [10, 5, 20, -5, 12, -10, 8, 6, 7, 3, 10]

       After each request (allocate/free), print the entire free list
       for Best Fit, Worst Fit, and Next Fit.
       ============================================================ */












using System;
using System.Collections.Generic;

namespace AllocationAlgorithms.Question3.Runners.PartC
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
        public static void Run()
        {
            int[] sequence = { 10, 5, 20, -5, 12, -10, 8, 6, 7, 3, 10 };

            var runner = new AllocationTraceRunner(memorySize: 100);
            runner.RunFixedSequence(sequence);
        }
    }
}
