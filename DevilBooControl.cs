using UnityEngine;
using System.Collections;
using SMBZG;
using System.Reflection;
using MelonLoader;
using System.ComponentModel;

public class DevilBooControl : CustomBaseCharacter
{
    public enum PossessedENUM
    {
        None,
        KoopaBro,
        AxemPink,
        AxemGreen,
        AxemBlack,
        AxemYellow,
        Other
    }

    public class PossessedClass
    {
        public PossessedClass()
        {
            Target = null;
            Enum = PossessedENUM.None;
            TimeLeft = -1;
            OriginalTag = "";
        }

        public void StopPossessing()
        {
            Target.tag = OriginalTag;
            Transform[] transforms = Target.GetComponentsInChildren<Transform>();
            foreach (Transform transform in transforms)
                transform.tag = OriginalTag;

            SoundCache.ins.PlaySound(EndSound);

            if (Enum == PossessedENUM.KoopaBro)
            {
                CustomKoopaBroControl KoopaNPC = (CustomKoopaBroControl)Target;
                SpriteRenderer Comp_SprRen = (SpriteRenderer)typeof(KoopaBroControl).GetField("Comp_SprRen", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(KoopaNPC);
                CustomKoopaRedControl KoopaLeader = KoopaNPC.KoopaLeader;

                Comp_SprRen.material.color = Color.white;
                KoopaLeader.KoopaBroQueue.Insert(KoopaLeader.KoopaBroQueue.Count, KoopaNPC);
                typeof(KoopaBroControl).GetMethod("ClearQueue", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(KoopaNPC, null);
                KoopaNPC.ActualLeader = KoopaLeader;
            }
            else
            {
                Target.Comp_SpriteRenderer.material.color = Color.white;
                if (Target.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
                {
                    BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)Target;
                    AxemRangersX_RangerDataModel.Ranger StatusData = (AxemRangersX_RangerDataModel.Ranger)AxemNPC.GetType().GetField("StatusData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AxemNPC);
                    BaseAxemRanger_NPC.SpawnStateENUM SpawnState =
                            (BaseAxemRanger_NPC.SpawnStateENUM)AxemNPC.GetType().GetField("SpawnState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AxemNPC);

                    AxemNPC.ClearTaskQueue();
                    if (SpawnState != BaseAxemRanger_NPC.SpawnStateENUM.Despawned)
                    {
                        AxemNPC.Despawn();
                        StatusData.CooldownTimer = 1f;
                    }
                }
            }

            Target = null;
            Enum = PossessedENUM.None;
        }

        public void Update()
        {
            if (!Target || TimeLeft < 0) return;

            switch(Enum)
            {
                case PossessedENUM.KoopaBro:
                    CustomKoopaBroControl KoopaTarget = (CustomKoopaBroControl)Target;
                    SpriteRenderer Comp_SprRen = (SpriteRenderer)typeof(KoopaBroControl).GetField("Comp_SprRen", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(KoopaTarget);
                    Comp_SprRen.material.color = new Color(1, 0.75f, 0.75f);

                    CustomKoopaRedControl KoopaLeader = KoopaTarget.KoopaLeader;
                    PlayerStateENUM k_PlayerState = (PlayerStateENUM)KoopaLeader.GetType().GetField("PlayerState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(KoopaLeader);
                    bool k_IsRushing = (bool)KoopaLeader.GetType().GetField("IsRushing", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(KoopaLeader);
                    if (k_IsRushing || k_PlayerState == PlayerStateENUM.Cinematic_NoInput)
                    {
                        StopPossessing();
                        return;
                    }
                    break;

                default:
                    Target.Comp_SpriteRenderer.material.color =  new Color(1, 0.75f, 0.75f);
                    if (!Target.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
                        break;

                    BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)Target;
                    AxemRangersX_RangerDataModel.Ranger StatusData = (AxemRangersX_RangerDataModel.Ranger)AxemNPC.GetType().GetField("StatusData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AxemNPC);
                    StatusData.CooldownTimer = 1f;

                    break;
            }

            bool IsRushing = (bool)Target.GetType().GetField("IsRushing", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Target);
            TimeLeft -= BattleController.instance.ActorDeltaTime;
            if (TimeLeft < 0 || IsRushing)
            {
                StopPossessing();
            }
        }

        public BaseCharacter Target;
        public PossessedENUM Enum;
        public float TimeLeft;
        public string OriginalTag;
        public AudioClip EndSound;
    }

    public List<Func<IEnumerator>> TaskQueue;
    public BaseCharacter Leader;

    bool QueueInProcess;
    public bool Spawned;

    PossessedClass Possession;

    public bool IsAvailable => (IsIdle && !IsHurt && Possession.Enum == PossessedENUM.None);
    public bool IsPossessing => (Possession.Target != null && Possession.Enum != PossessedENUM.None && Possession.TimeLeft > 0f);

    public override BattleParticipantDataModel GetMyParticipantDataReference()
    {
        if (Leader.GetType().IsSubclassOf(typeof(CustomBaseCharacter)))
        {
            return ((CustomBaseCharacter)Leader).GetMyParticipantDataReference();
        }

        return (BattleParticipantDataModel)Leader.GetType().GetMethod("GetMyParticipantDataReference", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Leader, null);
    }

    protected override void Awake()
    {
        base.Awake();

        SoundEffect_Land = AudioClip.Create("empty", 1024, 1, 8000, false);

        IsNPC = true;
        QueueInProcess = false;
        Spawned = true;
        TaskQueue = new List<Func<IEnumerator>>();
        Possession = new PossessedClass();
    }

    protected override void Start()
    {
        base.HitBox_0 = base.transform.Find("HitBox_0").GetComponent<HitBox>();
        base.HitBox_0.tag = base.tag;
        //TaskQueue.Add(MatchStartDespawn);
        Despawn();
        Comp_InterplayerCollider.Disable();
    }

    protected override void Update_Moving()
    {
        if (Spawned && TaskQueue.Count == 0 && IsIdle)
        {
            TaskQueue.Add(FlyBackToLeader);
        }
    }

    public void Setup(CustomCharacter c, DevilMarioControl lead)
    {
        cc = c;
        Leader = lead;

        tag = lead.tag;
        Comp_Hurtbox.tag = lead.tag;
        IsFacingRight = lead.IsFacingRight;

        typeof(BattleParticipantDataModel).GetField("PreventManualEnergyGain", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GetMyParticipantDataReference(), false);
        GetMyParticipantDataReference().OnComboEnd += Handle_OnComboEnd;

        Comp_Animator.enabled = false;
        Comp_CustomAnimator.SetAnimList(cc.companions["Boo"].animations, transform.Find("SpriteRenderer").gameObject, cc.companions["Boo"].offset, cc.companions["Boo"].scale, HitBox_0);
        Comp_CustomAnimator.m_GetUpTimer = 0;
        Comp_CustomAnimator.Play("Idle");
    }

    protected override void Update()
    {
        base.Update();

        if (SMBZGlobals.IsPaused)
        {
            return;
        }

        Comp_CustomAnimator.IgnoreIngameSprite = true;

        Possession.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        if (QueueInProcess)
        {
            yield break;
        }

        QueueInProcess = true;
        while (TaskQueue.Count > 0)
        {
            if (base.IsHurt || base.IsAttacking)
            {
                yield return null;
                continue;
            }

            yield return TaskQueue[0]();
            if (TaskQueue.Count > 0)
            {
                TaskQueue.RemoveAt(0);
            }
        }

        QueueInProcess = false;
    }

    public void ClearTaskQueue()
    {
        if (TaskQueue.Count != 0)
        {
            StopAllCoroutines();
            QueueInProcess = false;
            TaskQueue.Clear();
        }
    }

    public virtual void Spawn(Vector2 spawnLocation, bool isFacingRight)
    {
        IsFacingRight = isFacingRight;
        transform.position = spawnLocation;
        SetPlayerState(PlayerStateENUM.Idle);
        Comp_SpriteRenderer.enabled = true;
        Spawned = true;

        SoundCache.ins.PlaySound(cc.sounds["boo"]);
        Comp_CustomAnimator.Play("Idle");

        CustomEffectSprite fx = CustomEffectSprite.Create(transform.position, cc.effects["booParticle"], FaceDir == 1, true);
        fx.AnchorPositionToObject(Leader.transform, Vector2.zero);

        SetGravityOverride(0f);
        SetVelocity(0, 17);
        TaskQueue.Add(WaitTilStopMoving);
    }

    public virtual void Despawn()
    {
        SetPlayerState(PlayerStateENUM.Idle);
        SetField("IsIntangible", true);
        if (GetHitboxDamageProperties() != null)
        {
            GetHitboxDamageProperties().IsNullified = true;
        }

        Comp_SpriteRenderer.enabled = false;
        Spawned = false;

        /*
        if (QueuedToDestroySelfOnDespawn)
        {
            StartCoroutine(Task_DestroySelf());
        }
        */
    }

    public override void Hurt(TakeDamageRequest request)
    {
        HitBox_0.IsActive = false;

        request.intensity *= 0.3f;
        if (!request.blockStun.HasValue)
        {
            request.blockStun = request.hitStun / 3f;
        }

        if (!request.stun.HasValue)
        {
            request.stun = request.damage * 3f;
        }

        if (!request.energyGain.HasValue)
        {
            request.energyGain = request.damage * 1f;
        }

        SMBZGlobals.Intensity.IncreaseBy(request.intensity);
        if (base.IsGuarding && !request.isBackAttack && !request.isUnblockable)
        {
            SetVelocity(request.blockedLaunch);
            SetField("BlockStun", request.blockStun.Value);
            EffectSprite.Create(base.transform.position, EffectSprite.Sprites.HitsparkBlock, FaceDir == 1);
            SoundCache.ins.PlaySound(SoundCache.ins.Battle_Hit_1B);
            request.AttackingParticipant.IncrementEnergy(request.energyGain.Value * 0.5f * request.AttackingParticipant.GetActiveEnergyGainMultiplier());
        }
        else
        {
            SetPlayerState(PlayerStateENUM.Hurt);
            DeactivateAllHitboxes();
            base.HitStun = request.hitStun;
            if (base.HitStun > 2f)
            {
                base.HitStun = 2f;
            }

            //WasHitWhileInPlay = true;
            Comp_CustomAnimator.Play("Hit");
            if (request.EffectParameters != null)
            {
                EffectSprite.Create(base.transform.position, request.EffectParameters.SpriteHash, FaceDir == 1, request.EffectParameters.DieTimer, request.EffectParameters.FadeOutTime);
            }

            if (request.OnHitSoundEffect != null)
            {
                SoundCache.ins.PlaySound(request.OnHitSoundEffect);
            }

            if (base.IsOnGround && request.launch.y < -5f)
            {
                request.launch.y *= -1f;
                SetField("DragOverride", 2f);
                SetVelocity(request.launch);
            }
            else
            {
                SetField("DragOverride", 2f);
                SetVelocity(request.launch);
            }

            ClearTaskQueue();
            InterruptAndNullifyPreparedAttack();
            request.AttackingParticipant.IncrementEnergy(request.energyGain.Value * 0.5f * request.AttackingParticipant.GetActiveEnergyGainMultiplier());
            OnDamaged(request.damage);
        }

        PursueData = null;
        CurrentAttackData = null;
        JumpCharge = 0f;
    }

    public override void OnClash(BaseCharacter opponent)
    {
        base.OnClash(opponent);

        HitBox_0.IsActive = false;
        Comp_CustomAnimator.Play("Hit");
        if (CurrentAttackData != null) CurrentAttackData.OnAnimationEnd();
        SetVelocity(-GetVelocity());
        ClearTaskQueue();
        TaskQueue.Add(WaitTilStopMoving);
    }

    public BaseCharacter FindClosestNPC()
    {
        BaseCharacter result = null;
        float num = 10000f;
        foreach (string teamTag in BattleCache.teamTags)
        {
            if (teamTag == base.tag)
                continue;

            GameObject[] array = GameObject.FindGameObjectsWithTag(teamTag);
            foreach (GameObject obj in array)
            {
                if (obj.layer != LayerMask.NameToLayer("Player"))
                    continue;

                BaseCharacter component = obj.GetComponent<BaseCharacter>();
                if (!component)
                    continue;

                bool c_IsNPC = (bool)component.GetType().GetField("IsNPC", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component);
                if (!c_IsNPC)
                    continue;

                PlayerStateENUM state = (PlayerStateENUM)component.GetType().GetField("PlayerState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component);
                if (state == PlayerStateENUM.Cinematic_NoInput)
                    continue;

                if (component.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
                {
                    BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)component;
                    bool despawned =
                        (BaseAxemRanger_NPC.SpawnStateENUM)AxemNPC.GetType().GetField("SpawnState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AxemNPC)
                        == BaseAxemRanger_NPC.SpawnStateENUM.Despawned;

                    if (despawned)
                        continue;
                }
                else if (component.GetType().IsSubclassOf(typeof(CustomBaseCharacter)))
                {
                    if (!component.Comp_SpriteRenderer.enabled)
                        continue;
                }

                float num2 = Vector2.Distance(transform.position, obj.transform.position);
                if (num2 < num)
                {
                    num = num2;
                    result = component;
                }
            }
        }

        return result;
    }

    private IEnumerator WaitTilStopMoving()
    {
        SetField("DragOverride", 15f);
        SetField("IsIntangible", true);
        SetPlayerState(PlayerStateENUM.Attacking);
        HitBox_0.IsActive = false;

        while (GetVelocity().magnitude > 0.1f) yield return null;

        SetField("IsIntangible", false);
        SetPlayerState(PlayerStateENUM.Idle);
        yield break;
    }

    private IEnumerator FlyBackToLeader()
    {
        SetField("DragOverride", 0f);
        SetField("IsIntangible", true);
        UpdateSpriteSortOrder(-30);
        SetPlayerState(PlayerStateENUM.Attacking);
        HitBox_0.IsActive = false;
        Comp_CustomAnimator.Play("Move");

        float elapsed = 0;
        Vector2 startPos = transform.position;
        while (elapsed < 0.5f)
        {
            transform.position = Vector2.Lerp(startPos, Leader.transform.position, elapsed / 0.5f);
            IsFacingRight = (Leader.transform.position.x - transform.position.x) >= 0;

            elapsed += BattleController.instance.ActorDeltaTime;

            yield return null;
        }

        CustomEffectSprite fx = CustomEffectSprite.Create(transform.position, cc.effects["booParticle"], FaceDir == 1, true);
        fx.AnchorPositionToObject(Leader.transform, Vector2.zero);

        SoundCache.ins.PlaySound(cc.sounds["boo_return"], 0.75f);

        Despawn();
        yield break;
    }

    public void QueueUp_Attack_Launch()
    {
        if (!IsAvailable) return;
        TaskQueue.Add(Attack_Launch);
    }

    private IEnumerator Attack_Launch()
    {
        SetField("DragOverride", 0f);
        SetField("IsIntangible", false);

        BaseCharacter target = FindClosestTarget(true);
        if (target == null)
        {
            SetPlayerState(PlayerStateENUM.Idle);
            yield break;
        }
        Melon<DevilMarioMod.Core>.Logger.Msg($"picked target: {target.gameObject.name}");

        float speed = 0;
        float dir = 0;
        float lifetime = 3;
        bool hit = false;
        PrepareAnAttack(new AttackBundle
        {
            AnimationName = "Move",
            OnAnimationStart = delegate
            {
                SetPlayerState(PlayerStateENUM.Attacking);
                SetHitboxDamageProperties(new HitBoxDamageParameters
                {
                    Owner = this,
                    Tag = base.tag,
                    Damage = 3f,
                    HitStun = 0.5f,
                    FreezeTime = 0.02f,
                    GetLaunch = () => new Vector2(5 * FaceDir, 2f),
                    Priority = BattleCache.PriorityType.Light,
                    HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                    OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"],
                });
                HitBox_0.IsActive = true;
                HitBox_0.transform.localPosition = Vector2.zero;
                HitBox_0.transform.localScale = new Vector2(0.8f, 0.8f);
            },
            OnUpdate = delegate
            {
                lifetime -= BattleController.instance.ActorDeltaTime;
                if (lifetime < 0)
                {
                    CurrentAttackData.OnAnimationEnd();
                    return;
                }
                if (!hit)
                {
                    IsFacingRight = (target.transform.position.x - transform.position.x) >= 0;
                    dir = Helpers.Vector2ToDegreeAngle_180(transform.position, target.transform.position) * Mathf.Deg2Rad;
                }
                SetVelocity(Mathf.Cos(dir) * speed, Mathf.Sin(dir) * speed);
                speed = Mathf.Min(speed + 1, 25);
            },
            OnHit = delegate (BaseCharacter target, bool wasBlocked)
            {
                bool t_IsNPC = (bool)target.GetType().GetField("IsNPC", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
                if (!t_IsNPC)
                {
                    lifetime = 1;
                    hit = true;
                }
            }
        });

        yield break;
    }

    public bool Queue_Attack_Possess(BaseCharacter target)
    {
        if (!IsAvailable)
            return false;

        Possession.Enum = PossessedENUM.None;
        if (target.GetType() == typeof(CustomKoopaBroControl))
            Possession.Enum = PossessedENUM.KoopaBro;
        else if (target.GetType() == typeof(AxemPinkControl))
            Possession.Enum = PossessedENUM.AxemPink;
        else if (target.GetType() == typeof(AxemGreenControl))
            Possession.Enum = PossessedENUM.AxemGreen;
        else if (target.GetType() == typeof(AxemBlackControl))
            Possession.Enum = PossessedENUM.AxemBlack;
        else if (target.GetType() == typeof(AxemYellowControl))
            Possession.Enum = PossessedENUM.AxemYellow;
        else if (target.GetType().IsSubclassOf(typeof(CustomBaseCharacter)))
            Possession.Enum = PossessedENUM.Other;

        if (Possession.Enum == PossessedENUM.None)
        {
            TaskQueue.Add(Attack_Launch);
            return false;
        }

        if (target.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
        {
            BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)target;
            bool despawned =
                (BaseAxemRanger_NPC.SpawnStateENUM)AxemNPC.GetType().GetField("SpawnState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AxemNPC)
                == BaseAxemRanger_NPC.SpawnStateENUM.Despawned;

            if (despawned)
            {
                TaskQueue.Add(Attack_Launch);
                return false;
            }
        }

        Melon<DevilMarioMod.Core>.Logger.Msg($"possessing: {Possession.Enum}");
        Possession.Target = target;
        TaskQueue.Add(Attack_Possess);
        return true;
    }

    private IEnumerator Attack_Possess()
    {
        SetField("DragOverride", 0f);
        SetField("IsIntangible", true);
        UpdateSpriteSortOrder(-30);
        SetPlayerState(PlayerStateENUM.Attacking);
        Comp_CustomAnimator.Play("Move");

        float speed = 0;
        while (Vector2.Distance(transform.position, Possession.Target.transform.position) > 0.2f)
        {
            IsFacingRight = (Possession.Target.transform.position.x - transform.position.x) >= 0;
            float dir = Helpers.Vector2ToDegreeAngle_180(transform.position, Possession.Target.transform.position) * Mathf.Deg2Rad;
            speed = Mathf.Min(speed + 1, 25);
            SetVelocity(Mathf.Cos(dir) * speed, Mathf.Sin(dir) * speed);

            PlayerStateENUM state = (PlayerStateENUM)typeof(BaseCharacter).GetField("PlayerState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Possession.Target);
            if (state == PlayerStateENUM.Cinematic_NoInput)
            {
                ClearTaskQueue();
                SetPlayerState(PlayerStateENUM.Idle);
                yield break;
            }

            yield return null;
        }

        typeof(BaseCharacter).GetField("ArmorHealth", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Possession.Target, 0f);
        Possession.Target.Hurt(new TakeDamageRequest(GetMyParticipantDataReference())
        {
            damage = 1,
            hitStun = 0.75f,
            isUnblockable = true,
            OnHitSoundEffect = cc.sounds["boo_return"]
        });

        Possession.TimeLeft = 15;
        Possession.OriginalTag = Possession.Target.tag;
        Possession.EndSound = cc.sounds["boo_return"];

        Possession.Target.tag = Leader.tag;
        Transform[] transforms = Possession.Target.GetComponentsInChildren<Transform>();
        foreach (Transform transform in transforms)
            transform.tag = Leader.tag;

        if (Possession.Target.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
        {
            BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)Possession.Target;
            AxemNPC.ClearTaskQueue();
        }
        else if (Possession.Target.GetType() == typeof(CustomKoopaBroControl))
        {
            CustomKoopaBroControl KoopaNPC = (CustomKoopaBroControl)Possession.Target;
            CustomKoopaRedControl KoopaLeader = KoopaNPC.KoopaLeader;
            KoopaLeader.KoopaBroQueue.RemoveAt(KoopaLeader.KoopaBroQueue.IndexOf(KoopaNPC));
            KoopaNPC.ActualLeader = Leader;
        }

        CustomEffectSprite fx = CustomEffectSprite.Create(Possession.Target.transform.position, cc.effects["booParticle"], FaceDir == 1, true);
        fx.AnchorPositionToObject(Possession.Target.transform, Vector2.zero);

        Despawn();

        yield break;
    }

    public void CommandNPCToAttack()
    {
        if (!IsPossessing)
            return;

        if (Possession.Target.GetType().IsSubclassOf(typeof(BaseAxemRanger_NPC)))
        {
            BaseAxemRanger_NPC AxemNPC = (BaseAxemRanger_NPC)Possession.Target;
            Vector2 groundPositionViaRaycast = Leader.GetGroundPositionViaRaycast();
            if (Possession.Enum == PossessedENUM.AxemGreen)
            {
                groundPositionViaRaycast.x += -3 * Leader.FaceDir;
                groundPositionViaRaycast.y += 3.5f;
            }
            else
                groundPositionViaRaycast.x += -2 * Leader.FaceDir;
            AxemNPC.Spawn(groundPositionViaRaycast, Leader.FaceDir == 1);
        }

        MethodInfo[] methods = Possession.Target.GetType().GetMethods();
        List<MethodInfo> QueueUpMethods = new List<MethodInfo>();
        foreach (MethodInfo method in methods)
        {
            if (method.Name.StartsWith("QueueUp") && !method.Name.ToLower().Contains("rush") && !method.Name.ToLower().Contains("critical"))
                QueueUpMethods.Add(method);
        }

        MethodInfo attackMethod = QueueUpMethods.GetRandom();
        attackMethod.Invoke(Possession.Target, null);
    }
}