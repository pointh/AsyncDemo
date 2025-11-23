namespace ContinueAndWait
{
    public class ContinueAndWait
    {
        public static void Main()
        {
            // Program starts
            Console.WriteLine("Main starts ...");
            // Start a new Task to execute the specified method asynchronously
            Task task = Task.Run(() => SomeMethod());

            // Do other work while the asynchronous operation is in progress
            Console.WriteLine("Main thread continues...");

            // Wait for the asynchronous operation to complete
            task.Wait();

            // Now it is done.
            Console.WriteLine("Program completed");
        }

        public static void SomeMethod()
        {
            // Simulate some time-consuming operation
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(800);
                Console.Write(".");
            }
            Console.WriteLine("SomeMethod completed.");
        }
    }
}
