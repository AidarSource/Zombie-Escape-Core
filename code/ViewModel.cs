using Sandbox;
using System;

// ...
// Offsets by - github.com/Jake-NSW
// ...

public partial class ViewModel : BaseViewModel
{
	public Carriable Active { get; set; }
	protected float SwingInfluence => 0.05f;
	protected float ReturnSpeed => 5.0f;
	protected float MaxOffsetLength => 10.0f;
	protected float BobCycleTime => 7;
	// this is shit, need to redo to configs
	public float xCfg;
	public float yCfg;
	public float zCfg;
	public float waveZ_ak47;
	protected Vector3 BobDirection => new Vector3( xCfg, yCfg, zCfg );

	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;

	private bool activated = false;
	public bool IsAimming { get; set; } = false;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		//FieldOfView = camSetup.FieldOfView;
		camSetup.ViewModel.FieldOfView = camSetup.FieldOfView;

		base.PostCameraSetup( ref camSetup );

		if ( !Local.Pawn.IsValid() )
			return;

		if ( !activated )
		{
			lastPitch = camSetup.Rotation.Pitch();
			lastYaw = camSetup.Rotation.Yaw();

			activated = true;
		}

		Position = camSetup.Position;
		Rotation = camSetup.Rotation;

		//camSetup.ViewModel.FieldOfView = FieldOfView;
		camSetup.ViewModel.FieldOfView = camSetup.FieldOfView;

		var playerVelocity = Local.Pawn.Velocity;

		if ( Local.Pawn is ZePlayer player )
		{
			var controller = player.GetActiveController();
			if ( controller != null && controller.HasTag( "noclip" ) )
			{
				playerVelocity = Vector3.Zero;
			}
		}

		

		Sway( ref camSetup );

		//ViewmodelBreathe( ref camSetup );
		ViewmodelBob( ref camSetup );
		JumpOffset( ref camSetup );
		MoveOffset( ref camSetup );
		CrouchOffset( ref camSetup );
		StrafeOffset( ref camSetup );

		var newPitch = Rotation.Pitch();
		var newYaw = Rotation.Yaw();

		var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
		var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
		pitchDelta -= verticalDelta * 1;

		var offset = CalcSwingOffset( pitchDelta, yawDelta );
		offset += CalcBobbingOffset( playerVelocity );

		Position += Rotation * offset;

