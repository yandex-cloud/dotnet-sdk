using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Yandex.Cloud.SDK.Tests")]

namespace Yandex.Cloud.Functions
{
    public interface Context
    {
        string RequestId { get; }
        string FunctionId { get; }
        string FunctionVersion { get; }
        int MemoryLimitInMB { get; }
        string LogGroupName { get; }
        string StreamName { get; }
        string TokenJson { get; }
    }

    internal sealed class YcFunctionContext : Context
    {
        public string RequestId { get; set; }

        public string FunctionId { get; set; }

        public string FunctionVersion { get; set; }

        public int MemoryLimitInMB { get; set; }

        public string LogGroupName { get; set; }

        public string StreamName { get; set; }

        public string TokenJson { get; set; }
    }

    internal sealed class YcDictionaryFunctionContext : Context
    {
        private readonly IReadOnlyDictionary<string, object> _values;
        private readonly bool _throwOnError;

        public YcDictionaryFunctionContext(IReadOnlyDictionary<string, object> values, bool throwOnError)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
            _throwOnError = throwOnError;
        }
        
        public string RequestId => GetValue<string>();

        public string FunctionId => GetValue<string>();

        public string FunctionVersion => GetValue<string>();

        public int MemoryLimitInMB => GetValue<int>();

        public string LogGroupName => GetValue<string>();

        public string StreamName => GetValue<string>();

        public string TokenJson => GetValue<string>();

        private T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (_values.TryGetValue(propertyName, out var rawValue))
            {
                return (T)rawValue;
            }

            if (_throwOnError) throw new ArgumentException($"'{propertyName}' has no value", nameof(propertyName));

            return default;
        }
    }
}
