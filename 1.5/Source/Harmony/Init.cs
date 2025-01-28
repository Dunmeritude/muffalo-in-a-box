using HarmonyLib;

namespace StasisBox
{
	[StaticConstructorOnStartup]
	internal static class Init
	{
		static Init()
		{
			Harmony harmony = new("Thekiborg.StasisBox");
			harmony.PatchAll();
		}
	}
}
