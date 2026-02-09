using System;
using InfiniteImbueFramework.Configuration;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public enum IIFLogLevel
    {
        Off = 0,
        Basic = 1,
        Verbose = 2
    }

    public static class IIFLog
    {
        private const string Prefix = "[IIF]";

        public static IIFLogLevel CurrentLevel
        {
            get
            {
                string value = IIFModOptions.LogLevel ?? string.Empty;
                if (value.Equals("Verbose", StringComparison.OrdinalIgnoreCase))
                {
                    return IIFLogLevel.Verbose;
                }
                if (value.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    return IIFLogLevel.Basic;
                }
                return IIFLogLevel.Off;
            }
        }

        public static bool IsBasicEnabled => CurrentLevel >= IIFLogLevel.Basic;
        public static bool IsVerboseEnabled => CurrentLevel >= IIFLogLevel.Verbose;

        public static void Info(string message, bool force = false)
        {
            if (force || IsBasicEnabled)
            {
                Debug.Log($"{Prefix} {message}");
            }
        }

        public static void Verbose(string message, bool force = false)
        {
            if (force || IsVerboseEnabled)
            {
                Debug.Log($"{Prefix} {message}");
            }
        }

        public static void Warn(string message, bool force = false)
        {
            if (force || IsBasicEnabled)
            {
                Debug.LogWarning($"{Prefix} {message}");
            }
        }

        public static void Error(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }
    }
}
