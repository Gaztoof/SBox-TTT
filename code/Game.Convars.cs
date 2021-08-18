using Sandbox;

public partial class Game
{
	public static class ConVars
	{
		[ServerVar( "ttt_min_players", Help = "The minimum players required to start.", Saved = true )]
		public static int MinPlayers { get; set; } = 2;
		[ServerVar( "ttt_detective_karma_min", Help = "The minimum karma to be detective.", Saved = true )]
		public static int MinDetectiveKarma { get; set; } = 600;
		[ServerVar( "ttt_detective_min_players", Help = "The minimum players to get a detective.", Saved = true )]
		public static int DetectiveMinPlayers { get; set; } = 6;
		[ServerVar( "ttt_detective_max", Help = "The maximum detectives count.", Saved = true )]
		public static int MaxDetective { get; set; } = 32;
		[ServerVar( "ttt_detective_pct", Saved = true )]
		public static float DetectivePct { get; set; } = 0.125f;
		[ServerVar( "ttt_traitor_max", Help = "The maximum traitors count.", Saved = true )]
		public static int MaxTraitor { get; set; } = 32;
		[ServerVar( "ttt_traitor_pct", Saved = true )]
		public static float TraitorPct { get; set; } = 0.25f;


		[ServerVar( "ttt_karma_enabled", Help = "Karma system enabled.", Saved = true )]
		public static bool KarmaEnabled { get; set; } = true;
		[ServerVar( "ttt_karma_starting", Help = "The karma you start with.", Saved = true )]
		public static int KarmaStarting { get; set; } = 1000;
		[ServerVar( "ttt_karma_strict", Help = "The karma you start with.", Saved = true )]
		public static bool KarmaStrict { get; set; } = true;
		[ServerVar( "ttt_karma_max", Help = "The maximum karma you can have.", Saved = true )]
		public static int KarmaMax { get; set; } = 1000;
		[ServerVar( "ttt_karma_ratio", Help = "The karma ratio.", Saved = true )]
		public static float KarmaRatio { get; set; } = 0.001f;
		[ServerVar( "ttt_karma_kill_penalty", Help = "The karma penalty you get for killing wrongfully.", Saved = true )]
		public static int KarmaKillPenalty { get; set; } = 15;
		[ServerVar( "ttt_karma_round_increment", Help = "The karma you get per round.", Saved = true )]
		public static int KarmaRoundHeal { get; set; } = 5;
		[ServerVar( "ttt_karma_clean_bonus", Help = "The karma you get per round for being clean.", Saved = true )]
		public static int KarmaCleanBonus { get; set; } = 30;
		[ServerVar( "ttt_karma_traitorkill_bonus", Help = "The karma you get for kiling a traitor.", Saved = true )]
		public static int KarmaTraitorKillBonus { get; set; } = 40;
		[ServerVar( "ttt_karma_traitordmg_ratio", Help = "", Saved = true )]
		public static float KarmaTraitorDmgRatio { get; set; } = 0.0003f;


		[ServerVar( "ttt_karma_persist", Help = "", Saved = true )]
		public static bool KarmaPersist { get; set; } = false;
		[ServerVar( "ttt_karma_clean_half", Help = "", Saved = true )]
		public static float KarmaFallOff { get; set; } = 0.25f;


		[ServerVar( "ttt_karma_low_autokick", Help = "", Saved = true )]
		public static bool KarmaAutoKick { get; set; } = true;
		[ServerVar( "ttt_karma_low_amount", Help = "", Saved = true )]
		public static int KarmaKickLevel { get; set; } = 450;
		[ServerVar( "ttt_karma_low_autokick", Help = "", Saved = true )]
		public static bool KarmaAutoBan { get; set; } = false;
		[ServerVar( "ttt_karma_low_amount", Help = "", Saved = true )]
		public static int KarmaBanlevel { get; set; } = 1;
		[ServerVar( "ttt_karma_low_amount", Help = "", Saved = true )]
		public static int KarmaBanMinutes { get; set; } = 60;


		[ServerVar( "ttt_highlight_admins", Help = "Highlight admins/devs on the scoreboard?", Saved = true )]
		public static bool HighlightAdmins { get; set; } = true;

		[ServerVar( "ttt_infinite_ammos", Help = "Infinite ammos cheat.", Saved = true )]
		public static bool InfiniteAmmos { get; set; } = true;
	}
}
