using System;

namespace AllocationAlgorithms.Bitmap
{
    /*
     * This file represents the rewritten version of the bitmap allocation algorithm.
     * The initial implementation was generated using an AI tool,
     * but I rewrote and commented the code to better understand
     * how bitmap-based allocation works internally.
     *
     * I focused on making variable names clearer and
     * explaining the logic in my own words.
     *
     */

    public class BitmapAllocatorRewritten
    {
        // In the AI version, this variable name was shorter.
        // I renamed it to 'diskBlocks' to clearly represent
        // that each element corresponds to a block on the disk.
        private int[] diskBlocks;

       
        public BitmapAllocatorRewritten(int totalBlocks)
        {
            diskBlocks = new int[totalBlocks];
        }

        /*
         * This method implements the core logic of bitmap allocation.
         * The original AI version already worked correctly,
         * but I rewrote it to improve readability and understanding.
         *
         * The goal here is to find the first group of
         * consecutive free blocks (first-fit strategy)
         * and mark them as allocated.
         */
        public int AllocateBlocks(int requiredBlocks)
        {
           
            int freeBlockCount = 0;

            
            int startPosition = -1;

           
            for (int i = 0; i < diskBlocks.Length; i++)
            {
               
                if (diskBlocks[i] == 0)
                {
                   
                    if (freeBlockCount == 0)
                        startPosition = i;

                    freeBlockCount++;

                   
                    if (freeBlockCount == requiredBlocks)
                    {
                      
                        for (int j = startPosition; j < startPosition + requiredBlocks; j++)
                            diskBlocks[j] = 1;

                       
                        return startPosition;
                    }
                }
                else
                {
                    
                    freeBlockCount = 0;
                }
            }

            
            return -1;
        }

        /*
         * I kept this method simple to focus on
         * the main idea of deallocation.
         */
        public void FreeBlocks(int startIndex, int blockCount)
        {
            for (int i = startIndex; i < startIndex + blockCount; i++)
                diskBlocks[i] = 0;
        }

        
        public string GetDiskState()
        {
            return string.Join(" ", diskBlocks);
        }
    }
}
