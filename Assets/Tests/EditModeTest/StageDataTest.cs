using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Treevel.Common.Entities;
using Treevel.Common.Entities.GameDatas;
using Treevel.Common.Managers;
using UnityEngine.TestTools;

namespace Tests.EditModeTest
{
    public class StageDataTest
    {
        private List<StageData> _stageDataList;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (_stageDataList != null) {
                yield break;
            }

            var task = GameDataManager.InitializeAsync();

            while (task.Status == UniTaskStatus.Pending) yield return null;

            if (!task.Status.IsCompletedSuccessfully()) {
                Assert.Fail();
            }

            _stageDataList = GameDataManager.GetAllStages().ToList();
        }

        /// <summary>
        /// トルネードのプロパティが正しく設定されているかの確認
        /// </summary>
        [Test]
        public void TestGimmickProperties_Tornado()
        {
            _stageDataList.ForEach(stageData => {
                stageData.GimmickDatas.Where(data => data.type == EGimmickType.Tornado)
                    .ToList()
                    .ForEach(tornadoData => {
                        Assert.IsNotEmpty(tornadoData.targetDirections, $"[{stageData.StageId}]: target directions should not be empty");
                        Assert.IsNotEmpty(tornadoData.targetLines, $"[{stageData.StageId}]: target directions should not be empty");
                        Assert.AreEqual(tornadoData.targetDirections.Count, tornadoData.targetLines.Count, $"[{stageData.StageId}]: length of target directions and target lines should be the same.");

                        // ランダム用のパラメータが入っているか
                        if (tornadoData.isRandom) {
                            Assert.NotZero(tornadoData.randomColumn.Sum(), $"[{stageData.StageId}]: random column not valid");
                            Assert.NotZero(tornadoData.randomDirection.Sum(), $"[{stageData.StageId}]: random direction not valid");
                            Assert.NotZero(tornadoData.randomRow.Sum(), $"[{stageData.StageId}]: random row not valid");
                        }
                    });
            });
        }

        [Test(ExpectedResult = 120)]
        public int TestStageNum()
        {
            return _stageDataList.Count;
        }

        [Test]
        public void TestNumOfGoalBottlesEqualsGoalTiles()
        {
            _stageDataList.ForEach(data => {
                var goalBottleNum = data.BottleDatas.Count(bottleData => bottleData.type == EBottleType.Normal);
                var goalTileNum = data.TileDatas.Count(tileData => tileData.type == ETileType.Goal);
                Assert.AreEqual(goalBottleNum, goalTileNum, $"invalid stage data: [{data.StageId}]");
            });
        }

        [Test]
        public void TestStageDataNotDuplicate()
        {
            var duplicates = _stageDataList.GroupBy(x => x.StageId)
                .Where(g => g.Count() > 1)
                .Select(y => new { StageId = y.Key })
                .ToList();

            var isDuplicated = duplicates.Any();
            Assert.That(isDuplicated, Is.False, duplicates.ToString);
        }
    }
}
