
//namespace Metapsi
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    [DataItem("f4683b8a-d24a-4aa7-80c2-5527b6c29cd6")]
//    public partial class HttpGetRequestHeader : IRecord
//    {
//        [DataItemField("7ca9f632-5031-4cd8-874f-12d51fbef4b0")]
//        [ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [DataItemField("04ec96f7-4537-4f74-b0de-075c0d621f43")]
//        [ScalarTypeName("String")]
//        public System.String RelativeUrl { get; set; } = System.String.Empty;
//        public HttpGateway.HttpGetRequestHeader Clone()
//        {
//            var clone = new HttpGateway.HttpGetRequestHeader();
//            clone.Id = this.Id;
//            clone.RelativeUrl = this.RelativeUrl;
//            return clone;
//        }

//        public static System.Guid GetId(HttpGateway.HttpGetRequestHeader dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpGetRequestHeader).Id;
//        }

//        public static System.String GetRelativeUrl(HttpGateway.HttpGetRequestHeader dataRecord)
//        {
//            return dataRecord.RelativeUrl;
//        }

//        public static System.String GetRelativeUrl(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpGetRequestHeader).RelativeUrl;
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    [DataItem("0d7ff693-98dc-456f-933f-db1781cda9b5")]
//    public partial class HttpPostHeader : IRecord
//    {
//        [DataItemField("1c4674e0-4029-4ca2-a512-d5776f1a4731")]
//        [ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [DataItemField("d35234aa-f689-4448-867f-ad415c2bd2e1")]
//        [ScalarTypeName("String")]
//        public System.String RelativeUrl { get; set; } = System.String.Empty;
//        [DataItemField("8e287f3a-2553-4dd1-b7d6-910d57b8726e")]
//        [ScalarTypeName("String")]
//        public System.String Body { get; set; } = System.String.Empty;
//        public HttpGateway.HttpPostHeader Clone()
//        {
//            var clone = new HttpGateway.HttpPostHeader();
//            clone.Id = this.Id;
//            clone.RelativeUrl = this.RelativeUrl;
//            clone.Body = this.Body;
//            return clone;
//        }

//        public static System.Guid GetId(HttpGateway.HttpPostHeader dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpPostHeader).Id;
//        }

//        public static System.String GetRelativeUrl(HttpGateway.HttpPostHeader dataRecord)
//        {
//            return dataRecord.RelativeUrl;
//        }

//        public static System.String GetRelativeUrl(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpPostHeader).RelativeUrl;
//        }

//        public static System.String GetBody(HttpGateway.HttpPostHeader dataRecord)
//        {
//            return dataRecord.Body;
//        }

//        public static System.String GetBody(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpPostHeader).Body;
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    [DataItem("e04a620b-ccff-4387-b48c-b221eab77f32")]
//    public partial class HttpUrlSegment : IRecord
//    {
//        [DataItemField("fa8a14a8-b5ad-407a-b41a-66d61ef11739")]
//        [ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [DataItemField("481c7f42-237c-4b9b-b07e-ddffb336aabe")]
//        [ScalarTypeName("Int")]
//        public System.Int32 OrderIndex { get; set; }

//        [DataItemField("e363adbf-cdf5-43a3-8c5a-8d98f97a6aa1")]
//        [ScalarTypeName("String")]
//        public System.String Value { get; set; } = System.String.Empty;
//        [DataItemField("8b04a639-3332-479f-a25a-cd977c68f239")]
//        [ScalarTypeName("Id")]
//        public System.Guid RequestHeaderId { get; set; }

//        public HttpGateway.HttpUrlSegment Clone()
//        {
//            var clone = new HttpGateway.HttpUrlSegment();
//            clone.Id = this.Id;
//            clone.OrderIndex = this.OrderIndex;
//            clone.Value = this.Value;
//            clone.RequestHeaderId = this.RequestHeaderId;
//            return clone;
//        }

