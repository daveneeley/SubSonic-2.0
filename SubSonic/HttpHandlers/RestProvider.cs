using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;

namespace SubSonic.WebUtility
{
    public class RestProvider : ProviderBase
    {
        private string _spList = String.Empty;
        private string _tableList = String.Empty;
        private RESTReturnType _returnType;

        /// <summary>
        /// Gets or sets the allowed table list.
        /// </summary>
        /// <value>The allowed table list.</value>
        public string[] AllowedTables
        {
            get
            {
                string[] result = new string[0];
                if (!String.IsNullOrEmpty(_tableList))
                    result = CodeService.GetItemListArray(_tableList);

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the allowed table list.
        /// </summary>
        /// <value>The allowed table list.</value>
        public string AllowedTableList
        {
            get { return _tableList; }
        }

        /// <summary>
        /// Gets or sets the allowed sp list.
        /// </summary>
        /// <value>The allowed sp list.</value>
        public string[] AllowedSps
        {
            get
            {
                string[] result = new string[0];
                if (!String.IsNullOrEmpty(_spList))
                    result = CodeService.GetItemListArray(_spList);

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the allowed sp list.
        /// </summary>
        /// <value>The allowed sp list.</value>
        public string AllowedSpList
        {
            get { return _spList; }
        }

        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        public RESTReturnType ReturnType
        {
            get { return _returnType; }
            set { _returnType = value; }
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
            ApplyConfig(config, ref _tableList, ConfigurationPropertyName.INCLUDE_TABLE_LIST);
            ApplyConfig(config, ref _spList, ConfigurationPropertyName.INCLUDE_PROCEDURE_LIST);
        }

        /// <summary>
        /// Applies the config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="configName">Name of the config.</param>
        private static void ApplyConfig(System.Collections.Specialized.NameValueCollection config, ref string parameterValue, string configName)
        {
            if (config[configName] != null)
                parameterValue = config[configName];
        }


    }

    /// <summary>
    /// Summary for the RestProviderCollection class
    /// </summary>
    public class RestProviderCollection : ProviderCollection
    {
        private static readonly object _lockProvider = new object();

        /// <summary>
        /// Gets the <see cref="SubSonic.RestProvider"/> with the specified name.
        /// </summary>
        /// <value></value>
        public new RestProvider this[string name]
        {
            get { return (RestProvider)base[name]; }
        }

        /// <summary>
        /// Adds a provider to the collection.
        /// </summary>
        /// <param name="provider">The provider to be added.</param>
        /// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="provider"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.Configuration.Provider.ProviderBase.Name"/> of <paramref name="provider"/> is null.- or -The length of the <see cref="P:System.Configuration.Provider.ProviderBase.Name"/> of <paramref name="provider"/> is less than 1.</exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        /// </PermissionSet>
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is RestProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            if (base[provider.Name] == null)
            {
                lock (_lockProvider)
                {
                    if (base[provider.Name] == null)
                        base.Add(provider);
                }
            }
        }
    }

}
