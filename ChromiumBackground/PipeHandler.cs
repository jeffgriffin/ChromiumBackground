using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChromiumBackground
{
    public class PipeHandler
    {
        public PipeHandler()
        {
        }

        public void StartListening(Action<string> messageRecieved)
        {
                Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                using (
                                    var pipe = new NamedPipeClientStream(".", "htmlbackground", PipeDirection.In,
                                                                         PipeOptions.Asynchronous))
                                {
                                    pipe.Connect(500);
                                    if (pipe.IsConnected)
                                    {
                                        using (var streamReader = new StreamReader(pipe))
                                        {
                                            var message = streamReader.ReadToEnd();

                                            if (messageRecieved != null)
                                            {
                                                // invoke the message received action 
                                                messageRecieved(message);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (TimeoutException)
                            {
                            }
                        }
                    }, TaskCreationOptions.LongRunning);
        }
    }
}