//        public static System.Guid GetId(HttpGateway.HttpUrlSegment dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpUrlSegment).Id;
//        }

//        public static System.Int32 GetOrderIndex(HttpGateway.HttpUrlSegment dataRecord)
//        {
//            return dataRecord.OrderIndex;
//        }

//        public static System.Int32 GetOrderIndex(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpUrlSegment).OrderIndex;
//        }

//        public static System.String GetValue(HttpGateway.HttpUrlSegment dataRecord)
//        {
//            return dataRecord.Value;
//        }

//        public static System.String GetValue(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpUrlSegment).Value;
//        }

//        public static System.Guid GetRequestHeaderId(HttpGateway.HttpUrlSegment dataRecord)
//        {
//            return dataRecord.RequestHeaderId;
//        }

//        public static System.Guid GetRequestHeaderId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.HttpUrlSegment).RequestHeaderId;
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    [DataItem("ea6be0e9-f076-4ffe-a170-ffb324fe6013")]
//    public partial class InitParameters : IRecord
//    {
//        [DataItemField("3b43d0d8-3253-479f-bcd5-e0727d53fbe1")]
//        [ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [DataItemField("b06ce1cf-8bfb-4054-8390-883b020e633d")]
//        [ScalarTypeName("String")]
//        public System.String ListeningUrl { get; set; } = System.String.Empty;
//        [DataItemField("ff4f942d-94b1-499b-9ec7-e2864d1486cc")]
//        [ScalarTypeName("String")]
//        public System.String WebRootPath { get; set; } = System.String.Empty;
//        [DataItemField("0ca25221-671c-422a-9288-17b1e85a9e6b")]
//        [ScalarTypeName("String")]
//        public System.String HttpGatewayName { get; set; } = System.String.Empty;
//        public HttpGateway.InitParameters Clone()
//        {
//            var clone = new HttpGateway.InitParameters();
//            clone.Id = this.Id;
//            clone.ListeningUrl = this.ListeningUrl;
//            clone.WebRootPath = this.WebRootPath;
//            clone.HttpGatewayName = this.HttpGatewayName;
//            return clone;
//        }

//        public static System.Guid GetId(HttpGateway.InitParameters dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.InitParameters).Id;
//        }

//        public static System.String GetListeningUrl(HttpGateway.InitParameters dataRecord)
//        {
//            return dataRecord.ListeningUrl;
//        }

//        public static System.String GetListeningUrl(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.InitParameters).ListeningUrl;
//        }

//        public static System.String GetWebRootPath(HttpGateway.InitParameters dataRecord)
//        {
//            return dataRecord.WebRootPath;
//        }

//        public static System.String GetWebRootPath(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.InitParameters).WebRootPath;
//        }

//        public static System.String GetHttpGatewayName(HttpGateway.InitParameters dataRecord)
//        {
//            return dataRecord.HttpGatewayName;
//        }

//        public static System.String GetHttpGatewayName(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.InitParameters).HttpGatewayName;
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    [DataItem("e20ff524-71c1-4efa-b9fb-6c443c0969cd")]
//    public partial class QueryStringValue : IRecord
//    {
//        [DataItemField("39ae74e1-848a-420d-ba89-14424368f5d0")]
//        [ScalarTypeName("Id")]
//        public System.Guid Id { get; set; } = System.Guid.NewGuid();
//        [DataItemField("5ebc37e9-2597-44f8-9eaa-07167d6f60e3")]
//        [ScalarTypeName("String")]
//        public System.String Name { get; set; } = System.String.Empty;
//        [DataItemField("66a212fa-bfb2-491d-8b2e-a7cb95eacd61")]
//        [ScalarTypeName("String")]
//        public System.String Value { get; set; } = System.String.Empty;
//        [DataItemField("64b890a6-3069-40c5-bfe1-b11eeeedc0a9")]
//        [ScalarTypeName("Id")]
//        public System.Guid RequestHeaderId { get; set; }

