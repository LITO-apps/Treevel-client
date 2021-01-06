﻿using System;
using TouchScript.Gestures.TransformGestures;
using Treevel.Common.Entities;
using Treevel.Common.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Treevel.Modules.MenuSelectScene.LevelSelect
{
    public class ScaleContent : MonoBehaviour
    {
        private TransformGesture _transformGesture;
        private RectTransform _contentRect;

        /// <summary>
        /// タッチした2点
        /// </summary>
        private Vector2 _prePoint1;
        private Vector2 _prePoint2;

        /// <summary>
        /// タッチした2点間の距離
        /// </summary>
        private float _preScreenDist = 0.0f;

        /// <summary>
        /// タッチした2点の中点
        /// </summary>
        private Vector3 _preMeanPoint;

        /// <summary>
        /// 拡大率
        /// </summary>
        private float _preScale;
        /// <summary>
        /// 拡大率の上限
        /// </summary>
        private const float _SCALE_MAX = 1.50f;
        /// <summary>
        /// 拡大率の下限
        /// </summary>
        private const float _SCALE_MIN = 0.5f;

        /// <summary>
        /// 補正後のキャンバスの大きさ
        /// </summary>
        private Vector2 _scaledCanvas;

        private void Awake()
        {
            _contentRect = GetComponent<ScrollRect>().content;
            _transformGesture = GetComponent<TransformGesture>();
            _scaledCanvas = RuntimeConstants.ScaledCanvasSize.SIZE_DELTA;

            Observable.Merge(
                Observable.FromEvent(
                    _ => _transformGesture.TransformStarted += OnTransformStarted,
                    _ => _transformGesture.TransformStarted -= OnTransformStarted),
                Observable.FromEvent(
                    _ => _transformGesture.Transformed += OnTransformed,
                    _ => _transformGesture.Transformed -= OnTransformed)
            ).Subscribe().AddTo(this);
        }

        private void OnEnable()
        {
            _preScale = UserSettings.LevelSelectCanvasScale;
            _contentRect.localScale = new Vector2(_preScale, _preScale);
            // 道の拡大縮小
            LevelSelectDirector.Instance.ScaleRoad(_preScale);
        }

        private void OnDisable()
        {
            UserSettings.LevelSelectCanvasScale = _preScale;
        }

        private void OnTransformStarted(object sender, EventArgs e)
        {
            // タッチ位置を取得
            _prePoint1 = _transformGesture.ActivePointers[0].Position;
            _prePoint2 = _transformGesture.ActivePointers[1].Position;
            // スクリーン上の距離を計算
            _preScreenDist = Vector2.Distance(_prePoint1, _prePoint2);
            // スクリーン上の中点座標を計算
            _preMeanPoint = (_prePoint1 + _prePoint2) / 2;
        }

        private void OnTransformed(object sender, EventArgs e)
        {
            // タッチ位置等の計算
            var newPoint1 = _transformGesture.ActivePointers[0].Position;
            var newPoint2 = _transformGesture.ActivePointers[1].Position;
            var newScreenDist = Vector2.Distance(newPoint1, newPoint2);
            var newMeanPoint = (Vector3)((newPoint1 + newPoint2) / 2);

            // 2点のタッチ開始時の距離と現在の距離の比から拡大率を求める
            var newScale = _preScale * newScreenDist / _preScreenDist;
            // 拡大率を閾値内に抑える
            newScale = Mathf.Clamp(newScale, _SCALE_MIN, _SCALE_MAX);
            // Contentの拡大縮小
            _contentRect.localScale = new Vector2(newScale, newScale);
            // 道の拡大縮小
            LevelSelectDirector.Instance.ScaleRoad(newScale);

            // 拡大縮小前の中点を拡大縮小後の中点に合わせるようにContentの平行移動量を求める
            var preContentPoint = ConvertFromScreenToContent(_preMeanPoint, _preScale);
            var newContentPoint = ConvertFromScreenToContent(newMeanPoint, _preScale);
            // Content空間での2点の差分
            var moveAmount = newContentPoint - preContentPoint * newScale / _preScale;
            moveAmount = KeepInContent(moveAmount, newScale);
            // Contentの平行移動
            _contentRect.transform.localPosition += new Vector3(moveAmount.x, moveAmount.y, 0);

            // 値の更新
            _prePoint1 = newPoint1;
            _prePoint2 = newPoint2;
            _preScreenDist = newScreenDist;
            _preMeanPoint = newMeanPoint;
            _preScale = newScale;
        }

        /// <summary>
        /// スクリーン空間の座標をContent空間の座標に変換する
        /// </summary>
        /// <param name="screenPoint"> スクリーン上の座標 </param>
        /// <param name="scale"> Contentの拡大率 </param>
        /// <returns> Content空間の座標 </returns>
        private Vector3 ConvertFromScreenToContent(Vector3 screenCoordinate, float scale)
        {
            return ((-1) * _contentRect.transform.localPosition + (screenCoordinate - new Vector3(_scaledCanvas.x / 2, _scaledCanvas.y / 2))) / scale;
        }

        /// <summary>
        /// 平行移動後にContentの外を表示しないようにする
        /// </summary>
        /// <param name="moveAmount"> 平行移動量 </param>
        /// <param name="scale"> Contentの拡大率 </param>
        /// <returns> 平行移動量 </returns>
        private Vector2 KeepInContent(Vector2 moveAmount, float scale)
        {
            // 平行移動前のスクリーン中心のContent空間での座標
            var _preLocalPosition = _contentRect.transform.localPosition;

            // Contentの左端のチェック
            var leftLimit = (SavableScrollRect._LEFT_OFFSET * scale - 0.5f) * _scaledCanvas.x;
            if (_preLocalPosition.x + moveAmount.x >= leftLimit) {
                moveAmount.x = leftLimit - _preLocalPosition.x;
            }
            // Contentの右端のチェック
            var rightLimit = (-1) * ((SavableScrollRect._RIGHT_OFFSET * scale - 0.5f) * _scaledCanvas.x);
            if (_preLocalPosition.x + moveAmount.x <= rightLimit) {
                moveAmount.x = rightLimit - _preLocalPosition.x;
            }
            // Contentの上端のチェック
            var topLimit = (-1) * ((SavableScrollRect._TOP_OFFSET * scale - 0.5f) * _scaledCanvas.y);
            if (_preLocalPosition.y + moveAmount.y <= topLimit) {
                moveAmount.y = topLimit - _preLocalPosition.y;
            }
            // Contentの下端のチェック
            var bottomLimit = (SavableScrollRect._BOTTOM_OFFSET * scale - 0.5f) * _scaledCanvas.y;
            if (_preLocalPosition.y + moveAmount.y >= bottomLimit) {
                moveAmount.y = bottomLimit - _preLocalPosition.y;
            }

            return moveAmount;
        }
    }
}
