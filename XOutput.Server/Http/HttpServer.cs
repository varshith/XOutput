﻿using NLog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using XOutput.Core.DependencyInjection;
using XOutput.Core.External;
using XOutput.Server.Rest;
using XOutput.Server.Websocket;

namespace XOutput.Server.Http
{
    public class HttpServer
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly CommandRunner commandRunner;
        private readonly RestService restService;
        private readonly WebSocketService webSocketService;

        private bool running;
        private CancellationTokenSource cancellationTokenSource;
        private HttpListener listener;

        [ResolverMethod]
        public HttpServer(CommandRunner commandRunner, RestService restService, WebSocketService webSocketService)
        {
            this.commandRunner = commandRunner;
            this.restService = restService;
            this.webSocketService = webSocketService;
        }

        public void Start(string uri)
        {
            if (running)
            {
                return;
            }
            cancellationTokenSource = new CancellationTokenSource();
            listener = new HttpListener();
            listener.Prefixes.Add(uri);
            try
            {
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                logger.Warn(ex);
                AddPersmissions(uri);
                listener = new HttpListener();
                listener.Prefixes.Add(uri);
                listener.Start();
            }
            running = true;
            Task.Run(() => AcceptClientsAsync(listener));
        }

        public void AddPersmissions(string uri)
        {
            var domainUser = Environment.UserDomainName + "\\\\" + Environment.UserName;
            var process = commandRunner.CreatePowershell($"netsh http add urlacl url={uri} user={domainUser}");
            commandRunner.RunProcess(process);
        }

        public void Stop()
        {
            if (listener != null && running)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                    running = false;
                    listener.Stop();
                    listener = null;
                }
                catch
                {
                    logger.Error("Failed to stop http server");
                }
            }
        }

        private async Task AcceptClientsAsync(HttpListener server)
        {
            while (running)
            {
                try
                {
                    var httpContext = await server.GetContextAsync();
                    if (!webSocketService.Handle(httpContext, cancellationTokenSource.Token) && !restService.Handle(httpContext))
                    {
                        httpContext.Response.StatusCode = 404;
                        httpContext.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (server.IsListening)
                    {
                        logger.Error(ex, "Failed to handle connection");
                    }
                }
            }
        }
    }
}