//        public HttpGateway.QueryStringValue Clone()
//        {
//            var clone = new HttpGateway.QueryStringValue();
//            clone.Id = this.Id;
//            clone.Name = this.Name;
//            clone.Value = this.Value;
//            clone.RequestHeaderId = this.RequestHeaderId;
//            return clone;
//        }

//        public static System.Guid GetId(HttpGateway.QueryStringValue dataRecord)
//        {
//            return dataRecord.Id;
//        }

//        public static System.Guid GetId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.QueryStringValue).Id;
//        }

//        public static System.String GetName(HttpGateway.QueryStringValue dataRecord)
//        {
//            return dataRecord.Name;
//        }

//        public static System.String GetName(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.QueryStringValue).Name;
//        }

//        public static System.String GetValue(HttpGateway.QueryStringValue dataRecord)
//        {
//            return dataRecord.Value;
//        }

//        public static System.String GetValue(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.QueryStringValue).Value;
//        }

//        public static System.Guid GetRequestHeaderId(HttpGateway.QueryStringValue dataRecord)
//        {
//            return dataRecord.RequestHeaderId;
//        }

//        public static System.Guid GetRequestHeaderId(IRecord dataRecord)
//        {
//            return (dataRecord as HttpGateway.QueryStringValue).RequestHeaderId;
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// Used for APIs
//        /// </summary>
//    public static partial class HttpListener
//    {
//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class Command
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("67ffd792-3ffa-4850-8f97-9fa39f9e6bf0")]
//            public partial class StartListening : IRecord
//            {
//                [DataItemField("16dabd29-7649-4cb0-9c94-7aa6b923a32d")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public HttpGateway.HttpListener.Command.StartListening Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Command.StartListening();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Command.StartListening dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Command.StartListening).Id;
//                }
//            }
//        }

//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class DemandResultItem
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("1d05f72d-672a-444e-901c-51bbaf4a4165")]
//            public partial class HttpResponse : IRecord
//            {
//                [DataItemField("38d9c727-fd26-4933-8e17-3737fd43d547")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [DataItemField("f28748f3-7cc5-4875-854d-ff37164eb99c")]
//                [ScalarTypeName("String")]
//                public System.String ContentType { get; set; } = System.String.Empty;
//                [DataItemField("3e7aa280-cf84-4267-a34e-70280323a393")]
//                [ScalarTypeName("String")]
//                public System.String ResponseContent { get; set; } = System.String.Empty;
//                [DataItemField("9df3f769-33cc-4030-98c3-144a1e184e95")]
//                [ScalarTypeName("Int")]
//                public System.Int32 ResponseCode { get; set; }

//                public HttpGateway.HttpListener.DemandResultItem.HttpResponse Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.DemandResultItem.HttpResponse();
//                    clone.Id = this.Id;
//                    clone.ContentType = this.ContentType;
//                    clone.ResponseContent = this.ResponseContent;
//                    clone.ResponseCode = this.ResponseCode;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.DemandResultItem.HttpResponse dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.DemandResultItem.HttpResponse).Id;
//                }

//                public static System.String GetContentType(HttpGateway.HttpListener.DemandResultItem.HttpResponse dataRecord)
//                {
//                    return dataRecord.ContentType;
//                }

//                public static System.String GetContentType(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.DemandResultItem.HttpResponse).ContentType;
//                }

//                public static System.String GetResponseContent(HttpGateway.HttpListener.DemandResultItem.HttpResponse dataRecord)
//                {
//                    return dataRecord.ResponseContent;
//                }

//                public static System.String GetResponseContent(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.DemandResultItem.HttpResponse).ResponseContent;
//                }

//                public static System.Int32 GetResponseCode(HttpGateway.HttpListener.DemandResultItem.HttpResponse dataRecord)
//                {
//                    return dataRecord.ResponseCode;
//                }

//                public static System.Int32 GetResponseCode(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.DemandResultItem.HttpResponse).ResponseCode;
//                }
//            }
//        }

