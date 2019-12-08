using System;
using System.Text;
using Unity;

namespace UnityResolveOverrideFactory
{
    class FooUser
    {
        public FooUser(IFoo foo, IBar bar)
        {
            Console.WriteLine($"{GetType().Name} uses {foo.Name} and \"{bar.BarName}\" bar");
        }
    }

    class SpecialFooUser
    {
        public SpecialFooUser(IFoo foo, IBar bar)
        {
            Console.WriteLine($"{GetType().Name} uses {foo.Name} and \"{bar.BarName}\" bar");
        }
    }

    class Program
    {
        static void ExplicitRegistrations(IUnityContainer container)
        {
            container
                .RegisterType<IFoo, Foo>()
                .RegisterType<IBar, Bar>()
                .RegisterFactory<SpecialFooUser>(c => new SpecialFooUser(c.Resolve<SpecialFoo>(), c.Resolve<IBar>()));
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var container = new UnityContainer();
            ExplicitRegistrations(container);

            container.Resolve<FooUser>();
            container.Resolve<SpecialFooUser>();
        }
    }
}
