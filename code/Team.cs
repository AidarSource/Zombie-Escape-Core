using Sandbox;
using System.Collections.Generic;
using System.Linq;


	public enum Team
	{
		None,
		Zombies,
		Humans
	}

public static class TeamExtensions
{
	public static string GetHudClass( this Team team )
	{
		if ( team == Team.Humans )
			return "team_blue";
		else if ( team == Team.Zombies )
			return "team_red";
		else
			return "team_none";
	}

	public static Color GetColor( this Team team )
	{
		if ( team == Team.Humans )
			return Color.Cyan;
		else if ( team == Team.Zombies )
			return new Color( 1f, 0.38f, 0.27f );
		else
			return new Color( 1f, 1f, 0f );
	}

	public static string GetName( this Team team )
	{
		if ( team == Team.Humans )
			return "Humans";
		else if ( team == Team.Zombies )
			return "Zombies";
		else
			return "Neutral";
	}

	public static To GetTo( this Team team )
	{
		return To.Multiple( team.GetAll().Select( e => e.Client ) );
	}

	public static IEnumerable<Player> GetAll( this Team team )
	{
		return Entity.All.OfType<ZePlayer>().Where( e => e.Team == team );
	}

	public static int GetCount( this Team team )
	{
		return Entity.All.OfType<ZePlayer>().Where( e => e.Team == team ).Count();
	}
}
