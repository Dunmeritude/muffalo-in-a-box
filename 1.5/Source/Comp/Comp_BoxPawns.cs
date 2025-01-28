using System.Linq;

namespace StasisBox
{
	public class Comp_BoxPawns : ThingComp
	{
		private Pawn amazonWorker;
		private Pawn pawnToPack;
		private float hpRecoveryPct;
		private float deteriorationPct;


		readonly TargetingParameters amazonWorkerParams = new()
		{
			canTargetBuildings = false,
			canTargetAnimals = false,
			canTargetMechs = false,
			canTargetMutants = false,
			
		};


		private Comp_StaticBoxShelf ParentContainer
		{
			get
			{
				IThingHolder parentHolder = base.ParentHolder;
				if (parentHolder == null || parentHolder is not Comp_StaticBoxShelf result)
				{
					return null;
				}
				return result;
			}
		}

		public bool Deteriorating
		{
			get
			{
				Comp_StaticBoxShelf parentContainer = ParentContainer;
				if (parentContainer == null || !parentContainer.PowerOn)
				{
					return true;
				}
				return false;
			}
		}

		public CompProperties_BoxPawns Props => (CompProperties_BoxPawns)props;


		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (Props.openable)
			{
				MapComp_StaticBoxCache mapComp = parent.Map.GetComponent<MapComp_StaticBoxCache>();
				mapComp?.StaticBoxCache.Add(parent);
			}
		}


		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			map?.GetComponent<MapComp_StaticBoxCache>().StaticBoxCache.Remove(parent);
		}


		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			

			if (!Props.openable)
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


				Command_TargetCustomMessage command = new()
				{
					defaultLabel = "StasisBox_BoxPawn".Translate(),
					targetingParams = amazonWorkerParams,
					message = "StasisBox_SelectPackingPawn".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Designators/Open"),
					action = (LocalTargetInfo t) =>
					{
						amazonWorker = t.Pawn;

						Messages.Message("StasisBox_SelectPackedPawn".Translate(), MessageTypeDefOf.NeutralEvent);

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


		public override void CompTickRare()
		{
			base.CompTickRare();
			if (Props.openable)
			{
				if (Deteriorating)
				{
					float statValue = parent.GetStatValue(StatDefOf.DeteriorationRate);
					if (statValue > 0.001f)
					{
						deteriorationPct += statValue * 250f / 60000f;
						if (deteriorationPct >= 1f)
						{
							deteriorationPct -= 1f;
							SteadyEnvironmentEffects.DoDeteriorationDamage(parent, parent.PositionHeld, parent.MapHeld, sendMessage: true);
						}
					}
				}
				else
				{
					if (parent.HitPoints >= parent.MaxHitPoints)
					{
						return;
					}
					hpRecoveryPct += 0.004166667f;
					if (hpRecoveryPct >= 1f)
					{
						hpRecoveryPct -= 1f;
						parent.HitPoints++;
						if (parent.HitPoints == parent.MaxHitPoints)
						{
							hpRecoveryPct = 0f;
						}
					}
				}
			}
		}


		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref hpRecoveryPct, "hpRecoveryPct", 0f);
			Scribe_Values.Look(ref deteriorationPct, "deteriorationPct", 0f);
		}


		public void SpawnFilledBox(Pawn pawnToPack)
		{
			foreach (var kvp in Props.filledBoxMapping)
			{
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
