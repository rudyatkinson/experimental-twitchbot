using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Granzwelt.TwitchBot.Unity
{
    public class TestClient
    {
        NamedPipeClientStream pipeClient;
        Stream stream;
        UnicodeEncoding encoding;

        public string connectionName { get; private set; }

        public TestClient(string connectionName)
        {
            this.connectionName = connectionName;
        }

        public void Connect()
        {
            pipeClient = new NamedPipeClientStream(".", connectionName, PipeDirection.InOut, PipeOptions.Asynchronous);

            pipeClient.Connect();

            if(pipeClient.IsConnected)
            {
                stream = pipeClient;
                encoding = new UnicodeEncoding();
            }
        }

        public bool IsConnected()
        {
            if(pipeClient.IsConnected)
            {
                Console.WriteLine("pipeClient bağlandı.");
                Console.WriteLine(ReadString());
            }
            return pipeClient.IsConnected;
        }

        public string ReadString()
        {
            using(StreamReader reader = new StreamReader(pipeClient))
            {
                return reader.ReadLine();
            }
        }
    }
}