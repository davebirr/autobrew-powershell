﻿namespace AutoBrew.PowerShell.Commands
{
    using System.Management.Automation;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Models;
    using Models.Authentication;
    using Properties;

    /// <summary>
    /// Cmdlet that provides the ability to update an environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AbEnvironment", SupportsShouldProcess = true)]
    public class SetAbEnvironment : ModuleCmdlet
    {
        /// <summary>
        /// Gets or sets the Active Directory authority for the environment.
        /// </summary>
        [Parameter(HelpMessage = "The Active Directory authority for the environment.", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string ActiveDirectoryAuthority { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the application for the environment.
        /// </summary>
        [Parameter(HelpMessage = "The identifier of the application for the environment.", Mandatory = false)]
        [ValidatePattern(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", Options = RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of Microsoft Graph for the environment.
        /// </summary>
        [Parameter(HelpMessage = "The endpoint of Microsoft Graph for the environment.", Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string MicrosoftGraphEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the name for the environment.
        /// </summary>
        [Parameter(HelpMessage = "The name for the environment.", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tenant for the environment.
        /// </summary>
        [Parameter(HelpMessage = "The tenant for the environment.", Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Tenant { get; set; }

        /// <summary>
        /// Performs the actions associated with the command.
        /// </summary>
        protected override void PerformCmdlet()
        {
            ModuleSession.Instance.TryGetEnvironment(Name, out ModuleEnvironment environment);
            string[] excludeProperties = { "Name", "Type" };

            if (environment == null)
            {
                environment = new()
                {
                    ActiveDirectoryAuthority = ActiveDirectoryAuthority,
                    ApplicationId = ApplicationId,
                    MicrosoftGraphEndpoint = MicrosoftGraphEndpoint,
                    Name = Name,
                    Tenant = Tenant,
                    Type = ModuleEnvironmentType.UserDefined
                };
            }
            else if (environment.Type == ModuleEnvironmentType.BuiltIn)
            {
                throw new ModuleException($"Only user defined environments can be updated. {Name} is a built-in environment, so it cannot be modified.");
            }
            else
            {
                foreach (PropertyInfo property in typeof(ModuleEnvironment).GetProperties().Where(p => excludeProperties.Contains(p.Name) == false))
                {
                    SetValue(environment, property.Name, GetType().GetProperty(property.Name).GetValue(this)?.ToString());
                }
            }

            ConfirmAction(Resources.UpdateEnvironmentTarget, Name, () => ModuleSession.Instance.RegisterEnvironment(Name, environment, true));
        }

        /// <summary>
        /// Sets the value for the specified property.
        /// </summary>
        /// <param name="environment">The instance of the <see cref="ModuleEnvironment" /> class where the property should be updated.</param>
        /// <param name="property">The name for the property to update.</param>
        /// <param name="value">The value for the property.</param>
        /// <exception cref="ArgumentException">
        /// The property parameter is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The environment parameter is null.
        /// </exception>
        private void SetValue(ModuleEnvironment environment, string property, string value)
        {
            environment.AssertNotNull(nameof(environment));
            property.AssertNotEmpty(nameof(property));

            if (string.IsNullOrEmpty(value) == false)
            {
                typeof(ModuleEnvironment).GetProperty(property).SetValue(environment, value);
            }
        }
    }
}