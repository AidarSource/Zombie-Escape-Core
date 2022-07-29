
public static partial class ZeEvent
{
	public static class Player
	{
		/// <summary>
		/// Occurs when a player dies
		/// </summary>
		public const string Died = "player.died";

		/// <summary>
		/// Occurs when a player initializes
		/// <para>The <strong><see cref="Sandbox.Client"/></strong> instance of the player who spawned initially.</para>
		/// </summary>
		public const string InitialSpawn = "player.initialspawn";

		/// <summary>
		/// Occurs when a player spawns
		/// <para>Event is passed the <strong><see cref="ZePlayer"/></strong> instance of the player spawned.</para>
		/// </summary>
		public const string Spawned = "player.spawned";

		/// <summary>
		/// Occurs when a player connects
		/// <para>The <strong><see cref="Sandbox.Client"/></strong> instance of the player who connected.</para>
		/// </summary>
		public const string Connected = "player.connected";

		/// <summary>
		/// Occurs when a player disconnects
		/// </summary>
		public const string Disconnected = "player.disconnected";

		/// <summary>
		/// Occurs when a player takes damage
		/// </summary>
		public const string TakeDamage = "player.takedamage";


		public static class Spectating
		{
			/// <summary>
			/// Occurs when the player is changed to spectate
			/// </summary>
			public const string Change = "player.spectating.change";
		}
	}
}