//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class DemandSpecifications
//        {
//            /// <summary>
//                        /// Http get request
//                        /// </summary>
//            [DataStructure("096a858a-bc5c-4f01-a5a8-0c0fc45346f8")]
//            public partial class HttpGetRequest : IDataStructure, HttpGateway.IHttpGetRequest
//            {
//                public RecordCollection<HttpGateway.HttpGetRequestHeader> RequestHeader { get; set; } = new RecordCollection<HttpGateway.HttpGetRequestHeader>();
//                public RecordCollection<HttpGateway.QueryStringValue> QueryStringValues { get; set; } = new RecordCollection<HttpGateway.QueryStringValue>();
//                public RecordCollection<HttpGateway.HttpUrlSegment> UrlSegments { get; set; } = new RecordCollection<HttpGateway.HttpUrlSegment>();

//                public static RecordCollection<HttpGateway.HttpGetRequestHeader> GetRequestHeader(HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest dataStructure)
//                {
//                    return dataStructure.RequestHeader;
//                }

//                public static IRecordCollection GetRequestHeader(IDataStructure dataStructure)
//                {
//                    return (dataStructure as HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest).RequestHeader;
//                }

//                public static RecordCollection<HttpGateway.QueryStringValue> GetQueryStringValues(HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest dataStructure)
//                {
//                    return dataStructure.QueryStringValues;
//                }

//                public static IRecordCollection GetQueryStringValues(IDataStructure dataStructure)
//                {
//                    return (dataStructure as HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest).QueryStringValues;
//                }

//                public static RecordCollection<HttpGateway.HttpUrlSegment> GetUrlSegments(HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest dataStructure)
//                {
//                    return dataStructure.UrlSegments;
//                }

//                public static IRecordCollection GetUrlSegments(IDataStructure dataStructure)
//                {
//                    return (dataStructure as HttpGateway.HttpListener.DemandSpecifications.HttpGetRequest).UrlSegments;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial class HttpGetRequest
//            {
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataStructure("b04053af-9b71-4665-8cb1-da332bbd1375")]
//            public partial class HttpPostRequest : IDataStructure, HttpGateway.IHttpPostRequest
//            {
//                public RecordCollection<HttpGateway.HttpPostHeader> RequestHeader { get; set; } = new RecordCollection<HttpGateway.HttpPostHeader>();
//                public RecordCollection<HttpGateway.HttpUrlSegment> UrlSegments { get; set; } = new RecordCollection<HttpGateway.HttpUrlSegment>();
//                public DataStructureDiff DiffToPrevious(HttpGateway.HttpListener.DemandSpecifications.HttpPostRequest previous)
//                {
//                    var diff = new DataStructureDiff();
//                    diff = Compare.MergeDiffs(diff, Compare.GetTypedCollectionDiff(previous.RequestHeader, this.RequestHeader, "RequestHeader"));
//                    diff = Compare.MergeDiffs(diff, Compare.GetTypedCollectionDiff(previous.UrlSegments, this.UrlSegments, "UrlSegments"));
//                    return diff;
//                }

//                public static RecordCollection<HttpGateway.HttpPostHeader> GetRequestHeader(HttpGateway.HttpListener.DemandSpecifications.HttpPostRequest dataStructure)
//                {
//                    return dataStructure.RequestHeader;
//                }

//                public static IRecordCollection GetRequestHeader(IDataStructure dataStructure)
//                {
//                    return (dataStructure as HttpGateway.HttpListener.DemandSpecifications.HttpPostRequest).RequestHeader;
//                }

//                public static RecordCollection<HttpGateway.HttpUrlSegment> GetUrlSegments(HttpGateway.HttpListener.DemandSpecifications.HttpPostRequest dataStructure)
//                {
//                    return dataStructure.UrlSegments;
//                }

