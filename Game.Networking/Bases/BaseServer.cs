using Game.Network.Data;
using Game.Network.Events;
using Game.Network.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Bases
{
    public partial class BaseServer
    {
        #region Server Variables
        private bool _isSocketConnected = false;
        #endregion

        /// <summary>
        /// Get new logger with BaseServer type
        /// </summary>
        private static ILog log = LogManager.GetLogger(typeof(BaseServer));

        /// <summary>
        /// Server connection socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Connection endpoint (address)
        /// </summary>
        private IPEndPoint _endPoint;

        /// <summary>
        /// Clients inside the server
        /// </summary>
        public BaseClient[] _clients = new BaseClient[NetworkProperties.MAX_PLAYERS];

        /// <summary>
        /// Current class instance
        /// </summary>
        public static BaseServer Instance;

        /// <summary>
        /// Create new instance of current class
        /// </summary>
        /// <returns></returns>
        public static BaseServer CreateInstance()
        {
            if (Instance == null)
                Instance = new BaseServer();

            return Instance;
        }

        /// <summary>
        /// Create new socket
        /// </summary>
        /// <param name="endPoint">Server address to receive new connections</param>
        public virtual bool CreateSocket(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                log.Info("Socket can't be created without endpoint connection!");
                return false;
            }

            this._endPoint = endPoint;
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return true;
        }

        /// <summary>
        /// Starts new socket connection on network
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            if (this._isSocketConnected)
            {
                log.WarnFormat($"[{this.GetType()}] Socket is already connected on {this._endPoint.Address}:{this._endPoint.Port}!");
                return this._isSocketConnected;
            }

            try
            {
                this._socket.Bind(this._endPoint);
                this._socket.Listen(NetworkProperties.SOCKET_BACK_LOG);
                this._socket.BeginAccept(new AsyncCallback(this.OnConnectionCallback), this._socket);
                this._isSocketConnected = true;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat($"[{this.GetType()}][Start] {ex.Message}:{ex.StackTrace}");
            }
            return this._isSocketConnected;
        }

        /// <summary>
        /// Stop current socket server connection oppened
        /// </summary>
        /// <returns></returns>
        public virtual bool Stop()
        {
            if (!this._isSocketConnected)
            {
                log.WarnFormat($"[{this.GetType()}] Socket is not connected!");
                return this._isSocketConnected;
            }

            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
                this._socket.Close();
                this._socket.Dispose();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled || log.IsFatalEnabled)
                    log.ErrorFormat($"[{this.GetType()}][Stop] {ex.Message}:{ex.StackTrace}");
            }

            if (!this._socket.Connected)
                this._isSocketConnected = false;

            return this._isSocketConnected;
        }

        /// <summary>
        /// Handle connection data callback
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnConnectionCallback(IAsyncResult asyncResult)
        {
            Socket clientSocket = null;
            try
            {
                //Collect user client trying connection on server
                clientSocket = this._socket.EndAccept(asyncResult);

                //Learn about client handshake with server-side
                byte[] handshakeBuffer = new byte[NetworkProperties.HANDSHAKE_BUFFER];
                int receivedHandshake = clientSocket.Receive(handshakeBuffer);

                //Get handshake security websocket key
                string handshakeKey = RFC6455Helper.GetHandshakeQuestKey(Encoding.Default.GetString(handshakeBuffer));
                clientSocket.Send(RFC6455Helper.GetHandshakeResponse(handshakeKey));

                var gameClient = this.CreateClient(socket: clientSocket);
                if (gameClient == null)
                    throw new Exception($"The server can't receive new connection why is full, MaxPlayers: {NetworkProperties.MAX_PLAYERS}");

                this.OnClientConnected(gameClient);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10054 && log.IsWarnEnabled)
                {
                    log.WarnFormat("[SocketException] Microsoft not writed about this error code! [10054]");
                }
                else if (log.IsErrorEnabled)
                {
                    log.ErrorFormat($"[{this.GetType()}] {ex.Message}:{ex.StackTrace}");
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat($"[{this.GetType()}] {ex.Message}:{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled && !ex.Message.Contains("Socket not connected receiving data"))
                    log.ErrorFormat($"[{this.GetType()}][{ex.Message}][{ex.StackTrace}]");
            }

            this._socket.BeginAccept(new AsyncCallback(this.OnConnectionCallback), this._socket);
        }

        /// <summary>
        /// Close connection with one client socket
        /// </summary>
        /// <param name="socket"></param>
        public void CloseSocket(Socket socket)
        {
            if (socket == null)
                return;

            if (!socket.Connected)
                return;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat("Process of socket shutdown is not completed!");
            }
        }

        /// <summary>
        /// Create new connected client on server
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual BaseClient CreateClient(Socket socket)
        {
            var client = new BaseClient(this, socket);
            BaseClient clientOffset = null;
            for (int i = 0; i < this._clients.Length; i++)
            {
                if (this._clients[i] == null)
                {
                    this._clients[i] = client;
                    clientOffset = (this._clients[i]);
                    break;
                }
            }
            return clientOffset;
        }

        /// <summary>
        /// Check if the server has a specific client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool HaveClient(BaseClient client)
        {
            return (this.GetClient(client) != null);
        }

        /// <summary>
        /// Collect a specific client on server
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private BaseClient GetClient(BaseClient client)
        {
            for (int i = 0; i < this._clients.Length; i++)
            {
                if (this._clients[i] == client)
                    return this._clients[i];
            }
            return (BaseClient)null;
        }

        /// <summary>
        /// Remove a specific client from server
        /// </summary>
        /// <param name="client"></param>
        public virtual void RemoveClient(BaseClient client)
        {
            if (client == null)
                return;

            for (int i = 0; i < this._clients.Length; i++)
            {
                if (this._clients[i] == client)
                    this._clients[i] = null;
            }
        }

        /// <summary>
        /// Send information to a client
        /// </summary>
        /// <param name="target">Client the server information addresses</param>
        /// <param name="data">Information to be carried</param>
        public void SendData(BaseClient target, byte[] data)
        {
            if (target == null) return;
            if (!target.Socket.Connected) return;

            target.Socket.Send(data);
        }

        /// <summary>
        /// Send data to all connected clients
        /// </summary>
        /// <param name="data"></param>
        public void SendDataToAll(byte[] data)
        {
            for(int i = 0; i < this._clients.Length; i++)
            {
                if (this._clients[i] == null)
                    continue;

                if (this._clients[i].Socket.Connected)
                    this._clients[i].Socket.Send(data);
            }
        }

        #region Events
        /// <summary>
        /// Fires when a new client connects to the server
        /// </summary>
        /// <param name="connectedClient">Client that connected to the server</param>
        public void OnClientConnected(BaseClient connectedClient)
        {
            if (this.OnClientConnectedEvent == null) return;
            this.OnClientConnectedEvent(this, new OnClientConnectedHandler(connectedClient));
        }

        /// <summary>
        /// Fires when a client disconnects from the server
        /// </summary>
        /// <param name="disconnectedClient">Client disconnected from server</param>
        public void OnClientDisconnected(BaseClient disconnectedClient)
        {
            if (this.OnClientDisconnectedEvent == null) return;
            this.OnClientDisconnectedEvent(this, new OnClientDisconnectedHandler(disconnectedClient));
        }

        /// <summary>
        /// New message received by the server
        /// </summary>
        /// <param name="sourceClient">Client who sent the message to the server</param>
        /// <param name="message">Message sent</param>
        public void OnMessageReceived(BaseClient sourceClient, string message)
        {
            if (this.OnMessageReceivedEvent == null) return;
            this.OnMessageReceivedEvent(this, new OnMessageReceivedHandler(sourceClient, message));
        }

        public event EventHandler<OnClientConnectedHandler> OnClientConnectedEvent;
        public event EventHandler<OnClientDisconnectedHandler> OnClientDisconnectedEvent;
        public event EventHandler<OnMessageReceivedHandler> OnMessageReceivedEvent;
        #endregion
    }
}
