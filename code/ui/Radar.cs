using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


public class RadarPoint : Panel
{
	private Vector3 Position;
	private Panel canvas;
	private Label DistanceLabel;

	public RadarPoint( Panel parent, Vector3 pos )
	{
		Position = pos;

		canvas = parent.Add.Panel( "RadarPoint" );
		DistanceLabel = canvas.Add.Label( "", "distance" );
	}
	public void Remove( )
	{
		canvas.Delete();
	}
	public override void Tick()
	{
		base.Tick();
		if ( Local.Pawn is not Player player ) return;
		DistanceLabel.Text = $"{player.Position.Distance( Position ):n0}";

		Vector2 screenPos = Position.ToScreen();

		canvas.Style.Left = screenPos.x * Screen.Width;// tf?
		canvas.Style.Top = Length.Fraction( screenPos.y );

		canvas.Style.Dirty();
	}
}
public partial class Radar : Panel
{
	private static List<RadarPoint> points = new();
	private static Radar _this;

	public Radar()
	{
		_this = this;
	}
	[ClientRpc]
	public static void Clear()
	{
		foreach ( var point in points )
		{
			point.Remove();
		}
		points.Clear();

	}
	public override void Tick()
	{
		base.Tick();
		foreach ( var point in points )
			point.Tick();
	}

	[ClientRpc]
	public static void AddPoint( Vector3 position )
	{
		points.Add( new RadarPoint( _this, position ) );
	}
}
