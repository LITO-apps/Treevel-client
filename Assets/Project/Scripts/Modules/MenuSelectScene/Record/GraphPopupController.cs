using Treevel.Common.Entities;
using Treevel.Common.Entities.GameDatas;
using Treevel.Common.Networks;
using Treevel.Common.Networks.Objects;
using Treevel.Common.Networks.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Treevel.Modules.MenuSelectScene.Record
{
    public class GraphPopupController : MonoBehaviour
    {
        /// <summary>
        /// [Text] 挑戦回数
        /// </summary>
        [SerializeField] private Text _challengeNumText;

        /// <summary>
        /// [Text] 成功率
        /// </summary>
        [SerializeField] private Text _clearRateText;

        /// <summary>
        /// 現在表示しているポップアップのステージ番号
        /// </summary>
        public int currentStageNumber;

        public async void Initialize(ETreeId treeId, int stageNumber, float positionX)
        {
            // TODO: 季節が拡張されたら、季節に応じた色に設定する
            gameObject.GetComponent<Image>().color = Color.magenta;

            var challengeNum = StageStatus.Get(treeId, stageNumber).challengeNum;
            _challengeNumText.text = challengeNum.ToString();

            // ポップアップが表示される位置を、該当する棒グラフの位置に合わせて変える
            var rectTransform = GetComponent<RectTransform>();
            // 左右に必要なマージン。pivot が 0.5 なので、ポップアップの横幅も考慮する
            var margin = 50 + rectTransform.rect.width / 2;
            var position = rectTransform.position;
            if (positionX < margin) {
                // 左限界
                position.x = margin;
            } else if (Screen.width - margin < positionX) {
                // 右限界
                position.x = Screen.width - margin;
            } else {
                position.x = positionX;
            }
            rectTransform.position = position;

            var data = await NetworkService.Execute(new GetStageStatsRequest(StageData.EncodeStageIdKey(treeId, stageNumber)));
            var stageStats = (StageStats) data;
            _clearRateText.text = $"{stageStats.ClearRate * 100f:n2}";

            currentStageNumber = stageNumber;
        }
    }
}
