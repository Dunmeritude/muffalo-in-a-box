using HarmonyLib;

namespace StasisBox
{
	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
	internal class FloatMenuMakerMap_AddHumanlikeOrders_OpenStasisBoxPostfix
	{
		[HarmonyPostfix]
		internal static void OpenstasisBox(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			foreach (Thing thing in clickCell.GetThingList(pawn.Map))
			{
				var compBoxPawns = thing.TryGetComp<Comp_BoxPawns>();

				if (compBoxPawns is null) continue;

				if (compBoxPawns.Props.openable)
				{
					Job job = JobMaker.MakeJob(DefOfs.StasisBox_OpenStasisBox, thing);
					opts.Add(new FloatMenuOption("StasisBox_OpenBox".Translate(thing), () =>
					{
						pawn.jobs.TryTakeOrderedJob(job);
					}));
				}
			}
		}
	}
}
