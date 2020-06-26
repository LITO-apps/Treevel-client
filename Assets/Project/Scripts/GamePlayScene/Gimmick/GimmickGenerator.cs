﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.GameDatas;
using Project.Scripts.GamePlayScene;
using Project.Scripts.Utils;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.Utils.Patterns;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Project.Scripts.GamePlayScene.Gimmick
{
    public class GimmickGenerator : SingletonObject<GimmickGenerator>
    {
        private List<IEnumerator> _coroutines = new List<IEnumerator>();

        private readonly Dictionary<EGimmickType, string> _prefabAddressableKeys = new Dictionary<EGimmickType, string>()
        {
            {EGimmickType.NormalCartridge, Address.NORMAL_CARTRIDGE_GENERATOR_PREFAB},
            {EGimmickType.RandomNormalCartridge, Address.NORMAL_CARTRIDGE_GENERATOR_PREFAB},
            {EGimmickType.TurnCartridge, Address.TURN_CARTRIDGE_GENERATOR_PREFAB},
            {EGimmickType.RandomTurnCartridge, Address.TURN_CARTRIDGE_GENERATOR_PREFAB},
            {EGimmickType.NormalHole, Address.NORMAL_HOLE_GENERATOR_PREFAB},
            {EGimmickType.RandomNormalHole, Address.NORMAL_HOLE_GENERATOR_PREFAB},
            {EGimmickType.AimingHole, Address.AIMING_HOLE_GENERATOR_PREFAB},
            {EGimmickType.RandomAimingHole, Address.AIMING_HOLE_GENERATOR_PREFAB},
            {EGimmickType.Tornado, Address.TORNADO_PREFAB}
        };


        public void Initialize(List<GimmickData> gimmicks)
        {
            _coroutines = gimmicks.Select(CreateGimmickCoroutine).ToList();
        }


        public void TriggerGimmicks()
        {
            _coroutines.ForEach(cr => StartCoroutine(cr));
        }

        IEnumerator CreateGimmickCoroutine(GimmickData data)
        {
            // 出現時間経つまで待つ
            yield return new WaitForSeconds(data.appearTime);

            do {
                // ギミックインスタンス作成
                AsyncOperationHandle<GameObject> gimmickObjectOp;
                yield return gimmickObjectOp = AddressableAssetManager.Instantiate(_prefabAddressableKeys[data.type]);
                var gimmickObject = gimmickObjectOp.Result;

                var gimmick = gimmickObject.GetComponent<AbstractGimmickController>();
                if (gimmick == null) {
                    Debug.LogError($"ギミックコントローラが{gimmickObject.name}にアタッチされていません。");
                    yield break;
                }

                // 初期化
                gimmick.Initialize(data);

                // ギミック発動
                yield return gimmick.Trigger();

                // ギミック発動間隔
                yield return new WaitForSeconds(data.interval);
            } while (data.loop);
        }

        private void OnEnable()
        {
            GamePlayDirector.OnSucceed += GameFinish;
            GamePlayDirector.OnFail += GameFinish;
        }

        private void OnDisable()
        {
            GamePlayDirector.OnSucceed -= GameFinish;
            GamePlayDirector.OnFail -= GameFinish;
        }

        private void GameFinish()
        {
            // 全てのBulletGroupを停止させる
            StopAllCoroutines();
        }
    }
}