		lastPitch = newPitch;
		lastYaw = newYaw;
	}

	protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += (swingVelocity * SwingInfluence);

		if ( swingOffset.Length > MaxOffsetLength )
		{
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	protected Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( bobAnim > twoPI )
		{
			bobAnim -= twoPI;
		}

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		speed = speed > 10.0 ? speed : 0.0f;
		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}

	protected static float walkBobDelta;
	protected static float walkBobScale;
	protected static float dampedSpeed;
	protected static Vector3 lastWalkBob;

	protected virtual void ViewmodelBob( ref CameraSetup cameraSetup )
	{
		var speed = Owner.Velocity.Length.LerpInverse( 0, 240 );
		dampedSpeed = dampedSpeed.LerpTo( speed, 2 * Time.Delta );
		walkBobScale = walkBobScale.LerpTo( Local.Pawn.GroundEntity != null ? speed : 0, 10 * Time.Delta );
		walkBobDelta += Time.Delta * 15.0f * walkBobScale;

		// Waves
		lastWalkBob.x = MathF.Sin( walkBobDelta * 0.7f ) * 0.6f; // 0.6
		lastWalkBob.y = MathF.Sin( walkBobDelta * 0.7f ) * 0.05f; // 0.4
		lastWalkBob.z = MathF.Cos( walkBobDelta * 1.3f ) * waveZ_ak47; // 0.8f

		// Scale walk bob off property
		//lastWalkBob *= Setup.WalkbobScale * dampedSpeed;

		Position += Rotation.Up * lastWalkBob.z;
		Position += Rotation.Left * lastWalkBob.y * 1.25f;
		Rotation *= Rotation.From( lastWalkBob.z * 2, lastWalkBob.y * 4, lastWalkBob.x * 4 );
	}

	public static float lastJumpOffset;

	protected virtual void JumpOffset( ref CameraSetup camSetup )
	{
		lastJumpOffset = lastJumpOffset.LerpTo( Local.Pawn.Velocity.z / 75, 15 * Time.Delta );
		lastJumpOffset = lastJumpOffset.Clamp( -8, 8 );

		Position += Rotation.Up * lastJumpOffset;
		Rotation *= Rotation.From( lastJumpOffset, 0, 0 );
	}

	protected static Vector3 lastCrouchOffset;
	protected static Rotation lastCrouchRot;

	protected virtual void CrouchOffset( ref CameraSetup camSetup )
	{
		if ( Local.Pawn is not ZePlayer pawn )
			return;

		var controller = pawn.GetActiveController() as WalkController;

		if ( controller is null )
			return;

		var enabled = controller.Duck.IsActive;
		var delta = (pawn.EyeLocalPosition.z / 5) * Time.Delta;

		lastCrouchOffset = lastCrouchOffset.LerpTo( enabled ? new Vector3( -4, 0, 0f ) : Vector3.Zero, delta );
		lastCrouchRot = Rotation.Slerp( lastCrouchRot, enabled ? Rotation.From( 0, 0, -15 ) : Rotation.Identity, delta );

		Position += (Rotation.Forward * lastCrouchOffset.x) + (Rotation.Left * lastCrouchOffset.y) + (Rotation.Up * lastCrouchOffset.z);
		// Rotation *= lastCrouchRot;
	}

	protected static float lastStrafeRotateOffset;

	protected virtual void StrafeOffset( ref CameraSetup camSetup )
	{
		lastStrafeRotateOffset = lastStrafeRotateOffset.LerpTo( -Transform.NormalToLocal( Local.Pawn.Velocity ).y / 180, 8 * Time.Delta );
		lastStrafeRotateOffset = lastStrafeRotateOffset.Clamp( -10, 10 );

		Rotation *= Rotation.From( 0, 0, lastStrafeRotateOffset * 4 );
	}

	protected static float lastMoveOffset;
	public static float lastSidewayMoveOffset;

	protected virtual void MoveOffset( ref CameraSetup camSetup )
	{
		lastMoveOffset = lastMoveOffset.LerpTo( Transform.NormalToLocal( Local.Pawn.Velocity ).x / 90, 18 * Time.Delta );
		lastMoveOffset = lastMoveOffset.Clamp( -5, 5 );

		lastSidewayMoveOffset = lastSidewayMoveOffset.LerpTo( -Transform.NormalToLocal( Local.Pawn.Velocity ).y / 180, 18 * Time.Delta );
		lastSidewayMoveOffset = lastSidewayMoveOffset.Clamp( -5, 5 );

		Position += Rotation.Backward * lastMoveOffset / 1.5f;
		Position += Rotation.Down * (lastMoveOffset / 4);
		Position += Rotation.Left * (lastSidewayMoveOffset / 1.5f);
	}

	public static Rotation lastSwayRot;
	public static Vector3 lastSwayPos;

	protected virtual void Sway( ref CameraSetup camSetup )
	{
		var mouse = Mouse.Delta.Clamp( -10, 10 );
		lastSwayRot = Rotation.Slerp( lastSwayRot, Rotation.From( mouse.y.Clamp( -5, 5 ), -mouse.x, mouse.x / 2 ), 8 * RealTime.Delta );
		lastSwayPos = lastSwayPos.LerpTo( Rotation.Up * mouse.y.Clamp( -5, 5 ) + Rotation.Left * mouse.x / 2, 8 * RealTime.Delta );

		Rotation *= lastSwayRot;
		Position += lastSwayPos;
	}

	protected static Vector3 lastEnergyBreatheScale;
	protected static float breatheBobDelta;

	protected virtual void ViewmodelBreathe ( ref CameraSetup camSetup )
	{
		var owner = Local.Pawn as ZePlayer;

		//
		// TODO : CHANGE THIS SHIT FROM HEALTH TO ENERGY
		//
		lastEnergyBreatheScale = lastEnergyBreatheScale.LerpTo(owner.Health < 80 ? new Vector3(4, 4, 3) * owner.Health.Remap(100, 0, 0, 1) : IsAimming ? new Vector3(0.3f) : Vector3.One, 1 * Time.Delta);

		var breatheSpeed = 0.6f * lastEnergyBreatheScale.x;
		var breatheIntensity = 0.25f * lastEnergyBreatheScale.z;

		breatheBobDelta += (Time.Delta) * breatheSpeed;

		var breatheUp = MathF.Cos(breatheBobDelta * 1f) * 1.3f * breatheIntensity;
		var breatheLeft = MathF.Sin(breatheBobDelta * 0.5f) * 0.8f * breatheIntensity;
		var breatheForward = MathF.Sin(breatheBobDelta * 0.5f) * 0.5f * breatheIntensity;

		Position += Rotation.Up * breatheUp;
		Position += Rotation.Left * breatheLeft;
		Rotation *= Rotation.From(breatheUp * lastEnergyBreatheScale.x, breatheLeft * lastEnergyBreatheScale.y, breatheForward * lastEnergyBreatheScale.z);

		camSetup.Rotation *= Rotation.From(breatheUp * -lastEnergyBreatheScale.z * 1.5f, breatheLeft * -lastEnergyBreatheScale.z * 2, breatheForward * lastEnergyBreatheScale.z * 3);
	}
}

public static class Vector2Extensions
{
	public static Vector2 Clamp( this Vector2 value, float min, float max )
	{
		value.x = MathX.Clamp( value.x, min, max );
		value.y = MathX.Clamp( value.y, min, max );

		return value;
	}
}
