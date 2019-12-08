using System;
using System.Text;
using Unity;
using Unity.Resolution;

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
            Console.WriteLine("Applying explicit registrations");
            container
                .RegisterType<IFoo, Foo>()
                .RegisterType<IBar, Bar>()
                .RegisterType<SpecialFooUser>("stdreg")
                .RegisterFactory<SpecialFooUser>(
                    c => c.Resolve<SpecialFooUser>("stdreg", new DependencyOverride<IFoo>(c.Resolve<SpecialFoo>()))
                );
        }

        static void FluentRegistrations(IUnityContainer container)
        {
            Console.WriteLine("Applying fluent registrations");
            container
                .RegisterType<IFoo, Foo>()
                .RegisterType<IBar, Bar>()
                .Register(new CustomFactory<SpecialFooUser>().DependencyOverride<IFoo, SpecialFoo>());
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var container = new UnityContainer();
            if (args.Length > 0 && args[0] == "-e")
            {
                ExplicitRegistrations(container);
            }
            else
            {
                FluentRegistrations(container);
            }

            container.Resolve<FooUser>();
            container.Resolve<SpecialFooUser>();
        }
    }
}
