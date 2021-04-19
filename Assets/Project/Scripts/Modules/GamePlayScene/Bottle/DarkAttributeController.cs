﻿using Treevel.Common.Entities;
using UniRx;
using UnityEngine;

namespace Treevel.Modules.GamePlayScene.Bottle
{
    public class DarkAttributeController : BottleAttributeControllerBase
    {
        private GoalBottleController _bottleController;

        /// <summary>
        /// 成功状態かどうか
        /// </summary>
        private bool _isSuccess;

        private static readonly int _ANIMATOR_IS_DARK = Animator.StringToHash("IsDark");
        private static readonly int _ANIMATOR_PARAM_FLOAT_SPEED = Animator.StringToHash("DarkSpeed");

        public void Initialize(GoalBottleController bottleController)
        {
            Initialize();
            transform.parent = bottleController.transform;
            transform.localPosition = Vector3.zero;
            _bottleController = bottleController;

            // イベントに処理を登録する
            _bottleController.EnterTile.Merge(_bottleController.ExitTile)
                .Subscribe(_ => {
                    _isSuccess = _bottleController.IsSuccess();
                    animator.SetBool(_ANIMATOR_IS_DARK, !_isSuccess);
                }).AddTo(this);
            _bottleController.longPressGesture.OnLongPress.AsObservable().Subscribe(_ => animator.SetBool(_ANIMATOR_IS_DARK, false)).AddTo(compositeDisposableOnGameEnd, this);
            _bottleController.releaseGesture.OnRelease.AsObservable().Subscribe(_ => animator.SetBool(_ANIMATOR_IS_DARK, !_isSuccess)).AddTo(compositeDisposableOnGameEnd, this);

            GamePlayDirector.Instance.GameEnd.Subscribe(_ => animator.SetFloat(_ANIMATOR_PARAM_FLOAT_SPEED, 0f)).AddTo(this);

            // 描画順序の設定
            spriteRenderer.sortingOrder = EBottleAttributeType.Dark.GetOrderInLayer();

            // 初期状態の登録
            _isSuccess = _bottleController.IsSuccess();
            animator.SetBool(_ANIMATOR_IS_DARK, !_isSuccess);
        }
    }
}
