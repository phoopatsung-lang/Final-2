using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PhoneServer : MonoBehaviour
{
    public const int MaxPlayers = 4;
    public static PhoneServer Instance { get; private set; }

    public int port = 8080;
    public float reconnectGrace = 30f;   // หลุดแล้วกลับมาภายในกี่วิ ได้ช่องเดิม
    public TMP_Text statusText;
    public Color[] colors = { Color.red, new Color(0.2f, 0.5f, 1f), Color.green, Color.yellow };

    public class Player
    {
        public string id, name;
        public bool connected, a, b, aDown, bDown;
        public Vector2 tilt;
        public int conn = -1;
        public float lastSeen;
    }
    public readonly Player[] players = new Player[MaxPlayers];

    [Serializable] class Msg { public string t, id, name; public float x, y; public int a, b; }
    class Client { public TcpClient tcp; public NetworkStream ns; }

    TcpListener listener;
    volatile bool running;
    string htmlPath;
    int nextConn;
    readonly ConcurrentQueue<(int conn, string text)> inbox = new();
    readonly ConcurrentDictionary<int, Client> clients = new();

    public static Player Get(int slot) =>
        Instance != null && slot >= 0 && slot < MaxPlayers ? Instance.players[slot] : null;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // โหลดฉากใหม่ มือถือไม่หลุด
        for (int i = 0; i < MaxPlayers; i++) players[i] = new Player();
        htmlPath = Path.Combine(Application.streamingAssetsPath, "controller.html");
        listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        running = true;
        new Thread(AcceptLoop) { IsBackground = true }.Start();
        Debug.Log("PhoneServer: http://127.0.0.1:" + port);
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        running = false;
        listener.Stop();
        foreach (var c in clients.Values) c.tcp.Close();
    }

    // ===== Main thread =====
    void Update()
    {
        float now = Time.realtimeSinceStartup;
        foreach (var p in players) p.aDown = p.bDown = false;

        while (inbox.TryDequeue(out var e))
        {
            int slot = SlotOf(e.conn);
            if (e.text == null)   // หลุดการเชื่อมต่อ
            {
                if (slot >= 0)
                {
                    var p = players[slot];
                    p.connected = false; p.conn = -1; p.tilt = Vector2.zero;
                    p.a = p.b = false; p.lastSeen = now;
                }
                continue;
            }

            Msg m;
            try { m = JsonUtility.FromJson<Msg>(e.text); } catch { continue; }
            if (m == null) continue;

            if (m.t == "join") Join(e.conn, m, now);
            else if (m.t == "in" && slot >= 0)
            {
                var p = players[slot];
                p.tilt = Vector2.ClampMagnitude(new Vector2(m.x, m.y), 1f);
                if (m.a == 1 && !p.a) p.aDown = true;
                if (m.b == 1 && !p.b) p.bDown = true;
                p.a = m.a == 1; p.b = m.b == 1; p.lastSeen = now;
            }
        }

        foreach (var p in players)
        {
            if (p.connected && now - p.lastSeen > 0.5f) p.tilt = Vector2.zero;
            if (!p.connected && p.id != null && now - p.lastSeen > reconnectGrace) p.id = null;
        }

        if (statusText != null)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < MaxPlayers; i++)
                sb.Append("P").Append(i + 1).Append(": ")
                  .Append(players[i].connected ? players[i].name
                        : players[i].id != null ? "(reconnecting)" : "-")
                  .Append('\n');
            statusText.text = sb.ToString();
        }
    }

    void Join(int conn, Msg m, float now)
    {
        if (string.IsNullOrEmpty(m.id)) return;
        int slot = -1;
        for (int i = 0; i < MaxPlayers; i++) if (players[i].id == m.id) slot = i;
        if (slot < 0)
            for (int i = 0; i < MaxPlayers; i++) if (players[i].id == null) { slot = i; break; }
        if (slot < 0) { SendTo(conn, "{\"t\":\"full\"}"); return; }

        var p = players[slot];
        if (p.conn >= 0 && p.conn != conn && clients.TryGetValue(p.conn, out var old)) old.tcp.Close();
        p.id = m.id;
        p.name = string.IsNullOrEmpty(m.name) ? "P" + (slot + 1)
               : (m.name.Length > 12 ? m.name.Substring(0, 12) : m.name);
        p.conn = conn; p.connected = true; p.lastSeen = now;
        Send(slot, "{\"t\":\"welcome\",\"slot\":" + slot + ",\"color\":\"#"
                   + ColorUtility.ToHtmlStringRGB(colors[slot]) + "\"}");
    }

    int SlotOf(int conn)
    {
        for (int i = 0; i < MaxPlayers; i++) if (players[i].conn == conn) return i;
        return -1;
    }

    public void Send(int slot, string json) { if (players[slot].connected) SendTo(players[slot].conn, json); }
    public void Vibrate(int slot, int ms) => Send(slot, "{\"t\":\"vib\",\"ms\":" + ms + "}");

    void SendTo(int conn, string json)
    {
        if (clients.TryGetValue(conn, out var c)) SendFrame(c, 0x1, Encoding.UTF8.GetBytes(json));
    }

    // ===== Background threads =====
    void AcceptLoop()
    {
        while (running)
        {
            try
            {
                var tcp = listener.AcceptTcpClient();
                new Thread(() => HandleClient(tcp)) { IsBackground = true }.Start();
            }
            catch { if (!running) return; }
        }
    }

    void HandleClient(TcpClient tcp)
    {
        int conn = Interlocked.Increment(ref nextConn);
        bool isWs = false;
        try
        {
            tcp.NoDelay = true;
            tcp.ReceiveTimeout = 10000;
            tcp.SendTimeout = 2000;
            var ns = tcp.GetStream();
            string req = ReadHeaders(ns);
            if (req == null) return;
            string key = Header(req, "Sec-WebSocket-Key");

            if (key == null)   // ขอหน้าเว็บ
            {
                string path = req.Split(' ')[1].Split('?')[0];
                if (path == "/" && File.Exists(htmlPath))
                {
                    byte[] body = File.ReadAllBytes(htmlPath);
                    WriteText(ns, "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: "
                        + body.Length + "\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n");
                    ns.Write(body, 0, body.Length);
                }
                else WriteText(ns, "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                return;
            }

            string accept = Convert.ToBase64String(SHA1.Create().ComputeHash(
                Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            WriteText(ns, "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: "
                + accept + "\r\n\r\n");

            var client = new Client { tcp = tcp, ns = ns };
            clients[conn] = client;
            isWs = true;
            string msg;
            while ((msg = ReadFrame(client)) != null) inbox.Enqueue((conn, msg));
        }
        catch { }
        finally
        {
            if (isWs) { clients.TryRemove(conn, out _); inbox.Enqueue((conn, null)); }
            tcp.Close();
        }
    }

    static string ReadFrame(Client c)
    {
        var s = c.ns;
        while (true)
        {
            byte[] h = ReadN(s, 2);
            int op = h[0] & 0x0F;
            int len = h[1] & 0x7F;
            if (len == 126) { var e = ReadN(s, 2); len = (e[0] << 8) | e[1]; }
            else if (len == 127) return null;
            byte[] mask = (h[1] & 0x80) != 0 ? ReadN(s, 4) : null;
            byte[] data = ReadN(s, len);
            if (mask != null) for (int i = 0; i < data.Length; i++) data[i] ^= mask[i & 3];

            if (op == 0x8) return null;                            // close
            if (op == 0x9) { SendFrame(c, 0xA, data); continue; }  // ping → pong
            if (op == 0x1) return Encoding.UTF8.GetString(data);   // text
        }
    }

    static byte[] ReadN(Stream s, int n)
    {
        var buf = new byte[n];
        for (int off = 0; off < n;)
        {
            int r = s.Read(buf, off, n - off);
            if (r <= 0) throw new IOException("closed");
            off += r;
        }
        return buf;
    }

    static string ReadHeaders(Stream s)
    {
        var sb = new StringBuilder();
        while (sb.Length < 8192)
        {
            int c = s.ReadByte();
            if (c < 0) return null;
            sb.Append((char)c);
            if (c == '\n' && sb.ToString().EndsWith("\r\n\r\n")) return sb.ToString();
        }
        return null;
    }

    static string Header(string req, string name)
    {
        foreach (var line in req.Split('\n'))
        {
            int i = line.IndexOf(':');
            if (i > 0 && string.Equals(line.Substring(0, i).Trim(), name, StringComparison.OrdinalIgnoreCase))
                return line.Substring(i + 1).Trim();
        }
        return null;
    }

    static void WriteText(Stream s, string text)
    {
        byte[] b = Encoding.UTF8.GetBytes(text);
        s.Write(b, 0, b.Length);
    }

    static void SendFrame(Client c, int op, byte[] data)
    {
        byte[] head = data.Length < 126
            ? new byte[] { (byte)(0x80 | op), (byte)data.Length }
            : new byte[] { (byte)(0x80 | op), 126, (byte)(data.Length >> 8), (byte)data.Length };
        lock (c)
        {
            try { c.ns.Write(head, 0, head.Length); c.ns.Write(data, 0, data.Length); } catch { }
        }
    }
}