﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Treevel.Common.Entities;
using Treevel.Common.Entities.GameDatas;
using Treevel.Common.Managers;
using Treevel.Common.Patterns.Singleton;
using Treevel.Common.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Treevel.Modules.GamePlayScene.Gimmick
{
    public class GimmickGenerator : SingletonObject<GimmickGenerator>
    {
        private List<IEnumerator> _coroutines = new List<IEnumerator>();

        /// <summary>
        /// FireGimmickが呼ばれた時刻
        /// </summary>
        private float _startTime;

        private readonly Dictionary<EGimmickType, string> _prefabAddressableKeys = new Dictionary<EGimmickType, string>()
        {
            {EGimmickType.Tornado, Constants.Address.TORNADO_PREFAB},
            {EGimmickType.Meteorite, Constants.Address.METEORITE_PREFAB},
            {EGimmickType.AimingMeteorite, Constants.Address.AIMING_METEORITE_PREFAB},
            {EGimmickType.Thunder, Constants.Address.THUNDER_PREFAB},
            {EGimmickType.SolarBeam, Constants.Address.SOLAR_BEAM_PREFAB},
            {EGimmickType.GustWind, Constants.Address.GUST_WIND_PREFAB},
            {EGimmickType.Fog, Constants.Address.FOG_PREFAB},
            {EGimmickType.Powder, Constants.Address.POWDER_PREFAB},
        };

        public void Initialize(List<GimmickData> gimmicks)
        {
            _coroutines = gimmicks.Select(CreateGimmickCoroutine).ToList();
        }

        /// <summary>
        /// ギミック発動
        /// </summary>
        public void FireGimmick()
        {
            _startTime = Time.time;
            _coroutines.ForEach(cr => StartCoroutine(cr));
        }

        /// <summary>
        /// ギミックの実体を生成するコルチーン
        /// </summary>
        /// <param name="data">生成するギミックのデータ</param>
        private IEnumerator CreateGimmickCoroutine(GimmickData data)
        {
            // 出現時間経つまで待つ
            yield return new WaitForSeconds(data.appearTime - (Time.time - _startTime));

            do {
                var instantiateTime = Time.time;
                // ギミックインスタンス作成
                AsyncOperationHandle<GameObject> gimmickObjectOp;
                yield return gimmickObjectOp = AddressableAssetManager.Instantiate(_prefabAddressableKeys[data.type]);
                var gimmickObject = gimmickObjectOp.Result;

                if (gimmickObject == null) {
                    Debug.LogError($"ギミックの生成が失敗しました。");
                    yield break;
                }

                var gimmick = gimmickObject.GetComponent<AbstractGimmickController>();
                if (gimmick == null) {
                    Debug.LogError($"ギミックコントローラが{gimmickObject.name}にアタッチされていません。");
                    yield break;
                }

                // 初期化
                gimmick.Initialize(data);

                // ギミック発動
                StartCoroutine(gimmick.Trigger());

                // ギミック発動間隔
                yield return new WaitForSeconds(data.interval - (Time.time - instantiateTime));
            } while (data.loop);
        }

        private void OnEnable()
        {
            GamePlayDirector.Instance.GameSucceeded.Subscribe(_ => {
                OnGameEnd();
            }).AddTo(this);
            GamePlayDirector.Instance.GameFailed.Subscribe(_ => {
                OnGameEnd();
            }).AddTo(this);
        }

        private void OnGameEnd()
        {
            // 全てのGimmickを停止させる
            StopAllCoroutines();
        }
    }
}
