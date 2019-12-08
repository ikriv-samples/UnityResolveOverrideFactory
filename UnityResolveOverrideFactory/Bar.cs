namespace UnityResolveOverrideFactory
{
    interface IBar
    {
        string BarName { get; }
    }

    class Bar : IBar
    {
        public string BarName => "T\u00EDr na n\u00D3g";
    }
}
