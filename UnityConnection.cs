using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Security.Principal;
using System.Security;

namespace Granzwelt.TwitchBot.Unity
{
    public class UnityConnection
    {
        NamedPipeServerStream pipeServer;
        Stream stream;
        UnicodeEncoding encoding;

        public string connectionName { get; private set; }

        public UnityConnection(string connectionName)
        {
            this.connectionName = connectionName;
        }

        public void CreateServer()
        {
            pipeServer = new NamedPipeServerStream(connectionName, PipeDirection.InOut, 1);
            pipeServer.WaitForConnection();
            stream = pipeServer;
            encoding = new UnicodeEncoding();
        }

        public bool IsConnected()
        {
            if (pipeServer.IsConnected)
            {
                Console.WriteLine("pipeServer kuruldu ve bağlandı.");
                WriteString("Bu bir denemedir.");
            }
            return pipeServer.IsConnected;
        }

        public void DisposeServer()
        {
            if (pipeServer.IsConnected)
            {
                pipeServer.Dispose();
                pipeServer.Disconnect();
            }
        }

        public void WriteString(string outString)
        {
            using(StreamWriter writer = new StreamWriter(pipeServer))
            {
                writer.WriteLine(outString);
            }
            CreateServer();
        }
    }
}