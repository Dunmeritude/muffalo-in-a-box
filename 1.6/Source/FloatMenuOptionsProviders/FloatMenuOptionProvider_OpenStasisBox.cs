namespace StasisBox
{
	internal class FloatMenuOptionProvider_OpenStasisBox : FloatMenuOptionProvider
	{
		protected override bool Drafted => false;
		protected override bool Undrafted => true;
		protected override bool Multiselect => false;


		protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
		{
			var compBoxPawns = clickedThing.TryGetComp<Comp_BoxPawns>();

			if (compBoxPawns is not null && compBoxPawns.Props.openable)
			{
				return new FloatMenuOption("StasisBox_OpenBox".Translate(clickedThing), () =>
				{
					Job job = JobMaker.MakeJob(DefOfs.StasisBox_OpenStasisBox, clickedThing);
					context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
				});
			}
			return null;
		}
	}
}
