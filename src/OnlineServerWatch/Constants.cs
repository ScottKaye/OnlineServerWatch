using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineServerWatch
{
	internal class Constants
	{
		/// <summary>
		/// Maximum number of times to retry an RCON connection before giving up.
		/// </summary>
		internal const ushort MAX_RETRIES = 3;
	}
}