using Sandbox;
using System;
using System.Linq;

partial class Inventory : BaseInventory
{
	public Inventory(ZePlayer player) : base( player )
	{

	}

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		return !IsCarryingType( entity.GetType() );
	}

	//public override bool Add( Entity entity, bool makeActive = false )
	//{
	//	var player = Owner as ZePlayer;
	//	var weapon = entity as ZePlayer;
	//	var notices = !player.SupressPickupNotices;
	//	//
	//	// We don't want to pick up the same weapon twice
	//	// But we'll take the ammo from it Winky Face
	//	//
	//	if ( weapon != null && IsCarryingType( entity.GetType() ) )
	//	{
	//		var ammo = weapon.AmmoClip;
	//		var ammoType = weapon.AmmoType;

	//		if ( ammo > 0 )
	//		{
	//			player.GiveAmmo( ammoType, ammo );

	//			if ( notices )
	//			{
	//				//Sound.FromWorld( "dm.pickup_ammo", ent.Position );
	//				//PickupFeed.OnPickup( To.Single( player ), $"+{ammo} {ammoType}" );
	//			}
	//		}

	//		//ItemRespawn.Taken( entity );

	//		// Despawn it
	//		entity.Delete();
	//		return false;
	//	}

	//	if ( weapon != null && notices )
	//	{
	//		Sound.FromWorld( "dm.pickup_weapon", entity.Position );
	//		//PickupFeed.OnPickup( To.Single( player ), $"{entity.ClassInfo.Title}" );
	//	}


	//	//ItemRespawn.Taken( entity );
	//	return base.Add( entity, makeActive );
	//}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}

	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		ent.OnCarryDrop( Owner );

		return ent.Parent == null;
	}
}
