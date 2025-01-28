namespace StasisBox
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
	[DefOf]
	public static class DefOfs
	{
		static DefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DefOfs));
		}

		public static JobDef StasisBox_BoxPawn;
		public static JobDef StasisBox_OpenStasisBox;
		public static JobDef StasisBox_CarryBoxToShelf;
		public static SoundDef CrateOpeningStarted;
		public static LifeStageDef AnimalAdult;
		public static ThingDef StasisBox_AnimalBoxShelf;
	}
}
