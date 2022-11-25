using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Packets.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketHandlerAttribute : Attribute
    {
        /// <summary>
        /// Specify package code
        /// </summary>
        protected int code;
        public int Code => code;

        /// <summary>
        /// Specify package description
        /// </summary>
        protected string desc;
        public string Desc => desc;

        public PacketHandlerAttribute(int code, string desc)
        {
            this.code = code;
            this.desc = desc;
        }
    }
}
