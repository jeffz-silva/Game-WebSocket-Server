using Game.Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Network.Structures
{
    /// <summary>
    /// Holds data for a encoded message frame
    /// </summary>
    public struct SFrameMaskData
    {
        public int DataLength, KeyIndex, TotalLength;

        public EOpcodeType opcodeType;

        public SFrameMaskData(int dataLength, int keyIndex, int totalLength, EOpcodeType opcode)
        {
            this.DataLength = dataLength;
            this.KeyIndex = keyIndex;
            this.TotalLength = totalLength;
            this.opcodeType = opcode;
        }
    }
}
