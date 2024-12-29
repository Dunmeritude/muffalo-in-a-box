using System.Linq;

namespace StasisBox
{
	public class Comp_BoxPawns : ThingComp
	{
		private Pawn amazonWorker;
		private Pawn pawnToPack;

		readonly TargetingParameters amazonWorkerParams = new()
		{
			canTargetBuildings = false,
			canTargetAnimals = false,
			canTargetMechs = false,
			canTargetMutants = false,
			
		};


		public CompProperties_BoxPawns Props => (CompProperties_BoxPawns)props;


		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			
			if (Props.openable)
			{
				Command_Action command = new()
				{
					defaultLabel = "Open",
					action = () =>
					{
						try
						{
							PawnGenerationRequest request = new(Props.pawnKindToSpawnWhenOpened,
								Faction.OfPlayer,
								allowDead: false,
								allowDowned: false,
								fixedGender: Props.genderOfPawn,
								fixedBiologicalAge: 1f,
								fixedChronologicalAge: 1f);
							Pawn pawn = PawnGenerator.GeneratePawn(request);
							GenSpawn.Spawn(pawn, parent.Position, parent.Map);
						}
						finally
						{
							parent.Destroy();
						}
					},
				};
				yield return command;
			}
			else
			{
				TargetingParameters pawnToPackParams = Props.targetingParams;
				pawnToPackParams.validator = (TargetInfo t) =>
				{
					if (t.Thing is not Pawn pawnTarget || pawnTarget.Faction != Faction.OfPlayer)
					{
						return false;
					}
					if (pawnTarget.DevelopmentalStage < DevelopmentalStage.Adult)
					{
						return false;
					}
					if (pawnTarget.IsColonyMechRequiringMechanitor())
					{
						return false;
					}
					if (!Props.filledBoxMapping.Any(kvp => kvp.Key.pawnKindDef == pawnTarget.kindDef))
					{
						return false;
					}
					return true;
				};


				Command_Target command = new()
				{
					defaultLabel = "Box",
					targetingParams = amazonWorkerParams,
					action = (LocalTargetInfo t) =>
					{
						amazonWorker = t.Pawn;

						Find.Targeter.BeginTargeting(pawnToPackParams, (LocalTargetInfo t) =>
						{
							pawnToPack = t.Pawn;

							if (amazonWorker is not null && pawnToPack is not null)
							{
								Job job = JobMaker.MakeJob(DefOfs.StasisBox_BoxPawn, pawnToPack, parent);
								job.WithCount(2);
								amazonWorker.jobs.TryTakeOrderedJob(job);
							}
						}, null);
					},
				};
				yield return command;
			}
		}


		public void SpawnFilledBox(Pawn pawnToPack)
		{
			foreach (var kvp in Props.filledBoxMapping)
			{
				Log.Message(kvp.Key);
				if (kvp.Key.pawnKindDef == pawnToPack.kindDef 
					&& kvp.Key.genderOfPawn == pawnToPack.gender)
				{
					Thing thing = ThingMaker.MakeThing(kvp.Value);
					GenSpawn.Spawn(thing, pawnToPack.Position, parent.MapHeld);
				}
			}
		}
	}
}
