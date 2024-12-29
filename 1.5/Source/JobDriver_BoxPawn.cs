namespace StasisBox
{
	public class JobDriver_BoxPawn : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(TargetB, job))
			{
				return pawn.Reserve(TargetA, job);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);

			Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.B);
			startCarryingThing.AddPreInitAction(() =>
			{
				Pawn victim = TargetA.Pawn;
				PawnUtility.ForceWait(victim, 15000, maintainPosture: true);
			});
			yield return startCarryingThing;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

			Toil waitWhileBoxing = Toils_General.WaitWith(TargetIndex.A, 2000, true);
			yield return waitWhileBoxing;

			Toil results = ToilMaker.MakeToil("MakeNewToils");
			results.defaultCompleteMode = ToilCompleteMode.Instant;
			results.AddFinishAction(() =>
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;

				if (carriedThing is not null)
				{
					carriedThing.TryGetComp<Comp_BoxPawns>().SpawnFilledBox(TargetA.Pawn);
					TargetA.Pawn.DeSpawn();
					carriedThing.SplitOff(1).Destroy();
				}
			});
			yield return results;
		}
	}
}
