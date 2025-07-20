namespace StasisBox
{
	public class MapComp_StaticBoxCache : MapComponent
	{
		public MapComp_StaticBoxCache(Map map) : base(map) { }

		internal List<Thing> StaticBoxCache = [];
	}
}
