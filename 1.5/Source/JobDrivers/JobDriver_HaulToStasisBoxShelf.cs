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
				if (CompStaticBoxShelf == null || CompStaticBoxShelf.Full)
				{
					return true;
				}
				return !CompStaticBoxShelf.autoLoad && (!CompStaticBoxShelf.leftToLoad.Contains(Box));
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.Touch);
			Toil toil = Toils_General.Wait(InsertTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B);
			toil.handlingFacing = true;
			yield return toil;
			yield return DepositHauledStasisBoxInContainer(TargetIndex.B, TargetIndex.A, delegate
			{
				Box.def.soundDrop.PlayOneShot(SoundInfo.InMap(Container));
				CompStaticBoxShelf.leftToLoad.Remove(Box);
				MoteMaker.ThrowText(Container.DrawPos, pawn.Map, "InsertedThing".Translate($"{CompStaticBoxShelf.innerContainer.Count} / {CompStaticBoxShelf.Props.maxCapacity}"));
			});
		}

		private static Toil DepositHauledStasisBoxInContainer(TargetIndex containerInd, TargetIndex reserveForContainerInd, Action onDeposited = null)
		{
			Toil toil = ToilMaker.MakeToil("DepositHauledThingInContainer");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat(actor, " tried to place hauled thing in container but is not hauling anything."));
				}
				else
				{
					Thing thing = curJob.GetTarget(containerInd).Thing;
					ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
					if (thingOwner != null)
					{
						int num = actor.carryTracker.CarriedThing.stackCount;
						if (thing is IHaulEnroute haulEnroute)
						{
							ThingDef def = actor.carryTracker.CarriedThing.def;
							num = Mathf.Min(haulEnroute.GetSpaceRemainingWithEnroute(def, actor), num);
							if (reserveForContainerInd != 0)
							{
								Thing thing2 = curJob.GetTarget(reserveForContainerInd).Thing;
								if (!thing2.DestroyedOrNull() && thing2 != haulEnroute && thing2 is IHaulEnroute enroute)
								{
									int spaceRemainingWithEnroute = enroute.GetSpaceRemainingWithEnroute(def, actor);
									num = Mathf.Min(num, actor.carryTracker.CarriedThing.stackCount - spaceRemainingWithEnroute);
								}
							}
						}
						Thing carriedThing = actor.carryTracker.CarriedThing;
						int num2 = actor.carryTracker.innerContainer.TryTransferToContainer(carriedThing, thingOwner, num, false);
						if (num2 != 0)
						{
							if (thing is IHaulEnroute container)
							{
								thing.Map.enrouteManager.ReleaseFor(container, actor);
							}
							if (thing is INotifyHauledTo notifyHauledTo)
							{
								notifyHauledTo.Notify_HauledTo(actor, carriedThing, num2);
							}
							if (thing is ThingWithComps thingWithComps)
							{
								foreach (ThingComp allComp in thingWithComps.AllComps)
								{
									if (allComp is INotifyHauledTo notifyHauledTo2)
									{
										notifyHauledTo2.Notify_HauledTo(actor, carriedThing, num2);
									}
								}
							}
							if (curJob.def == JobDefOf.DoBill)
							{
								HaulAIUtility.UpdateJobWithPlacedThings(curJob, carriedThing, num2);
							}
							onDeposited?.Invoke();
						}
					}
					else if (curJob.GetTarget(containerInd).Thing.def.Minifiable)
					{
						actor.carryTracker.innerContainer.ClearAndDestroyContents();
					}
					else
					{
						Log.Error("Could not deposit hauled thing in container: " + curJob.GetTarget(containerInd).Thing);
					}
				}
			};
			return toil;
		}
	}
}
