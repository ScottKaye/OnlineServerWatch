using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineServerWatch
{
    internal static class Extensions
    {
		internal static void DoNotAwait(this Task task) { }
	}
}
