﻿using System;
using System.Xml;
using R6 = Tridion.ContentManager.CoreService.Client;
using Tridion.Logging;

namespace DXA.CM.Extensions.DXAResolver.Models
{
    public partial class Services
    {
        private static XmlNamespaceManager _ns;
        private const string CLIENT_ENDPOINT_NAME_81 = "netSamlTcp_201501";

        internal static XmlNamespaceManager getNameSpacemanager()
        {
            using (Tracer.GetTracer().StartTrace())
            {
                if (_ns == null)
                {
                    _ns = new XmlNamespaceManager(new NameTable());
                    _ns.AddNamespace(Constants.DXA_RESOLVER_CONFIGURATION_PREFIX, Constants.DXA_RESOLVER_CONFIGURATION_NAMESPACE);
                }

                return _ns;
            }
        }

        internal static string LoadConfigurationImpl()
        {
            using (Tracer.GetTracer().StartTrace())
            {
                using (var client = new R6.SessionAwareCoreServiceClient(CLIENT_ENDPOINT_NAME_81))
                {
                    try
                    {
                        R6.ApplicationData appData = client.ReadApplicationData(null, Constants.DXA_RESOLVER_CONFIGURATION_NAME);
                        if (appData != null)
                        {
                            R6.ApplicationDataAdapter ada = new R6.ApplicationDataAdapter(appData);
                            XmlElement appDataXml = ada.GetAs<XmlElement>();
                            return appDataXml.OuterXml;
                        }

                        return String.Format("<Configuration xmlns:{0}=\"{1}\"></Configuration>",
                            Constants.DXA_RESOLVER_CONFIGURATION_PREFIX, Constants.DXA_RESOLVER_CONFIGURATION_NAMESPACE);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(Resources.DXA_CM_Extensions_DXAResolver_Models_Strings.CR_LoadConfigurationFailed, ex);
                    }
                }
            }
        }

        internal static string SaveConfigurationImpl(string configurationXml)
        {
            using (Tracer.GetTracer().StartTrace(configurationXml))
            {
                using (var client = new R6.SessionAwareCoreServiceClient(CLIENT_ENDPOINT_NAME_81))
                {
                    try
                    {
                        XmlDocument appDataXml = new XmlDocument();
                        appDataXml.LoadXml(configurationXml);
                        R6.ApplicationDataAdapter ada = new R6.ApplicationDataAdapter(Constants.DXA_RESOLVER_CONFIGURATION_NAME, appDataXml.DocumentElement);
                        client.SaveApplicationData(null, new[] {ada.ApplicationData});

                        return configurationXml;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(Resources.DXA_CM_Extensions_DXAResolver_Models_Strings.CR_SaveConfigurationFailed, ex);
                    }
                }
            }
        }
    }
}
