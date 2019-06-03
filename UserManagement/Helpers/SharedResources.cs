using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esdi.Shared
{
    public class SharedResources
    {
    }

    public interface ISharedStringLocalizer : IStringLocalizer
    {

    }

    // Alternative implementation for creating localizer via a service

    public class SharedLocalizationService
    {
        private readonly IStringLocalizer _localizer;

        public string this[string key]
        {
            get { return _localizer[key]; }
        }

        public SharedLocalizationService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResources);
            _localizer = factory.Create(typeof(SharedResources));

            // Code if localization class is in a different assembly
            // var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            // _localizer = factory.Create("SharedResource", assemblyName.Name);
        }
    }
}
