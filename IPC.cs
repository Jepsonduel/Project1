using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPC
{
    internal class Server
    {
        public static void serverPipe()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.Out))
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
            Thread thread2 = new Thread(new ThreadStart(clientPipe));
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

        }
    }
}