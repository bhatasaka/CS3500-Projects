﻿using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network_Support
{
    /// <summary>
    /// This class holds all the necessary state to represent a socket connection
    /// Note that all of its fields are public because we are using it like a "struct"
    /// It is a simple collection of fields
    /// </summary>
    public class SocketState
    {
        public Socket theSocket;
        public int ID;
        public Networking.CallMe callMe;

        // This is the buffer where we will receive data from the socket
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConnectionState
    {
        public TcpListener listener;

        public Networking.CallMe callMe;


        public ConnectionState(TcpListener listen)
        {
            listener = listen;
        }
    }

    public static class Networking
    {
        public const int DEFAULT_PORT = 11000;
        public const int HTTP_PORT = 80;
        public delegate void CallMe(SocketState ss);


        /// <summary>
        /// Creates a Socket object for the given host string
        /// </summary>
        /// <param name="hostName">The host name or IP address</param>
        /// <param name="socket">The created Socket</param>
        /// <param name="ipAddress">The created IPAddress</param>
        public static void MakeSocket(string hostName, out Socket socket, out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None;
            socket = null;
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        throw new ArgumentException("Invalid address");
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // Disable Nagle's algorithm - can speed things up for tiny messages, 
                // such as for a game
                socket.NoDelay = true;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create socket. Error occured: " + e);
                throw new ArgumentException("Invalid address");
            }
        }

        /// <summary>
        /// callbackFunction - a function to be called when a connection is made.
        /// Your SpaceWars client will provide this function when it invokes ConnectedToServer.

        /// ConnectedToServer should attempt to connect to the server via a provided hostname. 
        /// It should save the callback function in a socket state object for use when data arrives.

        /// It will need to create a socket and then use the BeginConnect method.
        /// Note this method takes the "state" object and 
        /// "regurgitates" it back to you when a connection is made, thus allowing communication between this function and the ConnectedToServer function (below).

        /// </summary>
        /// <param name="callbackFunction">a function to be called when a connection is made</param>
        /// <param name="hostname">the name of the server to connect to</param>
        /// <returns></returns>
        public static SocketState ConnectToServer(CallMe callMe, string hostName)
        {
            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);

            // Create a TCP/IP socket.
            Socket socket;
            IPAddress ipAddress;

            Networking.MakeSocket(hostName, out socket, out ipAddress);
            SocketState state = new SocketState(socket, -1);

            state.callMe = callMe;

            IAsyncResult result = state.theSocket.BeginConnect(ipAddress, Networking.DEFAULT_PORT, ConnectedCallback, state);
            bool success = result.AsyncWaitHandle.WaitOne(3000, true);

            if (!socket.Connected)
            {
                socket.Close();
                throw new ApplicationException("Could not connect to server.");
            }
            return state;
        }


        /// <summary>
        /// This function is referenced by the BeginConnect method above and is called by the OS when the socket connects to the server.
        /// The "stateAsArObject" object contains a field "AsyncState" which contains the "state" object saved away in the above function.
        /// You will have to cast it from object to SocketState.

        /// Once a connection is established the "saved away" callbackFunction needs to called. 
        /// This function is saved in the socket state, and was originally passed in to ConnectToServer.
        /// </summary>
        /// <param name="stateAsArObject">contains the "state" object saved away in the above function</param>
        public static void ConnectedCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }

            // Calls the callMe delegate contained in the socket state object
            ss.callMe(ss);
        }

        /// <summary>
        /// This is a small helper function that the client code will call whenever it wants more data.
        /// Note: the client will probably want more data every time it gets data, and has finished processing it in its callbackFunction.
        /// </summary>
        /// <param name="state"></param>
        public static void GetData(SocketState state)
        {
            state.theSocket.BeginReceive(state.messageBuffer, 0, state.messageBuffer.Length,
                SocketFlags.None, ReceiveCallback, state);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                SocketState ss = (SocketState)ar.AsyncState;

                int bytesRead = ss.theSocket.EndReceive(ar);

                // If the socket is still open
                if (bytesRead > 0)
                {
                    string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                    // Append the received data to the growable buffer.
                    // It may be an incomplete message, so we need to start building it up piece by piece
                    ss.sb.Append(theMessage);

                    ss.callMe(ss);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket.
        /// This function needs to convert the data into bytes and then send them using socket.BeginSend.
        /// Returns true if the socket is still good, else false.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static bool Send(Socket socket, String data)
        {
            try
            {
                byte[] message = Encoding.UTF8.GetBytes(data);
                socket.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, socket);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            try
            {
                socket.EndSend(ar);
            }
            catch (Exception) { }
        }


        public static void ServerAwaitingClientLoop(Networking.CallMe callme) {
            TcpListener lstn = new TcpListener(IPAddress.Any, Networking.DEFAULT_PORT);

            lstn.Start();
            ConnectionState cs = new ConnectionState(lstn);
            cs.callMe = callme;
            lstn.BeginAcceptSocket(AcceptNewClient, cs);
        }


        public static void AcceptNewClient(IAsyncResult ar)
        {
            ConnectionState cs = (ConnectionState)ar.AsyncState;
            Socket socket = cs.listener.EndAcceptSocket(ar);
            SocketState ss = new SocketState(socket, -1);
            ss.callMe = cs.callMe;
            ss.callMe(ss);
            cs.listener.BeginAcceptSocket(AcceptNewClient, cs);
        }
















        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************






        public static void ServerAwaitingClientLoop(Networking.CallMe callme, int port)
        {
            TcpListener lstn = new TcpListener(IPAddress.Any, port);

            lstn.Start();
            ConnectionState cs = new ConnectionState(lstn);
            cs.callMe = callme;
            lstn.BeginAcceptSocket(AcceptNewClient, cs);
        }
        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket.
        /// This function needs to convert the data into bytes and then send them using socket.BeginSend.

        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void SendAndClose(Socket socket, String data)
        {
            try
            {
                byte[] message = Encoding.UTF8.GetBytes(data);
                socket.BeginSend(message, 0, message.Length, SocketFlags.None, CloseAfterSending, socket);


            }
            catch (Exception)
            {
                return;
            }


        }

        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        private static void CloseAfterSending(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            //\socket.EndSend(ar);
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            socket.Dispose();
        }















    }


}
