namespace live_simulation
{
    public interface IBridgeUtilListener
    {
        BridgeUtil _Util { get; set; }
        System.Action<float, float> OnBeat { get; }
    }
}