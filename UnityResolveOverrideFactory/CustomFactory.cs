using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Lifetime;
using Unity.Resolution;

namespace UnityResolveOverrideFactory
{
    public class CustomFactory<T>
    {
        private List<IResolverOverrideFactory> _overrides = new List<IResolverOverrideFactory>();

        public interface IResolverOverrideFactory
        {
            ResolverOverride GetOverride(IUnityContainer container);
        }

        public interface IDependencyOverrideFactory<TDependency>
        {
            CustomFactory<T> With<TImpl>() where TImpl : TDependency;
            CustomFactory<T> With(TDependency value);
        }

        private class DependencyOverrideFactory<TDependency> : IResolverOverrideFactory, IDependencyOverrideFactory<TDependency>
        {
            private readonly CustomFactory<T> _parent;
            private Func<IUnityContainer, DependencyOverride> _getOverride;

            public DependencyOverrideFactory(CustomFactory<T> parent)
            {
                _parent = parent;
            }

            public CustomFactory<T> With<TImpl>() where TImpl : TDependency
            {
                if (_getOverride != null) throw new InvalidOperationException("As() has already been called");
                _getOverride = c => new DependencyOverride<TDependency>(c.Resolve<TImpl>());
                return _parent.AddOverride(this);
            }

            public CustomFactory<T> With(TDependency value)
            {
                if (_getOverride != null) throw new InvalidOperationException("As() has already been called");
                _getOverride = c => new DependencyOverride<TDependency>(value);
                return _parent.AddOverride(this);
            }

            public ResolverOverride GetOverride(IUnityContainer container)
            {
                if (_getOverride == null) throw new InvalidOperationException("Override value was not defined");
                return _getOverride(container);
            }
        }

        public IDependencyOverrideFactory<TDependency> DependencyOverride<TDependency>()
        {
            return new DependencyOverrideFactory<TDependency>(this);
        }

        public CustomFactory<T> DependencyOverride<TDependency, TImpl>() where TImpl : TDependency
        {
            return DependencyOverride<TDependency>().With<TImpl>();
        }

        public IUnityContainer RegisterWithContainer(IUnityContainer container, ITypeLifetimeManager lifetimeManager)
        {
            string name = typeof(T).Name + "." + Guid.NewGuid().ToString();
            return container
                .RegisterType<T>(name, lifetimeManager)
                .RegisterFactory<T>(c => c.Resolve<T>(name, _overrides.Select(f => f.GetOverride(c)).ToArray()));
        }

        private CustomFactory<T> AddOverride(IResolverOverrideFactory factory)
        {
            _overrides.Add(factory);
            return this;
        }
    }

    public static class CustomFactoryUnityExtensions
    {
        public static IUnityContainer Register<T>(this IUnityContainer container, ITypeLifetimeManager lifetimeManager, CustomFactory<T> factory)
        {
            return factory.RegisterWithContainer(container, lifetimeManager);
        }

        public static IUnityContainer Register<T>(this IUnityContainer container, CustomFactory<T> factory)
        {
            return factory.RegisterWithContainer(container, new TransientLifetimeManager());
        }
    }
}
