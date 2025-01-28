namespace StasisBox
{
	public class WorkGiver_HaulToStasisBoxShelf : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;


		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn?.Map.GetComponent<MapComp_StaticBoxCache>().StaticBoxCache;
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing shelf = FindStasisShelf(pawn, t);

			if (shelf != null && pawn.CanReserve(t))
			{
				Job job = JobMaker.MakeJob(DefOfs.StasisBox_CarryBoxToShelf, t, shelf);
				job.count = t.stackCount;
				return job;
			}
			return null;
		}


		private static Thing FindStasisShelf(Pawn pawn, Thing box)
		{
			return GenClosest.ClosestThingReachable(box.Position, box.Map, ThingRequest.ForDef(DefOfs.StasisBox_AnimalBoxShelf), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, delegate (Thing shelf)
			{
				if (shelf.IsForbidden(pawn) || !pawn.CanReserve(shelf))
				{
					return false;
				}
				Comp_StaticBoxShelf compShelf = shelf.TryGetComp<Comp_StaticBoxShelf>();
				if (compShelf == null || compShelf.Full || !compShelf.autoLoad)
				{
					return false;
				}
				return true;
			});
		}
	}
}
