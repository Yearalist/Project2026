using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ToySiege.Core
{
    /// <summary>
    /// Toy Siege merkezi log yardımcısı.
    ///
    /// Tüm metodlar [Conditional("TOYSIEGE_DEBUG")] ile işaretlidir.
    /// Bu sayede:
    ///   - Editor ve Development Build'de loglar görünür
    ///   - Release Build'de derleyici tüm çağrıları otomatik çıkarır (GC sıfır)
    ///
    /// KULLANIM:
    ///   1) Edit → Project Settings → Player → Scripting Define Symbols'a "TOYSIEGE_DEBUG" ekle
    ///   2) Script'lerde: TSLogger.Log("mesaj") veya TSLogger.LogFSM("state bilgisi")
    ///   3) Release build yaparken TOYSIEGE_DEBUG'ı kaldır → tüm loglar sessizce kaybolur
    /// </summary>
    public static class TSLogger
    {
        // ── Genel log ──

        [Conditional("TOYSIEGE_DEBUG")]
        public static void Log(string message)
        {
            Debug.Log($"[ToySiege] {message}");
        }

        [Conditional("TOYSIEGE_DEBUG")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"[ToySiege] {message}");
        }

        // ── FSM State geçişleri ──

        [Conditional("TOYSIEGE_DEBUG")]
        public static void LogFSM(string system, string from, string to)
        {
            Debug.Log($"<color=yellow>[{system} FSM] {from} → {to}</color>");
        }

        // ── Savaş / Hasar ──

        [Conditional("TOYSIEGE_DEBUG")]
        public static void LogCombat(string message)
        {
            Debug.Log($"<color=red>[Combat] {message}</color>");
        }

        // ── AI / Düşman ──

        [Conditional("TOYSIEGE_DEBUG")]
        public static void LogAI(string message)
        {
            Debug.Log($"<color=orange>[AI] {message}</color>");
        }

        // ── Performans uyarıları ──

        [Conditional("TOYSIEGE_DEBUG")]
        public static void LogPerf(string message)
        {
            Debug.LogWarning($"<color=magenta>[Perf] {message}</color>");
        }
    }
}
