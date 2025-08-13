using MelonLoader;
using System.Reflection;
using System.Collections;
using UnityEngine;

public class DevilMarioControl : CustomBaseCharacter
{
    // yoinked from mecha sonic
    public class GrappleClass
    {
        public BaseCharacter GrappledTarget { get; set; }

        public Transform HoldPosition { get; set; }

        public bool IsActive
        {
            get
            {
                return GrappledTarget != null;
            }
        }

        public void Grab(BaseCharacter Target, Transform holdPos)
        {
            if (GrappledTarget != null)
            {
                Release();
            }

            GrappledTarget = Target;
            GrappledTarget.Comp_InterplayerCollider.Disable();
            HoldPosition = holdPos;
        }

        public BaseCharacter Release()
        {
            BaseCharacter grappledTarget = GrappledTarget;
            if (GrappledTarget != null)
            {
                GrappledTarget.ResetGravity();
                GrappledTarget.Comp_InterplayerCollider.Enable();
            }

            GrappledTarget = null;
            return grappledTarget;
        }
    }

    public GrappleClass GrappleData;

    public DevilMarioInventoryDataModel InventoryData
    {
        get
        {
            DevilMarioInventoryDataModel result = null;
            MyCharacterControl.ParticipantDataReference.AdditionalCharacterSpecificDataDictionary.TryGetValue("DevilMarioInventoryData", out var value);
            if (value != null)
            {
                result = value as DevilMarioInventoryDataModel;
            }

            return result;
        }
    }

    public Base_RushProperties RushProperties { get; set; }

    StaleableMove Staleable_Uppercut = new StaleableMove();
    StaleableMove Staleable_LegSweep = new StaleableMove();
    StaleableMove Staleable_SmackDown = new StaleableMove();
    StaleableMove Staleable_SuperUppercut = new StaleableMove();
    StaleableMove Staleable_AirGrab = new StaleableMove();

    bool AlternateCombo;
    bool MovingCombo;
    float HeavyPunch_Charge;
    float GroundGrab;
    float PossessionCooldown;

    DevilBooControl[] Boos;

    // AutoComboX_YZ
    // X: 1=stationary attack, 2=moving attack (MovingCombo = true)
    // Y: combo counter
    // Z: combo type. a=quick key press, b=delayed key press (AlternateCombo = true)
    // AutoCombo1_3a is the last in the quick key press combo

    public readonly int ASN_AutoCombo1_1 = Animator.StringToHash("AutoCombo1_1");
    public readonly int ASN_AutoCombo1_2a = Animator.StringToHash("AutoCombo1_2a");
    public readonly int ASN_AutoCombo1_2b = Animator.StringToHash("AutoCombo1_2b");
    public readonly int ASN_AutoCombo1_3a = Animator.StringToHash("AutoCombo1_3a");
    public readonly int ASN_AutoCombo1_3b = Animator.StringToHash("AutoCombo1_3b");
    public readonly int ASN_AutoCombo1_4 = Animator.StringToHash("AutoCombo1_4");

    public readonly int ASN_AutoCombo2_1 = Animator.StringToHash("AutoCombo2_1");
    public readonly int ASN_AutoCombo2_2a = Animator.StringToHash("AutoCombo2_2a");
    public readonly int ASN_AutoCombo2_2b = Animator.StringToHash("AutoCombo2_2b");
    public readonly int ASN_AutoCombo2_3a = Animator.StringToHash("AutoCombo2_3a");
    public readonly int ASN_AutoCombo2_3b = Animator.StringToHash("AutoCombo2_3b");

    public readonly int ASN_PrePursue = Animator.StringToHash("PrePursue");
    public readonly int ASN_PrePursueSpin = Animator.StringToHash("PrePursueSpin");
    public readonly int ASN_Pursue = Animator.StringToHash("Pursue");
    public readonly int ASN_PursueHit = Animator.StringToHash("PursueHit");
    public readonly int ASN_PursueHitMax = Animator.StringToHash("PursueHitMax");

    public readonly int ASN_Uppercut = Animator.StringToHash("Uppercut");
    public readonly int ASN_LegSweep = Animator.StringToHash("LegSweep");
    public readonly int ASN_AxeKick = Animator.StringToHash("AxeKick");
    public readonly int ASN_Smackdown = Animator.StringToHash("Smackdown");
    public readonly int ASN_Flurrykicks = Animator.StringToHash("Flurrykicks");
    public readonly int ASN_Flurrykicks_Land = Animator.StringToHash("Flurrykicks_Land");
    public readonly int ASN_AirGrab = Animator.StringToHash("AirGrab");
    public readonly int ASN_AirGrab_Success = Animator.StringToHash("AirGrab_Success");
    public readonly int ASN_AirGrab_Land = Animator.StringToHash("AirGrab_Land");
    public readonly int ASN_SuperUppercut = Animator.StringToHash("SuperUppercut");
    public readonly int ASN_TryGrab = Animator.StringToHash("TryGrab");
    public readonly int ASN_Grab = Animator.StringToHash("Grab");
    public readonly int ASN_Headbutt = Animator.StringToHash("Headbutt");
    public readonly int ASN_FacePunch = Animator.StringToHash("FacePunch");
    public readonly int ASN_BodySlam = Animator.StringToHash("BodySlam");
    public readonly int ASN_HeavyPunch_Charge = Animator.StringToHash("HeavyPunch_Charge");
    public readonly int ASN_HeavyPunch = Animator.StringToHash("HeavyPunch");
    public readonly int ASN_GroundPound_Start = Animator.StringToHash("GroundPound_Start");
    public readonly int ASN_GroundPound_Land = Animator.StringToHash("GroundPound_Land");
    public readonly int ASN_Groundbreaker = Animator.StringToHash("Groundbreaker");
    public readonly int ASN_Groundbreaker_Land = Animator.StringToHash("Groundbreaker_Land");
    public readonly int ASN_DevastatingCombo = Animator.StringToHash("DevastatingCombo");
    public readonly int ASN_ReleaseBoo = Animator.StringToHash("ReleaseBoo");
    public readonly int ASN_CommandBoo = Animator.StringToHash("CommandBoo");
    public readonly int ASN_CriticalStrike = Animator.StringToHash("CriticalStrike");

    public readonly int ASN_SuperUppercut_Rush = Animator.StringToHash("SuperUppercut_Rush");
    public readonly int ASN_Rush1 = Animator.StringToHash("Rush1");
    public readonly int ASN_Rush2a = Animator.StringToHash("Rush2a");
    public readonly int ASN_Rush2b = Animator.StringToHash("Rush2b");
    public readonly int ASN_Rush3_Flip = Animator.StringToHash("Rush3_Flip");
    public readonly int ASN_Rush3 = Animator.StringToHash("Rush3");
    public readonly int ASN_Rush4a = Animator.StringToHash("Rush4a");
    public readonly int ASN_Rush4b = Animator.StringToHash("Rush4b");
    public readonly int ASN_Rush4c = Animator.StringToHash("Rush4c");
    public readonly int ASN_Rush4d_Spin = Animator.StringToHash("Rush4d_Spin");
    public readonly int ASN_Rush4d = Animator.StringToHash("Rush4d");
    public readonly int ASN_Rush5 = Animator.StringToHash("Rush5");
    public readonly int ASN_Rush6a = Animator.StringToHash("Rush6a");
    public readonly int ASN_Rush6b = Animator.StringToHash("Rush6b");

