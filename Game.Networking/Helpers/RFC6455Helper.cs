using Game.Network.Data;
using Game.Network.Enums;
using Game.Network.Structures;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Helpers
{
    public class RFC6455Helper
    {
        private static ILog log = LogManager.GetLogger(typeof(RFC6455Helper));
        /// <summary>
        /// Only breakline gamb
        /// </summary>
        private const string EOL_CHARS = "\r\n";

        /// <summary>
        /// Gets the opcode of a frame
        /// </summary>
        /// <param name="frame">The frame to get the opcode from</param>
        /// <returns>The opcode of a the frame</returns>
        public static EOpcodeType GetFrameOpcode(byte[] frame)
        {
            if (frame.Length == 0) return EOpcodeType.ClosedConnection;

            return (EOpcodeType)frame[0] - 128;
        }

        /// <summary>Gets data for a encoded websocket frame message</summary>
        /// <param name="Data">The data to get the info from</param>
        /// <returns>The frame data</returns>
        public static SFrameMaskData GetFrameData(byte[] Data)
        {
            // Get the opcode of the frame
            int opcode = Data[0] - 128;

            // If the length of the message is in the 2 first indexes
            if (Data[1] - 128 <= 125)
            {
                int dataLength = (Data[1] - 128);
                return new SFrameMaskData(dataLength, 2, dataLength + 6, (EOpcodeType)opcode);
            }

            // If the length of the message is in the following two indexes
            if (Data[1] - 128 == 126)
            {
                // Combine the bytes to get the length
                int dataLength = BitConverter.ToInt16(new byte[] { Data[3], Data[2] }, 0);
                return new SFrameMaskData(dataLength, 4, dataLength + 8, (EOpcodeType)opcode);
            }

            // If the data length is in the following 8 indexes
            if (Data[1] - 128 == 127)
            {
                // Get the following 8 bytes to combine to get the data 
                byte[] combine = new byte[8];
                for (int i = 0; i < 8; i++) combine[i] = Data[i + 2];

                // Combine the bytes to get the length
                //int dataLength = (int)BitConverter.ToInt64(new byte[] { Data[9], Data[8], Data[7], Data[6], Data[5], Data[4], Data[3], Data[2] }, 0);
                int dataLength = (int)BitConverter.ToInt64(combine, 0);
                return new SFrameMaskData(dataLength, 10, dataLength + 14, (EOpcodeType)opcode);
            }

            // error
            return new SFrameMaskData(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the decoded frame data from the given byte array
        /// </summary>
        /// <param name="data">The byte array to decode</param>
        /// <returns>The decoded data</returns>
        public static string GetDataFromFrame(byte[] data)
        {
            try
            {
                //Get the frame data
                SFrameMaskData frameData = GetFrameData(data);

                //Get the decode frame key from the frame data
                byte[] decodeKey = new byte[4];
                for (int i = 0; i < decodeKey.Length; i++)
                    decodeKey[i] = data[frameData.KeyIndex + i];

                int dataIndex = frameData.KeyIndex + 4;
                int count = 0;

                //Decode the data using the key
                for(int i = dataIndex; i < frameData.TotalLength; i++)
                {
                    data[i] = (byte)(data[i] ^ decodeKey[count % 4]);
                    count++;
                }

                return Encoding.Default.GetString(data, dataIndex, frameData.DataLength);
            }
            catch (Exception ex)
            {
                log.ErrorFormat($"{ex.Message}:{ex.StackTrace}");
                return "";
            }
        }

        /// <summary>Gets an encoded websocket frame to send to a client from a string</summary>
        /// <param name="Message">The message to encode into the frame</param>
        /// <param name="Opcode">The opcode of the frame</param>
        /// <returns>Byte array in form of a websocket frame</returns>
        public static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
        {
            byte[] response;

            //Enconding.Default
            byte[] bytesRaw = Encoding.UTF8.GetBytes(Message);

            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)(128 + (int)Opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        /// <summary>
        /// Gets the http request string to send to the websocket client
        /// </summary>
        /// <param name="websocketKey">The SHA1 hashed key to respond with</param>
        /// <returns></returns>
        public static Byte[] GetHandshakeResponse(string websocketKey) => Encoding.UTF8.GetBytes(
            "HTTP/1.1 101 Switching Protocols" + EOL_CHARS +
            "Upgrade: websocket" + EOL_CHARS +
            "Connection: Upgrade" + EOL_CHARS +
            "Sec-WebSocket-Accept: " + HashKey(websocketKey) + EOL_CHARS + EOL_CHARS);

        /// <summary>
        /// Get http handshake request key
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static string GetHandshakeQuestKey(string httpRequest)
        {
            string handshakeKey = null;

            int webSocketKey = httpRequest.IndexOf("Sec-WebSocket-Key: ") + 19;
            for (int i = webSocketKey; i < (webSocketKey + 24); i++)
            {
                handshakeKey += httpRequest[i];
            }

            return handshakeKey;
        }

        /// <summary>
        /// Hash a request key with SHA1 to get the response key
        /// </summary>
        /// <param name="websocketKey">The request key</param>
        /// <returns></returns>
        public static string HashKey(string websocketKey)
        {
            string longKey = (websocketKey + NetworkProperties.HANDSHAKE_KEY);
            SHA1 sh1 = SHA1.Create();

            byte[] hashBytes = sh1.ComputeHash(Encoding.ASCII.GetBytes(longKey));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Create a new guid identifier
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateGuid(string prefix, int length = 16) => string.Format("{0}-{1}", prefix, Guid.NewGuid());
    }
}
