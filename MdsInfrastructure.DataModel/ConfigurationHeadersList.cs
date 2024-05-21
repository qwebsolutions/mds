﻿using Metapsi.Hyperapp;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public class ConfigurationHeadersList
    {
        public List<InfrastructureConfiguration> ConfigurationHeaders { get; set; } = new();
    }

    public class ListConfigurationsPage
    {
        public ConfigurationHeadersList ConfigurationHeadersList { get; set; } = new();
        public Metapsi.Ui.User User { get; set; } = new();
    }

    public class AddConfigurationPage 
    {
        public EditConfigurationPage EditConfigurationPage { get; set; } = new();
    }

    public class ReviewConfigurationPage
    {
        public InfrastructureConfiguration SavedConfiguration { get; set; }
        public List<MdsCommon.ServiceConfigurationSnapshot> Snapshot { get; set; } = new();
        public Metapsi.Ui.User User { get; set; } = new();
        public string FilterValue { get; set; } = string.Empty;
    }
}
