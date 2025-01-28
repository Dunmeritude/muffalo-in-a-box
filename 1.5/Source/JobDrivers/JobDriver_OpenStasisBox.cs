namespace StasisBox
{
	public class JobDriver_OpenStasisBox : JobDriver
	{
		Comp_BoxPawns CompBoxPawns => TargetA.Thing.TryGetComp<Comp_BoxPawns>();


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(TargetA, job);
		}


		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

			Toil toil2 = Toils_General.Wait(300, TargetIndex.A)
				.WithProgressBarToilDelay(TargetIndex.A);

			toil2.PlaySoundAtStart(SoundDefOf.CryptosleepCasket_Eject);
			yield return toil2;

			Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
			toil3.defaultCompleteMode = ToilCompleteMode.Instant;
			toil3.AddPreInitAction(() =>
			{
				if (CompBoxPawns.Props.pawnKindToSpawnWhenOpened is not null)
				{
					Thing emptyBox = ThingMaker.MakeThing(CompBoxPawns.Props.emptyBox);
					GenSpawn.Spawn(emptyBox, TargetA.Thing.Position, TargetA.Thing.Map);

					float minAdultAge = CompBoxPawns.Props.pawnKindToSpawnWhenOpened.race.race.lifeStageAges.Find(stage => stage.def == DefOfs.AnimalAdult).minAge;

					PawnGenerationRequest request = new(
						CompBoxPawns.Props.pawnKindToSpawnWhenOpened,
						pawn.Faction,
						allowDead: false,
						allowDowned: false,
						fixedGender: CompBoxPawns.Props.genderOfPawn,
						fixedBiologicalAge: minAdultAge,
						fixedChronologicalAge: minAdultAge);

					Pawn poorSoul = PawnGenerator.GeneratePawn(request);
					GenSpawn.Spawn(poorSoul, TargetA.Thing.Position, pawn.Map);

					TargetA.Thing.SplitOff(1).Destroy();
				}
			});
			yield return toil3;
		}
	}
}
