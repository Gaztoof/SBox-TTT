using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

public class BaseRagdollNameTag : Panel
{
	public Panel line1Panel;
	public Label NameLabel;
	public Image Avatar;
	public Label inspectLabel;
	private PlayerRagdoll ragdoll;

	public BaseRagdollNameTag( PlayerRagdoll ragdoll )
	{
		this.ragdoll = ragdoll;

		var client = ragdoll.Owner.GetClientOwner();
		Avatar = null;

		line1Panel = Add.Panel( "line1" );

		NameLabel = line1Panel.Add.Label( "Unidentified body" );

		inspectLabel = Add.Label( "Press E to inspect", "inspectLabel" );
	}
	public virtual void UpdateFromRagdoll( PlayerRagdoll player )
	{
		if(ragdoll.IsIdentified && Avatar == null && player.GetClientOwner() is Client client )
		{
			line1Panel.DeleteChildren( true );
			Avatar = line1Panel.Add.Image( $"avatar:{client.SteamId}" );
			NameLabel = line1Panel.Add.Label( $"{client.Name}'s body" );
		}
	}
}

public class RagdollNameTags : Panel
{
	Dictionary<PlayerRagdoll, BaseRagdollNameTag> ActiveTags = new ();

	public float MaxDrawDistance = 400;
	public int MaxTagsToShow = 5;

	public RagdollNameTags()
	{
		StyleSheet.Load( "/ui/RagdollNameTags.scss" );
	}

	public override void Tick()
	{
		base.Tick();


		var deleteList = new List<PlayerRagdoll>();
		deleteList.AddRange( ActiveTags.Keys );

		int count = 0;
		foreach ( var ragdoll in Entity.All.OfType<PlayerRagdoll>().OrderBy( x => Vector3.DistanceBetween( x.Position, CurrentView.Position ) ) )
		{
			if ( UpdateNameTag( ragdoll ) )
			{
				deleteList.Remove( ragdoll );
				count++;
			}

			if ( count >= MaxTagsToShow )
				break;
		}

		foreach ( var ragdoll in deleteList )
		{
			ActiveTags[ragdoll].Delete();
			ActiveTags.Remove( ragdoll );
		}

	}

	public virtual BaseRagdollNameTag CreateNameTag( PlayerRagdoll ragdoll )
	{
		if ( ragdoll.GetClientOwner() == null )
			return null;

		var tag = new BaseRagdollNameTag( ragdoll );
		tag.Parent = this;
		return tag;
	}

	public bool UpdateNameTag( PlayerRagdoll ragdoll )
	{
		// Don't draw local player
		if ( ragdoll.Owner == Local.Pawn )
			return false;

		//
		// Where we putting the label, in world coords
		//
		var head = ragdoll.GetAttachment( "hat" ) ?? new Transform( ragdoll.Position );

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


		MaxDrawDistance = 150.0f;

		// Max Draw Distance


		var alpha = dist.LerpInverse( MaxDrawDistance, MaxDrawDistance * 0.7f, true );

		// If I understood this I'd make it proper function
		var objectSize = 0.05f / dist / (2.0f * MathF.Tan( (CurrentView.FieldOfView / 2.0f).DegreeToRadian() )) * 1500.0f;

		objectSize = objectSize.Clamp( 0.05f, 1.0f );

		if ( !ActiveTags.TryGetValue( ragdoll, out var tag ) )
		{
			tag = CreateNameTag( ragdoll );
			if ( tag != null )
			{
				ActiveTags[ragdoll] = tag;
			}
		}

		tag.UpdateFromRagdoll( ragdoll );

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
