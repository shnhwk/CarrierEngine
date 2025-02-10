using System;

namespace CarrierEngine.Domain
{
    public static class ServiceLocator
    {
        public static IServiceProvider Instance { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Instance = serviceProvider;
        }
    }
}