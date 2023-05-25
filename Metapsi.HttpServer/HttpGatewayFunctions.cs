using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System;

namespace Metapsi
{
    public static partial class HttpServer
    {
        public class ListeningStarted : IData
        {
            public int Port { get; set; }
        }

        public class Request : IRecord
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Method { get; set; }
            public string Path { get; set; }
            public string Body { get; set; }

            public List<Query> Query { get; set; } = new List<Query>();

            public IEnumerable<string> Segments => Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public class Query : IRecord
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public class Response
        {
            public Guid Id { get; set; } = System.Guid.NewGuid();
            public string ContentType { get; set; } = "application/json";
            public string ResponseContent { get; set; } = System.String.Empty;
            public int ResponseCode { get; set; }
        }

        public class Configuration
        {
            public int Port { get; set; }
            public string WebRootPath { get; set; }
        }

        public static Request<Response, Request> GetResponse { get; set; } = new Request<Response, Request>(nameof(GetResponse));

        public class State
        {
            public string ListeningUrl { get; set; }
            public string WebRootPath { get; set; }
            public IWebHost WebHost { get; set; }
            public Task WebHostTask { get; set; }
        }

        //public static partial class Event
        //{
        //    public partial class HttpRequestDone : IData { }
        //    public partial class HttpRequestReceived: IData { }
        //    public partial class ListeningStarted : IData { }
        //    public partial class SuccessfulShutdown : IData { }
        //}

        public static async Task StartListening(CommandContext commandContext, State state, Configuration configuration)
        {
            state.ListeningUrl = $"http://0.0.0.0:{configuration.Port}";
            state.WebRootPath = configuration.WebRootPath;

            WebHostBuilder webHostBuilder = new Microsoft.AspNetCore.Hosting.WebHostBuilder();
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls(state.ListeningUrl);
            if (!string.IsNullOrWhiteSpace(state.WebRootPath))
            {
                webHostBuilder.UseWebRoot(state.WebRootPath);
            }
            webHostBuilder.Configure(app =>
            {
                if (!string.IsNullOrWhiteSpace(state.WebRootPath))
                {
                    app.UseDefaultFiles();
                    app.UseStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = new PhysicalFileProvider(state.WebRootPath),
                        RequestPath = ""
                    });
                }
                app.Run(async (httpContext) =>
                {
                    if (httpContext.Request.Path.Value.Contains("favicon.ico"))
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    Request request = new Request()
                    {
                        Method = "GET",
                        Path = httpContext.Request.Path.Value,
                    };

                    switch (httpContext.Request.Method)
                    {
                        case "GET":
                            {
                                foreach (var query in httpContext.Request.Query)
                                {
                                    request.Query.Add(new Query()
                                    {
                                        Key = query.Key,
                                        Value = query.Value
                                    });
                                }
                            }
                            break;
                        case "POST":
                            {
                                using (StreamReader streamReader = new StreamReader(httpContext.Request.Body))
                                {
                                    request.Body = await streamReader.ReadToEndAsync();
                                }
                            }
                            break;
                    }

                    var respose = await commandContext.Do(GetResponse, request);

                    httpContext.Response.StatusCode = respose.ResponseCode;
                    httpContext.Response.ContentType = respose.ContentType;
                    await httpContext.Response.WriteAsync(respose.ResponseContent);
                });
            });
            state.WebHost = webHostBuilder.Build();
            state.WebHostTask = state.WebHost.RunAsync();
            commandContext.PostEvent(new ListeningStarted() { Port = configuration.Port });
        }

        public static async Task Stop(CommandContext commandContext, State state)
        {
            await state.WebHost.StopAsync();
        }
    }
}
