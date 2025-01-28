using Verse.Sound;

namespace StasisBox
{
#pragma warning disable CA1051

	public class Command_TargetCustomMessage : Command
	{
		public Action<LocalTargetInfo> action;
		public TargetingParameters targetingParams;
		public string message;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Find.DesignatorManager.Deselect();
			Messages.Message(message, MessageTypeDefOf.NeutralEvent);
			Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo target)
			{
				action(target);
			});
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			return false;
		}
	}
}
