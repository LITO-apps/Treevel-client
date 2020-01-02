﻿using System;
using System.Collections;
using JetBrains.Annotations;
using Project.Scripts.GamePlayScene.Bullet.Controllers;
using Project.Scripts.GamePlayScene.BulletWarning;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.Utils.Library;
using UnityEngine;

namespace Project.Scripts.GamePlayScene.Bullet.Generators
{
    public class TurnCartridgeGenerator : NormalCartridgeGenerator
    {
        /// <summary>
        /// TurnCartridgeのPrefab
        /// </summary>
        [SerializeField] private GameObject _turnCartridgePrefab;

        /// <summary>
        /// TurnCartridgeWarningのPrefab
        /// </summary>
        [SerializeField] private GameObject _turnCartridgeWarningPrefab;

        /// <summary>
        /// 銃弾が生成されてから動き始めるまでのフレーム数
        /// </summary>
        public static int turnCartridgeWaitingFrames;

        /// <summary>
        /// 曲がる方向
        /// </summary>
        [CanBeNull] private int[] _turnDirection = null;

        /// <summary>
        /// 曲がる行(または列)
        /// </summary>
        [CanBeNull] private int[] _turnLine = null;

        /// <summary>
        /// 曲がる方向の重み
        /// </summary>
        private int[] _randomTurnDirections = BulletLibrary.GetInitialArray(Enum.GetNames(typeof(ECartridgeDirection)).Length - 1);

        /// <summary>
        /// 曲がる行の重み
        /// </summary>
        private int[] _randomTurnRow = BulletLibrary.GetInitialArray(Enum.GetNames(typeof(ERow)).Length - 1);

        /// <summary>
        /// 曲がる列の重み
        /// </summary>
        private int[] _randomTurnColumn = BulletLibrary.GetInitialArray(Enum.GetNames(typeof(EColumn)).Length - 1);

        protected override void Awake()
        {
            base.Awake();
            turnCartridgeWaitingFrames = BulletWarningParameter.WARNING_DISPLAYED_FRAMES / 2;
        }

        /// <summary>
        /// TurnCartridgeを生成するGeneratorの初期化
        /// </summary>
        /// <param name="ratio"> Generatorの出現割合 </param>
        /// <param name="direction" 移動する方向 ></param>
        /// <param name="line"> 移動する行(列) </param>
        /// <param name="turnDirection"> 曲がる方向 </param>
        /// <param name="turnLine"> 曲がる行(列) </param>
        public void Initialize(int ratio, ECartridgeDirection direction, int line, int[] turnDirection, int[] turnLine)
        {
            Initialize(ratio, direction, line);
            _turnDirection = turnDirection;
            _turnLine = turnLine;
        }

        /// <summary>
        /// ランダムな行(または列)を移動し、ランダムな列でランダムな方向に曲がるTurnCartridgeを生成するGeneratorの初期化
        /// </summary>
        /// <param name="ratio"> Generatorの出現割合 </param>
        /// <param name="randomCartridgeDirection"> 移動する方向の重み </param>
        /// <param name="randomRow"> 移動する行の重み </param>
        /// <param name="randomColumn"> 移動する列の重み </param>
        /// <param name="randomTurnDirections"> 曲がる方向の重み </param>
        /// <param name="randomTurnRow"> 曲がる行の重み </param>
        /// <param name="randomTurnColumn"> 曲がる列の重み </param>
        public void Initialize(int ratio,
            int[] randomCartridgeDirection, int[] randomRow, int[] randomColumn,
            int[] randomTurnDirections, int[] randomTurnRow, int[] randomTurnColumn)
        {
            Initialize(ratio, randomCartridgeDirection, randomRow, randomColumn);
            _randomTurnDirections = randomTurnDirections;
            _randomTurnRow = randomTurnRow;
            _randomTurnColumn = randomTurnColumn;
        }

