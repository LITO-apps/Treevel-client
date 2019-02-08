﻿using System;
using Project.Scripts.Utils.Definitions;
using UnityEngine;

namespace Project.Scripts.GamePlayScene.Bullet
{
	public abstract class CartridgeController : BulletController
	{
		public float additionalMargin = 0.00001f;
		public Vector2 motionVector;
		[System.NonSerialized] public float speed = 0.10f;

		protected override void Awake()
		{
			base.Awake();
			// BocCollider2Dのアタッチメント
			gameObject.AddComponent<BoxCollider2D>();
			// 銃弾の先頭部分のみに当たり判定を与える
			const float betweenPanels = TileSize.WIDTH - PanelSize.WIDTH;
			gameObject.GetComponent<BoxCollider2D>().offset =
				new Vector2(-(originalWidth - betweenPanels / 2) / 2, 0);
			gameObject.GetComponent<BoxCollider2D>().size =
				new Vector2(betweenPanels / 2, originalHeight);
			gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
			// RigidBodyのアタッチメント
			gameObject.AddComponent<Rigidbody2D>();
			gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
		}

		public abstract void Initialize(CartridgeDirection direction, int line);

		protected override void OnEnable()
		{
			GamePlayDirector.OnSucceed += OnSucceed;
			GamePlayDirector.OnFail += OnFail;
		}

		protected override void OnFail()
		{
			speed = 0;
		}

		protected void Update()
		{
			// Check if bullet goes out of window
			if (transform.position.x <
			    -((WindowSize.WIDTH + CartridgeSize.WIDTH * localScale) / 2 + additionalMargin) ||
			    transform.position.x > (WindowSize.WIDTH + CartridgeSize.WIDTH * localScale) / 2 + additionalMargin ||
			    transform.position.y <
			    -((WindowSize.HEIGHT + CartridgeSize.WIDTH * localScale) / 2 + additionalMargin) ||
			    transform.position.y > (WindowSize.HEIGHT + CartridgeSize.WIDTH * localScale) / 2 + additionalMargin)
				Destroy(gameObject);
		}

		protected void FixedUpdate()
		{
			transform.Translate(motionVector * speed, Space.World);
		}

		// 銃弾の初期配置の設定
		protected void SetInitialPosition(CartridgeDirection direction, int line)
		{
			switch (direction)
			{
				case CartridgeDirection.ToLeft:
					transform.position = new Vector2((WindowSize.WIDTH + CartridgeSize.WIDTH * localScale) / 2,
						WindowSize.HEIGHT * 0.5f - (TileSize.MARGIN_TOP + TileSize.HEIGHT * 0.5f) -
						TileSize.HEIGHT * (line - 1));
					motionVector = Vector2.left;
					break;
				case CartridgeDirection.ToRight:
					transform.position = new Vector2(-(WindowSize.WIDTH + CartridgeSize.WIDTH * localScale) / 2,
						WindowSize.HEIGHT * 0.5f - (TileSize.MARGIN_TOP + TileSize.HEIGHT * 0.5f) -
						TileSize.HEIGHT * (line - 1));
					motionVector = Vector2.right;
					break;
				case CartridgeDirection.ToUp:
					transform.position = new Vector2(TileSize.WIDTH * (line - 2),
						-(WindowSize.HEIGHT + CartridgeSize.WIDTH * localScale) / 2);
					motionVector = Vector2.up;
					break;
				case CartridgeDirection.ToBottom:
					transform.position = new Vector2(TileSize.WIDTH * (line - 2),
						(WindowSize.HEIGHT + CartridgeSize.WIDTH * localScale) / 2);
					motionVector = Vector2.down;
					break;
				default:
					throw new NotImplementedException();
			}

			// Check if a bullet should be flipped vertically
			transform.localScale = new Vector2(CartridgeSize.WIDTH / originalWidth,
				                       -1 * Mathf.Sign(motionVector.x) * CartridgeSize.HEIGHT / originalHeight) *
			                       localScale;

			// Calculate rotation angle
			var angle = Vector2.Dot(motionVector, Vector2.left) / motionVector.magnitude;
			angle = (float) (Mathf.Acos(angle) * 180 / Math.PI);
			angle *= -1 * Mathf.Sign(motionVector.y);

			transform.Rotate(new Vector3(0, 0, angle), Space.World);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			// 数字パネルとの衝突以外は考えない
			if (!other.gameObject.CompareTag("NumberPanel")) return;
			// 衝突したオブジェクトは赤色に変える
			gameObject.GetComponent<SpriteRenderer>().color = Color.red;
		}
	}
}
