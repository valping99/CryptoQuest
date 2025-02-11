using System.Collections.Generic;
using CryptoQuest.Gameplay.Battle;

namespace CryptoQuest.Gameplay.Encounter
{
    public static class DataValidator
    {
        public static bool MonsterDataValidator(MonsterUnitDataModel data)
        {
            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(data);
                if (value == null || !IsMatchCustomBusinessRule(value))
                    return false;
            }

            return true;
        }

        public static bool IsStringsNotNull(string[] datas, int length, List<int> columnsException = null)
        {
            for (int i = 0; i < length; i++)
            {
                if (columnsException != null && columnsException.Contains(i))
                    continue;
                if (string.IsNullOrEmpty(datas[i])) return false;
            }

            return true;
        }

        public static bool IsValidPair(string firstData, string secondData)
        {
            return ((!string.IsNullOrEmpty(firstData)) && (!string.IsNullOrEmpty(secondData)));
        }

        public static bool IsCorrectBattleFieldSetup(EncounterData data)
        {
            float totalProbability = 0;
            foreach (var encounterSetup in data.Configs)
            {
                if (encounterSetup.Battlefield == null)
                    return false;
                totalProbability += encounterSetup.Probability;
            }

            return totalProbability == 1;
        }

        public static bool IsValidNumberOfMonsterSetup(List<string> battleIds)
        {
            int count = 0;
            foreach (var battleId in battleIds)
            {
                if (!string.IsNullOrEmpty(battleId))
                {
                    count += battleId.Split(',').Length;
                }
            }

            return count <= 4;
        }

        public static bool MonsterPartyValidator(EncounterDataModel data)
        {
            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(data);
                if (value == null || !IsMatchCustomBusinessRule(value))
                    return false;
            }

            return true;
        }

        private static bool IsMatchCustomBusinessRule(object value)
        {
            if (value is string || value is char || value is short)
                return !string.IsNullOrEmpty((string)value);

            if (value is int)
                return ((int)value) >= 0;

            if (value is double)
                return ((double)value) >= 0;

            if (value is float)
                return ((float)value) >= 0;
            return false;
        }
    }


    public class MonsterUnitDataModel
    {
        public int MonsterId { get; set; }
        public string LocalizedKey { get; set; }
        public string MonsterName { get; set; }
        public int ElementId { get; set; }
        public float MaxHP { get; set; }
        public float HP { get; set; }
        public float MP { get; set; }
        public float Strength { get; set; }
        public float Vitality { get; set; }
        public float Agility { get; set; }
        public float Intelligence { get; set; }
        public float Luck { get; set; }
        public float Attack { get; set; }
        public float SkillPower { get; set; }
        public float Defense { get; set; }
        public float EvasionRate { get; set; }
        public float CriticalRate { get; set; }
        public int Exp { get; set; }
        public float Gold { get; set; }
        public string DropItemID { get; set; }
        public float DropItemRate { get; set; }
        public string MonsterPrefabName { get; set; }
    }

    public class EncounterDataModel
    {
        public string Id { get; set; }
        public List<BattlePartyDataModel> BattleParties { get; set; } = new();
        public string BackgroundName { get; set; }
    }


    public class BattleFieldDataModel
    {
        public int BattleFieldId { get; set; }
        public List<int> BattleEncounterSetups { get; set; } = new();
        public List<int[]> BattleEnemyGroups { get; set; } = new();
        public EBattleType BattleType { get; set; }

        public enum EBattleType
        {
            BattleArea = 0,
            EventBattle = 1
        }
    }

    public class BattlePartyDataModel
    {
        public int BattleDataId { get; set; }
        public float Probability { get; set; }
    }

    public class LootTableDataModel
    {
        public int Id { get; set; }
        public float GoldAmount { get; set; }
        public List<RewardDefs> RewardDefs { get; set; }
    }

    public struct RewardDefs
    {
        public string Id;
        public int Amount;
    }
}