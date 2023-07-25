﻿using Metapsi.Ui;
using System.Collections.Generic;

namespace MdsInfrastructure
{
    public static class Node
    {
        public class List
        {
            public Metapsi.Ui.User User { get; set; }

            public List<EnvironmentType> EnvironmentTypes { get; set; } = new List<EnvironmentType>();
            public List<InfrastructureNode> InfrastructureNodes { get; set; } = new List<InfrastructureNode>();
            public List<InfrastructureService> InfrastructureServices { get; set; } = new List<InfrastructureService>();
        }

        public class EditPage : IHasValidationPanel
        {
            public Metapsi.Ui.User User { get; set; }

            public InfrastructureNode InfrastructureNode { get; set; } = new InfrastructureNode();
            public List<EnvironmentType> EnvironmentTypes { get; set; } = new List<EnvironmentType>();
            public string ValidationMessage { get; set; } = string.Empty;
        }
    }
}
