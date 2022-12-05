﻿namespace AutoBrew.PowerShell.AssemblyLoadContext
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Loader;

    /// <summary>
    /// Initializes for the assembly load context that handles resolution of assemblies.
    /// </summary>
    public static class AssemblyLoadContextInitializer
    {
        /// <summary>
        /// The dictionary that maps a given assembly to the expected version.
        /// </summary>
        private static ConcurrentDictionary<string, Version> SharedAssemblyMap { get; set; }

        /// <summary>
        /// Initializes the instance of the <see cref="AssemblyLoadContextInitializer" /> class.
        /// </summary>
        static AssemblyLoadContextInitializer()
        {
            Dictionary<string, Version> sharedAssemblies = new()
            {
                { "Azure.Core", new Version("1.2500.22.33004") },
                { "Azure.Identity", new Version("1.800.22.55901") },
                { "Microsoft.ApplicationInsights", new Version("2.21.0.429") },
                { "Microsoft.Bcl.AsyncInterfaces", new Version("7.0.22.51805") },
                { "Microsoft.Extensions.Primitives", new Version("7.0.22.51805") },
                { "Microsoft.Identity.Client", new Version("4.46.0.0") },
                { "Microsoft.Identity.Client.Extensions.Msal", new Version("2.25.0.0") },
                { "Microsoft.IdentityModel.Abstractions", new Version("6.25.1.31130") },
                { "Microsoft.Rest.ClientRuntime", new Version("2.3.23.0") },
                { "System.Text.Encodings.Web", new Version("7.0.22.51805") },
                { "System.Text.Json", new Version("7.0.22.51805") },
            };

            SharedAssemblyMap = new ConcurrentDictionary<string, Version>(sharedAssemblies, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles the resolution of assemblies into the assembly load context.
        /// </summary>
        /// <param name="assemblyLoadContext">The instance of <see cref="AssemblyLoadContext" /> where the assembly will be loaded.</param>
        /// <param name="assemblyName">An instance of the <see cref="AssemblyName" /> class that represents the assembly to be loaded.</param>
        /// <returns>An instance of the <see cref="Assembly" /> class that represents the loaded assembly or null.</returns>
        private static Assembly Default_Resolving(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            return SharedAssemblyMap.ContainsKey(assemblyName.Name) && SharedAssemblyMap[assemblyName.Name] >= assemblyName.Version
                ? SharedAssemblyLoadContext.Instance.LoadFromAssemblyName(assemblyName) : null;
        }

        /// <summary>
        /// Performs the registration of the shared assembly load context.
        /// </summary>
        public static void RegisterSharedAssemblyLoadContext()
        {
            AssemblyLoadContext.Default.Resolving += Default_Resolving;
        }
    }
}