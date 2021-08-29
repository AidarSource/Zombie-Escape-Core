using System;
using System.Threading.Tasks;
using Sandbox;

namespace ExtensionMethods
{
	public static class TaskHelper
	{
		public static Task DelaySeconds( float seconds )
		{
			return Task.Delay( (seconds * 1000).FloorToInt() );
		}
	}
}
