using System.Text.RegularExpressions;

namespace CoreRCON.Parsers
{
	public interface IParser<T>
		where T : class
	{
		/// <summary>
		/// Returns if the line received from the server can be parsed into the desired type.
		/// </summary>
		/// <param name="line">Single line from the server.</param>
		bool IsMatch(string line);

		/// <summary>
		/// Parses the line from the server into the desired type.
		/// </summary>
		/// <param name="line">Single line from the server.</param>
		T Parse(string line);
	}
}
