using Game.Server.Packets.Client.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Server
{
    public class GameSocketOut : ISocketOut
    {
        /// <summary>
        /// Client connected to socket
        /// </summary>
        protected GameClient _connectedClient;

        public GameSocketOut(GameClient connectedClient)
        {
            _connectedClient = connectedClient;
        }

        /// <summary>
        /// Sends to the client to create a simple alert
        /// </summary>
        /// <param name="message">Message to contain in the alert</param>
        public void SendSimpleAlert(string message)
        {
            this.SendData(new
            {
                Opcode = (int)EPackageType.SimpleAlert,
                Message = message
            });
        }

        /// <summary>
        /// Send the status of the wallet to the client
        /// </summary>
        /// <param name="isCreated">The specified wallet has registration or not</param>
        public void SendUserWalletConnectedState(bool isCreated)
        {
            this.SendData(new
            {
                Opcode = (int)EPackageType.WalletConnect,
                IsCreated = isCreated
            });
        }

        /// <summary>
        /// Sends to the customer that the registration was successful
        /// </summary>
        public void SendCompletedRegister()
        {
            this.SendData(new
            {
                Opcode = (int)EPackageType.CompletedRegister
            });
        }

        /// <summary>
        /// Sends to the client that the connection was completed successfully!
        /// </summary>
        public void SendCompletedLogin()
        {
            this.SendData(new
            {
                Opcode = (int)EPackageType.Login
            });
        }

        /// <summary>
        /// Data transport in bytes to the connected client (parse)
        /// </summary>
        /// <param name="data">Data to be transported</param>
        private void SendData(byte[] data)
        {
            if((this._connectedClient != null) && (this._connectedClient.Socket.Connected))
            {
                this._connectedClient.SendData(data);
            }
        }

        /// <summary>
        /// Text data transport to connected client
        /// </summary>
        /// <param name="data">Text to be transported</param>
        private void SendData(string data) => SendData(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// Transports an object to the connected client (parse)
        /// </summary>
        /// <param name="data">Transports an object to the connected client</param>
        private void SendData(Object data) => SendData(JsonConvert.SerializeObject(data));
    }
}
