/*
 * SubSonic - http://subsonicproject.com
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an 
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Text;
using System.Web.Configuration;
using SubSonic.Utilities;

namespace SubSonic.WebUtility
{
    /// <summary>
    /// Summary for the RestService class
    /// </summary>
    public static class RestService
    {
        /// <summary>
        /// 
        /// </summary>
        public static SubSonicSection ConfigSectionSettings;


        #region Provider-specific bits

        private static readonly object _lock = new object();
        private static RestProviderCollection _provider;
        private static RestProvider defaultProvider;
        private static SubSonicSection section;

        /// <summary>
        /// Gets the provider count.
        /// </summary>
        /// <value>The provider count.</value>
        public static int ProviderCount
        {
            get
            {
                if (_provider != null)
                    return _provider.Count;
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the config section.
        /// </summary>
        /// <value>The config section.</value>
        public static SubSonicSection ConfigSection
        {
            get { return section; }
            set { section = value; }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public static RestProvider Provider
        {
            get
            {
                if (defaultProvider == null)
                    LoadProviders();

                return defaultProvider;
            }
            set { defaultProvider = value; }
        }

        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        /// <value>The providers.</value>
        public static RestProviderCollection Providers
        {
            get
            {
                if (_provider == null)
                    LoadProviders();

                return _provider;
            }
            set { _provider = value; }
        }

        /// <summary>
        /// Gets the provider names.
        /// </summary>
        /// <returns></returns>
        public static string[] GetProviderNames()
        {
            if (Providers != null)
            {
                int providerCount = Providers.Count;
                string[] providerNames = new string[providerCount];

                int i = 0;
                foreach (RestProvider provider in Providers)
                {
                    providerNames[i] = provider.Name;
                    i++;
                }
                return providerNames;
            }
            return new string[] { };
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static RestProvider GetInstance(string providerName)
        {
            //ensure load
            LoadProviders();

            //ensure it's instanced
            if (String.IsNullOrEmpty(providerName) || String.IsNullOrEmpty(providerName.Trim()))
                return defaultProvider;

            RestProvider provider = _provider[providerName];
            if (provider != null)
                return provider;

            throw new ArgumentException("No provider is defined with the name " + providerName, "providerName");
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        private static RestProvider GetInstance()
        {
            return GetInstance(null);
        }

        /// <summary>
        /// Loads the providers.
        /// </summary>
        public static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (defaultProvider == null)
            {
                lock (_lock)
                {
                    // Do this again to make sure DefaultProvider is still null
                    if (defaultProvider == null)
                    {
                        //we allow for passing in a configuration section
                        //check to see if one's been passed in
                        if (section == null)
                        {
                            section = ConfigSectionSettings ?? (SubSonicSection)ConfigurationManager.GetSection(ConfigurationSectionName.SUB_SONIC_SERVICE);

                            //if it's still null, throw an exception
                            if (section == null)
                                throw new ConfigurationErrorsException("Can't find the SubSonicService section of the application config file");
                        }

                        // Load registered providers and point DefaultProvider
                        // to the default provider
                        _provider = new RestProviderCollection();
                        ProvidersHelper.InstantiateProviders(section.RESTHandlers, _provider, typeof(RestProvider));

                        defaultProvider = _provider[section.DefaultProvider];

                        if (defaultProvider == null && _provider.Count > 0)
                        {
                            IEnumerator enumer = _provider.GetEnumerator();
                            enumer.MoveNext();
                            defaultProvider = (RestProvider)enumer.Current;
                            if (defaultProvider == null)
                                throw new ProviderException("No providers could be located in the SubSonicService section of the application config file.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public static void AddProvider(RestProvider provider)
        {
            if (_provider == null)
                _provider = new RestProviderCollection();
            _provider.Add(provider);
        }

        #endregion
    }
}