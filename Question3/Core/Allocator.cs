namespace AllocationAlgorithms.Question3.Core
{
    internal interface IAllocator
    {
        int Allocate(int size);
        void Free(int start, int size);
        string FreeListString();
    }
}
