namespace live_simulation
{
    public interface IBridgeUtilListener
    {
        BridgeUtil _Util { get; set; }
        void Beat(float a, float b);
        float Beat_T { get; set; }
    }
}