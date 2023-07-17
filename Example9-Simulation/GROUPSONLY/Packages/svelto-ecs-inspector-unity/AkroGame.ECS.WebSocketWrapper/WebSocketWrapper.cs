#if SIMPLE_WEB_TRANSPORT
using AkroGame.ECS.Websocket;
using System;
using System.Threading.Tasks;
using JamesFrowen.SimpleWeb;
using Svelto.ECS;
using UnityEngine;

public static class SveltoInspector
{
    public static void Attach(EnginesRoot enginesRoot)
    {
        WebSocketWrapper wrapper = new WebSocketWrapper();

        InspectorService inspector = new InspectorService(wrapper, enginesRoot);
        
        Task.Run(() => StartWebSocket(wrapper));

        Update(inspector);
    }

    static async void Update(InspectorService inspector)
    {
        DateTime time = DateTime.Now;

        while (Application.isPlaying)
        {
            inspector.Update((DateTime.Now - time));

            await Task.Yield();
        }

        _mustQuit = true;
    }

    static async void StartWebSocket(WebSocketWrapper wrapper)
    {
        while (_mustQuit == false)
        {
            wrapper.server.ProcessMessageQueue();

            await Task.Delay(100);
        }

        wrapper.server.Stop();
    }

    public class WebSocketWrapper: IWebSocket
    {
        internal readonly SimpleWebServer server;

        public WebSocketWrapper()
        {
            var tcpConfig = new TcpConfig(true, 5000, 5000);
            server = new SimpleWebServer(5000, tcpConfig, 32000, 5000, default);
            // listen for events
            server.onDisconnect += (id) => { OnClose?.Invoke(id); };
            server.onData += (id, data) => { OnData?.Invoke(new Envelope<int, ArraySegment<byte>>(id, data)); };

            // start server listening on port 9300
            server.Start(9300);
        }

        public event Action<Envelope<int, ArraySegment<byte>>> OnData;
        public event Action<int> OnClose;

        public void Send(int connectionId, ArraySegment<byte> source)
        {
            server.SendOne(connectionId, source);
        }
    }
    
    static bool _mustQuit;
}

#endif