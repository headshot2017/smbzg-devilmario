using MelonLoader;
using UnityEngine;
using SMBZG;
using System.Reflection;

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

            // modify koopa bros to use a custom class
            // so when possessed, it'll follow Devil Mario
            GameObject KoopaBrosPrefab = BattleCache.ins.CharacterData_KoopaBros.Prefab_BattleGameObject;

            KoopaRedControl original = KoopaBrosPrefab.GetComponent<KoopaRedControl>();
            CustomKoopaRedControl custom = KoopaBrosPrefab.AddComponent<CustomKoopaRedControl>();
            /*
            foreach (PropertyInfo property in original.GetType().GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    custom.GetType().BaseType.GetProperty(property.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(custom, property.GetValue(original));
                }
                catch (Exception)
                {

                }
            }
            */
            foreach (FieldInfo field in original.GetType().GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    custom.GetType().BaseType.GetField(field.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(custom, field.GetValue(original));
                }
                catch (Exception)
                {

                }
            }


            KoopaBroControl originalBro = original.KoopaBro_Prefab.GetComponent<KoopaBroControl>();
            CustomKoopaBroControl customBro = original.KoopaBro_Prefab.AddComponent<CustomKoopaBroControl>();
            foreach (FieldInfo field in originalBro.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    customBro.GetType().BaseType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(customBro, field.GetValue(originalBro));
                }
                catch (Exception)
                {

                }
            }

            GameObject.Destroy(originalBro);
            GameObject.Destroy(original);
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