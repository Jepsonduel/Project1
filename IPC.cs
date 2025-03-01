using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace IPC
{
    internal class Server
    {
        /*
         This method creates a NamedPipeServerStream object which waits for a client pipe to connect in order to write to it using user entered text.
        Source: 
        dotnet-bot, “NamedPipeServerStream Class (System.IO.Pipes),” Microsoft.com, 2025. https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream?view=net-9.0
         */
        public static void serverPipe()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.Out)) //Sever pipe created
            {
                Console.WriteLine("Connecting to client");
                pipeServer.WaitForConnection();
                Console.WriteLine("Connection obtained");
                try
                {
                    using (StreamWriter sw = new StreamWriter(pipeServer))
                    {
                        sw.AutoFlush = true;
                        Console.Write("Text: ");
                        sw.WriteLine(Console.ReadLine());
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /*
         This method creates a NamedPipeClientStream object which waits for a client pipe to connect in order to write to it using user entered text.
        Source: 
        dotnet-bot, “NamedPipeClientStream Class (System.IO.Pipes),” Microsoft.com, 2025. https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeclientstream?view=net-9.0 (accessed Mar. 01, 2025).
         */
        public static void clientPipe()
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.In))
            {
                Console.WriteLine("Connecting to pipe");
                pipeClient.Connect();
                Console.WriteLine("Connected");
                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    string read;
                    while ((read = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("Recieved: " + read);
                    }
                }
            }
            Console.WriteLine("Process complete");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Thread thread1 = new Thread(new ThreadStart(serverPipe));
            Thread thread2 = new Thread(new ThreadStart(clientPipe)); // threads initialized
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

        }
    }
}
