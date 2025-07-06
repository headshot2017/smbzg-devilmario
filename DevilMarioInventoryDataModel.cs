using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DevilMarioInventoryDataModel
{
    public const int MAX_BOOS = 2;
    public const float RELOAD_TIME = 10f;

    private Color FullColor = new Color(1f, 1f, 1f, 1f);
    private Color DevelopingColor = new Color(1f, 1f, 1f, 0.5f);

    public float ElapsedReloadTime;
    internal float ReloadSpeedMultiplier = 1f;

    private int _boos;
    public int LostBoos;

    private List<UI_InventoryItem> UI_Boos = new List<UI_InventoryItem>();

    private List<Action> callbacks = new List<Action>();

    public event Action OnAmmoChangeEvent;

    public int Boos
    {
        get
        {
            return _boos;
        }
        set
        {
            _boos = Mathf.Min(value, MAX_BOOS-LostBoos);
            OnAmmoChangeEvent?.Invoke();
        }
    }

    public DevilMarioInventoryDataModel()
    {
        Boos = MAX_BOOS;
        LostBoos = 0;
    }

    public void Update()
    {
        if (Boos < MAX_BOOS-LostBoos)
        {
            float num = ((BattleController.instance != null) ? BattleController.instance.ActorDeltaTime : Time.deltaTime);
            ElapsedReloadTime += num * Mathf.Clamp01(ReloadSpeedMultiplier);
            if (ElapsedReloadTime >= RELOAD_TIME)
            {
                ElapsedReloadTime = 0f;
                Boos++;
            }
            else
                OnAmmoChangeEvent?.Invoke();
        }
    }

    public void SetupInventoryUI(UI_InventoryContainer ui)
    {
        foreach (Action callback in callbacks)
        {
            int num = 0;
            while (num < UI_Boos.Count)
            {
                UI_InventoryItem uI_InventoryItem = UI_Boos[num];
                if (uI_InventoryItem != null)
                {
                    UnityEngine.Object.Destroy(uI_InventoryItem.gameObject);
                }

                UI_Boos.RemoveAt(num);
            }

            OnAmmoChangeEvent -= callback;
        }

        callbacks.Clear();
        ui.transform.RemoveAllChildren();

        Image Comp_Background = (Image)ui.GetType().GetField("Comp_Background", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui);
        Comp_Background.enabled = true;

        CustomCharacter cc = DevilMarioMod.Core.devilMarioCC;

        Action action = delegate
        {
            for (int i = 0; i < MAX_BOOS; i++)
            {
                UI_InventoryItem uI_InventoryItem2 = UI_Boos.ElementAtOrDefault(i);
                if (uI_InventoryItem2 == null)
                {
                    uI_InventoryItem2 = ui.InstantiateItem(cc.companions["Boo"].animations[Animator.StringToHash("Move")].actions[0].frame);
                    uI_InventoryItem2.Comp_Sprite.type = Image.Type.Filled;
                    uI_InventoryItem2.Comp_Sprite.fillMethod = Image.FillMethod.Horizontal;
                    uI_InventoryItem2.Comp_Sprite.fillOrigin = 0;
                    uI_InventoryItem2.Comp_Sprite.preserveAspect = true;
                    UI_Boos.Add(uI_InventoryItem2);
                }

                if (i < Boos)
                {
                    uI_InventoryItem2.Comp_Sprite.color = FullColor;
                    uI_InventoryItem2.Comp_Sprite.fillAmount = 1f;
                }
                else
                {
                    uI_InventoryItem2.Comp_Sprite.color = DevelopingColor;
                    uI_InventoryItem2.Comp_Sprite.fillAmount = (i == Boos) ? ElapsedReloadTime / RELOAD_TIME : 0;
                }
            }
        };
        action();
        OnAmmoChangeEvent += action;
        callbacks.Add(action);
    }
}