//                public static IRecordCollection GetUrlSegments(IDataStructure dataStructure)
//                {
//                    return (dataStructure as HttpGateway.HttpListener.DemandSpecifications.HttpPostRequest).UrlSegments;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            public partial class HttpPostRequest
//            {
//            }
//        }

//        /// <summary>
//                /// 
//                /// </summary>
//        public static partial class Event
//        {
//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("34589504-9c96-4cf9-8426-32781925649d")]
//            public partial class HttpRequestDone : IRecord
//            {
//                [DataItemField("7d938b37-2717-4c68-93c8-727d5ba87ab6")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [DataItemField("bec84e2d-ebb5-43b2-bf19-4f82c1ef7dc6")]
//                [ScalarTypeName("String")]
//                public System.String Path { get; set; } = System.String.Empty;
//                [DataItemField("fd3d1bc7-3a67-47c6-bd5e-eb530ece1ed5")]
//                [ScalarTypeName("String")]
//                public System.String RequestBody { get; set; } = System.String.Empty;
//                [DataItemField("a6fa78a9-3eee-464b-8398-6b1b6301e419")]
//                [ScalarTypeName("String")]
//                public System.String Method { get; set; } = System.String.Empty;
//                [DataItemField("9c0d95ed-3d6e-404a-ba38-80a856a49b70")]
//                [ScalarTypeName("Int")]
//                public System.Int32 ResponseCode { get; set; }

//                [DataItemField("8538f39a-8f35-4b87-a7ec-3df35be6422d")]
//                [ScalarTypeName("String")]
//                public System.String ResponseBody { get; set; } = System.String.Empty;
//                public HttpGateway.HttpListener.Event.HttpRequestDone Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Event.HttpRequestDone();
//                    clone.Id = this.Id;
//                    clone.Path = this.Path;
//                    clone.RequestBody = this.RequestBody;
//                    clone.Method = this.Method;
//                    clone.ResponseCode = this.ResponseCode;
//                    clone.ResponseBody = this.ResponseBody;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).Id;
//                }

//                public static System.String GetPath(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.Path;
//                }

//                public static System.String GetPath(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).Path;
//                }

//                public static System.String GetRequestBody(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.RequestBody;
//                }

//                public static System.String GetRequestBody(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).RequestBody;
//                }

//                public static System.String GetMethod(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.Method;
//                }

//                public static System.String GetMethod(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).Method;
//                }

//                public static System.Int32 GetResponseCode(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.ResponseCode;
//                }

//                public static System.Int32 GetResponseCode(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).ResponseCode;
//                }

//                public static System.String GetResponseBody(HttpGateway.HttpListener.Event.HttpRequestDone dataRecord)
//                {
//                    return dataRecord.ResponseBody;
//                }

//                public static System.String GetResponseBody(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestDone).ResponseBody;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("96d93bc6-3fc9-4046-bb83-b06f6822bf69")]
//            public partial class HttpRequestReceived : IRecord
//            {
//                [DataItemField("c46278eb-2b14-4afe-85d8-9d1406e0c71c")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [DataItemField("f33688d1-1f36-4e5c-b9fd-b0cc44f80ffb")]
//                [ScalarTypeName("String")]
//                public System.String Path { get; set; } = System.String.Empty;
//                [DataItemField("842572e0-c710-434b-8ca5-ae5e2f5fee09")]
//                [ScalarTypeName("String")]
//                public System.String Body { get; set; } = System.String.Empty;
//                [DataItemField("9cef4b3f-0159-4575-963a-88e697f28596")]
//                [ScalarTypeName("String")]
//                public System.String Method { get; set; } = System.String.Empty;
//                public HttpGateway.HttpListener.Event.HttpRequestReceived Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Event.HttpRequestReceived();
//                    clone.Id = this.Id;
//                    clone.Path = this.Path;
//                    clone.Body = this.Body;
//                    clone.Method = this.Method;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Event.HttpRequestReceived dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestReceived).Id;
//                }

