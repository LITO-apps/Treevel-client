﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.GamePlayScene.BulletWarning
{
	public class AimingHoleWarningController : NormalHoleWarningController
	{
		public void Initialize(Dictionary<string, int[]> additionalInfo)
		{
			// 撃ち抜くPanelを取得する
			var aimingPanel = additionalInfo["AimingPanel"];
			var count = additionalInfo["Count"][0];
			var panelNumber = aimingPanel[((count - 1) % aimingPanel.Length)];
			var panel = GameObject.Find("NumberPanel" + panelNumber);
			// 警告の表示位置をPanelと同じ位置にする
			transform.position = panel.transform.position;
			// 次の表示位置を求める
			additionalInfo.Remove("Count");
			additionalInfo.Add("Count", new[] {count + 1});
		}
	}
}
