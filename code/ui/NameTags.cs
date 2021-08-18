using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

public class BaseNameTag : Panel
{
	public Label NameLabel;
	public Image Avatar;
	public Label health;
	public Label Role;
	Player player;

	public BaseNameTag( Player player )
	{
		this.player = player;

		var client = player.GetClientOwner();

		var panel = Add.Panel( "line1" );
		{
			Avatar = panel.Add.Image( $"avatar:{client.SteamId}" );
			NameLabel = panel.Add.Label( $"{client.Name}" );
		}

		health = Add.Label( "", "health" );
		Role = Add.Label( "", "role" );


	}
	public virtual void UpdateHealthColor(bool healthy = false, bool hurt = false, bool wounded = false, bool badlyWounded = false, bool nearDeath = false)
	{
		health.SetClass( "healthy", healthy );
		health.SetClass( "hurt", hurt );
		health.SetClass( "wounded", wounded );
		health.SetClass( "badlyWounded", badlyWounded );
		health.SetClass( "nearDeath", nearDeath );
	}
	public virtual void UpdateRoleColor(bool detective = false, bool traitor = false)
	{
		Role.SetClass( "detective", detective );
		Role.SetClass( "traitor", traitor );
		Role.Style.Display = (detective || traitor) ? DisplayMode.Flex : DisplayMode.None;
		Role.Style.Dirty();
	}
	public virtual void UpdateFromPlayer( Player player )
	{
		string healthText = "";
		if ( player.Health > player.maxHealth * 0.9 )
		{
			healthText = "Healthy";
			UpdateHealthColor( healthy: true );
		}
		else if ( player.Health > player.maxHealth * 0.7 )
		{
			healthText = "Hurt";
			UpdateHealthColor( hurt: true );
		}
		else if ( player.Health > player.maxHealth * 0.45 )
		{
			healthText = "Wounded";
			UpdateHealthColor( wounded: true );
		}
		else if ( player.Health > player.maxHealth * 0.2 )
		{
			healthText = "Badly Wounded";
			UpdateHealthColor( badlyWounded: true );
		}
		else
		{
			healthText = "Near Death";
			UpdateHealthColor( nearDeath: true );
		}
		health.SetText( healthText );
		if(player.role == Player.Role.Detective)
		{
			Role.SetText( "Detective" );
			UpdateRoleColor( detective: true );
		}
		else if(player.role == Player.Role.Traitor && (Local.Pawn is Player localPlayer) && localPlayer.role == Player.Role.Traitor)
		{
			Role.SetText( "Fellow Traitor" );
			UpdateRoleColor( traitor: true );
		}
		else
		{
			Role.SetText( "" );
			UpdateRoleColor();
		}
	}
}

public class NameTags : Panel
{
	Dictionary<Player, BaseNameTag> ActiveTags = new Dictionary<Player, BaseNameTag>();

	public float MaxDrawDistance = 400;
	public int MaxTagsToShow = 5;

	public NameTags()
	{
		StyleSheet.Load( "/ui/NameTags.scss" );
	}

	public override void Tick()
	{
		base.Tick();


		var deleteList = new List<Player>();
		deleteList.AddRange( ActiveTags.Keys );

		int count = 0;
		foreach ( var player in Entity.All.OfType<Player>().OrderBy( x => Vector3.DistanceBetween( x.Position, CurrentView.Position ) ) )
		{
			if ( UpdateNameTag( player ) )
			{
				deleteList.Remove( player );
				count++;
			}

			if ( count >= MaxTagsToShow )
				break;
		}

		foreach ( var player in deleteList )
		{
			ActiveTags[player].Delete();
			ActiveTags.Remove( player );
		}

	}

	public virtual BaseNameTag CreateNameTag( Player player )
	{
		if ( player.GetClientOwner() == null )
			return null;

		var tag = new BaseNameTag( player );
		tag.Parent = this;
		return tag;
	}

	public bool UpdateNameTag( Player player )
	{
		// Don't draw local player
		if ( player == Local.Pawn )
			return false;

		if ( player.IsDead() || player.IsSpectator() )
			return false;
		//if ( player.IsDead() || player.IsSpectator() )
			//return false;

		//
		// Where we putting the label, in world coords
		//
		var head = player.GetAttachment( "hat" ) ?? new Transform( player.EyePos );

		var labelPos = head.Position + head.Rotation.Up * 5;


		//
		// Are we too far away?
		//
		float dist = labelPos.Distance( CurrentView.Position );
		if ( dist > MaxDrawDistance )
			return false;

		//
		// Are we looking in this direction?
		//
		var lookDir = (labelPos - CurrentView.Position).Normal;
		if ( CurrentView.Rotation.Forward.Dot( lookDir ) < 0.5 )
			return false;

		// TODO - can we see them


		MaxDrawDistance = 400;

		// Max Draw Distance


		var alpha = dist.LerpInverse( MaxDrawDistance, MaxDrawDistance * 0.1f, true );

		// If I understood this I'd make it proper function
		var objectSize = 0.05f / dist / (2.0f * MathF.Tan( (CurrentView.FieldOfView / 2.0f).DegreeToRadian() )) * 1500.0f;

		objectSize = objectSize.Clamp( 0.05f, 1.0f );

		if ( !ActiveTags.TryGetValue( player, out var tag ) )
		{
			tag = CreateNameTag( player );
			if ( tag != null )
			{
				ActiveTags[player] = tag;
			}
		}

		tag.UpdateFromPlayer( player );

		var screenPos = labelPos.ToScreen();

		tag.Style.Left = Length.Fraction( screenPos.x );
		tag.Style.Top = Length.Fraction( screenPos.y );
		tag.Style.Opacity = alpha;

		var transform = new PanelTransform();
		transform.AddTranslateY( Length.Fraction( -1.0f ) );
		transform.AddScale( objectSize );
		transform.AddTranslateX( Length.Fraction( -0.5f ) );

		tag.Style.Transform = transform;
		tag.Style.Dirty();

		return true;
	}
}