//                public static System.String GetPath(HttpGateway.HttpListener.Event.HttpRequestReceived dataRecord)
//                {
//                    return dataRecord.Path;
//                }

//                public static System.String GetPath(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestReceived).Path;
//                }

//                public static System.String GetBody(HttpGateway.HttpListener.Event.HttpRequestReceived dataRecord)
//                {
//                    return dataRecord.Body;
//                }

//                public static System.String GetBody(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestReceived).Body;
//                }

//                public static System.String GetMethod(HttpGateway.HttpListener.Event.HttpRequestReceived dataRecord)
//                {
//                    return dataRecord.Method;
//                }

//                public static System.String GetMethod(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.HttpRequestReceived).Method;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("376aaf93-69f3-41c5-a21f-b2daad89fea7")]
//            public partial class Initialized : IRecord
//            {
//                [DataItemField("9aecb075-80a3-4d27-819f-90dc505a22e9")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                [DataItemField("39cc5404-f26d-4d25-948e-e836beaccb82")]
//                [ScalarTypeName("String")]
//                public System.String HttpGatewayName { get; set; } = System.String.Empty;
//                public HttpGateway.HttpListener.Event.Initialized Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Event.Initialized();
//                    clone.Id = this.Id;
//                    clone.HttpGatewayName = this.HttpGatewayName;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Event.Initialized dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.Initialized).Id;
//                }

//                public static System.String GetHttpGatewayName(HttpGateway.HttpListener.Event.Initialized dataRecord)
//                {
//                    return dataRecord.HttpGatewayName;
//                }

//                public static System.String GetHttpGatewayName(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.Initialized).HttpGatewayName;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("52e0e9d5-d503-4266-982d-f80a5e4e7313")]
//            public partial class ListeningStarted : IRecord
//            {
//                [DataItemField("c440eb65-7a2e-487a-a11e-8d0c07093ced")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public HttpGateway.HttpListener.Event.ListeningStarted Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Event.ListeningStarted();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Event.ListeningStarted dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.ListeningStarted).Id;
//                }
//            }

//            /// <summary>
//                        /// 
//                        /// </summary>
//            [DataItem("273807f9-55bd-4829-a8d1-fceee515b5fb")]
//            public partial class SuccessfulShutdown : IRecord
//            {
//                [DataItemField("7ebd2b2b-965c-4872-8fdd-dd9f488fd62d")]
//                [ScalarTypeName("Id")]
//                public System.Guid Id { get; set; } = System.Guid.NewGuid();
//                public HttpGateway.HttpListener.Event.SuccessfulShutdown Clone()
//                {
//                    var clone = new HttpGateway.HttpListener.Event.SuccessfulShutdown();
//                    clone.Id = this.Id;
//                    return clone;
//                }

//                public static System.Guid GetId(HttpGateway.HttpListener.Event.SuccessfulShutdown dataRecord)
//                {
//                    return dataRecord.Id;
//                }

//                public static System.Guid GetId(IRecord dataRecord)
//                {
//                    return (dataRecord as HttpGateway.HttpListener.Event.SuccessfulShutdown).Id;
//                }
//            }
//        }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// Http get request
//        /// </summary>
//    public partial interface IHttpGetRequest : IDataStructure
//    {
//        public RecordCollection<HttpGateway.HttpGetRequestHeader> RequestHeader { get; set; }

//        public RecordCollection<HttpGateway.QueryStringValue> QueryStringValues { get; set; }

//        public RecordCollection<HttpGateway.HttpUrlSegment> UrlSegments { get; set; }
//    }
//}

//namespace HttpGateway
//{
//    /// <summary>
//        /// 
//        /// </summary>
//    public partial interface IHttpPostRequest : IDataStructure
//    {
//        public RecordCollection<HttpGateway.HttpPostHeader> RequestHeader { get; set; }

//        public RecordCollection<HttpGateway.HttpUrlSegment> UrlSegments { get; set; }
//    }
//}