    AttackBundle AttBun_AutoCombo1_1
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_AutoCombo1_1,
                OnAnimationStart = delegate
                {
                    AlternateCombo = false;
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 1f,
                        HitStun = 0.4f,
                        Launch = new Vector2(2 * FaceDir, 0),
                        FreezeTime = 0.03f,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
                    });
                },
            };
            atk.OnCustomQueue = delegate
            {
                IsComboLinkAvailable = true;
                if (atk.CustomQueueCallCount > 0)
                    AlternateCombo = true;
            };
            return atk;
        }
    }

    AttackBundle AttBun_AutoCombo1_2a
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_AutoCombo1_2a,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 1f,
                        HitStun = 0.35f,
                        Launch = new Vector2(2 * FaceDir, 0),
                        FreezeTime = 0.03f,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
                    });
                }
            };
            atk.OnCustomQueue = delegate
            {
                IsComboLinkAvailable = true;
                if (atk.CustomQueueCallCount > 0)
                    AlternateCombo = true;
            };
            return atk;
        }
    }

    AttackBundle AttBun_AutoCombo1_2b
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_AutoCombo1_2b,
                OnAnimationStart = delegate
                {
                    AlternateCombo = false;
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 1f,
                        HitStun = 0.5f,
                        Launch = new Vector2(2 * FaceDir, 0),
                        FreezeTime = 0.03f,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
                    });
                }
            };
            atk.OnCustomQueue = delegate
            {
                IsComboLinkAvailable = true;
                if (atk.CustomQueueCallCount > 0)
                    AlternateCombo = true;
            };
            return atk;
        }
    }

    AttackBundle AttBun_AutoCombo1_3a => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo1_3a,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 0.5f,
                Launch = new Vector2(7 * FaceDir, 2f),
                FreezeTime = 0.1f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful2"]
            });
        }
    };

    AttackBundle AttBun_AutoCombo1_3b
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_AutoCombo1_3b,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 0.5f,
                        HitStun = 0.55f,
                        Launch = new Vector2(2 * FaceDir, 0),
                        FreezeTime = 0.04f,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
                    });
                }
            };
            atk.OnCustomQueue = delegate
            {
                HitBox_0.ReinitializeID();
                if (atk.CustomQueueCallCount > 0)
                    IsComboLinkAvailable = true;
            };
            return atk;
        }
    }

    AttackBundle AttBun_AutoCombo1_4 => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo1_4,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 0.6f,
                Launch = new Vector2(10 * FaceDir, 2.5f),
                FreezeTime = 0.1f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful2"]
            });
        }
    };

    AttackBundle AttBun_AutoCombo2_1
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_AutoCombo2_1,
                OnAnimationStart = delegate
                {
                    AlternateCombo = false;
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 1f,
                        HitStun = 0.5f,
                        Launch = new Vector2(GetVelocity().x * 0.5f, 0),
                        FreezeTime = 0.03f,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
                    });
                },
            };
            atk.OnCustomQueue = delegate
            {
                IsComboLinkAvailable = true;
                if (atk.CustomQueueCallCount > 0)
                    AlternateCombo = true;
            };
            return atk;
        }
    }

    AttackBundle AttBun_AutoCombo2_2a => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo2_2a,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 0.45f,
                Launch = new Vector2(2 * FaceDir, 0),
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
            });
        },
        OnCustomQueue = delegate
        {
            IsComboLinkAvailable = true;
        }
    };

    AttackBundle AttBun_AutoCombo2_2b => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo2_2b,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 0.55f,
                Launch = new Vector2(2 * FaceDir, 0),
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
            });
        },
        OnCustomQueue = delegate
        {
            IsComboLinkAvailable = true;
        }
    };

    AttackBundle AttBun_AutoCombo2_3a => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo2_3a,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1.5f,
                HitStun = 0.5f,
                Launch = new Vector2(8 * FaceDir, 2.5f),
                FreezeTime = 0.1f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful2"]
            });
        }
    };

    AttackBundle AttBun_AutoCombo2_3b => new AttackBundle
    {
        AnimationNameHash = ASN_AutoCombo2_3b,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 0.6f,
                Launch = new Vector2(10 * FaceDir, 2.5f),
                FreezeTime = 0.1f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful2"]
            });
        }
    };

    AttackBundle AttBun_Smackdown => new AttackBundle
    {
        AnimationNameHash = ASN_Smackdown,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                GetHitStun = () => 0.6f - (0.1f * Staleable_SmackDown.StaleHitCount),
                Launch = new Vector2(6 * FaceDir, -10f),
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful1"],
                OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                {
                    bool IsNPC = SMBZGlobals.GetIsNPC(target);
                    if (target != null && !IsNPC && !wasBlocked)
                    {
                        Staleable_SmackDown.Set();
                    }
                }
            });
        }
    };

    AttackBundle AttBun_LegSweep => new AttackBundle
    {
        AnimationNameHash = ASN_LegSweep,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                GetHitStun = () => 0.6f - (0.1f * Staleable_LegSweep.StaleHitCount),
                Launch = new Vector2(7f, 4f),
                BlockedLaunch = new Vector2(3f, 0f),
                IsLaunchPositionBased = true,
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(11, 14)}"],
                OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                {
                    bool IsNPC = SMBZGlobals.GetIsNPC(target);
                    if (target != null && !IsNPC && !wasBlocked)
                    {
                        Staleable_LegSweep.Set();
                    }
                }
            });
        }
    };

    AttackBundle AttBun_AxeKick => new AttackBundle
    {
        AnimationNameHash = ASN_AxeKick,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 0.6f,
                Launch = new Vector2(3 * FaceDir, 12f),
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"],
            });
        }
    };

    AttackBundle AttBun_Flurrykicks
    {
        get
        {
            HitBoxDamageParameters dmgParams = new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1 / 4f,
                HitStun = 0.3f,
                GetLaunch = () => GetVelocity(),
                FreezeTime = 0.02f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
            };
            AttackBundle end = new AttackBundle
            {
                AnimationNameHash = ASN_Flurrykicks_Land,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                }
            };
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_Flurrykicks,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(dmgParams);
                },
                OnLanding = delegate
                {
                    PrepareAnAttack(end);
                },
                OnUpdate = delegate
                {
                    Vector2 vel = GetVelocity();
                    if (Mathf.Sign(vel.x) != FaceDir || Mathf.Abs(vel.x) < 4f) vel.x = 4f * FaceDir;
                    if (vel.y < -2f) vel.y = -2f;
                    SetVelocity(vel);
                },
                OnCustomQueue = delegate
                {
                    dmgParams.OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"];
                    base.HitBox_0.ReinitializeID();
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_Uppercut => new AttackBundle
    {
        AnimationNameHash = ASN_Uppercut,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1.5f,
                GetHitStun = () => 0.7f - (0.1f * Staleable_Uppercut.StaleHitCount),
                Launch = new Vector2(2 * FaceDir, 16f),
                FreezeTime = 0.04f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful1"],
                OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                {
                    bool IsNPC = SMBZGlobals.GetIsNPC(target);
                    if (target != null && !IsNPC && !wasBlocked)
                    {
                        Staleable_Uppercut.Set();
                    }
                }
            });
        }
    };

    AttackBundle AttBun_SuperUppercut => new AttackBundle
    {
        AnimationNameHash = ASN_SuperUppercut,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 4f,
                GetHitStun = () => 1.22f - (0.25f * Staleable_SuperUppercut.StaleHitCount),
                Launch = new Vector2(3 * FaceDir, 14f),
                FreezeTime = 0.07f,
                Priority = BattleCache.PriorityType.Medium,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds["hit_powerful2"],
                OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                {
                    bool IsNPC = SMBZGlobals.GetIsNPC(target);
                    if (target != null && !IsNPC && !wasBlocked)
                    {
                        Staleable_SuperUppercut.Set();
                    }
                }
            });
        },
        OnHit = delegate (BaseCharacter target, bool wasBlocked)
        {
            bool IsNPC = SMBZGlobals.GetIsNPC(target);
            if (!IsNPC && !wasBlocked)
            {
                CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(target);
                if (t_MyCharacterControl.ParticipantDataReference.Stun.GetCurrent() >= t_MyCharacterControl.ParticipantDataReference.Stun.Max)
                {
                    SMBZGlobals.SetHitStun(target, 1.5f);
                    PrepareAnAttack(AttBun_SuperUppercut_Rush);
                }
            }
        },
        OnCustomQueue = delegate
        {
            SetVelocity(0, 14);
        }
    };

    AttackBundle AttBun_SuperUppercut_Rush
    {
        get
        {
            AttackBundle bundle = new AttackBundle
            {
                AnimationNameHash = ASN_SuperUppercut_Rush,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 2f,
                        HitStun = 5f,
                        IsUnblockable = true,
                        Launch = new Vector2(20 * FaceDir, 5f),
                        FreezeTime = 0.3f,
                        Priority = BattleCache.PriorityType.Critical,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                        OnHitSoundEffect = cc.sounds["hit5"]
                    });
                },
                OnCustomQueue = delegate
                {
                    SetVelocity(15 * FaceDir, GetVelocity().y+6);
                }
            };
            bundle.OnHit = delegate (BaseCharacter target, bool wasBlocked)
            {
                bool IsNPC = SMBZGlobals.GetIsNPC(target);
                if (target != null && !IsNPC && !wasBlocked)
                {
                    CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(target);

                    if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
                        SMBZGlobals.CameraManager.SetTargetGroup(t_MyCharacterControl.transform);

                    if (SaveData.Data.MovementRush_IsEnabled_ViaFullStunStrikes)
                    {
                        SMBZGlobals.MovementRushManager.StartNewMovementRush(IsFacingRight, new List<CharacterControl> { MyCharacterControl }, new List<CharacterControl> { t_MyCharacterControl });
                    }
                    else
                    {
                        CancelAndRefundPursue();
                        RushProperties = new Base_RushProperties();
                        RushProperties.Target = target;
                        Begin_Rush();

                        bundle.OnLanding = delegate
                        {
                            Comp_CustomAnimator.Play(ASN_Land, CurrentAttackData);
                        };
                        bundle.OnAnimationEnd = delegate
                        {

                        };
                    }
                }
            };
            return bundle;
        }
    }

    AttackBundle AttBun_TryGrab => new AttackBundle
    {
        AnimationNameHash = ASN_TryGrab,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            HitBoxDamageParameters properties = new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                HitStun = 0.8f,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(11, 13)}"]
            };
            properties.OnHitCallback = delegate (BaseCharacter target, bool wasblocked)
            {
                bool IsNPC = SMBZGlobals.GetIsNPC(target);
                if (IsNPC)
                {
                    target.Hurt(new TakeDamageRequest(GetMyParticipantDataReference())
                    {
                        damage = 5f,
                        intensity = 5f,
                        hitStun = 1f,
                        launch = new Vector2(10 * base.FaceDir, 0f),
                        isUnblockable = true
                    });
                }
                else if (target.IsHurt && !IsHurt && !GrappleData.IsActive)
                {
                    properties.HitStun = 0;
                    GrappleData.Grab(target, base.HitBox_0.transform);
                    PrepareAnAttack(AttBun_Grab);
                }
            };
            SetHitboxDamageProperties(properties);
        }
    };

    AttackBundle AttBun_Grab => new AttackBundle
    {
        AnimationNameHash = ASN_Grab,
        OnAnimationStart = delegate
        {
            GroundGrab = 0;
            Comp_InterplayerCollider.Disable();
            SetPlayerState(PlayerStateENUM.Attacking);
        },
        OnAnimationEnd = delegate
        {
            Comp_InterplayerCollider.Enable();
            SetPlayerState(PlayerStateENUM.Idle);
            CurrentAttackData = null;

            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
            if (target_Comp_CustomAnimator)
                target_Comp_CustomAnimator.enabled = true;
            else
                target_Comp_Animator.enabled = true;

            GrappleData.Release();
            GroundGrab = -1;
        },
        OnInterrupt = delegate
        {
            Comp_InterplayerCollider.Enable();

            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
            if (target_Comp_CustomAnimator)
                target_Comp_CustomAnimator.enabled = true;
            else
                target_Comp_Animator.enabled = true;

            GrappleData.Release();
            GroundGrab = -1;
        },
        OnUpdate = delegate
        {
            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
            if (target_Comp_CustomAnimator)
            {
                target_Comp_CustomAnimator.enabled = false;
                if (target_Comp_CustomAnimator.m_CurrentAnimation.hash != CustomAnimator.ASN_Hurt_AirUpwards)
                {
                    target_Comp_CustomAnimator.Play(CustomAnimator.ASN_Hurt_AirUpwards);
                }
            }
            else
            {
                target_Comp_Animator.enabled = false;

                if (target_Comp_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != ASN_Tumble)
                {
                    target_Comp_Animator.Play(ASN_Tumble);
                    target_Comp_Animator.SetBool("OnGround", false);
                    target_Comp_Animator.SetFloat("vspeed", 2f);
                    target_Comp_Animator.Update(BattleController.instance.ActorDeltaTime);
                }
            }

            GroundGrab += BattleController.instance.ActorDeltaTime;
            if (GroundGrab < 0.1f) return;

            int option = -1;
            if (!IsCPUControlled)
            {
                if (!MyCharacterControl.Button_A.IsHeld) return;

                if (MyCharacterControl.Button_Up.IsHeld)
                    option = 1;
                else if (MyCharacterControl.Button_Down.IsHeld)
                    option = 2;
                else
                    option = 0;
            }
            else if (GroundGrab >= 0.35f)
                option = UnityEngine.Random.Range(0, 3);

            switch (option)
            {
                case 0:
                    PrepareAnAttack(AttBun_Headbutt);
                    break;

                case 1:
                    PrepareAnAttack(AttBun_FacePunch);
                    break;

                case 2:
                    PrepareAnAttack(AttBun_BodySlam);
                    break;
            }
        }
    };

    AttackBundle AttBun_Headbutt
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_Headbutt,
                OnAnimationStart = delegate
                {
                    GroundGrab = -1;
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 6f,
                        HitStun = 0.6f,
                        Launch = new Vector2(10 * FaceDir, 3f),
                        FreezeTime = 0.04f,
                        IsUnblockable = true,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds["hit3"],
                        OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                        {
                            Comp_InterplayerCollider.Enable();

                            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
                            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
                            if (target_Comp_CustomAnimator)
                                target_Comp_CustomAnimator.enabled = true;
                            else
                                target_Comp_Animator.enabled = true;

                            GrappleData.Release();
                        }
                    });
                },
                OnInterrupt = delegate
                {
                    Comp_InterplayerCollider.Enable();

                    CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
                    Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
                    if (target_Comp_CustomAnimator)
                        target_Comp_CustomAnimator.enabled = true;
                    else
                        target_Comp_Animator.enabled = true;

                    GrappleData.Release();
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_FacePunch
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_FacePunch,
                OnAnimationStart = delegate
                {
                    GroundGrab = -1;
                    UpdateSpriteSortOrder(-2);
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 2f,
                        HitStun = 0.6f,
                        Launch = new Vector2(0, 0),
                        FreezeTime = 0,
                        IsUnblockable = true,
                        Priority = BattleCache.PriorityType.Light,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds["hit3"],
                        OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                        {
                            UpdateSpriteSortOrder(-2);
                        }
                    });
                },
                OnInterrupt = delegate
                {
                    Comp_InterplayerCollider.Enable();

                    CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
                    Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
                    if (target_Comp_CustomAnimator)
                        target_Comp_CustomAnimator.enabled = true;
                    else
                        target_Comp_Animator.enabled = true;

                    GrappleData.Release();
                }
            };
            atk.OnCustomQueue = delegate
            {
                HitBox_0.ReinitializeID();
                if (atk.CustomQueueCallCount == 1)
                {
                    HitBoxDamageParameters properties = GetHitboxDamageProperties();
                    properties.Launch = new Vector2(12 * FaceDir, 3f);
                    properties.OnHitCallback = delegate (BaseCharacter target, bool wasblocked)
                    {
                        Comp_InterplayerCollider.Enable();

                        CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
                        Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
                        if (target_Comp_CustomAnimator)
                            target_Comp_CustomAnimator.enabled = true;
                        else
                            target_Comp_Animator.enabled = true;

                        GrappleData.Release();
                    };
                    SetHitboxDamageProperties(properties);
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_BodySlam
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_BodySlam,
                OnAnimationStart = delegate
                {
                    GroundGrab = -1;

                    if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
                    {
                        SMBZGlobals.CameraManager.SetTargetGroup(MyCharacterControl.transform);
                    }

                    UpdateSpriteSortOrder(-2);
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 3f,
                        HitStun = 0.6f,
                        Launch = new Vector2(0, 0),
                        FreezeTime = 0,
                        IsUnblockable = true,
                        Priority = BattleCache.PriorityType.Medium,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds["hit14"],
                        OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                        {
                            UpdateSpriteSortOrder(-2);
                        }
                    });
                },
                OnInterrupt = delegate
                {
                    Comp_InterplayerCollider.Enable();

                    BaseCharacter grappled = GrappleData.Release();

                    CustomAnimator target_Comp_CustomAnimator = grappled.GetComponent<CustomAnimator>();
                    Animator target_Comp_Animator = grappled.GetComponent<Animator>();
                    if (target_Comp_CustomAnimator)
                        target_Comp_CustomAnimator.enabled = true;
                    else
                        target_Comp_Animator.enabled = true;

                    SMBZGlobals.CameraManager.SetTargetGroup_Default();
                    SMBZGlobals.CameraManager.SetFocusPosition(null);

                    Transform targetTransform = grappled.Comp_SpriteContainer;
                    if (!targetTransform)
                    {
                        targetTransform = grappled.Comp_SpriteRenderer.transform;
                        if (!targetTransform)
                            return;
                    }

                    targetTransform.localEulerAngles = Vector3.zero;
                    targetTransform.localPosition = Vector3.zero;
                },
                OnAnimationEnd = delegate
                {
                    Comp_InterplayerCollider.Enable();

                    SMBZGlobals.CameraManager.SetTargetGroup_Default();
                    SMBZGlobals.CameraManager.SetFocusPosition(null);

                    CurrentAttackData = null;
                    SetPlayerState(PlayerStateENUM.Idle);
                },
                OnUpdate = delegate
                {
                    if (!GrappleData.GrappledTarget)
                        return;

                    Transform targetTransform = GrappleData.GrappledTarget.Comp_SpriteContainer;
                    if (!targetTransform)
                    {
                        targetTransform = GrappleData.GrappledTarget.Comp_SpriteRenderer.transform;
                        if (!targetTransform)
                            return;
                    }

                    int[] frameAngles = new int[] { -45, -90, -45, 0, 70, 105, 90 };
                    float[] frameY = new float[] { 0, 0, 0, 0, -0.25f, -0.5f, -0.75f };
                    int mult = (FaceDir == GrappleData.GrappledTarget.FaceDir) ? -1 : 1;
                    targetTransform.localEulerAngles = new Vector3(0, 0, frameAngles[Comp_CustomAnimator.m_Frame] * mult);
                    targetTransform.localPosition = new Vector2(0, frameY[Comp_CustomAnimator.m_Frame]);
                }
            };
            atk.OnCustomQueue = delegate
            {
                HitBox_0.ReinitializeID();
                if (atk.CustomQueueCallCount == 0)
                {
                    HitBoxDamageParameters properties = GetHitboxDamageProperties();
                    properties.Launch = new Vector2(10 * FaceDir, 8f);
                    properties.OnHitCallback = delegate (BaseCharacter target, bool wasblocked)
                    {
                        Comp_InterplayerCollider.Enable();

                        BaseCharacter grappled = GrappleData.Release();

                        CustomAnimator target_Comp_CustomAnimator = grappled.GetComponent<CustomAnimator>();
                        Animator target_Comp_Animator = grappled.GetComponent<Animator>();
                        if (target_Comp_CustomAnimator)
                            target_Comp_CustomAnimator.enabled = true;
                        else
                            target_Comp_Animator.enabled = true;

                        SMBZGlobals.CameraManager.SetTargetGroup_Default();
                        SMBZGlobals.CameraManager.SetFocusPosition(null);

                        Transform targetTransform = grappled.Comp_SpriteContainer;
                        if (!targetTransform)
                        {
                            targetTransform = grappled.Comp_SpriteRenderer.transform;
                            if (!targetTransform)
                                return;
                        }

                        targetTransform.localEulerAngles = Vector3.zero;
                        targetTransform.localPosition = Vector3.zero;
                    };
                    SetHitboxDamageProperties(properties);
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_HeavyPunch_Charge => new AttackBundle
    {
        AnimationNameHash = ASN_HeavyPunch_Charge,
        OnAnimationStart = delegate
        {
            HeavyPunch_Charge = -1;
            SetPlayerState(PlayerStateENUM.Attacking);
        },
        OnUpdate = delegate
        {
            if (HeavyPunch_Charge < 0) return;

            HeavyPunch_Charge += BattleController.instance.ActorDeltaTime;

            bool release = (HeavyPunch_Charge >= 0.55f) || (!IsCPUControlled ? !MyCharacterControl.Button_Z_Attack.IsHeld : UnityEngine.Random.Range(0, 100) >= 80);
            if (release)
                PrepareAnAttack(HeavyPunch_Charge >= 0.4f ? AttBun_HeavyPunch_Big : AttBun_HeavyPunch_Small);
        },
        OnCustomQueue = delegate
        {
            HeavyPunch_Charge = 0;
        }
    };

    AttackBundle AttBun_HeavyPunch_Big => new AttackBundle
    {
        AnimationNameHash = ASN_HeavyPunch,
        OnAnimationStart = delegate
        {
            HeavyPunch_Charge = -1;
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 6f,
                HitStun = 1f,
                Launch = new Vector2(10 * FaceDir, 3f),
                FreezeTime = 0.06f,
                Priority = BattleCache.PriorityType.Medium,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds[$"hit_powerful{UnityEngine.Random.Range(1, 3)}"]
            });
        }
    };

    AttackBundle AttBun_HeavyPunch_Small => new AttackBundle
    {
        AnimationNameHash = ASN_HeavyPunch,
        OnAnimationStart = delegate
        {
            HeavyPunch_Charge = -1;
            SetPlayerState(PlayerStateENUM.Attacking);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 3f,
                HitStun = 0.7f,
                Launch = new Vector2(8 * FaceDir, 3f),
                FreezeTime = 0.04f,
                Priority = BattleCache.PriorityType.Medium,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit4"]
            });
        }
    };

    AttackBundle AttBun_AirGrab => new AttackBundle
    {
        AnimationNameHash = ASN_AirGrab,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            HitBoxDamageParameters properties = new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                HitStun = 3f,
                FreezeTime = 0.1f,
                Priority = BattleCache.PriorityType.Light,
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(11, 13)}"]
            };
            properties.OnHitCallback = delegate (BaseCharacter target, bool wasblocked)
            {
                bool IsNPC = SMBZGlobals.GetIsNPC(target);
                if (IsNPC)
                {
                    target.Hurt(new TakeDamageRequest(GetMyParticipantDataReference())
                    {
                        damage = 5f,
                        intensity = 5f,
                        hitStun = 1f,
                        launch = new Vector2(10 * base.FaceDir, 0f),
                        isUnblockable = true
                    });
                }
                else if (target.IsHurt && !IsHurt && !GrappleData.IsActive)
                {
                    properties.HitStun = 0;
                    properties.OnHitSoundEffect = null;
                    IsIntangible = true;
                    GrappleData.Grab(target, base.HitBox_0.transform);
                    PrepareAnAttack(AttBun_AirGrab_Success);
                }
            };
            SetHitboxDamageProperties(properties);
        }
    };

    AttackBundle AttBun_AirGrab_Success => new AttackBundle
    {
        AnimationNameHash = ASN_AirGrab_Success,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            UpdateSpriteSortOrder(-2);
        },
        OnUpdate = delegate
        {
            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
            if (target_Comp_CustomAnimator)
            {
                target_Comp_CustomAnimator.enabled = false;
                if (target_Comp_CustomAnimator.m_CurrentAnimation.hash != ASN_Grounded)
                    target_Comp_CustomAnimator.Play(ASN_Grounded);
                return;
            }

            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
            target_Comp_Animator.enabled = false;
            if (target_Comp_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != ASN_Grounded)
            {
                target_Comp_Animator.Play(ASN_Grounded);
                target_Comp_Animator.Update(BattleController.instance.ActorDeltaTime);
            }
        },
        OnLanding = delegate
        {
            PrepareAnAttack(AttBun_AirGrab_Land);
        }
    };

    AttackBundle AttBun_AirGrab_Land => new AttackBundle
    {
        AnimationNameHash = ASN_AirGrab_Land,
        OnAnimationStart = delegate
        {
            SoundCache.ins.PlaySound(cc.sounds["hit4"]);

            CustomAnimator target_Comp_CustomAnimator = GrappleData.GrappledTarget.GetComponent<CustomAnimator>();
            Animator target_Comp_Animator = GrappleData.GrappledTarget.GetComponent<Animator>();
            if (target_Comp_CustomAnimator)
                target_Comp_CustomAnimator.enabled = true;
            else
                target_Comp_Animator.enabled = true;

            GrappleData.GrappledTarget.Hurt(new TakeDamageRequest(GetMyParticipantDataReference())
            {
                damage = 3f - (0.5f * Staleable_AirGrab.StaleHitCount),
                intensity = 3f - (0.5f * Staleable_AirGrab.StaleHitCount),
                hitStun = 0.75f - (2f * Staleable_AirGrab.StaleHitCount),
                launch = new Vector2(GetVelocity().x, 5f),
                isUnblockable = true
            });
            GrappleData.Release();

            Staleable_AirGrab.Set();

            IsIntangible = false;
            SetPlayerState(PlayerStateENUM.Attacking);
        }
    };

    AttackBundle AttBun_Groundpound
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_GroundPound_Start,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetVelocity(0, 0);
                    SetGravityOverride(0);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 2f,
                        HitStun = 0.65f,
                        GetLaunch = () => new Vector2(0f, Mathf.Lerp(-5f, -15f, (base.transform.position.y - GetGroundPositionViaRaycast().y) / 5f)),
                        FreezeTime = 0.15f,
                        Priority = BattleCache.PriorityType.Medium,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"],
                    });
                },
                OnInterrupt = delegate
                {
                    Comp_InterplayerCollider.Enable();
                    ResetGravity();
                },
                OnClashCallback = delegate
                {
                    Comp_InterplayerCollider.Enable();
                    ResetGravity();
                    SoundCache.ins.PlaySound(cc.sounds["explosion"]);
                }
            };
            atk.OnCustomQueue = delegate
            {
                SetVelocity(0f, -20f);
                ResetGravity();
                atk.OnLanding = delegate
                {
                    Comp_CustomAnimator.Play(ASN_GroundPound_Land, atk);
                    Vector2 groundPositionViaRaycast = GetGroundPositionViaRaycast();
                    EffectSprite.Create(groundPositionViaRaycast + new Vector2(0f, 0.2f), EffectSprite.Sprites.DustPuff, FaceDir == 1);
                    EffectSprite.Create(groundPositionViaRaycast + new Vector2(0f, 0.2f), EffectSprite.Sprites.DustPuff, FaceDir != 1);
                    GetHitboxDamageProperties().Damage = 3f;
                    GetHitboxDamageProperties().GetHitStun = delegate
                    {
                        float num = 0.85f;
                        /*
                        if (Staleable_Stomp.IsStale)
                        {
                            num -= 0.3f + (float)Staleable_Stomp.StaleHitCount * 0.2f;
                        }
                        */

                        return num;
                    };
                    GetHitboxDamageProperties().BlockStun = 0.1f;
                    GetHitboxDamageProperties().GetLaunch = null;
                    GetHitboxDamageProperties().Launch = new Vector2(3f, 10f);
                    GetHitboxDamageProperties().BlockedLaunch = new Vector2(3f, 0f);
                    GetHitboxDamageProperties().IsLaunchPositionBased = true;
                    GetHitboxDamageProperties().Priority = BattleCache.PriorityType.Light;
                    GetHitboxDamageProperties().OnHitSoundEffect = cc.sounds["hit4"];
                    GetHitboxDamageProperties().OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                    {
                        bool IsNPC = SMBZGlobals.GetIsNPC(target);
                        if (target != null && !IsNPC && !wasBlocked)
                        {
                            //Staleable_Stomp.Set();
                        }
                    };
                    base.HitBox_0.ReinitializeID();
                };
            };
            return atk;
        }
    }

    AttackBundle AttBun_Groundbreaker => new AttackBundle
    {
        AnimationNameHash = ASN_Groundbreaker,
        OnAnimationStart = delegate
        {
            CustomEffectSprite.Create(GetGroundPositionViaRaycast(), cc.effects["groundbreaker_aoe"], AlwaysAppearAboveFighters: true);
            CustomEffectSprite spr = CustomEffectSprite.Create(transform.position, cc.effects["groundbreaker_circle"], AlwaysAppearAboveFighters: true);
            spr.AnchorPositionToObject(transform, Vector2.zero);

            SetPlayerState(PlayerStateENUM.Attacking);
            SetGravityOverride(3f);
            SetVelocity(0, 15f);
            IsIntangible = true;
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 15f,
                HitStun = 1f,
                IsUnblockable = true,
                Launch = new Vector2(20, 8f),
                IsLaunchPositionBased = true,
                FreezeTime = 0.08f,
                Priority = BattleCache.PriorityType.Critical,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 4)}"]
            });
        },
        OnLanding = delegate
        {
            PrepareAnAttack(AttBun_Groundbreaker_Land);
        }
    };

    AttackBundle AttBun_Groundbreaker_Land => new AttackBundle
    {
        AnimationNameHash = ASN_Groundbreaker_Land,
        OnAnimationStart = delegate
        {
            SMBZGlobals.CameraManager.SetShake(0.2f, 0.15f);
            if (SMBZGlobals.ActiveBattleBackgroundData.Prefab_Crater != null)
                GameObject.Instantiate(SMBZGlobals.ActiveBattleBackgroundData.Prefab_Crater, GetGroundPositionViaRaycast(), Quaternion.identity);

            ResetGravity();
            SetPlayerState(PlayerStateENUM.Attacking);

            CustomEffectSprite.Create(GetGroundPositionViaRaycast(), cc.effects["groundbreaker_aoe"], AlwaysAppearAboveFighters: true);

            CustomEffectEntry effect = cc.effects["lightning"];
            Vector2 pos = GetGroundPositionViaRaycast();
            pos.y += effect.texture.height * (effect.anim.scale.y / 40f);
            CustomEffectSprite.Create(pos, effect);
        },
        OnAnimationEnd = delegate
        {
            SetPlayerState(PlayerStateENUM.Idle);
            CurrentAttackData = null;
        },
        OnCustomQueue = delegate
        {
            IsIntangible = false;
        }
    };

    AttackBundle AttBun_DevastatingCombo
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_DevastatingCombo,
                OnAnimationStart = delegate
                {
                    HitBoxDamageParameters dmgProperties = new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 12f / 11f,
                        ChipDamage = 6f / 11f,
                        EnergyGain = 0f,
                        IsChipDamageFatal = true,
                        HitStun = 0.5f,
                        BlockStun = 0.25f,
                        Launch = new Vector2(0, 0),
                        FreezeTime = 0.01f,
                        Priority = BattleCache.PriorityType.Heavy,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                        OnHitSoundEffect = cc.sounds["hit8"],
                    };
                    dmgProperties.OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                    {
                        dmgProperties.OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(8, 10)}"];
                        SMBZGlobals.CameraManager.SetShake(0.04f, 0.08f);
                    };
                    SetPlayerState(PlayerStateENUM.Attacking);
                    SetHitboxDamageProperties(dmgProperties);
                },
                CinematicEffects = new List<CinematicEffect>
                {
                    new CinematicEffect
                    {
                        StartupDelay = 0.02f,
                        PauseAndDimDuringStartup = false,
                        Duration = 0.75f,
                        PauseAndDimDuringDuration = true,
                        EffectToCreate = new EffectSprite.Parameters
                        {
                            SpriteHash = EffectSprite.Sprites.CriticalPower
                        }
                    }
                },
                IsCinematicsQueued = true
            };
            atk.OnCustomQueue = delegate
            {
                SetVelocity(8f * FaceDir, 0);
                HitBox_0.ReinitializeID();
                if (atk.CustomQueueCallCount == 10)
                {
                    HitBoxDamageParameters dmgProperties = GetHitboxDamageProperties();
                    dmgProperties.Launch = new Vector2(15 * FaceDir, 6f);
                    dmgProperties.BlockedLaunch = new Vector2(10 * FaceDir, 0);
                    dmgProperties.OnHitSoundEffect = cc.sounds["hit10"];
                    dmgProperties.HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy);
                    dmgProperties.OnHitCallback = delegate (BaseCharacter target, bool wasBlocked)
                    {
                        SMBZGlobals.CameraManager.SetShake(0.05f, 0.1f);
                    };
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_BooLaunch => new AttackBundle
    {
        AnimationNameHash = ASN_ReleaseBoo,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            foreach (DevilBooControl boo in Boos)
            {
                if (boo.Spawned || boo.IsPossessing) continue;
                boo.Spawn(transform.position, FaceDir == 1);
                boo.QueueUp_Attack_Launch();
                break;
            }
        },
    };

    AttackBundle AttBun_BooPossession => new AttackBundle
    {
        AnimationNameHash = ASN_ReleaseBoo,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);

            foreach (DevilBooControl boo in Boos)
            {
                if (boo.Spawned) continue;

                boo.Spawn(transform.position, FaceDir == 1);

                BaseCharacter target = boo.FindClosestNPC();
                if (!target)
                {
                    boo.QueueUp_Attack_Launch();
                    break;
                }

                bool success = boo.Queue_Attack_Possess(target);
                if (!success)
                {
                    Melon<DevilMarioMod.Core>.Logger.Msg($"unsuccessful");
                }
                else
                {
                    InventoryData.LostBoos++;
                }

                break;
            }
        }
    };

    AttackBundle AttBun_BooPossession_Command => new AttackBundle
    {
        AnimationNameHash = ASN_CommandBoo,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Attacking);
            CustomEffectSprite fx = CustomEffectSprite.Create(transform.position, cc.effects["eyeGlint"], FaceDir == 1, true);
            fx.AnchorPositionToObject(transform, new Vector2(0, 0.32f));
        }
    };

    AttackBundle AttBun_CriticalStrike
    {
        get
        {
            AttackBundle atk = new AttackBundle
            {
                AnimationNameHash = ASN_CriticalStrike,
                OnAnimationStart = delegate
                {
                    SetPlayerState(PlayerStateENUM.Cinematic_NoInput);
                    SetHitboxDamageProperties(new HitBoxDamageParameters
                    {
                        Owner = this,
                        Tag = base.tag,
                        Damage = 10f,
                        Stun = 200f,
                        HitStun = 4f,
                        Intensity = 0f,
                        IsCriticalStrike = true,
                        IsUnblockable = true,
                        Launch = new Vector2(40 * FaceDir, 25f),
                        FreezeTime = 0.3f,
                        Priority = BattleCache.PriorityType.Critical,
                        HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                        OnHitSoundEffect = cc.sounds["hit5"]
                    });
                },
                CinematicEffects = new List<CinematicEffect>
                {
                    new CinematicEffect
                    {
                        StartupDelay = 0.02f,
                        PauseAndDimDuringStartup = false,
                        Duration = 0.75f,
                        PauseAndDimDuringDuration = true,
                        EffectToCreate = new EffectSprite.Parameters
                        {
                            SpriteHash = EffectSprite.Sprites.CriticalPower
                        }
                    }
                },
                OnClashCallback = delegate
                {
                    HitBoxDamageParameters DamageProperties = GetHitboxDamageProperties();
                    DamageProperties.IsNullified = true;
                    SetHitboxDamageProperties(DamageProperties);
                },
                IsCinematicsQueued = true
            };
            atk.OnHit = delegate (BaseCharacter target, bool wasBlocked)
            {
                bool t_IsNPC = SMBZGlobals.GetIsNPC(target);
                if (!(target == null) && !t_IsNPC && target.IsHurt)
                {
                    SetPlayerState(PlayerStateENUM.Cinematic_NoInput);
                    CancelAndRefundPursue();
                    IsIntangible = true;
                    if (SaveData.Data.MovementRush_IsEnabled_ViaCriticalStrikes)
                    {
                        CharacterControl targetControl = SMBZGlobals.GetCharacterControl(target);
                        SMBZGlobals.MovementRushManager.StartNewMovementRush(FaceDir == 1, new List<CharacterControl> { MyCharacterControl }, new List<CharacterControl> { targetControl });
                    }
                    else
                    {
                        CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(target);

                        if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
                        {
                            SMBZGlobals.CameraManager.SetTargetGroup(t_MyCharacterControl.transform);
                        }

                        target.SetVelocity(20 * FaceDir, 10f);

                        CancelAndRefundPursue();
                        RushProperties = new Base_RushProperties();
                        RushProperties.Target = target;
                        Begin_Rush();

                        SMBZGlobals.Intensity.Clear();
                    }
                }
            };
            return atk;
        }
    }

    AttackBundle AttBun_Rush1 => new AttackBundle
    {
        AnimationNameHash = ASN_Rush1,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Cinematic_NoInput);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 1f,
                Launch = new Vector2(0, 2f),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0.03f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit1"]
            });
        },
        OnAnimationEnd = delegate
        {

        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 2;
            RushProperties.CinematicWaitTimer = .3f;
        }
    };

    AttackBundle AttBun_Rush2a => new AttackBundle
    {
        AnimationNameHash = ASN_Rush2a,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 0.5f,
                HitStun = 1f,
                Launch = new Vector2(3*FaceDir, 0),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 2)}"]
            });
        },
        OnAnimationEnd = delegate
        {

        },
        OnHit = delegate
        {
            RushProperties.CinematicPart++;
            RushProperties.CinematicWaitTimer = .01f;
        }
    };

    AttackBundle AttBun_Rush2b => new AttackBundle
    {
        AnimationNameHash = ASN_Rush2b,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 0.5f,
                HitStun = 1f,
                Launch = new Vector2(3 * FaceDir, 0),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(0, 2)}"]
            });
        },
        OnAnimationEnd = delegate
        {

        },
        OnHit = delegate
        {
            RushProperties.CinematicPart++;
            RushProperties.CinematicWaitTimer = .01f;
        }
    };

    AttackBundle AttBun_Rush2c => new AttackBundle
    {
        AnimationNameHash = ASN_Rush2a,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 1f,
                Launch = new Vector2(20 * FaceDir, 15),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0.02f,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit_powerful3"]
            });
        },
        OnAnimationEnd = delegate
        {

        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 8;
            RushProperties.CinematicWaitTimer = 0.4f;
        }
    };

    AttackBundle AttBun_Rush3 => new AttackBundle
    {
        AnimationNameHash = ASN_Rush3_Flip,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 3f,
                HitStun = 1f,
                Launch = new Vector2(20 * FaceDir, -7),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds["hit_echo"]
            });
        },
        OnAnimationEnd = delegate
        {
            Comp_CustomAnimator.Play(ASN_Rush3, CurrentAttackData);
        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 9;
            RushProperties.CinematicWaitTimer = 0.3f;
            BattleController.instance.Cinematic_SlowMotion(0.75f, 0.02f);
            SetVelocity(GetVelocity().x, 15);
            SetGravityOverride(0);
        }
    };

    AttackBundle AttBun_Rush4a => new AttackBundle
    {
        AnimationNameHash = ASN_Rush4a,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 1f,
                Launch = new Vector2(32 * FaceDir, 3f),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit15"]
            });
        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 11;
            RushProperties.CinematicWaitTimer = 0.05f;
        }
    };

    AttackBundle AttBun_Rush4b => new AttackBundle
    {
        AnimationNameHash = ASN_Rush4b,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 1f,
                Launch = new Vector2(32 * FaceDir, 3f),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit1"]
            });
        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 12;
            RushProperties.CinematicWaitTimer = 0.05f;
        }
    };

    AttackBundle AttBun_Rush4c => new AttackBundle
    {
        AnimationNameHash = ASN_Rush4c,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1f,
                HitStun = 1f,
                Launch = new Vector2(0, 8f),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkBlunt),
                OnHitSoundEffect = cc.sounds["hit0"]
            });
        },
        OnAnimationEnd = delegate
        {
            PrepareAnAttack(AttBun_Rush4d);
        }
    };

    AttackBundle AttBun_Rush4d => new AttackBundle
    {
        AnimationNameHash = ASN_Rush4d_Spin,
        OnAnimationStart = delegate
        {
            SoundCache.ins.PlaySound(SoundCache.ins.MarioWorld_CapedSpin);

            SetVelocity(0, 6);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 1.5f,
                HitStun = 1f,
                Launch = new Vector2(32 * FaceDir, 7f),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds["hit4"]
            });
        },
        OnAnimationEnd = delegate
        {

        },
        OnLanding = delegate
        {
            Comp_CustomAnimator.Play(ASN_Rush4d, CurrentAttackData);
        },
        OnHit = delegate
        {
            RushProperties.CinematicPart = 13;
            RushProperties.CinematicWaitTimer = 0.25f;
        }
    };

    AttackBundle AttBun_Rush5 => new AttackBundle
    {
        AnimationNameHash = ASN_Rush5,
        OnAnimationStart = delegate
        {
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 2f,
                HitStun = 1f,
                Launch = new Vector2(20 * FaceDir, 20),
                IsUnblockable = true,
                IsDamageFatal = false,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Light,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds["hit_powerful3"]
            });
        },
        OnHit = delegate (BaseCharacter target, bool wasBlocked)
        {
            RushProperties.CinematicPart = 14;
            RushProperties.CinematicWaitTimer = 0.75f;
            target.SetGravityOverride(0);
        }
    };

    AttackBundle AttBun_RushFinal => new AttackBundle
    {
        AnimationNameHash = ASN_Rush6a,
        OnAnimationStart = delegate
        {
            SetPlayerState(PlayerStateENUM.Cinematic_NoInput);
            SetHitboxDamageProperties(new HitBoxDamageParameters
            {
                Owner = this,
                Tag = base.tag,
                Damage = 5f + (5f * (RushProperties.PowerLevel / (float)RushProperties.PowerLevelMAX)),
                HitStun = 1f,
                Launch = new Vector2(15 * FaceDir, -15),
                IsUnblockable = true,
                FreezeTime = 0,
                Priority = BattleCache.PriorityType.Critical,
                HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                OnHitSoundEffect = cc.sounds["hit_maximum"]
            });
        },
        OnHit = delegate (BaseCharacter target, bool wasBlocked)
        {
            RushProperties.CinematicPart = 15;
            RushProperties.CinematicWaitTimer = 0.5f;
            Comp_CustomAnimator.Play(ASN_Rush6b, CurrentAttackData);
            SetVelocity(0, 0);

            BattleController.instance.WhiteFlash.Flash();
            BattleController.instance.Cinematic_SlowMotion(0.1f, 1f);
            SMBZGlobals.CameraManager.SetShake(3f, 0.2f);

            if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
            {
                SMBZGlobals.CameraManager.SetTargetGroup(MyCharacterControl.transform);
            }

            AudioSource src = SoundCache.ins.PlaySound(cc.sounds["swoophit"]);

            target.SetGravityOverride(0);
            target.AddOnContactGroundWhileHurtCallback(delegate
            {
                target.SetVelocity(0, 0);
                target.ResetGravity();
                SMBZGlobals.CameraManager.SetShake(0, 0);
                SMBZGlobals.CameraManager.SetShake(0.75f, 0.2f);
                src.Stop();
                SoundCache.ins.PlaySound(cc.sounds["explosion"]);

                Vector2 groundPositionViaRaycast2 = target.GetGroundPositionViaRaycast();
                EffectSprite effectSprite = EffectSprite.Create(groundPositionViaRaycast2, EffectSprite.Sprites.DustExplosion, isFacingRight: true, 1f, 0f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true);
                effectSprite.transform.localScale = effectSprite.transform.localScale * 2f;
                DustPoofEffect.Create(effectSprite.transform.position + Vector3.left, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: true, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 3f, 0.25f);
                DustPoofEffect.Create(effectSprite.transform.position, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: false, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 5f, 0.25f);
                DustPoofEffect.Create(effectSprite.transform.position + Vector3.right, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: true, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 3f, 0.25f);

                if (SMBZGlobals.ActiveBattleBackgroundData.Prefab_Crater != null)
                    GameObject.Instantiate(SMBZGlobals.ActiveBattleBackgroundData.Prefab_Crater, groundPositionViaRaycast2, Quaternion.identity);
            });
        }
    };


    protected override void Awake()
    {
        base.Awake();

        AerialAcceleration = 15f;
        GroundedAcceleration = 30f;
        GroundedDrag = 3f;
        HopPower = 10.5f;
        JumpChargeMax = 0f;
        Pursue_Speed = 40f;
        Pursue_StartupDelay = 0.1f;
        AlternateCombo = false;
        MovingCombo = false;
        HeavyPunch_Charge = -1;
        GroundGrab = -1;
        PossessionCooldown = 0;
        GrappleData = new GrappleClass();

        EnergyMax = 200f;
        EnergyStart = 100f;
    }

    protected override void Start()
    {
        base.Start();

        SoundEffect_MR_Dodge = SoundCache.ins.MarioWorld_CapedSpin;
        SoundEffect_MR_Whiff = cc.sounds["swing5"];
        SoundEffect_MR_Strike = cc.sounds["swing5"];

        //StartCoroutine(DelayedInit());

        Boos = new DevilBooControl[] {
            Instantiate(cc.companions["Boo"].prefab, transform.position, Quaternion.identity).GetComponent<DevilBooControl>(),
            Instantiate(cc.companions["Boo"].prefab, transform.position, Quaternion.identity).GetComponent<DevilBooControl>()
        };

        foreach (DevilBooControl boo in Boos)
        {
            boo.gameObject.SetActive(true);
            boo.Setup(cc, this);
        }
    }

    /*
    public IEnumerator DelayedInit()
    {
        yield return new WaitUntil(() => BattleHUDManager.ins != null);
        Boos = new DevilBooControl[] {
            Instantiate(cc.companions["Boo"].prefab, transform.position, Quaternion.identity).GetComponent<DevilBooControl>(),
            Instantiate(cc.companions["Boo"].prefab, transform.position, Quaternion.identity).GetComponent<DevilBooControl>()
        };

        foreach (DevilBooControl boo in Boos)
        {
            boo.gameObject.SetActive(true);
            boo.Setup(cc, this);
            boo.tag = base.tag;
            boo.IsFacingRight = IsFacingRight;
        }
    }
    */

    protected override void Update()
    {
        base.Update();

        if (InventoryData != null)
            InventoryData.Update();

        if (PossessionCooldown > 0)
            PossessionCooldown -= BattleController.instance.ActorDeltaTime;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (GrappleData != null && GrappleData.IsActive && GrappleData.GrappledTarget != null)
        {
            Rigidbody2D target_Comp_Rigidbody2D = SMBZGlobals.GetRigidbody2D(GrappleData.GrappledTarget);
            GrappleData.GrappledTarget.SetVelocity(GetVelocity());
            GrappleData.GrappledTarget.SetGravityOverride(0f);
            target_Comp_Rigidbody2D.MovePosition(Vector2.MoveTowards(target_Comp_Rigidbody2D.position, GrappleData.HoldPosition.position, 30f * BattleController.instance.ActorDeltaTime));
        }
    }

    protected override void Handle_OnComboEnd()
    {
        base.Handle_OnComboEnd();
        Staleable_Uppercut.Clear();
        Staleable_LegSweep.Clear();
        Staleable_SmackDown.Clear();
        Staleable_SuperUppercut.Clear();
        Staleable_AirGrab.Clear();
        if (InventoryData != null)
            InventoryData.ReloadSpeedMultiplier = 1f;
    }

    public override void PerformAction_Strike()
    {
        base.PerformAction_Strike();

        HitBoxDamageParameters damageParameters = GetHitboxDamageProperties();
        damageParameters.OnHitSoundEffect = cc.sounds[$"hit{UnityEngine.Random.Range(6, 8)}"];
        SetHitboxDamageProperties(damageParameters);
    }

    public override void PerformAction_Finale(CharacterControl target)
    {
        base.PerformAction_Finale(target);

        HitBoxDamageParameters damageParameters = GetHitboxDamageProperties();
        damageParameters.OnHitSoundEffect = cc.sounds["hit_maximum"];
        SetHitboxDamageProperties(damageParameters);

        target.CharacterGO.transform.position = base.transform.position + new Vector3(FaceDir*0.8f, -0.5f);
    }

    protected override void OnMovementRush_Clash()
    {
        SoundCache.ins.PlaySound(cc.sounds["explosion"]);
    }

    public override void OnClash(BaseCharacter opponent)
    {
        base.OnClash(opponent);

        if (IgnoreClashes) return;

        SoundCache.ins.PlaySound(cc.sounds["explosion"]);
    }

    protected override void Update_General()
    {
        base.Update_General();
        if (!IsPursuing)
            Update_ReadAttackInput();
    }

    protected override void Update_Pursue()
    {
        if (IsFrozen || PursueData == null)
        {
            return;
        }

        FieldInfo p_isCharging = typeof(PursueBundle).GetField("isCharging", BindingFlags.NonPublic | BindingFlags.Instance);

        if (PursueData.Target == null)
        {
            PursueData.Target = FindClosestTarget();
        }

        PursueData.StartupCountdown = Mathf.Clamp(PursueData.StartupCountdown - Time.deltaTime, 0f, float.MaxValue);
        PursueData.PursueCountdown = Mathf.Clamp(PursueData.PursueCountdown - Time.deltaTime, 0f, float.MaxValue);
        if (PursueData.IsPreping)
        {
            if (CurrentAttackData == null && Comp_CustomAnimator.m_CurrentAnimation.hash != ASN_PrePursue)
            {
                Comp_CustomAnimator.Play(IsOnGround ? ASN_PrePursue : ASN_PrePursueSpin);
            }

            if (HasContactGroundThisFrame && PursueData.StartupCountdown <= 0f)
            {
                PursueData.StartupCountdown = 0.15f;
            }

            if (PursueData.StartupCountdown <= 0f && (!(bool)p_isCharging.GetValue(PursueData) || ((bool)p_isCharging.GetValue(PursueData) && PursueData.ChargePower >= 100f)) && IsOnGround)
            {
                PursueData.PursueCountdown = 10f;
                PursueData.IsPursuing = true;
                PursueData.IsPreping = false;
                p_isCharging.SetValue(PursueData, false);
                SetPlayerState(PlayerStateENUM.Pursuing);
                ComboSwingCounter = 0;
                float num = Helpers.Vector2ToDegreeAngle_180(transform.position, PursueData.Target.transform.position);
                IsFacingRight = (-90f <= num && num <= 90f);
                PursueData.Direction = (FaceDir == 1 ? Vector2.right : Vector2.left);
                PursueData.Speed = 40f + (PursueData.ChargePower / 100f * 35f);

                if (PursueData.ChargePower >= 100f)
                {
                    StartCoroutine(SuperPursueCinematic());
                    return;
                }

                SoundCache.ins.PlaySound(cc.sounds["pursue"]);
                EffectSprite.Create(groundCheck.position, EffectSprite.Sprites.DustPuff, FaceDir == 1);

                SetupPursueHitbox();
            }
        }
        else if (PursueData.Target == null)
        {
            StartCoroutine(OnPursueMiss());
        }
        else if (PursueData.IsPursuing)
        {
            if (PursueData.IsHoming)
            {
                Vector3 vector = PursueData.Target.transform.position - transform.position;
                PursueData.Direction = vector / vector.magnitude;
            }

            bool flag = (FaceDir == 1 ? (PursueData.Target.transform.position.x + 3f < transform.position.x) : (transform.position.x < PursueData.Target.transform.position.x - 3f));
            if (HasContactGroundThisFrame && Comp_Rigidbody2D.velocity.y < -1f && Mathf.Abs(Comp_Rigidbody2D.velocity.x) < 3f)
            {
                PursueData.Direction = new Vector2((float)((PursueData.Direction.x > 0f) ? 1 : (-1)) * (Mathf.Abs(PursueData.Direction.x) + Mathf.Abs(PursueData.Direction.y)), 0f);
            }

            if (PursueData.PursueCountdown == 0f || flag)
            {
                StartCoroutine(OnPursueMiss());
            }
            else
            {
                SetVelocity(PursueData.Direction * PursueData.Speed);
            }
        }
    }

    public IEnumerator SuperPursueCinematic()
    {
        IsIntangible = true;
        Comp_InterplayerCollider.Disable();

        if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
        {
            SMBZGlobals.CameraManager.SetTargetGroup(MyCharacterControl.transform);
            SMBZGlobals.CameraSettings.FocusZoom = 3f;
            SMBZGlobals.CameraSettings.MoveSpeed = 40f;
        }

        BattleController.instance.Cinematic_PauseAndDim(0.9f);
        EffectSprite.Create(base.transform.position, EffectSprite.Sprites.CriticalPower, FaceDir == 1);
        yield return new WaitForSeconds(0.9f);

        IsIntangible = false;
        Comp_InterplayerCollider.Enable();
        SMBZGlobals.CameraManager.ResetSettings();
        SoundCache.ins.PlaySound(cc.sounds["pursue"]);
        Comp_CustomAnimator.Play(ASN_Pursue);
        EffectSprite.Create(groundCheck.position, EffectSprite.Sprites.DustPuff, FaceDir == 1);

        SetupPursueHitbox();
    }

    protected override IEnumerator OnPursueContact()
    {
        yield return new WaitForEndOfFrame();
        if (PursueData == null)
        {
            yield break;
        }

        if (PursueData.Target != null)
        {
            transform.position = new Vector3(PursueData.Target.transform.position.x + (float)FaceDir * -1.25f, transform.position.y, transform.position.z);
        }

        PursueData = null;
        if (IsCPUControlled)
        {
            AI.PursueIdea = null;
        }
    }

    void SetupPursueHitbox()
    {
        bool isFullPower = PursueData.ChargePower >= 100f;
        float a = 0.75f;
        float b = 1.5f;
        float a2 = 3f;
        float b2 = 8f;
        float x1 = 6f;
        float x2 = 13f;
        SetHitboxDamageProperties(new HitBoxDamageParameters
        {
            Owner = this,
            Tag = base.tag,
            Damage = Mathf.Lerp(a2, b2, PursueData.ChargePower / 100f),
            HitStun = Mathf.Lerp(a, b, PursueData.ChargePower / 100f),
            Launch = new Vector2(Mathf.Lerp(x1, x2, PursueData.ChargePower / 100f) * FaceDir, 6f),
            FreezeTime = (isFullPower ? 0f : 0.07f),
            Priority = ((!isFullPower) ? BattleCache.PriorityType.Medium : BattleCache.PriorityType.Critical),
            IsUnblockable = isFullPower,
            HitSpark = new EffectSprite.Parameters
            {
                SpriteHash = (isFullPower ? EffectSprite.Sprites.HitsparkHeavy : EffectSprite.Sprites.HitsparkBlunt)
            },
            OnHitSoundEffect = (isFullPower ? cc.sounds["pursue_max"] : cc.sounds["hit10"])
        });
        PrepareAnAttack(new AttackBundle
        {
            AnimationNameHash = ASN_Pursue,
            OnClashCallback = delegate
            {
                float value = MaxMoveSpeed.GetValue();
                SetVelocity(PursueData.Direction.x * value, Mathf.Clamp(Comp_Rigidbody2D.velocity.y, 0f - value, value));
                Comp_CustomAnimator.Play(isFullPower ? ASN_PursueHitMax : ASN_PursueHit, CurrentAttackData);
                StartCoroutine(OnPursueContact());
            },
            OnHit = delegate (BaseCharacter target, bool wasBlocked)
            {
                bool t_IsNPC = SMBZGlobals.GetIsNPC(target);
                if (!t_IsNPC)
                {
                    SetPlayerState(PlayerStateENUM.Attacking);
                    Comp_CustomAnimator.Play(isFullPower ? ASN_PursueHitMax : ASN_PursueHit, CurrentAttackData);
                    StartCoroutine(OnPursueContact());
                }

                float t_BlockStun = SMBZGlobals.GetBlockStun(target);
                Rigidbody2D t_Comp_Rigidbody2D = SMBZGlobals.GetRigidbody2D(target);

                if (target != null && (target.IsHurt || !(t_BlockStun <= 0f)))
                {
                    Comp_Rigidbody2D.velocity = new Vector2(t_Comp_Rigidbody2D.velocity.x * 1.25f, Comp_Rigidbody2D.velocity.y);
                    if (isFullPower)
                    {
                        SMBZGlobals.CameraManager.SetShake(0.35f, 0.1f);
                        BattleController.instance.Cinematic_SlowMotion(0.35f);
                    }
                }
            }
        });
        HitBox_0.IsActive = true;
        HitBox_0.transform.localPosition = Vector2.zero;
        HitBox_0.transform.localScale = new Vector2(3, 1);
    }

    public override IEnumerator OnBurst_Victory(CharacterControl target, BurstDataStore.VictoryStrikeENUM victoryStrikeType = BurstDataStore.VictoryStrikeENUM.General)
    {
        base.OnBurst_Victory(target);

        SetVelocity(0f, 0f);
        SetGravityOverride(0f);
        IsFacingRight = !IsFacingRight;
        Comp_InterplayerCollider.Disable();
        target.CharacterGO.SetGravityOverride(0f);
        target.InputLockTimer = 1f;
        bool isMrStarter = victoryStrikeType == BurstDataStore.VictoryStrikeENUM.MovementRushStarter;
        bool targetWasHit = false;
        transform.position = target.CharacterGO.transform.position;

        PrepareAnAttack(new AttackBundle
        {
            AnimationName = (isMrStarter ? "BurstVictoryStrike_MR" : "BurstVictoryStrike"),
            OnAnimationStart = delegate
            {
                SetHitboxDamageProperties(new HitBoxDamageParameters
                {
                    Owner = this,
                    Tag = base.tag,
                    Damage = 8f,
                    HitStun = (isMrStarter ? 4f : 1.5f),
                    IsUnblockable = true,
                    Launch = (isMrStarter ? new Vector2(20 * FaceDir, 5f) : new Vector2(5 * FaceDir, -20f)),
                    FreezeTime = 0.15f,
                    Priority = BattleCache.PriorityType.Critical,
                    HitSpark = new EffectSprite.Parameters(EffectSprite.Sprites.HitsparkHeavy),
                    OnHitSoundEffect = cc.sounds["hit5"],
                    OnHitCallback = delegate (BaseCharacter t, bool b)
                    {
                        CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(t);

                        if (t != null && target == t_MyCharacterControl)
                        {
                            targetWasHit = true;
                        }

                        if (isMrStarter)
                        {
                            bool IsNPC = SMBZGlobals.GetIsNPC(t);
                            if (SaveData.Data.MovementRush_IsEnabled_ViaCriticalStrikes && t != null && !IsNPC && t.IsHurt)
                            {
                                SMBZGlobals.MovementRushManager.StartNewMovementRush(FaceDir == 1, new List<CharacterControl> { MyCharacterControl }, new List<CharacterControl> { t_MyCharacterControl });
                            }
                        }
                        else
                        {
                            target.CharacterGO.ResetGravity();
                        }
                    }
                });
            },
            OnInterrupt = delegate
            {
                if (!targetWasHit)
                {
                    target.CharacterGO.ResetGravity();
                }

                ResetGravity();
                Comp_InterplayerCollider.Enable();
            },
            OnAnimationEnd = delegate
            {
                if (!targetWasHit)
                {
                    target.CharacterGO.ResetGravity();
                }

                ResetGravity();
                SetPlayerState(PlayerStateENUM.Idle);
                Comp_InterplayerCollider.Enable();
            }
        });
        yield return null;
    }

    void Perform_Taunt_EyeGlint()
    {
        PrepareAnAttack(new AttackBundle
        {
            AnimationName = "Taunt_EyeGlint",
            OnAnimationStart = delegate
            {
                SetPlayerState(PlayerStateENUM.Attacking);
            },
            OnCustomQueue = delegate
            {
                CustomEffectSprite fx = CustomEffectSprite.Create(transform.position, cc.effects["eyeGlint"], FaceDir == 1, true);
                fx.AnchorPositionToObject(transform, new Vector2(0.04f, 0.3f));
            }
        });
    }

    protected override void Perform_Grounded_NeutralAttack()
    {
        switch(ComboSwingCounter)
        {
            case 0:
                MovingCombo = MyCharacterControl.IsInputtingLeft || MyCharacterControl.IsInputtingRight;
                PrepareAnAttack(!MovingCombo ? AttBun_AutoCombo1_1 : AttBun_AutoCombo2_1);
                ComboSwingCounter++;
                break;

            case 1:
                if (!MovingCombo)
                    PrepareAnAttack(!AlternateCombo ? AttBun_AutoCombo1_2a : AttBun_AutoCombo1_2b);
                else
                    PrepareAnAttack(!AlternateCombo ? AttBun_AutoCombo2_2a : AttBun_AutoCombo2_2b);
                ComboSwingCounter++;
                break;

            case 2:
                if (!MovingCombo)
                    PrepareAnAttack(!AlternateCombo ? AttBun_AutoCombo1_3a : AttBun_AutoCombo1_3b);
                else
                    PrepareAnAttack(!AlternateCombo ? AttBun_AutoCombo2_3a : AttBun_AutoCombo2_3b);
                ComboSwingCounter = (!AlternateCombo || MovingCombo) ? 0 : ComboSwingCounter + 1;
                break;

            case 3:
                PrepareAnAttack(AttBun_AutoCombo1_4);
                ComboSwingCounter = 0;
                break;
        }
    }

    protected override void Perform_Grounded_UpAttack()
    {
        PrepareAnAttack(AttBun_Uppercut);
    }

    protected override void Perform_Grounded_DownAttack()
    {
        PrepareAnAttack(AttBun_LegSweep);
    }

    protected override void Perform_Grounded_NeutralSpecial()
    {
        PrepareAnAttack(AttBun_TryGrab);
    }

    protected override void Perform_Grounded_UpSpecial()
    {
        PrepareAnAttack(AttBun_SuperUppercut);
    }

    protected override void Perform_Grounded_DownSpecial()
    {
        if (SMBZGlobals.Intensity.IsCriticalHitReady(MyCharacterControl.ParticipantDataReference.ParticipantIndex))
        {
            SMBZGlobals.Intensity.UseCriticalStrike(MyCharacterControl.ParticipantDataReference.ParticipantIndex);
            PrepareAnAttack(AttBun_CriticalStrike);
        }
        else
        {
            PrepareAnAttack(AttBun_HeavyPunch_Charge);
        }
    }

    protected override void Perform_Aerial_NeutralAttack()
    {
        PrepareAnAttack(AttBun_Smackdown);
    }

    protected override void Perform_Aerial_UpAttack()
    {
        PrepareAnAttack(AttBun_AxeKick);
    }

    protected override void Perform_Aerial_DownAttack()
    {
        PrepareAnAttack(AttBun_Flurrykicks);
    }

    protected override void Perform_Aerial_NeutralSpecial()
    {
        PrepareAnAttack(AttBun_AirGrab);
    }

    protected override void Perform_Aerial_UpSpecial()
    {
        PrepareAnAttack(AttBun_SuperUppercut);
    }

    protected override void Perform_Aerial_DownSpecial()
    {
        PrepareAnAttack(AttBun_Groundpound);
    }

    protected override void Perform_Grounded_NeutralSuper()
    {
        if (MyCharacterControl.ParticipantDataReference.Energy.GetCurrent() < 100f)
            return;

        IsComboLinkAvailable = false;
        if (!IsCPUControlled)
        {
            MyCharacterControl.Button_A.IsBuffered = false;
            MyCharacterControl.Button_Z_Attack.IsBuffered = false;
        }

        DeactivateAllHitboxes();
        MyCharacterControl.ParticipantDataReference.UseEnergy(100f);
        PrepareAnAttack(AttBun_DevastatingCombo);
    }

    void Perform_BooLaunch()
    {
        if (InventoryData != null && InventoryData.Boos > 0)
        {
            InventoryData.Boos--;
            PrepareAnAttack(AttBun_BooLaunch);
        }
    }

    void Perform_BooPossession()
    {
        if (PossessionCooldown > 0) return;

        bool atLeastOnePossessing = false;
        foreach (DevilBooControl boo in Boos)
        {
            if (!boo.IsPossessing)
                continue;

            boo.CommandNPCToAttack();
            PrepareAnAttack(AttBun_BooPossession_Command);
            atLeastOnePossessing = true;
            PossessionCooldown = 5;
        }
        if (atLeastOnePossessing) return;

        if (InventoryData != null && InventoryData.Boos > 0)
        {
            InventoryData.Boos--;
            PrepareAnAttack(AttBun_BooPossession);
        }
    }

    protected override void Perform_Grounded_UpSuper()
    {
        Perform_BooLaunch();
    }

    protected override void Perform_Grounded_DownSuper()
    {
        Perform_BooPossession();
    }

    protected override void Perform_Aerial_NeutralSuper()
    {
        if (MyCharacterControl.ParticipantDataReference.Energy.GetCurrent() < 200f)
            return;

        IsComboLinkAvailable = false;
        if (!IsCPUControlled)
        {
            MyCharacterControl.Button_A.IsBuffered = false;
            MyCharacterControl.Button_Z_Attack.IsBuffered = false;
        }

        DeactivateAllHitboxes();
        MyCharacterControl.ParticipantDataReference.UseEnergy(200f);
        PrepareAnAttack(AttBun_Groundbreaker);
    }

    protected override void Perform_Aerial_UpSuper()
    {
        Perform_BooLaunch();
    }

    protected override void Perform_Aerial_DownSuper()
    {
        Perform_BooPossession();
    }

    protected override void Perform_Grounded_NeutralTaunt()
    {
        Perform_Taunt_EyeGlint();
    }

    protected override void Perform_Grounded_UpTaunt()
    {
        Perform_Taunt_EyeGlint();
    }

    protected override void Perform_Grounded_DownTaunt()
    {
        Perform_Taunt_EyeGlint();
    }

    protected void Begin_Rush()
    {
        BattleController.Invoke_OnRushStart_Event(MyCharacterControl);
        IsIntangible = true;
        IsRushing = true;
        SetPlayerState(PlayerStateENUM.Cinematic_NoInput);
        Comp_InterplayerCollider.Disable();

        if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
        {
            CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(RushProperties.Target);
            SMBZGlobals.CameraManager.SetTargetGroup(t_MyCharacterControl.transform);
        }

        StartCoroutine(DelayedRhythmCommand());

        SMBZGlobals.SetHitStun(RushProperties.Target, 20f);
        SMBZGlobals.SetPreventDrag(RushProperties.Target, true);
        RushProperties.CinematicWaitTimer = 0.75f;
        RushProperties.CinematicPart = 0;
    }

    public IEnumerator DelayedRhythmCommand()
    {
        yield return new WaitForSeconds(0.4f);

        MyCharacterControl.GetRhythmCommand().Activate(new List<RhythmCommand.Class_Button>
        {
            // Power Star 3

            // initial teleport
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Guard, 0.35f),

            // punch that halts target's momentum
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.4f),

            // flurry of punches
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.2f),
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.2f),
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.85f),

            // kick down to the ground, target goes sliding
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.8f),


            // Power Star 4

            // teleports again, back and forth attacks
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.4f),
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.4f),
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.25f),

            // spin jump and strong kick away
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Jump, 0.4f),
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack, 0.65f),

            // strong uppercut, send flying
            new RhythmCommand.Class_Button(RhythmCommand.Enum_ButtonType.Attack),

            // final smack down
        });
        RushProperties.PowerLevelMAX = MyCharacterControl.GetRhythmCommand().CommandList.Count;
    }

    protected override bool Update_Rush()
    {
        if (!base.Update_Rush())
            return false;

        if (RushProperties.Target == null)
        {
            End_Rush();
            return false;
        }

        SMBZGlobals.SetHitStun(RushProperties.Target, 20f);
        SMBZGlobals.SetPreventDrag(RushProperties.Target, true);

        Update_RushRhythmCommandReading(RushProperties);
        if (RushProperties.CinematicWaitTimer > 0f)
        {
            RushProperties.CinematicWaitTimer -= Time.deltaTime;
            return true;
        }

        CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(RushProperties.Target);

        switch (RushProperties.CinematicPart)
        {
            case 0:
                RushProperties.CinematicPart = 1;
                transform.position = RushProperties.Target.transform.position + new Vector3(8 * FaceDir, 0);
                IsFacingRight = !IsFacingRight;
                SetVelocity(Vector2.zero);
                PrepareAnAttack(AttBun_Rush1);
                break;

            case 2:
            case 4:
            case 6:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                SetVelocity(3.25f * FaceDir, 0);
                PrepareAnAttack(AttBun_Rush2a);
                break;

            case 3:
            case 5:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                SetVelocity(3.25f * FaceDir, 0);
                PrepareAnAttack(AttBun_Rush2b);
                break;

            case 7:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                SetVelocity(4 * FaceDir, 0);
                PrepareAnAttack(AttBun_Rush2c);
                break;

            case 8:
                RushProperties.CinematicWaitTimer = 5f;
                SetVelocity(RushProperties.Target.GetVelocity().x + (25 * FaceDir), 25);
                SetGravityOverride(2.5f);
                PrepareAnAttack(AttBun_Rush3);
                break;

            case 9:
                RushProperties.CinematicPart++;
                RushProperties.CinematicWaitTimer = 0.1f;
                SetVelocity(0, 0);
                break;

            case 10:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                transform.position = RushProperties.Target.transform.position + new Vector3(8 * FaceDir, 0);
                IsFacingRight = !IsFacingRight;
                ResetGravity();
                PrepareAnAttack(AttBun_Rush4a);
                break;

            case 11:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                transform.position = RushProperties.Target.transform.position + new Vector3(10 * FaceDir, 0);
                IsFacingRight = !IsFacingRight;
                PrepareAnAttack(AttBun_Rush4b);
                break;

            case 12:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                transform.position = RushProperties.Target.transform.position + new Vector3(10 * FaceDir, 0);
                IsFacingRight = !IsFacingRight;
                PrepareAnAttack(AttBun_Rush4c);
                break;

            case 13:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                transform.position = RushProperties.Target.transform.position + new Vector3(13 * FaceDir, 0);
                IsFacingRight = !IsFacingRight;
                PrepareAnAttack(AttBun_Rush5);
                break;

            case 14:
                RushIdleTimer = 0;
                RushProperties.CinematicWaitTimer = 5f;
                transform.position = RushProperties.Target.transform.position + new Vector3(6 * FaceDir, 6);
                IsFacingRight = !IsFacingRight;

                if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
                {
                    SMBZGlobals.CameraManager.SetTargetGroup_Default();
                    SMBZGlobals.CameraManager.SetFocusZoom(3.5f);
                }

                BattleController.instance.Cinematic_SlowMotion(3f, 0.1f);
                BattleController.instance.WhiteFlash.Flash();
                PrepareAnAttack(AttBun_RushFinal);
                SetGravityOverride(0);
                SetVelocity(10 * FaceDir, -10);
                break;

            case 15:
                if (SMBZGlobals.IsThereTwoOrLessPlayersAreAlive())
                {
                    SMBZGlobals.CameraManager.SetTargetGroup(t_MyCharacterControl.transform);
                    SMBZGlobals.CameraManager.SetShake(0, 0);
                    SMBZGlobals.CameraManager.SetShake(1f, 0.1f);
                }

                RushProperties.CinematicPart++;
                RushProperties.CinematicWaitTimer = 2.3f;
                break;

            case 16:
                ResetGravity();
                SetVelocity(3 * FaceDir, 0);
                transform.position = RushProperties.Target.transform.position + new Vector3(8 * -FaceDir, 3);
                End_Rush();
                break;
        }

        return true;
    }

    public override void End_Rush()
    {
        base.End_Rush();
        MyCharacterControl.DestroyRhythmCommand();
        if (RushProperties?.Target != null)
        {
            CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(RushProperties.Target);
            RushProperties.Target.ResetGravity();
            t_MyCharacterControl.ParticipantDataReference.SetStun(0f);
            SMBZGlobals.SetHitStun(RushProperties.Target, 0.75f);
            SMBZGlobals.SetPreventDrag(RushProperties.Target, false);
        }

        SMBZGlobals.CameraManager.ResetSettings();

        RushProperties = null;
        SetPlayerState(PlayerStateENUM.Idle);
        InterruptAndNullifyPreparedAttack();
        ResetGravity();
        IsRushing = false;
        IsIntangible = false;
        Comp_InterplayerCollider.Enable();
        MyCharacterControl.InputLockTimer = 0.5f;
    }

    protected override void Update_CPU_Thoughts()
    {
        // references: MarioControl, YoshiControl, BasilisxControl

        base.Update_CPU_Thoughts();

        if (!AI.DeltaData.ShouldContinueWithProcessing)
            return;

        if (AI.RethinkCooldown_Movement > 0f)
        {
            AI.RethinkCooldown_Movement -= BattleController.instance.UnscaledDeltaTime;
        }
        else
        {
            if (AI.GetMood() == AI_Bundle.Enum_Mood.Aggressive)
            {
                if (AI.DeltaData.distanceToTarget > 6f && UnityEngine.Random.Range(0, 15) == 1 && MyCharacterControl.ParticipantDataReference.Energy.GetCurrent() > 100f)
                {
                    AI.PursueIdea = new AI_Bundle.Internal_PursueIdea(UnityEngine.Random.Range(25, 101));
                }
                else if (AI.DeltaData.distanceToTarget > 1f)
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1));
                }
                else
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1), 1f, JustTurn: true);
                }
            }
            else if (AI.GetMood() == AI_Bundle.Enum_Mood.Defensive)
            {
                if (AI.DeltaData.distanceToTarget > 6f)
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1));
                }
                else
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1), 1f, JustTurn: true);
                }
            }
            else if (AI.GetMood() == AI_Bundle.Enum_Mood.Tactical)
            {
                if (AI.DeltaData.distanceToTarget > 1f)
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1));
                }
                else
                {
                    AI.MovementIdea.Reset(AI.DeltaData.IsTargetToMyRight ? 1 : (-1), 1f, JustTurn: true);
                }

                if (AI.DeltaData.vectorToTarget_Absolute.x > 6f)
                {
                    AI.JumpIdea = new AI_Bundle.Internal_JumpIdea(IsFullJump: true, AI.DeltaData.IsTargetToMyRight ? 1 : (-1));
                }
            }

            AI.RethinkCooldown_Movement = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Movement);
        }

        if (AI.RethinkCooldown_Guarding > 0f)
            AI.RethinkCooldown_Guarding -= BattleController.instance.UnscaledDeltaTime;
        else
            AI.FireMarioFireballHandling(this, AI.DeltaData.Target);

        if (AI.CommandList.ActionQueue.Count > 0)
            AI_CommandList_Update();
        else
        {
            if (IsAttacking)
                return;

            if (AI.RethinkCooldown_Attacking > 0f)
            {
                AI.RethinkCooldown_Attacking -= BattleController.instance.UnscaledDeltaTime;
                return;
            }

            if (IsOnGround && AI.DeltaData.IsCriticalHitReady && UnityEngine.Random.Range(0, 3) == 1 &&
                0f <= AI.DeltaData.vectorToTarget_Absolute.x && AI.DeltaData.vectorToTarget_Absolute.x <= 1.5f &&
                0f <= AI.DeltaData.vectorToTarget_Absolute.y && AI.DeltaData.vectorToTarget_Absolute.y <= 1f)
            {
                AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
                AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Down, ZTrigger: true);
            }

            if (IsOnGround && MyCharacterControl.ParticipantDataReference.Energy.GetCurrent() > 100f && UnityEngine.Random.Range(0, 8) == 1 && AI.DeltaData.vectorToTarget_Absolute.x < 2f && AI.DeltaData.IsTargetWithinAltitude)
            {
                if (AI.DeltaData.IsTargetToMyRight != IsFacingRight)
                {
                    AI.MovementIdea.Reset((AI.DeltaData.directionToTarget.x > 0f) ? 1 : (-1), 5f, JustTurn: true);
                }

                AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: true, SuperInput: true);
                AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
            }

            if (AI.RethinkCooldown_Attacking > 0)
                return;

            if (AI.DeltaData.IsTargetWithinAltitude)
            {
                if (AI.DeltaData.vectorToTarget_Absolute.x <= 1.5f)
                {
                    if (IsOnGround)
                    {
                        switch (UnityEngine.Random.Range(1, 5))
                        {
                            case 1:
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)),
                                }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                break;

                            case 2:
                                AI.MovementIdea.Reset(0);
                                AI.RethinkCooldown_Movement = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Movement);
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false), 0.3f),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false), 0.3f),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: false))
                                }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                if (AI.Difficulty >= AI_Bundle.Enum_DifficultyLevel.Normal)
                                {
                                    AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_JumpIdea(true, FaceDir)));
                                    AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false), 0.05f));
                                    if (AI.Difficulty == AI_Bundle.Enum_DifficultyLevel.Hard)
                                        AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)));
                                }
                                break;

                            case 3:
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: false)),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_JumpIdea(true, FaceDir)),
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false), 0.05f)
                                }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                if (AI.Difficulty == AI_Bundle.Enum_DifficultyLevel.Hard)
                                {
                                    AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)));
                                }
                                break;

                            case 4:
                                CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(AI.DeltaData.Target);
                                if (t_MyCharacterControl.ParticipantDataReference.Stun.GetCurrent() >= t_MyCharacterControl.ParticipantDataReference.Stun.Max)
                                {
                                    AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                    {
                                        new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: true)),
                                    }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (UnityEngine.Random.Range(1, 5))
                        {
                            case 1:
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: true)),
                                }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                break;

                            case 2:
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)),
                                }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                break;

                            case 3:
                                AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                {
                                    new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Down, ZTrigger: false)),
                                });
                                if (AI.Difficulty >= AI_Bundle.Enum_DifficultyLevel.Normal)
                                {
                                    AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)));
                                    if (AI.Difficulty == AI_Bundle.Enum_DifficultyLevel.Normal)
                                    {
                                        int choice = UnityEngine.Random.Range(0, 3);
                                        AI_Bundle.Enum_DirectionalInput inp =
                                            (choice == 0) ? AI_Bundle.Enum_DirectionalInput.Neutral :
                                            (choice == 1) ? AI_Bundle.Enum_DirectionalInput.Up :
                                            AI_Bundle.Enum_DirectionalInput.Down;
                                        AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(inp, ZTrigger: false)));
                                    }
                                }
                                break;

                            case 4:
                                CharacterControl t_MyCharacterControl = SMBZGlobals.GetCharacterControl(AI.DeltaData.Target);
                                if (t_MyCharacterControl.ParticipantDataReference.Stun.GetCurrent() >= t_MyCharacterControl.ParticipantDataReference.Stun.Max)
                                {
                                    AI.CommandList.Set(new AI_Bundle.AI_Action[]
                                    {
                                        new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: true)),
                                    }, () => AI.DeltaData.vectorToTarget_Absolute.x > 3f);
                                }
                                break;
                        }
                    }
                }
            }
            else if (AI.DeltaData.IsTargetAboveMe && AI.DeltaData.vectorToTarget_Absolute.x < 0.5f)
            {
                if (AI.DeltaData.Target.GetVelocity().y < 0.25f)
                {
                    AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, AI.DeltaData.vectorToTarget.y < 2f);
                    return;
                }

                switch (UnityEngine.Random.Range(1, 4))
                {
                    case 1:
                        if (AI.Difficulty < AI_Bundle.Enum_DifficultyLevel.Normal || InventoryData.Boos <= 0)
                            break;
                        AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
                        AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: true, SuperInput: true);
                        break;

                    case 2:
                        AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
                        AI.JumpIdea = null;
                        AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: true);
                        break;

                    case 3:
                        AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
                        AI.JumpIdea = new AI_Bundle.Internal_JumpIdea(IsFullJump: true, 0);
                        AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Up, ZTrigger: true);
                        break;
                }
            }
            else if (AI.DeltaData.IsTargetBelowMe)
            {
                if (AI.DeltaData.vectorToTarget_Absolute.x < 0.25f && AI.DeltaData.vectorToTarget.y > 0.5f)
                {
                    AI.RethinkCooldown_Attacking = AI.GetRethinkCooldown(AI_Bundle.Enum_RethinkCooldownType.Attacking);
                    AI.CommandList.Set(new AI_Bundle.AI_Action[]
                    {
                        new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Down, ZTrigger: false)),
                    });
                    if (AI.Difficulty >= AI_Bundle.Enum_DifficultyLevel.Normal)
                    {
                        AI.CommandList.ActionQueue.Add(new AI_Bundle.AI_Action(new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Neutral, ZTrigger: false)));
                    }
                }
                else if (0.5f < AI.DeltaData.vectorToTarget_Absolute.magnitude && AI.DeltaData.vectorToTarget_Absolute.magnitude < 4f)
                {
                    AI.AttackIdea = new AI_Bundle.Internal_AttackIdea(AI_Bundle.Enum_DirectionalInput.Down, ZTrigger: true);
                }
            }
        }
    }
}