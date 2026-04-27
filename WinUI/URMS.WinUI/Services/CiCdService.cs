using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace URMS.WinUI.Services
{
    /// <summary>gh CLI 経由で GitHub Actions 最新ステータスを取得</summary>
    public class CiCdService
    {
        private const string RepoRoot = @"D:\GitHub\URMS";

        public Task<string> GetLatestStatusAsync() => Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo("gh",
                    "run list --limit 1 --json status --jq .[0].status")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    WorkingDirectory       = RepoRoot
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "—";
                var output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit(3000);
                return output switch
                {
                    "completed"   => "成功",
                    "in_progress" => "実行中",
                    "queued"      => "待機中",
                    "failure"     => "失敗",
                    _             => string.IsNullOrWhiteSpace(output) ? "—" : output
                };
            }
            catch { return "—"; }
        });
    }
}
