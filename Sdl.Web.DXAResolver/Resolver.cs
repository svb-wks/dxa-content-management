﻿using System;
using System.Configuration;
using System.Linq;
using Tridion.Collections;
using Tridion.Configuration;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Publishing.Resolving;

namespace Sdl.Web.DXAResolver
{
    /// <summary>
    /// DXA Custom Resolver to publish components added to a page with the 'Generate Data Presentation' template.
    /// 
    /// Usage:
    ///   1) Add this assembly to the GAC on the machine hosting the CME
    ///   2) Modify the \config\Tridion.ContentManager.config file and add the following to the resolving/mappings section:
    ///         <section name="Sdl.Web.DXAResolver" type="System.Configuration.AppSettingsSection" />
    ///         ...
    ///         <Sdl.Web.Tridion.Resolver>
    ///             <add key = "dataPresentationTemplate" value="Generate Data Presentation" />
    ///         </Sdl.Web.Tridion.Resolver>
    ///         ...
    ///         <resolving>
    ///             <mappings>
    ///                 ...
    ///                 <add itemType="Tridion.ContentManager.CommunicationManagement.Page">
    ///                     <resolvers>
    ///                         ...
    ///                         <add type="Sdl.Web.DXAResolver.Resolver" assembly="Sdl.Web.DXAResolver, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b9a30ebcbde732e4" />
    ///                     </resolvers>
    ///                 </add>
    ///             </mappings>
    ///         </resolving>
    /// </summary>
    public class Resolver : IResolver
    {
        private readonly string _dataPresentationTemplateTitle;

        public Resolver()
        {
            var tcmConfigSections = (ConfigurationSections)ConfigurationManager.GetSection(ConfigurationSections.SectionName);
            var tcmSectionElem = tcmConfigSections.Sections.Cast<SectionElement>().FirstOrDefault(s => !string.IsNullOrEmpty(s.FilePath) && s.FilePath.EndsWith("tridion.contentmanager.config", StringComparison.InvariantCultureIgnoreCase));
            if (tcmSectionElem != null)
            {
                var tcmConfigFilePath = tcmSectionElem.FilePath;
                var map = new ExeConfigurationFileMap {ExeConfigFilename = tcmConfigFilePath};
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                var resolverSettings =
                    ((AppSettingsSection) config.GetSection("Sdl.Web.DXAResolver")).Settings;
                _dataPresentationTemplateTitle = resolverSettings["dataPresentationTemplate"].Value.ToString();
            }
            else
            {
                // default value for component
                _dataPresentationTemplateTitle = "Generate Data Presentation";
            }
        }

        public void Resolve(IdentifiableObject item, ResolveInstruction instruction, PublishContext context,
            ISet<ResolvedItem> resolvedItems)
        {
            if (instruction.Purpose != ResolvePurpose.Publish && instruction.Purpose != ResolvePurpose.RePublish)
                return;
            var sourceItem = (RepositoryLocalObject)item;
            var contextPublication = (Publication)sourceItem.ContextRepository;
            var filter = new ComponentTemplatesFilter(item.Session)
            {
                AllowedOnPage = false,
                BaseColumns = ListBaseColumns.IdAndTitle
            };
            var dataPresentationTemplate = contextPublication.GetComponentTemplates(filter).FirstOrDefault(
                ct => ct.Title == _dataPresentationTemplateTitle);
            var resolvedItemList = resolvedItems.ToArray();
            foreach (var resolvedItem in resolvedItemList)
            {
                if (!(resolvedItem.Item is Page)) continue;
                var page = (Page)resolvedItem.Item;
                if (page.ComponentPresentations.Count <= 0) continue;              
                // for each component lets resolve it with our data presentation template
                foreach (var cp in page.ComponentPresentations.Where(
                    cp =>
                        dataPresentationTemplate != null &&
                        dataPresentationTemplate.RelatedSchemas.Contains(cp.Component.Schema)))
                {
                    resolvedItems.Add(new ResolvedItem(cp.Component, dataPresentationTemplate));
                }
            }
        }
    }
}
