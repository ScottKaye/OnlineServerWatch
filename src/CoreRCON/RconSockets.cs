using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CoreRCON
{
    internal class RconSockets
    {
		internal Socket TCP { get; set; }
		internal Socket UDP { get; set; }
    }
}
