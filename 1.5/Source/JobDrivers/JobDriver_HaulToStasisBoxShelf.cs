using Verse.Sound;

namespace StasisBox
{
	public class JobDriver_HaulToStasisBoxShelf : JobDriver
	{
		private const int InsertTicks = 30;

		private Thing Container => job.GetTarget(TargetIndex.B).Thing;
		private Comp_StaticBoxShelf CompStaticBoxShelf => Container.TryGetComp<Comp_StaticBoxShelf>();
		private Thing Box => job.GetTarget(TargetIndex.A).Thing;


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}


		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				Comp_StaticBoxShelf shelfComp = CompStaticBoxShelf;
				if (shelfComp == null || shelfComp.Full)
				{
					return true;
				}
				return !shelfComp.autoLoad && (!shelfComp.leftToLoad.Contains(Box));
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.Touch);
			Toil toil = Toils_General.Wait(InsertTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B);
			toil.handlingFacing = true;
			yield return toil;
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
			{
				Box.def.soundDrop.PlayOneShot(SoundInfo.InMap(Container));
				Comp_StaticBoxShelf containerComp = CompStaticBoxShelf;
				containerComp.leftToLoad.Remove(Box);
				MoteMaker.ThrowText(Container.DrawPos, pawn.Map, "InsertedThing".Translate($"{containerComp.innerContainer.Count} / {containerComp.Props.maxCapacity}"));
			});
		}
	}
}
