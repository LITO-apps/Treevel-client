﻿using System.Collections;
using System.Collections.Generic;
using Treevel.Common.Components;
using UnityEngine;

namespace Treevel.Modules.GamePlayScene
{
    public class GameSpriteUnifier : SpriteUnifier
    {
        protected override void SetBaseWidth()
        {
            baseWidth = DisplayUnifier.Instance.gameWindowWidth;
        }
    }
}