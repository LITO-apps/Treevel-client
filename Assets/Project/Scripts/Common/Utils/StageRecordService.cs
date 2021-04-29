using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Treevel.Common.Entities;
using Treevel.Common.Entities.GameDatas;
using Treevel.Common.Managers;
using Treevel.Common.Networks;
using Treevel.Common.Networks.Requests;

namespace Treevel.Common.Utils
{
    public sealed class StageRecordService
    {
        // For Singleton
        private StageRecordService() { }

        /// <summary>
        /// インスタンス
        /// </summary>
        public static readonly StageRecordService Instance = new StageRecordService();

        /// <summary>
        /// オンメモリに StageRecord を保持する
        /// </summary>
        private readonly Dictionary<string, StageRecord> _cachedStageRecordDic = new Dictionary<string, StageRecord>();

        /// <summary>
        /// 全ステージの StageRecord を読み込んでおく
        /// </summary>
        public async UniTask PreloadAllStageRecordsAsync()
        {
            var stageRecords = await NetworkService.Execute(new GetAllStageRecordListRequest());
            var stageRecordList = stageRecords as List<StageRecord> ?? stageRecords.ToList();

            foreach (var stageData in GameDataManager.GetAllStages()) {
                StageRecord stageRecord;
                try {
                    stageRecord = stageRecordList.First(stageRecord => stageRecord.treeId == stageData.TreeId
                                                                       && stageRecord.stageNumber == stageData.StageNumber);
                } catch {
                    stageRecord = PlayerPrefsUtility.GetObjectOrDefault(StageData.EncodeStageIdKey(stageData.TreeId, stageData.StageNumber),
                                                                        new StageRecord(stageData.TreeId, stageData.StageNumber));
                }

                _cachedStageRecordDic[StageData.EncodeStageIdKey(stageData.TreeId, stageData.StageNumber)] = stageRecord;
            }
        }

        /// <summary>
        /// 特定の StageRecord を取得する
        /// </summary>
        /// <param name="treeId"> 木の Id </param>
        /// <param name="stageNumber"> ステージ番号 </param>
        /// <returns> StageRecord </returns>
        public StageRecord Get(ETreeId treeId, int stageNumber)
        {
            return _cachedStageRecordDic[StageData.EncodeStageIdKey(treeId, stageNumber)];
        }

        /// <summary>
        /// 特定の木の StageRecord を取得する
        /// </summary>
        /// <param name="treeId"> 木の Id </param>
        public IEnumerable<StageRecord> Get(ETreeId treeId)
        {
            return _cachedStageRecordDic.Values.ToList()
                .Where(stageRecord => stageRecord.treeId == treeId);
        }

        /// <summary>
        /// 全ての StageRecord を取得する
        /// </summary>
        public IEnumerable<StageRecord> Get()
        {
            return _cachedStageRecordDic.Values.ToList();
        }

        /// <summary>
        /// StageRecord を保存する
        /// </summary>
        /// <param name="treeId"> 木の Id </param>
        /// <param name="stageNumber"> ステージ番号 </param>
        /// <param name="stageRecord"> 保存する StageRecord </param>
        public void Set(ETreeId treeId, int stageNumber, StageRecord stageRecord)
        {
            var key = StageData.EncodeStageIdKey(treeId, stageNumber);

            NetworkService.Execute(new UpdateStageRecordRequest(key, stageRecord))
                .ContinueWith(isSuccess => {
                    if (isSuccess) {
                        // データの保存に成功したら、PlayerPrefs とオンメモリにも保存する
                        PlayerPrefsUtility.SetObject(key, stageRecord);
                        _cachedStageRecordDic[key] = stageRecord;
                    }
                });
        }
    }
}