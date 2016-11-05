using System;

namespace CoreRCON.Parsers
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ParserAttribute : Attribute
	{
		public Type ParserType { get; set; }

		public ParserAttribute(Type parserType)
		{
			ParserType = parserType;
		}
	}
}
