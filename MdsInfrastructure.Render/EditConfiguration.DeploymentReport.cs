//using MdsCommon;
//using Metapsi;
//using Metapsi.Hyperapp;
//using Metapsi.Syntax;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using MdsCommon.Controls;
//using System.Collections.Generic;
//using Metapsi.Html;

//namespace MdsInfrastructure
//{
//    public static partial class EditConfiguration
//    {
//        public static Var<IVNode> ChangesReport(this LayoutBuilder b, System.Collections.Generic.List<ServiceChange> serviceChanges)
//        {
//            List<Var<IVNode>> childNodes = new();

//            foreach (var service in serviceChanges)
//            {
//                switch (service.ServiceChangeType)
//                {
//                    case ChangeType.Added:
//                        childNodes.Add(b.NewService(service));
//                        break;
//                    case ChangeType.Changed:
//                        childNodes.Add(b.ChangedService(service));
//                        break;
//                    case ChangeType.Removed:
//                        childNodes.Add(b.RemovedService(service));
//                        break;
//                }
//            }

//            return b.HtmlDiv(
//                b =>
//                {
//                    b.SetClass("flex flex-col space-y-4 pt-4");
//                },
//                childNodes.ToArray());
//        }

//        public static Var<IVNode> NewService(this LayoutBuilder b, ServiceChange serviceChange)
//        {
//            return b.StyledDiv(
//                "flex flex-col bg-green-100 p-4 rounded text-green-800",
//                b.StyledDiv(
//                    "flex flex-row items-center  space-x-4",
//                    b.Bold(serviceChange.ServiceName),
//                    b.Svg(Icon.Enter),
//                    b.StyledSpan(
//                        "grid w-full justify-end",
//                        b.TextSpan($"Install {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}"))),
//                b.ListParameterChanges(serviceChange));
//        }

//        public static Var<IVNode> RemovedService(this LayoutBuilder b, ServiceChange serviceChange)
//        {
//            return b.StyledDiv(
//                "flex flex-col bg-red-100 p-4 rounded text-red-800",
//                b.StyledDiv(
//                    "flex flex-row items-center space-x-4",
//                    b.Bold(serviceChange.ServiceName),
//                    b.Svg(Icon.Remove),
//                    b.StyledSpan("grid w-full justify-end", b.TextSpan($"Uninstall {serviceChange.ProjectName.OldValue} {serviceChange.ProjectVersionTag.OldValue} from {serviceChange.NodeName.OldValue}"))),
//                b.ListParameterChanges(serviceChange));
//        }

//        public static Var<IVNode> ChangedService(this LayoutBuilder b, ServiceChange serviceChange)
//        {
//            var sameVersion =
//                serviceChange.ProjectName.OldValue == serviceChange.ProjectName.NewValue &&
//                serviceChange.ProjectVersionTag.OldValue == serviceChange.ProjectVersionTag.NewValue;

//            return b.StyledDiv(
//                "flex flex-col bg-sky-200 p-4 rounded text-sky-800",
//                b.StyledDiv(
//                    "flex flex-row items-center space-x-4",
//                    b.Bold(serviceChange.ServiceName),
//                    b.Svg(Icon.Changed),
//                    b.StyledSpan(
//                        "grid w-full justify-end",
//                        sameVersion ?
//                        b.TextSpan($"Restart {serviceChange.ProjectName.NewValue} {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}") :
//                        b.TextSpan($"Upgrade {serviceChange.ProjectName.NewValue} from {serviceChange.ProjectVersionTag.OldValue} to {serviceChange.ProjectVersionTag.NewValue} on {serviceChange.NodeName.NewValue}"))),
//                b.ListParameterChanges(serviceChange));
//        }

//        public static Var<IVNode> ListParameterChanges(this LayoutBuilder b, ServiceChange serviceChange)
//        {
//            List<Var<IVNode>> nodes = new();

//            foreach (var parameterChange in serviceChange.ServiceParameterChanges)
//            {
//                if(parameterChange.OldValue != parameterChange.NewValue)
//                {
//                    nodes.Add(b.ParameterChange(parameterChange));
//                }
//            }

//            return b.StyledDiv("flex flex-col space-y-1 text-sm py-2", nodes.ToArray());
//        }

//        public static Var<IVNode> ParameterChange(this LayoutBuilder b, ServicePropertyChange parameterChange)
//        {
//            // null is different from string.Empty in this case.
//            // null = parameter does not even exist, while string.Empty is valid parameter value

//            // removed, no new value
//            if (parameterChange.NewValue == null)
//            {
//                return b.StyledSpan(
//                    "text-red-800 line-through flex flex-row space-x-4",
//                    b.Bold(parameterChange.PropertyName),
//                    b.TextSpan(parameterChange.OldValue));
//            }

//            // added, no old value
//            if (parameterChange.OldValue == null)
//            {
//                return b.StyledSpan(
//                    "text-green-800 flex flex-row space-x-4",
//                    b.Bold(parameterChange.PropertyName),
//                    b.TextSpan(parameterChange.NewValue));
//            }

//            // value changed
//            {
//                return b.StyledSpan(
//                    "text-sky-800 flex flex-row space-x-4",
//                    b.Bold(parameterChange.PropertyName),
//                    b.TextSpan(parameterChange.OldValue),
//                    b.TextSpan("➔"),
//                    b.TextSpan(parameterChange.NewValue));
//            }
//        }
//    }
//}
