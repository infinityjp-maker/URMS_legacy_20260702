using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace URMS.WinUI.Services
{
    public record NetworkStatus(long LatencyMs, bool IsUp);

    /// <summary>ネットワーク疎通確認 & レイテンシ計測</summary>
    public class NetworkMonitorService
    {
        private const string PingTarget  = "8.8.8.8";
        private const int    PingTimeout = 1000;

        public Task<NetworkStatus> GetStatusAsync() => Task.Run(() =>
        {
            try
            {
                using var ping  = new Ping();
                var reply = ping.Send(PingTarget, PingTimeout);
                return reply.Status == IPStatus.Success
                    ? new NetworkStatus(reply.RoundtripTime, true)
                    : new NetworkStatus(-1, false);
            }
            catch { return new NetworkStatus(-1, false); }
        });
    }
}
