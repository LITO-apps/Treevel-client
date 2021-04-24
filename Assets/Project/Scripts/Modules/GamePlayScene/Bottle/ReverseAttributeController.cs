﻿using Treevel.Common.Entities;
using UniRx;
using UnityEngine;

namespace Treevel.Modules.GamePlayScene.Bottle
{
    public class ReverseAttributeController : BottleAttributeControllerBase
    {
        private static readonly int _ANIMATOR_PARAM_FLOAT_SPEED = Animator.StringToHash("ReverseSpeed");

        protected override void Awake()
        {
            base.Awake();
            GamePlayDirector.Instance.OpeningAnimationStart.Subscribe(_ => {
                spriteRenderer.enabled = true;
                animator.enabled = false;
            }).AddTo(compositeDisposableOnGameEnd, this);
            GamePlayDirector.Instance.GameStart.Subscribe(_ => animator.enabled = true).AddTo(this);
            GamePlayDirector.Instance.GameEnd.Subscribe(_ => animator.enabled = false).AddTo(this);
            // 描画順序の設定
            spriteRenderer.sortingOrder = EBottleAttributeType.Reverse.GetOrderInLayer();
        }

        public void Initialize(DynamicBottleController bottleController)
        {
            transform.parent = bottleController.transform;
            // y座標を中心から上に調整する
            transform.localPosition = new Vector3(0, 75f);
        }
    }
}
