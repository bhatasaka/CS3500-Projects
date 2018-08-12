using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network_Support;

namespace SpaceWars
{
    public class WebServer
    {    // The server must follow the HTTP protocol when replying to a client (web browser).
         // That involves sending an HTTP header before any web content.

        // This is the header to use when the server recognizes the browser's request
        private const string httpOkHeader =
          "HTTP/1.1 200 OK\r\n" +
          "Connection: close\r\n" +
          "Content-Type: text/html; charset=UTF-8\r\n" +
          "\r\n";

        // This is the header to use when the server does not recognize the browser's request
        private const string httpBadHeader =
          "HTTP/1.1 404 Not Found\r\n" +
          "Connection: close\r\n" +
          "Content-Type: text/html; charset=UTF-8\r\n" +
          "\r\n";


        public static void Main(string[] args)
        {
            // Start an event loop that listens for socket connections on port 80
            // This requires a slight modification to the Networking_Support_Support library to take the port argument
            Networking.ServerAwaitingClientLoop(HandleHttpConnection, Networking.HTTP_PORT);

            Console.Read();
        }

        /// <summary>
        /// This is the delegate for when a new socket is accepted
        /// The Networking_Support library will invoke this method when a browser connects
        /// </summary>
        /// <param name="state"></param>
        public static void HandleHttpConnection(SocketState state)
        {
            // Before receiving data from the browser, we need to change what we do when network activity occurs.
            state.callMe = ServeHttpRequest;

            Networking.GetData(state);
        }


        /// <summary>
        /// This method parses the HTTP request sent by the broswer, and serves the appropriate web page.
        /// </summary>
        /// <param name="state">The socket state representing the connection with the client</param>
        private static void ServeHttpRequest(SocketState state)
        {
            string request = state.sb.ToString();

            // Print it for debugging/examining
            Console.WriteLine("received http request: " + request);


            // If the browser requested the home page (e.g., localhost/)
            if (request.Contains("GET / HTTP/1.1"))
                Networking.SendAndClose(state.theSocket, httpOkHeader + "<h2>here is a web page!</h2>");
            // Otherwise, our very simple web server doesn't recognize any other URL
            else
                Networking.SendAndClose(state.theSocket, httpBadHeader + "<h2>page not found</h2>");
        }

        // NOTE: The above SendAndClose calls are an addition to the Networking_Support_Support library.
        //       The only difference from the basic Send method is that this method uses a callback
        //       that closes the socket after ending the send.
    }
}
