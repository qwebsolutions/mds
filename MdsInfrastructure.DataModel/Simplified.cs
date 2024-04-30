using Metapsi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using static MdsInfrastructure.Simplified;

namespace MdsInfrastructure
{
    public class ConversionResult
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Changes { get; set; } = new List<string>();
        public string Result { get; set; } = string.Empty;
    }

    public static class Simplified
    {
        public class Configuration
        {
            public string Name { get; set; }
            public List<Service> Services { get; set; } = new();
            public List<Variable> Variables { get; set; } = new();
            public List<string> Applications { get; set; } = new();
        }

        public class Parameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public string VariableName { get; set; }
        }

        public class Service
        {
            public string Name { get; set; }
            public string Application { get; set; }
            public string Project { get; set; }
            public string Version { get; set; }
            public string Node { get; set; }
            public bool Enabled { get; set; }
            public List<Parameter> Parameters { get; set; } = new();
            public List<Note> Notes { get; set; } = new();
        }

        public class Variable
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class Note
        {
            public string Type { get; set; }
            public string Reference { get; set; }
            public string Text { get; set; }
        }

        public static IEnumerable<string> GetDuplicates<T>(IEnumerable<T> item, Func<T, string> byName, string entityType)
        {
            return item.GroupBy(byName).Where(x => x.Count() > 1).Select(x => x.Key).Select(x => $"{entityType} {x} is declared multiple times");
        }

        public static List<string> Validate(this Configuration configuration, ExternalConfiguration externalConfiguration)
        {
            List<string> validationErrors = new List<string>();

            validationErrors.AddRange(GetDuplicates(configuration.Services, x => x.Name, "Service"));
            validationErrors.AddRange(GetDuplicates(configuration.Applications, x => x, "Application"));
            validationErrors.AddRange(GetDuplicates(configuration.Variables, x => x.Name, "Variable"));

            foreach (var service in configuration.Services)
            {
                if (!configuration.Applications.Contains(service.Application))
                {
                    validationErrors.Add($"Service {service.Name} does not use a valid application: {service.Application}");
                }

                var serviceProject = externalConfiguration.Projects.SingleOrDefault(x => x.Name == service.Project);

                if (serviceProject == null)
                {
                    validationErrors.Add($"Service {service.Name} does not use a valid project: {service.Project}");
                }
                else
                {
                    if (!serviceProject.Versions.Any(x => x.VersionTag == service.Version))
                    {
                        validationErrors.Add($"Service {service.Name} does not use a valid version: {service.Version}");
                    }
                }

                validationErrors.AddRange(GetDuplicates(service.Parameters, x => x.Name, "Parameter"));

                foreach (var parameter in service.Parameters)
                {
                    if (!string.IsNullOrEmpty(parameter.Value) && !string.IsNullOrEmpty(parameter.VariableName))
                    {
                        validationErrors.Add($"Service {service.Name} uses both value and variable binding for parameter {parameter.Name}");
                    }

                    var parameterTypeCodes = externalConfiguration.ParameterTypes.Select(x => x.Code).ToList();

                    if (!parameterTypeCodes.Contains(parameter.Type))
                    {
                        validationErrors.Add($"Service {service.Name} does not use a valid parameter type {parameter.Type} for {parameter.Name}. Available types are {string.Join(",", parameterTypeCodes)}.");
                    }
                }

                var parameterBindings = service.Parameters.Where(x => !string.IsNullOrEmpty(x.VariableName));

                foreach (var parameterBinding in parameterBindings)
                {
                    if (!configuration.Variables.Any(x => x.Name == parameterBinding.VariableName))
                    {
                        validationErrors.Add($"Service {service.Name} does not use a valid variable {parameterBinding.VariableName} for parameter {parameterBinding.Name}");
                    }
                }

                if (!externalConfiguration.Nodes.Any(x => x.NodeName == service.Node))
                {
                    validationErrors.Add($"Service {service.Name} does not use a valid node: {service.Node}");
                }

                foreach (var serviceNote in service.Notes)
                {
                    var noteTypeCodes = externalConfiguration.NoteTypes.Select(x => x.Code).ToList();
                    if (!noteTypeCodes.Contains(serviceNote.Type))
                    {
                        validationErrors.Add($"Service {service.Name} does not use a valid note type {serviceNote.Type}. Available types are {string.Join(",", noteTypeCodes)}.");
                    }
                }
            }

            return validationErrors;
        }

        public static InfrastructureConfiguration Sorted(this InfrastructureConfiguration input)
        {
            input.Applications = input.Applications.OrderBy(x => x.Name).ToList();
            input.InfrastructureVariables = input.InfrastructureVariables.OrderBy(x => x.VariableName).ToList();

            foreach (var service in input.InfrastructureServices)
            {
                service.InfrastructureServiceParameterDeclarations = service.InfrastructureServiceParameterDeclarations.OrderBy(x => x.ParameterName).ToList();
                service.InfrastructureServiceNotes = service.InfrastructureServiceNotes.OrderBy(x => x.Note).ToList();
            }

            input.InfrastructureServices = input.InfrastructureServices.OrderBy(x => x.ServiceName).ToList();

            return input;
        }

        public static InfrastructureConfiguration Complicate(this Configuration input, InfrastructureConfiguration saved, ExternalConfiguration externalConfiguration)
        {
            if (input.Name != saved.Name)
            {
                throw new System.Exception("Configuration names to not match");
            }

            saved = saved.Sorted();

            InfrastructureConfiguration result = new InfrastructureConfiguration()
            {
                Name = input.Name,
                Id = saved.Id
            };

            foreach (var application in input.Applications)
            {
                var existing = saved.Applications.SingleOrDefault(x => x.Name == application, new());

                result.Applications.Add(
                    new Application()
                    {
                        ConfigurationHeaderId = saved.Id,
                        Name = application,
                        Id = existing.Id
                    });
            }

            foreach (var variable in input.Variables)
            {
                var existing = saved.InfrastructureVariables.SingleOrDefault(x => x.VariableName == variable.Name, new());

                result.InfrastructureVariables.Add(new InfrastructureVariable()
                {
                    ConfigurationHeaderId = saved.Id,
                    Id = existing.Id,
                    VariableName = variable.Name,
                    VariableValue = variable.Value
                });
            }

            foreach (var service in input.Services)
            {
                var existingService = saved.InfrastructureServices.SingleOrDefault(x => x.ServiceName == service.Name);

                if (existingService == null)
                {
                    existingService = new InfrastructureService();
                }

                var toAdd = new InfrastructureService()
                {
                    ConfigurationHeaderId = saved.Id,
                    Id = existingService.Id,
                    ServiceName = service.Name,
                    ApplicationId = result.Applications.Single(x => x.Name == service.Application).Id,
                    Enabled = service.Enabled,
                    InfrastructureNodeId = externalConfiguration.Nodes.Single(x => x.NodeName == service.Node).Id,
                };

                var project = externalConfiguration.Projects.Single(x => x.Name == service.Project);
                toAdd.ProjectId = project.Id;
                var version = project.Versions.Single(x => x.VersionTag == service.Version);
                toAdd.ProjectVersionId = version.Id;

                foreach (var parameter in service.Parameters)
                {
                    var existingParameterDeclaration = existingService.InfrastructureServiceParameterDeclarations.SingleOrDefault(
                        x => x.ParameterName == parameter.Name);

                    if (existingParameterDeclaration == null)
                    {
                        existingParameterDeclaration = new InfrastructureServiceParameterDeclaration();
                    }

                    var newDeclaration = new InfrastructureServiceParameterDeclaration()
                    {
                        Id = existingParameterDeclaration.Id,
                        InfrastructureServiceId = toAdd.Id,
                        ParameterName = parameter.Name,
                        ParameterTypeId = externalConfiguration.ParameterTypes.Single(x => x.Code == parameter.Type).Id,
                    };

                    toAdd.InfrastructureServiceParameterDeclarations.Add(newDeclaration);

                    if (!string.IsNullOrEmpty(parameter.VariableName))
                    {
                        var boundVariable = result.InfrastructureVariables.Single(x => x.VariableName == parameter.VariableName);

                        var existingBinding = existingParameterDeclaration.InfrastructureServiceParameterBindings.SingleOrDefault(x => x.InfrastructureVariableId == boundVariable.Id);

                        // This variable was not changed at all, so try to keep it with the same ID
                        if (existingBinding != null)
                        {
                            newDeclaration.InfrastructureServiceParameterBindings.Add(existingBinding);
                        }
                        else
                        {
                            newDeclaration.InfrastructureServiceParameterBindings.Add(new InfrastructureServiceParameterBinding()
                            {
                                InfrastructureServiceParameterDeclarationId = newDeclaration.Id,
                                InfrastructureVariableId = boundVariable.Id
                            });
                        }
                    }
                    else
                    {
                        var existingValue = existingParameterDeclaration.InfrastructureServiceParameterValues.SingleOrDefault(x => x.ParameterValue == parameter.Value);
                        if (existingValue != null)
                        {
                            // Value didn't change at all
                            newDeclaration.InfrastructureServiceParameterValues.Add(existingValue);
                        }
                        else
                        {
                            newDeclaration.InfrastructureServiceParameterValues.Add(new InfrastructureServiceParameterValue()
                            {
                                InfrastructureServiceParameterDeclarationId = newDeclaration.Id,
                                ParameterValue = parameter.Value
                            });
                        }
                    }
                }

                foreach (var note in service.Notes)
                {
                    var noteTypeId = externalConfiguration.NoteTypes.Single(x => x.Code == note.Type).Id;
                    var exactMatch = existingService.InfrastructureServiceNotes.SingleOrDefault(x => x.NoteTypeId == noteTypeId && x.Note == note.Text && x.Reference == note.Reference);

                    // Keep the note unchanged if it is an exact match

                    if (exactMatch != null)
                    {
                        toAdd.InfrastructureServiceNotes.Add(exactMatch);
                    }
                    else
                    {
                        toAdd.InfrastructureServiceNotes.Add(new InfrastructureServiceNote()
                        {
                            InfrastructureServiceId = toAdd.Id,
                            Note = note.Text,
                            Reference = note.Reference,
                            NoteTypeId = noteTypeId
                        });
                    }
                }

                result.InfrastructureServices.Add(toAdd);
            }

            return result.Sorted();
        }

        public static Configuration Simplify(this InfrastructureConfiguration infrastructureConfiguration, ExternalConfiguration externalConfiguration)
        {
            return new Configuration()
            {
                Name = infrastructureConfiguration.Name,
                Applications = infrastructureConfiguration.Applications.Select(x => x.Name).Order().ToList(),
                Services = infrastructureConfiguration.InfrastructureServices.Select(x => x.Simplify(infrastructureConfiguration, externalConfiguration)).OrderBy(x => x.Name).ToList(),
                Variables = infrastructureConfiguration.InfrastructureVariables.Select(x => new Variable()
                {
                    Name = x.VariableName,
                    Value = x.VariableValue
                }).OrderBy(x => x.Name).ToList()
            };
        }

        public static async Task<string> SerializeSimplified(this InfrastructureConfiguration infrastructureConfiguration, ExternalConfiguration externalConfiguration)
        {
            // So weird that we need to write all this
            var simplified = infrastructureConfiguration.Simplify(externalConfiguration);
            using (MemoryStream outputStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(outputStream, simplified, options: Simplified.SerializerOptions);
                await outputStream.FlushAsync();
                outputStream.Position = 0;
                var json = await new StreamReader(outputStream).ReadToEndAsync();
                return (json.Trim() + Environment.NewLine).ReplaceLineEndings("\r\n");
            }
        }

        public static Service Simplify(this InfrastructureService infrastructureService, InfrastructureConfiguration configuration, ExternalConfiguration externalConfiguration)
        {
            return new Service()
            {
                Application = configuration.Applications.Single(x => x.Id == infrastructureService.ApplicationId).Name,
                Enabled = infrastructureService.Enabled,
                Name = infrastructureService.ServiceName,
                Node = externalConfiguration.Nodes.Single(x => x.Id == infrastructureService.InfrastructureNodeId).NodeName,
                Notes = infrastructureService.InfrastructureServiceNotes.Select(x => new Note()
                {
                    Reference = x.Reference,
                    Text = x.Note,
                    Type = externalConfiguration.NoteTypes.SingleOrDefault(noteType => noteType.Id == x.NoteTypeId, new NoteType() { Code = string.Empty }).Code,
                }).OrderBy(x => x.Text).ToList(),
                Project = externalConfiguration.Projects.Single(x => x.Id == infrastructureService.ProjectId).Name,
                Version = externalConfiguration.Projects.SelectMany(x => x.Versions).SingleOrDefault(x => x.Id == infrastructureService.ProjectVersionId).VersionTag,
                Parameters = infrastructureService.InfrastructureServiceParameterDeclarations.Select(x => new Parameter()
                {
                    Name = x.ParameterName,
                    Type = externalConfiguration.ParameterTypes.SingleOrDefault(pt => pt.Id == x.ParameterTypeId, new ParameterType()
                    {
                        Code = "string"
                    }).Code,
                    Value = x.InfrastructureServiceParameterValues.SingleOrDefault(
                        pv => pv.InfrastructureServiceParameterDeclarationId == x.Id,
                        new InfrastructureServiceParameterValue()
                        {
                            ParameterValue = null,
                        }).ParameterValue,
                    VariableName = GetVariableName(configuration, x)
                }).OrderBy(x => x.Name).ToList()
            };
        }

        private static string GetVariableName(InfrastructureConfiguration configuration, InfrastructureServiceParameterDeclaration parameter)
        {
            var variableBinding = parameter.InfrastructureServiceParameterBindings.SingleOrDefault(x => x.InfrastructureServiceParameterDeclarationId == parameter.Id);
            if (variableBinding == null)
                return null;

            var infrastructureVariable = configuration.InfrastructureVariables.SingleOrDefault(x => x.Id == variableBinding.InfrastructureVariableId);
            if (infrastructureVariable == null)
                return null;

            return infrastructureVariable.VariableName;
        }

        public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
    }


    public class ExternalConfiguration
    {
        public List<InfrastructureNode> Nodes { get; set; } = new();
        public List<NoteType> NoteTypes { get; set; } = new();
        public List<MdsCommon.Project> Projects { get; set; } = new();
        public List<ParameterType> ParameterTypes { get; set; } = new();
    }
}
