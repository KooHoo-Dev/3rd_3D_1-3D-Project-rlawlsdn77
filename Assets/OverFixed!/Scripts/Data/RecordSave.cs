using System;
using System.IO;
using UnityEngine;

namespace OverFixed
{
    // 최고 기록 (JSON, Application.persistentDataPath). 완료한 항해 수가 많으면, 같으면 누적 크레딧이 많을 때 갱신한다
    [Serializable]
    public class RecordSave
    {
        private const string FileName = "RecordSave.json";

        public int BestVoyageCount;
        public int BestTotalCredit;     // 그때의 누적 크레딧

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        // 파일이 없으면 0회, 0 크레딧
        public static RecordSave Load()
        {
            if (File.Exists(FilePath) == false) return new RecordSave();
            return JsonUtility.FromJson<RecordSave>(File.ReadAllText(FilePath));
        }

        // 더 좋으면 바꾸고 저장한다. 갱신했으면 true
        public bool TryUpdate(int voyageCount, int totalCredit)
        {
            bool isBetter = voyageCount > BestVoyageCount || (voyageCount == BestVoyageCount && totalCredit > BestTotalCredit);
            if (isBetter == false) return false;

            BestVoyageCount = voyageCount;
            BestTotalCredit = totalCredit;
            File.WriteAllText(FilePath, JsonUtility.ToJson(this));
            return true;
        }
    }
}
