namespace StasisBox
{
	public class CompProperties_StaticBoxShelf : CompProperties
	{
		public CompProperties_StaticBoxShelf()
		{
			compClass = typeof(Comp_StaticBoxShelf);
		}

		public int maxCapacity;
		public GraphicData filledSlotGraphicData;
		public List<OffsetPair> offsets;
	}

	public class OffsetPair
	{
		public float x;
		public float z;
	}
}
