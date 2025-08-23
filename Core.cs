using MelonLoader;
using UnityEngine;
using SMBZG;

[assembly: MelonInfo(typeof(DevilMarioMod.Core), "DevilMario", "1.3.1", "Headshotnoby/headshot2017", null)]
[assembly: MelonGame("Jonathan Miller aka Zethros", "SMBZ-G")]

namespace DevilMarioMod
{
    public class Core : MelonMod
    {
        public static CustomCharacter devilMarioCC;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            devilMarioCC = null;
            CharLoader.Core.afterCharacterLoad += LoadDevilMario;
            CharLoader.Core.resetBattleParticipant += ResetBattleParticipant;
            CharLoader.Core.setupSpecificInventory += SetupCharacterSpecificInventory;
        }

        void LoadDevilMario(CustomCharacter cc)
        {
            if (cc.internalName != "DevilMario") return;
            devilMarioCC = cc;

            GameObject Prefab = cc.characterData.Prefab_BattleGameObject;
            CustomBaseCharacter old = Prefab.GetComponent<CustomBaseCharacter>();
            DevilMarioControl devilMario = Prefab.AddComponent<DevilMarioControl>();
            devilMario.SetupFromOldComponent(cc, old);
            GameObject.Destroy(old);

            GameObject BooPrefab = cc.companions["Boo"].prefab;
            old = BooPrefab.GetComponent<CustomBaseCharacter>();
            DevilBooControl devilBoo = BooPrefab.AddComponent<DevilBooControl>();
            devilBoo.SetupFromOldComponent(cc, old);
            devilBoo.enabled = true;
            GameObject.Destroy(old);
        }

        void ResetBattleParticipant(BattleParticipantDataModel participant)
        {
            if (participant.InitialCharacterData != devilMarioCC.characterData)
                return;

            participant.AdditionalCharacterSpecificDataDictionary.TryGetValue("DevilMarioInventoryData", out var value);
            if (value == null)
            {
                DevilMarioInventoryDataModel value2 = new DevilMarioInventoryDataModel
                {
                    //FireFlowerQuantity = MarioInventoryDataModel.GetPowerupMaxQuantityByMaxHealth(Health.Max)
                };
                participant.AdditionalCharacterSpecificDataDictionary.Add("DevilMarioInventoryData", value2);
            }
        }

        void SetupCharacterSpecificInventory(UI_InventoryContainer container, BattleParticipantDataModel participant)
        {
            if (participant.InitialCharacterData != devilMarioCC.characterData)
                return;

            participant.AdditionalCharacterSpecificDataDictionary.TryGetValue("DevilMarioInventoryData", out var obj);
            (obj as DevilMarioInventoryDataModel).SetupInventoryUI(container);
            return;
        }
    }
}