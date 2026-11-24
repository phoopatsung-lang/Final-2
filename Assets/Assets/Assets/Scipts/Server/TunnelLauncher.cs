using System.Diagnostics;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

public class TunnelLauncher : MonoBehaviour
{
    public string cloudflaredPath = @"";
    public RawImage qrImage;
    public TMP_Text urlText;   // ใส่หรือไม่ใส่ก็ได้
    Process proc;
    volatile string url;
    string shownUrl;

    void Start()
    {
        var server = PhoneServer.Instance;
        if (server == null || server.gameObject != gameObject) return;
        try
        {
            proc = new Process();
            proc.StartInfo = new ProcessStartInfo(cloudflaredPath,
                "tunnel --no-autoupdate --url http://127.0.0.1:" + server.port)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            proc.ErrorDataReceived += OnLine;
            proc.OutputDataReceived += OnLine;
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
        }
        catch (System.Exception ex) { UnityEngine.Debug.LogError("เปิด cloudflared ไม่ได้: " + ex.Message); }
    }

    void OnLine(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null || url != null) return;
        var m = Regex.Match(e.Data, @"https://[a-z0-9-]+\.trycloudflare\.com");
        if (m.Success && !m.Value.StartsWith("https://api.")) url = m.Value;
    }

    void Update()
    {
        if (url == null || url == shownUrl) return;
        shownUrl = url;
        if (qrImage != null) qrImage.texture = MakeQr(url);
        if (urlText != null) urlText.text = url;
    }

    static Texture2D MakeQr(string text)
    {
        BitMatrix m = new QRCodeWriter().encode(text, BarcodeFormat.QR_CODE, 0, 0);
        var tex = new Texture2D(m.Width, m.Height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < m.Height; y++)
            for (int x = 0; x < m.Width; x++)
                tex.SetPixel(x, m.Height - 1 - y, m[x, y] ? Color.black : Color.white);
        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        try { if (proc != null && !proc.HasExited) proc.Kill(); } catch { }
    }
}