        public override IEnumerator CreateBullet(int bulletId)
        {
            // 銃弾の移動方向を指定する
            var nextCartridgeDirection = (cartridgeDirection == ECartridgeDirection.Random)
                ? GetCartridgeDirection()
                : cartridgeDirection;

            // 銃弾の出現する場所を指定する
            var nextCartridgeLine = line;
            if (nextCartridgeLine == (int) ERow.Random) {
                switch (nextCartridgeDirection) {
                    case ECartridgeDirection.ToLeft:
                    case ECartridgeDirection.ToRight:
                        nextCartridgeLine = GetRow();
                        break;
                    case ECartridgeDirection.ToUp:
                    case ECartridgeDirection.ToBottom:
                        nextCartridgeLine = GetColumn();
                        break;
                    case ECartridgeDirection.Random:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            // 警告の作成
            var warning = Instantiate(_turnCartridgeWarningPrefab);
            warning.GetComponent<Renderer>().sortingOrder = bulletId;
            var warningScript = warning.GetComponent<CartridgeWarningController>();
            var bulletMotionVector =
                warningScript.Initialize(ECartridgeType.Turn, nextCartridgeDirection, nextCartridgeLine);

            // 銃弾を生成するまで待つ
            for (var index = 0; index < BulletWarningParameter.WARNING_DISPLAYED_FRAMES - turnCartridgeWaitingFrames; index++) yield return new WaitForFixedUpdate();

            // ゲームが続いているなら銃弾を作成する
            if (gamePlayDirector.state != GamePlayDirector.EGameState.Playing) yield break;

            var nextCartridgeTurnDirection = _turnDirection ?? new int[] {
                GetRandomTurnDirection(nextCartridgeDirection, nextCartridgeLine)
            };

            var nextCartridgeTurnLine = _turnLine;
            if (nextCartridgeTurnLine == null) {
                switch (nextCartridgeDirection) {
                    case ECartridgeDirection.ToLeft:
                    case ECartridgeDirection.ToRight:
                        nextCartridgeTurnLine = new int[] {GetTurnColumn()};
                        break;
                    case ECartridgeDirection.ToUp:
                    case ECartridgeDirection.ToBottom:
                        nextCartridgeTurnLine = new int[] {GetTurnRow()};
                        break;
                    case ECartridgeDirection.Random:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            var cartridge = Instantiate(_turnCartridgePrefab);
            cartridge.GetComponent<TurnCartridgeController>().Initialize(nextCartridgeDirection, nextCartridgeLine,
                bulletMotionVector, nextCartridgeTurnDirection, nextCartridgeTurnLine);

            // 同レイヤーのオブジェクトの描画順序の制御
            cartridge.GetComponent<Renderer>().sortingOrder = bulletId;

            // 警告を削除する
            for (var index = 0; index < turnCartridgeWaitingFrames; index++) yield return new WaitForFixedUpdate();
            Destroy(warning);

        }

        /// <summary>
        /// 曲がる方向を重みに基づき決定する
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="line"></param>
        private int GetRandomTurnDirection(ECartridgeDirection direction, int line)
        {
            var randomTurnDirection = 0;
            // 最上行または最下行を移動している場合
            if ((direction == ECartridgeDirection.ToLeft || direction == ECartridgeDirection.ToRight) &&
                (line == (int) ERow.First || line == (int) ERow.Fifth)) {
                switch (line) {
                    case (int) ERow.First:
                        randomTurnDirection = (int) ECartridgeDirection.ToBottom;
                        break;
                    case (int) ERow.Fifth:
                        randomTurnDirection = (int) ECartridgeDirection.ToUp;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            // 最左列または最も最右列を移動している場合
            else if ((direction == ECartridgeDirection.ToUp || direction == ECartridgeDirection.ToBottom) &&
                (line == (int) EColumn.Left || line == (int) EColumn.Right)) {
                switch (line) {
                    case (int) EColumn.Left:
                        randomTurnDirection = (int) ECartridgeDirection.ToRight;
                        break;
                    case (int) EColumn.Right:
                        randomTurnDirection = (int) ECartridgeDirection.ToLeft;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            // 上記以外の場合(ランダムに決定する)
            else {
                // Cartridgeの移動方向に応じてCartridgeから見た右方向、左方向を取得する
                int cartridgeLocalLeft;
                int cartridgeLocalRight;
                switch (direction) {
                    case ECartridgeDirection.ToLeft:
                        cartridgeLocalLeft = (int) ECartridgeDirection.ToBottom;
                        cartridgeLocalRight = (int) ECartridgeDirection.ToUp;
                        break;
                    case ECartridgeDirection.ToRight:
                        cartridgeLocalLeft = (int) ECartridgeDirection.ToUp;
                        cartridgeLocalRight = (int) ECartridgeDirection.ToBottom;
                        break;
                    case ECartridgeDirection.ToUp:
                        cartridgeLocalLeft = (int) ECartridgeDirection.ToLeft;
                        cartridgeLocalRight = (int) ECartridgeDirection.ToRight;
                        break;
                    case ECartridgeDirection.ToBottom:
                        cartridgeLocalLeft = (int) ECartridgeDirection.ToRight;
                        cartridgeLocalRight = (int) ECartridgeDirection.ToLeft;
                        break;
                    case ECartridgeDirection.Random:
                        throw new Exception();
                    default:
                        throw new NotImplementedException();
                }

                // 乱数を取得する
                var randomValue = new System.Random().Next(_randomTurnDirections[cartridgeLocalLeft - 1] +
                    _randomTurnDirections[cartridgeLocalRight - 1]) + 1;
                // 乱数に基づいてCartridgeから見て右または左のどちらかの方向を選択する
                randomTurnDirection = randomValue <= _randomTurnDirections[cartridgeLocalLeft - 1]
                    ? (int) Enum.ToObject(typeof(ECartridgeDirection), cartridgeLocalLeft)
                    : (int) Enum.ToObject(typeof(ECartridgeDirection), cartridgeLocalRight);
            }

            return randomTurnDirection;
        }

        /// <summary>
        /// 曲がる行を重みに基づき決定する
        /// </summary>
        private int GetTurnRow()
        {
            var index = BulletLibrary.SamplingArrayIndex(_randomTurnRow) + 1;
            return (int) Enum.ToObject(typeof(ERow), index);
        }

        /// <summary>
        /// 曲がる列を重みに基づき決定する
        /// </summary>
        private int GetTurnColumn()
        {
            var index = BulletLibrary.SamplingArrayIndex(_randomTurnColumn) + 1;
            return (int) Enum.ToObject(typeof(EColumn), index);
        }
    }
}
