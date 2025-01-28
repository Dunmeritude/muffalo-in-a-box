using LudeonTK;

namespace StasisBox
{
#pragma warning disable CA1051, CA1002, CA1822, IDE0044
	public class Comp_StaticBoxShelf : ThingComp, IThingHolder, ISearchableContents
	{
		const float southXoffset = -1f;
		const float westXoffset = 0.15f;
		const float westZoffset = -0.24006f;
		const float eastXoffset = -1.16f;
		const float eastZoffset = -0.245f;

		[Unsaved(false)]
		private List<Thing> tmpBoxes = [];
		public ThingOwner innerContainer;
		public List<Thing> leftToLoad = [];
		public bool autoLoad = true;
		private static readonly CachedTexture EjectTex = new("UI/Gizmos/EjectAll");

		[TweakValue("StasisBoxShelfXOffset", -2f, 2f)]
		private static float xOffset = 0f;
		[TweakValue("StasisBoxShelfYOffset", -1f, 1f)]
		private static float yOffset = 0f;

		public CompProperties_StaticBoxShelf Props => (CompProperties_StaticBoxShelf)props;
		public bool StorageTabVisible => true;
		public bool PowerOn => parent.TryGetComp<CompPowerTrader>().PowerOn;
		public bool Full => ContainedBoxes.Count >= Props.maxCapacity;
		public bool CanLoadMore
		{
			get
			{
				if (!Full)
				{
					return ContainedBoxes.Count + leftToLoad.Count < Props.maxCapacity;
				}
				return false;
			}
		}
		public ThingOwner SearchableContents => innerContainer;
		public List<Thing> ContainedBoxes
		{
			get
			{
				tmpBoxes.Clear();
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i] is Thing box)
					{
						tmpBoxes.Add(box);
					}
				}
				return tmpBoxes;
			}
		}


		public override void PostDraw()
		{
			base.PostDraw();

			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (parent.Rotation == Rot4.North)
				{
					Vector3 drawPos = parent.Position.ToVector3().Yto0();
					drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
					drawPos.x += Props.offsets[i].x;
					drawPos.z += Props.offsets[i].z;

					Props.filledSlotGraphicData.Graphic.Draw(drawPos, Rot4.North, parent);
				}
				else if (parent.Rotation == Rot4.South)
				{
					Vector3 drawPos = parent.Position.ToVector3().Yto0();
					drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
					drawPos.x -= Props.offsets[i].x - southXoffset + xOffset;
					drawPos.z += Props.offsets[i].z + yOffset;

					Props.filledSlotGraphicData.Graphic.Draw(drawPos, Rot4.North, parent);
				}
				else if (parent.Rotation == Rot4.West)
				{
					Vector3 drawPos = parent.Position.ToVector3().Yto0();
					drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
					drawPos.x += Props.offsets[i].z + westZoffset + yOffset;
					drawPos.z += Props.offsets[i].x + westXoffset + xOffset;

					Props.filledSlotGraphicData.Graphic.Draw(drawPos, Rot4.North, parent);
				}
				else if (parent.Rotation == Rot4.East)
				{
					Vector3 drawPos = parent.Position.ToVector3().Yto0();
					drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
					drawPos.x += Props.offsets[i].z + eastZoffset + yOffset;
					drawPos.z -= Props.offsets[i].x + eastXoffset + xOffset;

					Props.filledSlotGraphicData.Graphic.Draw(drawPos, Rot4.North, parent);
				}
			}

		}


		public override void PostPostMake()
		{
			base.PostPostMake();
			innerContainer = new ThingOwner<Thing>(this);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			innerContainer.ClearAndDestroyContents();
			base.PostDestroy(mode, previousMap);
		}

		public override void PostDeSpawn(Map map)
		{
			EjectContents(map);
			for (int i = 0; i < leftToLoad.Count; i++)
			{
				((Genepack)leftToLoad[i]).targetContainer = null;
			}
			leftToLoad.Clear();
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			for (int i = 0; i < leftToLoad.Count; i++)
			{
				if (leftToLoad[i].Map == parent.Map)
				{
					GenDraw.DrawLineBetween(parent.DrawPos, leftToLoad[i].DrawPos);
				}
			}
		}

		public override void CompTick()
		{
			innerContainer.ThingOwnerTick();
		}

		public override void CompTickRare()
		{
			innerContainer.ThingOwnerTickRare();
		}

		public void EjectContents(Map destMap = null)
		{
			destMap ??= parent.Map;
			IntVec3 dropLoc = (parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position);
			innerContainer.TryDropAll(dropLoc, destMap, ThingPlaceMode.Near);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction == Faction.OfPlayer && innerContainer.Any)
			{
				Command_Action command_Action = new()
				{
					defaultLabel = "EjectAll".Translate(),
					defaultDesc = "EjectAllDesc".Translate(),
					icon = EjectTex.Texture,
					action = delegate
					{
						EjectContents(parent.Map);
					}
				};
				yield return command_Action;
			}
		}

		public override string CompInspectStringExtra()
		{
			return "StasisBox_StasisBoxesStored".Translate() + $": {innerContainer.Count} / {Props.maxCapacity}\n" + "CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Reference);
			Scribe_Values.Look(ref autoLoad, "autoLoad", defaultValue: true);
		}
	}
}
