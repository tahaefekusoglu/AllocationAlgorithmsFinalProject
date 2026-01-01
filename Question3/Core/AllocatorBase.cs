using System.Collections.Generic;

namespace AllocationAlgorithms.Question3.Core
{
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

            // Insert into free list sorted by Start
            if (head == null || freed.Start < head.Start)
            {
                freed.Next = head;
                head = freed;
                MergeAround(freed);
                return;
            }

            FreeSegment? prev = null;
            FreeSegment? cur = head;

            while (cur != null && cur.Start < freed.Start)
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

            // allocate from beginning of seg
            seg.Start += size;
            seg.Length -= size;

            // remove if empty
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
}
