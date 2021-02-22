using System;
using System.Collections.Generic;
using System.Text;

namespace UniGLTF
{
    public class ImporterContextSpeedLog
    {
        public struct KeyElapsed
        {
            public string Key;
            public TimeSpan Elapsed;
            public KeyElapsed(string key, TimeSpan elapsed)
            {
                Key = key;
                Elapsed = elapsed;
            }
        }

        public struct MeasureScope : IDisposable
        {
            Action m_onDispose;
            public MeasureScope(Action onDispose)
            {
                m_onDispose = onDispose;
            }
            public void Dispose()
            {
                m_onDispose();
            }
        }

        public List<KeyElapsed> m_speedReports = new List<KeyElapsed>();

        public IDisposable MeasureTime(string key)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            return new MeasureScope(() =>
            {
                m_speedReports.Add(new KeyElapsed(key, sw.Elapsed));
            });
        }

        public string GetSpeedLog()
        {
            var total = TimeSpan.Zero;

            var sb = new StringBuilder();
            sb.AppendLine("【SpeedLog】");
            foreach (var kv in m_speedReports)
            {
                sb.AppendLine(string.Format("{0}: {1}ms", kv.Key, (int)kv.Elapsed.TotalMilliseconds));
                total += kv.Elapsed;
            }
            sb.AppendLine(string.Format("total: {0}ms", (int)total.TotalMilliseconds));

            return sb.ToString();
        }
    }
}
