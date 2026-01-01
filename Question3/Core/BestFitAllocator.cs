namespace AllocationAlgorithms.Question3.Core
{
    internal sealed class BestFitAllocator : AllocatorBase
    {
        public BestFitAllocator(int totalMemory) : base(totalMemory) { }

        public override int Allocate(int size)
        {
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
}
