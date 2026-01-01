using System;

namespace AllocationAlgorithms.Bitmap
{
 
    public class BitmapAllocator
    {
        private int[] bitmap;

        public BitmapAllocator(int numberOfBlocks)
        {
            bitmap = new int[numberOfBlocks];
        }

        public int Allocate(int size)
        {
            int count = 0;
            int startIndex = -1;

            for (int i = 0; i < bitmap.Length; i++)
            {
                if (bitmap[i] == 0)
                {
                    if (count == 0)
                        startIndex = i;

                    count++;

                    if (count == size)
                    {
                        for (int j = startIndex; j < startIndex + size; j++)
                            bitmap[j] = 1;

                        return startIndex;
                    }
                }
                else
                {
                    count = 0;
                }
            }

            return -1;
        }
                public void Free(int startIndex, int size)
        {
            for (int i = startIndex; i < startIndex + size; i++)
                bitmap[i] = 0;
        }

        
        public string GetBitmap()
        {
            return string.Join(" ", bitmap);
        }
    }
}
