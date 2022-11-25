using Game.Network.Data;
using Game.Network.Enums;
using Game.Network.Helpers;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Bases
{
    public class BaseClient
    {
        private static ILog log = LogManager.GetLogger(typeof(BaseClient));

        /// <summary>
        /// Client socket
        /// </summary>
        private Socket _socket;
        public Socket Socket => _socket;

        /// <summary>
        /// Server socket
        /// </summary>
        private BaseServer _server;

        /// <summary>
        /// Client identifier
        /// </summary>
        private string _guid;
        public string Guid => _guid;

        public BaseClient(BaseServer server, Socket socket)
        {
            this._server = server;
            this._socket = socket;
            this._guid = RFC6455Helper.CreateGuid("game", 16);
            this._socket.BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, new AsyncCallback(OnMessageCallback), this._socket);
        }

        /// <summary>
        /// Message return control
        /// </summary>
        /// <param name="asyncResult"></param>
        public virtual void OnMessageCallback(IAsyncResult asyncResult)
        {
            byte[] buffer = new byte[NetworkProperties.DATA_BUFFFER];

            try
            {
                SocketError socketError;

                int receivedBytes = (this._socket.EndReceive(asyncResult, out socketError));
                if (socketError != SocketError.Success)
                    throw new SocketException(Convert.ToInt32(socketError));

                int bytesOffset = this._socket.Receive(buffer);

                if (bytesOffset < buffer.Length)
                    Array.Resize<Byte>(ref buffer, bytesOffset);

                EOpcodeType opcodeType = (RFC6455Helper.GetFrameOpcode(buffer));
                switch (opcodeType)
                {
                    case EOpcodeType.ClosedConnection:
                        this.Disconnect();
                        return;
                    case EOpcodeType.Ping:
                        this.OnReceivePing();
                        break;
                }
            }
            catch (SocketException ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat($"{ex.Message}:{ex.StackTrace}:{ex.SocketErrorCode}:{ex.HelpLink}");
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat($"{ex.Message}:{ex.StackTrace}");
            }

            this.OnReceiveMessage(buffer);
        }

        /// <summary>
        /// Reads the received message on return
        /// </summary>
        /// <param name="data">Message data to be read></param>
        public virtual void OnReceiveMessage(byte[] data)
        {
            var frameData = RFC6455Helper.GetDataFromFrame(data);
            this._server.OnMessageReceived(this, frameData);
            this._socket.BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, new AsyncCallback(this.OnMessageCallback), this._socket);
        }

        /// <summary>
        /// When server receive ping, he need send pong to the client (30 seconds+) (Handshake)
        /// </summary>
        public virtual void OnReceivePing()
        {
            var makePong = JsonConvert.SerializeObject(new { Type = "pong" });
            this.SendData(RFC6455Helper.GetFrameFromString(JsonConvert.SerializeObject(makePong), EOpcodeType.Pong));
        }

        /// <summary>
        /// Collects connection address of the client connected to the socket
        /// </summary>
        /// <returns></returns>
        public string GetConnectionAddress()
        {
            if (!this._socket.Connected)
                return "0.0.0.0";

            return this._socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// Send data to the client connected to this socket
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void SendData(byte[] data)
        {
            if (this._socket.Connected)
                this._socket.Send(RFC6455Helper.GetFrameFromString(Encoding.UTF8.GetString(data)));
        }

        /// <summary>
        /// Send data text to the client connected to this socket
        /// </summary>
        /// <param name="dataText">Data to be sent</param>
        public void SendData(string dataText)
        {
            if(this._socket.Connected)
                this._socket.Send(RFC6455Helper.GetFrameFromString(dataText));
        }

        /// <summary>
        /// Disconnect current socket
        /// </summary>
        public void Disconnect()
        {
            if ((this._socket != null) && (this._socket.Connected))
            {
                try
                {
                    this._socket.Shutdown(SocketShutdown.Both);
                    this._socket.Close();
                    this._server.OnClientDisconnected(this);
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                        log.ErrorFormat($"{ex.Message}:{ex.StackTrace}");
                }
            }
        }
    }
}
