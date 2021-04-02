
using System;
using Cysharp.Threading.Tasks;

namespace Treevel.Common.Networks.Database
{
    public interface IDatabaseService
    {
        UniTask<T> GetDataAsync<T>(string key) where T: new();

        UniTask<bool> UpdateDataAsync<T>(string key, T data);

        UniTask<bool> LoginAsync();
    }

    public class NetworkErrorException : Exception
    {
        public NetworkErrorException() {}
        public NetworkErrorException(string message) : base (message) {}
    }
}