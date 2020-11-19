﻿using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Treevel.Common.Networks
{
    public interface IServerRequest
    {
        UnityWebRequest ServerRequest {
            get;
        }

        void SetCache();
    }

    /// <summary>
    /// データ取得用コマンド（DBでSELECT文使う場合）
    /// </summary>
    public abstract class GetServerRequest : IServerRequest
    {
        public UnityWebRequest ServerRequest
        {
            get;
            protected set;
        }

        ~GetServerRequest()
        {
            ServerRequest?.Dispose();
        }

        /// <summary>
        /// サーバーから取得したデータをキャッシュに保存する
        /// </summary>
        public abstract void SetCache();

        public async Task<object> GetData()
        {
            Debug.Assert(ServerRequest != null, "ServerRequest not implemented.");

            // TODO Show Progress bar
            await ServerRequest.SendWebRequest();

            if (!IsRemoteDataValid()) {
                return GetData_Local();
            }
            // TODO check need to update cache

            return DeserializeResponse();
        }

        protected abstract object GetData_Local();

        protected bool IsRemoteDataValid()
        {
            return ServerRequest.isDone && !ServerRequest.isNetworkError && !ServerRequest.isHttpError;
        }

        /// <summary>
        /// サーバからもらったレスポンスをコマンド毎に必要な形に整形する
        /// </summary>
        /// <returns>呼び出し側が取得したいデータ</returns>
        protected abstract object DeserializeResponse();
    }

    /// <summary>
    /// データ更新用コマンド(DBでINSERT/UPDATE文を使う場合)
    /// </summary>
    public abstract class UpdateServerRequest : IServerRequest
    {
        public UnityWebRequest ServerRequest
        {
            get;
            protected set;
        }

        ~UpdateServerRequest()
        {
            ServerRequest?.Dispose();
        }

        public async Task<bool> Update()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                return Update_Local();
            } else {
                return await Update_Remote();
            }
        }

        public abstract void SetCache();

        protected abstract bool Update_Local();


        protected async Task<bool> Update_Remote()
        {
            if (ServerRequest == null)
                return false;

            await ServerRequest.SendWebRequest();

            // TODO: protocol for update commands to parse the success state
            var successFlag = ServerRequest.downloadHandler.text.Equals("success");

            if (!successFlag)
                return false;

            SetCache();
            return true;
        }
    }
}
