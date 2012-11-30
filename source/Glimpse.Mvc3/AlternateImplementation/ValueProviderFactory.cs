using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Glimpse.Core.Extensibility;
#if MVC2
using Glimpse.Mvc2.Backport;
#endif
using MvcValueProviderFactory = System.Web.Mvc.ValueProviderFactory;

namespace Glimpse.Mvc.AlternateImplementation
{
    public class ValueProviderFactory : AlternateType<MvcValueProviderFactory>
    {
        private IEnumerable<IAlternateMethod> allMethods;

        public ValueProviderFactory(IProxyFactory proxyFactory) : base(proxyFactory)
        {
            AlternateValidatedValueProvider = new ValueProvider<IValueProvider>(ProxyFactory);
            AlternateUnvalidatedValueProvider = new ValueProvider<IUnvalidatedValueProvider>(ProxyFactory);
        }

        public override IEnumerable<IAlternateMethod> AllMethods
        {
            get
            {
                return allMethods ?? (allMethods = new List<IAlternateMethod>
                    {
                        new GetValueProvider(ProxyValueProviderStrategy)
                    });
            }
        }

        private ValueProvider<IUnvalidatedValueProvider> AlternateUnvalidatedValueProvider { get; set; }

        private ValueProvider<IValueProvider> AlternateValidatedValueProvider { get; set; }

        public override bool TryCreate(MvcValueProviderFactory originalObj, out MvcValueProviderFactory newObj, IEnumerable<object> mixins = null, object[] constuctorArgs = null)
        {
            if (mixins == null)
            {
                mixins = Enumerable.Empty<object>();
            }

            if (!base.TryCreate(originalObj, out newObj, mixins, constuctorArgs))
            {
                newObj = new ValueProviderFactoryDecorator(originalObj, ProxyValueProviderStrategy);
            }

            return true;
        }

        private IValueProvider ProxyValueProviderStrategy(IValueProvider original)
        {
            if (original == null)
            {
                return null;
            }

            var originalUnvalidatedValueProvider = original as IUnvalidatedValueProvider;
            if (originalUnvalidatedValueProvider != null)
            {
                IUnvalidatedValueProvider newUnvalidatedValueProvider;
                if (AlternateUnvalidatedValueProvider.TryCreate(originalUnvalidatedValueProvider, out newUnvalidatedValueProvider))
                {
                    return newUnvalidatedValueProvider;
                }
            }

            IValueProvider newValueProvider;
            if (AlternateValidatedValueProvider.TryCreate(original, out newValueProvider))
            {
                return newValueProvider;
            }

            return null;
        }

        public class GetValueProvider : AlternateMethod
        {
            public GetValueProvider(Func<IValueProvider, IValueProvider> proxyValueProviderStrategy) : base(typeof(System.Web.Mvc.ValueProviderFactory), "GetValueProvider")
            {
                ProxyValueProviderStrategy = proxyValueProviderStrategy;
            }

            internal Func<IValueProvider, IValueProvider> ProxyValueProviderStrategy { get; set; }

            public override void PostImplementation(IAlternateImplementationContext context, TimerResult timerResult)
            {
                var originalValueProvider = context.ReturnValue as IValueProvider;

                var result = ProxyValueProviderStrategy(originalValueProvider);

                if (result != null)
                {
                    context.ReturnValue = result;
                }
            }
        }

        public class ValueProviderFactoryDecorator : MvcValueProviderFactory
        {
            public ValueProviderFactoryDecorator(MvcValueProviderFactory wrappedValueProviderFactory, Func<IValueProvider, IValueProvider> proxyValueProviderStrategy)
            {
                WrappedValueProviderFactory = wrappedValueProviderFactory;
                ProxyValueProviderStrategy = proxyValueProviderStrategy;
            }

            internal MvcValueProviderFactory WrappedValueProviderFactory { get; set; }

            internal Func<IValueProvider, IValueProvider> ProxyValueProviderStrategy { get; set; }

            public override IValueProvider GetValueProvider(ControllerContext controllerContext)
            {
                var result = WrappedValueProviderFactory.GetValueProvider(controllerContext);

                return ProxyValueProviderStrategy(result);
            }
        }
    }
}