global using System;
global using System.Collections.Generic;
global using RimWorld;
global using Verse;
global using UnityEngine;
global using Verse.AI;

namespace StasisBox
{
	public class CompProperties_BoxPawns : CompProperties
	{
		public CompProperties_BoxPawns()
		{
			compClass = typeof(Comp_BoxPawns);
		}

		public TargetingParameters targetingParams;
		public Dictionary<PawnKindGenderPair, ThingDef> filledBoxMapping;

		public PawnKindDef pawnKindToSpawnWhenOpened;
		public bool openable;
		public Gender genderOfPawn;
		public ThingDef emptyBox;
	}
}
