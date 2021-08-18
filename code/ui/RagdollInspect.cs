using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

public class RagdollInspect : Panel
{
	private class Element
	{

		private Elements Parent;
		private Panel canvas;
		public Image image;
		public ElementType Type;
		public Element( Elements parent, string Image, ElementType type)
		{
			Parent = parent;
			canvas = Parent.canvas.Add.Panel( "Element" );
			image = canvas.Add.Image( Image );
			image.AddEventListener( "onclick", () => Parent.ChangeActive( this ) );
			Type = type;
		}
		public void SetHidden(bool hidden)
		{
			canvas.Style.Display = hidden ? DisplayMode.None : DisplayMode.Flex;
			canvas.Style.Dirty();
		}
		public void SetActive(bool active)
		{
			canvas.SetClass( "active", active );
		}
		public string text = "";
		public void SetText(string Text)
		{
			text = Text;
		}
	}
	private class Elements
	{
		public RagdollInspect Parent;
		public Panel canvas;
		private List<Element> elements = new();
		public Element activeElement = null;
		public Elements( RagdollInspect parent )
		{
			Parent = parent;
			canvas = Parent.Add.Panel( "Elements" );
		}
		public void AddElement(string image, ElementType type)
		{
			elements.Add( new Element( this, image, type ) );
			if ( elements.Count == 1 ) // if it's the first, auto select it
				ResetSelected();
		}
		public void ResetSelected()
		{
			activeElement = elements[0];
		}
		public void ChangeActive(Element element)
		{
			activeElement = element;
		}
		public void Update()
		{
			if ( (Local.Pawn as Player).CurrentlyInspectingRagdoll is not PlayerRagdoll InspectedRagdoll ) return;

			if( InspectedRagdoll.Owner.GetClientOwner()  is not null)
				Parent.HeaderLabel.SetText("Body Search Results - " + InspectedRagdoll.Owner.GetClientOwner().Name);
			foreach ( var x in elements )
			{
				if ( activeElement == x )
				{
					if ( Parent.InfoBarImage.Texture != x.image.Texture )
					{
						Parent.UpdateInfoImage( x.image.Texture );
					}
					if (Parent.InfoBarLabel.Text != x.text)
					{
						Parent.UpdateInfoText( x.text );
					}
					x.SetActive( true );
				}
				else x.SetActive( false );

				Texture targetTexture = null;
				switch ( x.Type )
				{
					case ElementType.ID:
						x.SetText( $"This is the body of {InspectedRagdoll.Owner.GetClientOwner().Name}." );
						break;
					case ElementType.Role:
						switch ( InspectedRagdoll.Role )
						{
							case Player.Role.Detective:
								x.SetText( "This person was a Detective" );
								targetTexture = Texture.Load( "https://i.imgur.com/yfnT2EP.png", true );
								break;
							case Player.Role.Innocent:
								x.SetText( "This person was an innocent terrorist" );
								targetTexture = Texture.Load( "https://i.imgur.com/Mb6aiXx.png", true );
								break;
							case Player.Role.Spectator:
								x.SetText( "This person was a SPECTATOR LMAOOO?" );
								targetTexture = Texture.Load( "https://i.imgur.com/Mb6aiXx.png", true );
								break;
							case Player.Role.Traitor:
								x.SetText( "This person was a Traitor" );
								targetTexture = Texture.Load( "https://i.imgur.com/lFAAczu.png", true );
								break;
						}
						break;
					case ElementType.Weapon:
						x.SetHidden( InspectedRagdoll.KillerWeapon is null );
						if ( InspectedRagdoll.KillerWeapon is not null )
							x.SetText( $"It appears a {InspectedRagdoll.KillerWeapon.Name} was used to kill them." );
						break;
					case ElementType.Time:
						TimeSpan deathTimeSpan = TimeSpan.FromSeconds( InspectedRagdoll.TimeSinceDeath );
						x.SetText( $"They died {string.Format( "{0:D2}:{1:D2}", deathTimeSpan.Minutes, deathTimeSpan.Seconds )} before you conducted the research." );
						break;
					case ElementType.DeathType:
						targetTexture = null;
						switch ( InspectedRagdoll.DeathType )
						{
							case Player.DeathType.Burn:
								targetTexture = Texture.Load( "https://i.imgur.com/3uwxQzq.png", true );
								x.SetText( "Smells like roasted terrorist around here..." );
								break;
							case Player.DeathType.Melee:
								targetTexture = Texture.Load( "https://i.imgur.com/YvV5NBs.png", true );
								x.SetText( "The body is bruised and battered. Clearly they were clubbes to death." );
								break;
							case Player.DeathType.Weapon:
								targetTexture = Texture.Load( "https://i.imgur.com/eqyc9cW.png", true );
								x.SetText( "It is obvious they were shot to death." );
								break;
							case Player.DeathType.Explode:
								targetTexture = Texture.Load( "https://i.imgur.com/aIrLt2I.png", true );
								x.SetText( "Their wounds and singed clothes indicate an explosion caused their end." );
								break;
							case Player.DeathType.Fall:
								targetTexture = Texture.Load( "https://i.imgur.com/vYYd2RK.png", true );
								x.SetText( "They fell to their death." );
								break;
							case Player.DeathType.Suicide:
								targetTexture = Texture.Load( "https://i.imgur.com/07uywg5.png", true );
								x.SetText( "You cannot find a specific cause of this terrorist's death." );
								break;
						}
						break;
				}
				if ( targetTexture is not null ) { x.image.Texture = targetTexture; }
			}
		}
	}
	public enum ElementType
	{
		ID,
		Role,
		Time,
		Weapon,
		Fatal,
		DeathType,
	}
	private Elements elements;
	private Image InfoBarImage;
	private Label InfoBarLabel;
	public Label HeaderLabel;
	public RagdollInspect()
	{
		var Header = Add.Panel( "Header" );
		HeaderLabel = Header.Add.Label( "Body Search Results - Bot25" );

		elements = new Elements( this );
		{
			elements.AddElement( "https://i.imgur.com/ks4MlbH.png", ElementType.ID );
			elements.AddElement( "https://i.imgur.com/Mb6aiXx.png", ElementType.Role );
			elements.AddElement( "https://i.imgur.com/hnMPNdL.png", ElementType.Time );
			elements.AddElement( "https://i.imgur.com/Y5KT4dH.png", ElementType.Weapon );
			elements.AddElement( "https://i.imgur.com/Y5KT4dH.png", ElementType.DeathType );
		}

		var InfoBar = Add.Panel( "InfoBar" );
		InfoBarImage = InfoBar.Add.Image( "https://i.imgur.com/Mb6aiXx.png" );
		InfoBarLabel = InfoBar.Add.Label( "" );

		var Buttons = Add.Panel( "Buttons" );
		var confirmDeath = Buttons.Add.Button( "Confirm Death", "Button" );
		var callDetective = Buttons.Add.Button( "Call Detective", "Button" );
		var closeButton = Buttons.Add.Button( "Close", "Button" );

		confirmDeath.AddEventListener( "onclick", () => Game.ConfirmDeath( ) );
		closeButton.AddEventListener( "onclick", () => Game.StopInspectingRagdoll( ) );
		callDetective.AddEventListener( "onclick", () => Game.CallDetective( ) );
	}
	public override void Tick()
	{
		base.Tick();
		Style.Left = Length.Percent( (((Screen.Width / 2) - (425 / 2)) / Screen.Width) * 100);
		Style.Dirty();
		if ( (Local.Pawn is Player ply) && ply.CurrentlyInspectingRagdoll is not null )
		{
			if ( ply.CurrentlyInspectingRagdoll.Position.Distance( ply.Position ) > 80f )
			{
				Game.StopInspectingRagdoll( );
				SetClass( "active", false );
				elements.ResetSelected();
			}
			else SetClass( "active", true );
		}
		else
		{
			SetClass( "active", false );
			elements.ResetSelected();
		}

		elements.Update();
	}
	public void UpdateInfoText(string text)
	{
		InfoBarLabel.SetText( text );
	}
	public void UpdateInfoImage(Texture texture)
	{
		InfoBarImage.Texture = texture;
	}
}
