using MelonLoader;
using UnityEngine;
using SMBZG;

[assembly: MelonInfo(typeof(DevilMarioMod.Core), "DevilMario", "1.0.0", "Headshotnoby/headshot2017", null)]
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
            SonicControl old = Prefab.GetComponent<SonicControl>(); // CharLoader takes Sonic's character prefab as a base
            DevilMarioControl devilMario = Prefab.AddComponent<DevilMarioControl>();
            devilMario.Comp_Hurtbox = old.Comp_Hurtbox;
            devilMario.CharacterData = cc.characterData;
            GameObject.Destroy(old);

            GameObject BooPrefab = cc.companions["Boo"].prefab;
            old = BooPrefab.GetComponent<SonicControl>(); // same with companions
            DevilBooControl devilBoo = BooPrefab.AddComponent<DevilBooControl>();
            devilBoo.Comp_Hurtbox = old.Comp_Hurtbox;
            devilBoo.cc = cc;
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