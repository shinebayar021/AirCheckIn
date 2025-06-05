// File: src/Airport.Server/Services/WebSocketServerService.cs
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Airport.Server.Services
{
    public class WebSocketServerService : BackgroundService
    {
        private readonly HttpListener _httpListener;
        // Use + instead of 0.0.0.0 for HttpListener
        private const string Prefix = "http://+:5001/ws/";

        // олон холболт байж болно, тиймээс ConcurrentDictionary(map) ашиглана.
        private static readonly ConcurrentDictionary<string, WebSocket> _sockets
            = new ConcurrentDictionary<string, WebSocket>();

        public WebSocketServerService()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(Prefix);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _httpListener.Start();
                Console.WriteLine($"[WebSocketServerService] Listening on {Prefix}");
                await base.StartAsync(cancellationToken);
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"[WebSocketServerService] Failed to start: {ex.Message}");
                Console.WriteLine("Try running as administrator or use a different port.");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // zogsoh huselt ireegui uyd
                while (!stoppingToken.IsCancellationRequested)
                {
                    var context = await _httpListener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        // Client holbogdson bol ene taskiig hiine
                        _ = Task.Run(async () =>
                        {
                            var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                            var socket = wsContext.WebSocket;
                            //Guid.NewGuid() ni shine unique tanih temdeg
                            string socketId = Guid.NewGuid().ToString();

                            // Client holboltoo hadgalah esvel nemeh
                            _sockets.TryAdd(socketId, socket);
                            Console.WriteLine($"[WebSocketServerService] Client connected: {socketId}");

                            var buffer = new byte[4 * 1024];
                            // socket clientaas message huleej avaad text baival clientaas irsen message-iig uurchluhguigeer
                            // hariu ilgeene client->server
                            try
                            {
                                while (socket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                                {
                                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                                    if (result.MessageType == WebSocketMessageType.Text)
                                    {
                                        var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                        Console.WriteLine($"[WebSocketServerService] Received from {socketId}: {receivedText}");

                                        // Энэ жишээ код нь echo маягаар буцаах хэсэг:
                                        var echoText = $"Echo from server: {receivedText}";
                                        var echoBytes = Encoding.UTF8.GetBytes(echoText);
                                        await socket.SendAsync(
                                            new ArraySegment<byte>(echoBytes),
                                            WebSocketMessageType.Text,
                                            endOfMessage: true,
                                            cancellationToken: stoppingToken
                                        );
                                    }
                                    else if (result.MessageType == WebSocketMessageType.Close)
                                    {
                                        Console.WriteLine($"[WebSocketServerService] Client disconnected: {socketId}");
                                        break;
                                    }
                                }
                            }
                            catch { /* Huleen avah deer aldaa garsan bol urgeljlehgui. */ }
                            finally
                            {
                                // Holbolt haagdsan bol suljeeneesee salgaj avah
                                _sockets.TryRemove(socketId, out _);
                                if (socket.State != WebSocketState.Closed)
                                {
                                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                                           "Closed by server",
                                                           CancellationToken.None);
                                }
                                socket.Dispose();
                            }
                        }, stoppingToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995)
            {
                // HttpListener zogsoosnoos bolson exception-g end baridag
            }
            catch (OperationCanceledException)
            {
                // Tsutslagdsan bol
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _httpListener.Stop();
            Console.WriteLine("[WebSocketServerService] Listener stopped");

            // Suljeend baigaa buh socket-oo haah
            foreach (var kvp in _sockets)
            {
                var socket = kvp.Value;
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                    }
                    catch { }
                }
            }
            _sockets.Clear();

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _httpListener.Close();
            base.Dispose();
        }

        /// <summary>
        /// Холбогдсон бүх WebSocket рүү текст мессеж broadcast хийнэ.
        /// </summary>
        public static async Task BroadcastAsync(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach (var kvp in _sockets)
            {
                var socket = kvp.Value;

                if (socket != null && socket.State == WebSocketState.Open)
                {
                    tasks.Add(socket.SendAsync(new ArraySegment<byte>(data),
                                               WebSocketMessageType.Text,
                                               endOfMessage: true,
                                               cancellationToken: CancellationToken.None));
                }
            }

            // Бүх send-үүд дуусахыг хүлээх
            if (tasks.Count > 0)
                await Task.WhenAll(tasks);
        }
    }
}