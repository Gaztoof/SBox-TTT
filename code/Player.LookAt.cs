using Sandbox;
using System;

partial class Player
{
	public T LookAt<T>( float maxDistance ) where T : class
	{
		Trace trace;

		if ( IsClient )
		{
			Camera camera = MainCamera as Camera;
			trace = Trace.Ray( camera.Pos, camera.Pos + camera.Rot.Forward * maxDistance );
		}
		else
		{
			trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * maxDistance );
		}

		trace = trace.HitLayer(CollisionLayer.Debris).Ignore( this );

		TraceResult tr = trace.UseHitboxes().Run();
		if ( tr.Hit && tr.Entity is T type )
		{
			return type;
		}

		return default(T);
	}
}
