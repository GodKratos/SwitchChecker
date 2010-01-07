using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;

namespace SwitchChecker
{
    /// <summary>
    /// Class used to connect and Send/Recive data with a telnet connection.
    /// </summary>
    public class TelnetConnection
    {
        private IPEndPoint iep;
        //private AsyncCallback callbackProc;
        private string address;
        public string PROMPT = "#";
        private int port;
        private int timeout;
        private Socket s;
        private Byte[] m_byBuff = new Byte[32767];
        private string strWorkingData = "";					// Holds everything received from the server since our last processing
        private string strFullLog = "";
        private string NEW_LINE = "\r\n";

        /// <summary>
        /// Creates the Telnet connection to use
        /// </summary>
        /// <param name="Address">Host name or IP to connect to</param>
        /// <param name="Port">Port number to connect with</param>
        /// <param name="CommandTimeout">Command timeout in seconds</param>
        public TelnetConnection(string Host, int Port)
        {
            address = Host;
            port = Port;
            timeout = Properties.Settings.Default.Timeout;
        }


        private void OnRecievedData(IAsyncResult ar)
        {
            // Get The connection socket from the callback
            Socket sock = (Socket)ar.AsyncState;
            if (!sock.Connected) return;

            // Get The data , if any
            int nBytesRec = sock.EndReceive(ar);

            if (nBytesRec > 0)
            {
                // Decode the received data
                //string sRecieved = CleanDisplay(Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec));
                string sRecieved = Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec);

                // Write out the data
                //if (sRecieved.IndexOf("[c") != -1) Negotiate(1);
                //if (sRecieved.IndexOf("[6n") != -1) Negotiate(2);

                Console.WriteLine(sRecieved);
                strWorkingData += sRecieved;
                strFullLog += sRecieved;

                // Launch another callback to listen for data
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                if (!sock.Connected) return;
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);

            }
            else
            {
                // If no data was recieved then the connection is probably dead
                Console.WriteLine("Disconnected", sock.RemoteEndPoint);
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
                //Application.Exit();
            }
        }

        private void DoSend(string strText)
        {
            try
            {
                Byte[] smk = new Byte[strText.Length];
                for (int i = 0; i < strText.Length; i++)
                {
                    Byte ss = Convert.ToByte(strText[i]);
                    smk[i] = ss;
                }

                s.Send(smk, 0, smk.Length, SocketFlags.None);
            }
            catch (Exception ers)
            {
                //MessageBox.Show("ERROR IN RESPOND OPTIONS");
            }
        }

        private void DoSend(byte[] bytesToSend)
        {
            try
            {
                s.Send(bytesToSend, 0, bytesToSend.Length, SocketFlags.None);
            }
            catch (Exception ers)
            {
                //MessageBox.Show("ERROR IN RESPOND OPTIONS");
            }
        }

        /*
        private void Negotiate(int WhichPart)
        {
            StringBuilder x;
            string neg;
            if (WhichPart == 1)
            {
                x = new StringBuilder();
                x.Append((char)27);
                x.Append((char)91);
                x.Append((char)63);
                x.Append((char)49);
                x.Append((char)59);
                x.Append((char)50);
                x.Append((char)99);
                neg = x.ToString();
            }
            else
            {

                x = new StringBuilder();
                x.Append((char)27);
                x.Append((char)91);
                x.Append((char)50);
                x.Append((char)52);
                x.Append((char)59);
                x.Append((char)56);
                x.Append((char)48);
                x.Append((char)82);
                neg = x.ToString();
            }
            SendMessage(neg, true);
        }
        */

        /*
        private string CleanDisplay(string input)
        {

            input = input.Replace("(0x (B", "|");
            input = input.Replace("(0 x(B", "|");
            input = input.Replace(")0=>", "");
            input = input.Replace("[0m>", "");
            input = input.Replace("7[7m", "[");
            input = input.Replace("[0m*8[7m", "]");
            input = input.Replace("[0m", "");
            return input;
        }
        */



        /// <summary>
        /// Connects to the telnet server.
        /// </summary>
        /// <returns>True upon connection, False if connection fails</returns>
        public Exception Connect()
        {
            IPAddress[] addr = null;

            try
            {
                addr = Dns.GetHostAddresses(address);
            }
            catch (Exception exc)
            {
                return exc;
            }

            try
            {
                // Try a blocking connection to the server
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                iep = new IPEndPoint(addr[0], port);
                s.Connect(iep);

                // If the connect worked, setup a callback to start listening for incoming data
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                s.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, s);

                // All is good
                return null;
            }
            catch (Exception ers)
            {
                // Something failed
                return ers;
            }

        }


        public void Disconnect()
        {
            if (s != null && s.Connected) s.Close();
        }

        /// <summary>
        /// Waits for a specific string to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">The string to wait for</param>
        /// <param name="clean">True if you want the working data cleared.</param>
        /// <returns>Always returns 0 once the string has been found</returns>
        public bool WaitFor(string DataToWaitFor, bool clear)
        {
            // Get the starting time
            long lngStart = DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;

            while (strWorkingData.ToLower().IndexOf(DataToWaitFor.ToLower()) == -1)
            {
                // Timeout logic
                lngCurTime = DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    return false;
                }
                Thread.Sleep(100);
            }
            if (clear) strWorkingData = "";
            return true;
        }

        /// <summary>
        /// Waits for a specific string to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">The string to wait for</param>
        /// <param name="clean">True if you want the working data cleared.</param>
        /// <returns>Always returns 0 once the string has been found</returns>
        public bool WaitFor(string DataToWaitFor)
        {
            return WaitFor(DataToWaitFor, true);
        }


        /// <summary>
        /// Waits for a specific string to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">The byte array to wait for</param>
        /// <returns>Always returns 0 once the string has been found</returns>
        public bool WaitFor(byte[] DataToWaitFor)
        {
            // Get the starting time
            long lngStart = DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;

            while (strWorkingData.IndexOf(Encoding.ASCII.GetString(DataToWaitFor)) == -1)
            {
                // Timeout logic
                lngCurTime = DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    return false;
                }
                Thread.Sleep(100);
            }
            return true;
        }


        /// <summary>
        /// Waits for one of several possible strings to be found in the stream from the server
        /// </summary>
        /// <param name="DataToWaitFor">A delimited list of strings to wait for</param>
        /// <param name="BreakCharacters">The character to break the delimited string with</param>
        /// <returns>The index (zero based) of the value in the delimited list which was matched</returns>
        public int WaitFor(string DataToWaitFor, string BreakCharacter)
        {
            // Get the starting time
            long lngStart = DateTime.Now.AddSeconds(this.timeout).Ticks;
            long lngCurTime = 0;

            string[] Breaks = DataToWaitFor.Split(BreakCharacter.ToCharArray());
            int intReturn = -1;

            while (intReturn == -1)
            {
                // Timeout logic
                lngCurTime = DateTime.Now.Ticks;
                if (lngCurTime > lngStart)
                {
                    throw new Exception("Timed Out waiting for : " + DataToWaitFor);
                }

                Thread.Sleep(100);
                for (int i = 0; i < Breaks.Length; i++)
                {
                    if (strWorkingData.ToLower().IndexOf(Breaks[i].ToLower()) != -1)
                    {
                        intReturn = i;
                    }
                }
            }
            strWorkingData = "";
            return intReturn;

        }


        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="Message">The message to send to the server</param>
        /// <param name="SuppressCarriageReturn">True if you do not want to end the message with a carriage return</param>
        public void SendMessage(string Message, bool SuppressCarriageReturn)
        {
            //strFullLog += NEW_LINE + "SENDING DATA ====> " + Message + NEW_LINE;

            if (!SuppressCarriageReturn)
            {
                DoSend(Message + NEW_LINE);
            }
            else
            {
                DoSend(Message);
            }
        }


        /// <summary>
        /// Sends a message to the server, automatically appending a carriage return to it
        /// </summary>
        /// <param name="Message">The message to send to the server</param>
        public void SendMessage(string Message)
        {
            //strFullLog += NEW_LINE + "SENDING DATA ====> " + Message + NEW_LINE;
            DoSend(Message + NEW_LINE);
        }

        /// <summary>
        /// Sends a message to the server, automatically appending a carriage return to it
        /// </summary>
        /// <param name="Message">The byte array message to send to the server</param>
        public void SendMessage(byte[] Message)
        {
            string str = "";
            foreach (byte b in Message)
            {
                str += b.ToString() + " ";
            }
            //strFullLog += NEW_LINE + "SENDING DATA ====> " + str + NEW_LINE;
            DoSend(Message);
        }

        public string SendCommand(string Command)
        {
            Thread.Sleep(10);
            strWorkingData = "";
            SendBreak();
            if (WaitFor(PROMPT))
            {
                SendMessage(Command);
                WaitFor(PROMPT, false);
                return strWorkingData;
            }
            return "";
        }


        /// <summary>
        /// Waits for a specific string to be found in the stream from the server.
        /// Once that string is found, sends a message to the server
        /// </summary>
        /// <param name="WaitFor">The string to be found in the server stream</param>
        /// <param name="Message">The message to send to the server</param>
        /// <returns>Returns true once the string has been found, and the message has been sent</returns>
        public bool WaitAndSend(string WaitFor, string Message)
        {
            if (this.WaitFor(WaitFor))
            {
                SendMessage(Message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Waits for a specific string to be found in the stream from the server.
        /// Once that string is found, sends a message to the server
        /// </summary>
        /// <param name="WaitFor">The byte array to be found in the server stream</param>
        /// <param name="Message">The byte array message to send to the server</param>
        /// <returns>Returns true once the string has been found, and the message has been sent</returns>
        public bool WaitAndSend(byte[] WaitFor, byte[] Message)
        {
            if (this.WaitFor(WaitFor))
            {
                SendMessage(Message);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Sends a message to the server, and waits until the designated
        /// response is received
        /// </summary>
        /// <param name="Message">The message to send to the server</param>
        /// <param name="WaitFor">The response to wait for</param>
        /// <returns>True if the process was successful</returns>
        public bool SendAndWait(string Message, string WaitFor)
        {
            SendMessage(Message);
            return this.WaitFor(WaitFor);
        }

        /// <summary>
        /// Sends a message to the server, and waits until the designated
        /// response is received
        /// </summary>
        /// <param name="Message">The byte array message to send to the server</param>
        /// <param name="WaitFor">The byte array response to wait for</param>
        /// <returns>True if the process was successful</returns>
        public bool SendAndWait(byte[] Message, byte[] WaitFor)
        {
            SendMessage(Message);
            return this.WaitFor(WaitFor);
        }

        public int SendAndWait(string Message, string WaitFor, string BreakCharacter)
        {
            SendMessage(Message);
            return this.WaitFor(WaitFor, BreakCharacter);
        }

        /// <summary>
        /// Sends a new line command to the telnet session.
        /// </summary>
        public void SendBreak()
        {
            DoSend(NEW_LINE);
            //Thread.Sleep(10);
        }


        /// <summary>
        /// A full log of session activity
        /// </summary>
        public string SessionLog
        {
            get
            {
                return strFullLog;
            }
        }


        /// <summary>
        /// Clears all data in the session log
        /// </summary>
        public void ClearSessionLog()
        {
            strFullLog = "";
        }


        /// <summary>
        /// Searches for two strings in the session log, and if both are found, returns
        /// all the data between them.
        /// </summary>
        /// <param name="StartingString">The first string to find</param>
        /// <param name="EndingString">The second string to find</param>
        /// <param name="ReturnIfNotFound">The string to be returned if a match is not found</param>
        /// <returns>All the data between the end of the starting string and the beginning of the end string</returns>
        public string FindStringBetween(string StartingString, string EndingString, string ReturnIfNotFound)
        {
            int intStart;
            int intEnd;

            intStart = strFullLog.ToLower().IndexOf(StartingString.ToLower());
            if (intStart == -1)
            {
                return ReturnIfNotFound;
            }
            intStart += StartingString.Length;

            intEnd = strFullLog.ToLower().IndexOf(EndingString.ToLower(), intStart);

            if (intEnd == -1)
            {
                // The string was not found
                return ReturnIfNotFound;
            }

            // The string was found, let's clean it up and return it
            return strFullLog.Substring(intStart, intEnd - intStart).Trim();
        }


    }
}

