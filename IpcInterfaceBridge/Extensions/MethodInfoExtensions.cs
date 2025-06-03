using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SwissPension.IpcInterfaceBridge.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string GetHash(this MethodInfo methodInfo)
        {
            var hashInput = $"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(p => GetReadableName(p.ParameterType)))}): {GetReadableName(methodInfo.ReturnType)}";
            var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

            return hash;
        }

        private static string GetReadableName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var genericArguments = type.GetGenericArguments();
            var genericName = string.Join(", ", genericArguments.Select(GetReadableName));
            var genericType = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal));
            return $"{genericType}<{genericName}>";
        }
    }
}