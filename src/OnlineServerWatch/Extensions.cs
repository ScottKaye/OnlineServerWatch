using System.Threading.Tasks;

namespace OnlineServerWatch
{
	internal static class Extensions
    {
		internal static void DoNotAwait(this Task task) { }
	}
}
