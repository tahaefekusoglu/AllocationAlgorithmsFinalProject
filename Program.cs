using System;

internal class Program
{
    private static void Main()
    {
        while (true)
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("ALLOCATION ALGORITHMS - RUN MENU");
            Console.WriteLine("====================================================");
            Console.WriteLine("1) Question 2 - Bitmap vs LinkedList (AI vs Rewritten) Experiments");
            Console.WriteLine("2) Question 3 - Part B (Implementation Demo)");
            Console.WriteLine("3) Question 3 - Part C (Experiment 1: Allocation Trace)");
            Console.WriteLine("4) Question 3 - Part D (Experiment 2: Fragmentation Test)");
            Console.WriteLine("5) Question 3 - Part E (Experiment 3: Speed Test)");
            Console.WriteLine("0) Exit");
            Console.WriteLine("----------------------------------------------------");
            Console.Write("Select: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        Question2Runner.Run(); // namespace yok -> direkt çağırılır
                        break;

                    case "2":
                        AllocationAlgorithms.Question3.Runners.PartB.Program.Run();
                        break;

                    case "3":
                        AllocationAlgorithms.Question3.Runners.PartC.Program.Run();
                        break;

                    case "4":
                        AllocationAlgorithms.Question3.Runners.PartDRunner.Run();
                        break;

                    case "5":
                        AllocationAlgorithms.Question3.Runners.PartERunner.Run();
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please select 0-5.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR while running selected part:");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("====================================================");
            Console.WriteLine("Done. Press ENTER to go back to menu...");
            Console.WriteLine("====================================================");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
