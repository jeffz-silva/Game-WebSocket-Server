using Game.Network.Bases;
using Game.Network.Helpers;
using Game.Server.Network;
using Game.Server.Packets.Server;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public class GameClient : BaseClient
    {
        private static ILog log = LogManager.GetLogger(typeof(GameClient));

        private BaseServer _currentServer;
        private Socket _currentSocket;
        public GameSocketOut Out;

        private PacketProcessor _processor;

        /// <summary>
        /// Wallet connected to this client on the server
        /// </summary>
        private string _connectedWallet;
        public string ConnectedWallet => _connectedWallet;

        public GameClient(BaseServer server, Socket socket) : base(server, socket)
        {
            this._currentServer = server;
            this._currentSocket = socket;

            if (this.Out == null)
                this.Out = new GameSocketOut(this);

            this._connectedWallet = "";
        }

        /// <summary>
        /// Updates the wallet connected to that client on the server
        /// </summary>
        /// <param name="wallet">Connected wallet</param>
        public void SetConnectedWallet(string wallet)
        {
            this._connectedWallet = wallet;
        }

        /// <summary>
        /// Reads the received message on return
        /// </summary>
        /// <param name="data">Message data to be read></param>
        public override void OnReceiveMessage(byte[] data)
        {
            this.HandleData(data);
        }

        /// <summary>
        /// Processes data sent to the client, converts it into a json object and redirects it to the final processor
        /// </summary>
        /// <param name="data"></param>
        private void HandleData(byte[] data)
        {
            var frameData = RFC6455Helper.GetDataFromFrame(data);
            try
            {
                if (this._processor == null)
                    this._processor = new PacketProcessor(this);

                this._processor.HandlePacket(JObject.Parse(frameData));
            }
            catch(JsonException ex) { }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                    log.ErrorFormat($"{ex.Message}:{ex.StackTrace}");
            }
            this._currentServer.OnMessageReceived(this, frameData);
            this._currentSocket.BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, new AsyncCallback(this.OnMessageCallback), this._currentSocket);
        }
    }
}
