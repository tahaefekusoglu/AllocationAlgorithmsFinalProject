namespace AllocationAlgorithms.Question3.Core
{
    internal sealed class WorstFitAllocator : AllocatorBase
    {
        public WorstFitAllocator(int totalMemory) : base(totalMemory) { }

        public override int Allocate(int size)
        {
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
}
