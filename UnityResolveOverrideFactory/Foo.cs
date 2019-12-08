namespace UnityResolveOverrideFactory
{
    interface IFoo
    {
        string Name { get; }
    }

    class Foo : IFoo
    {
        public string Name => "Foo";
    }

    class SpecialFoo : IFoo
    {
        public string Name => "SpecialFoo";
    }
}
