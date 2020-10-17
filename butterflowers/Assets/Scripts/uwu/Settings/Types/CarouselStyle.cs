using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Carousel Style", menuName = "Settings/Global/Carousel", order = 52)]
	public class CarouselStyle : Global<FloatSetting, float>
	{
		public bool infiniteScroll;
		public bool clamped;
		public bool scaling;

		public AnimationCurve scaleCurve;
	}
}