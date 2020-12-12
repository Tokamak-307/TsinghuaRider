﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// 技能抽象类，增加新技能时继承此抽象类，实现Perform方法
/// </summary>
abstract public class Skill : ICloneable
{
    protected MonsterAgent agent;
    protected float CD;
    protected float CDTimer;
    protected float performTimer;
    protected bool skillStartFlag;

    protected float beforePerformTime;
    protected float afterPerformTime;

    protected Vector3 performDirection;
    protected int nowPerformStage;
    protected List<Func<bool>> performStage;
    public virtual void Init(MonsterAgent agent)
    {
        this.agent = agent;
        CD = agent.ActualMonster.AttackSpeed;
        CDTimer = 0;
        performTimer = 0;
        skillStartFlag = false;

        beforePerformTime = CD / agent.ActualMonster.Agility;
        afterPerformTime = CD / agent.ActualMonster.Agility;

        nowPerformStage = 0;
        performStage = new List<Func<bool>>
        {
            BeforePerform,
            DuringPerform,
            AfterPerform
        };
    }
    public bool Perform()
    {
        try
        {
            if (agent != null)
            {
                CDTimer += Time.deltaTime;
                if (CDTimer >= CD)
                {
                    return PerformSkill();
                }
            }
        }
        catch (MissingReferenceException ex)
        {
            Debug.Log(ex);
        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex);
        }
        return true;
    }

    public void EndSkill()
    {
        EndPerform();
    }
    protected bool PerformSkill()
    {
        if (agent == null)
        {
            return true;
        }

        if (!skillStartFlag)
        {
            if (!InitPerform())
            {
                EndPerform();
                return true;
            }
        }

        if (performStage[nowPerformStage].Invoke())
        {
            nowPerformStage++;
        }

        if (nowPerformStage == performStage.Count)
        {
            EndPerform();
            return true;
        }
        return false;
    }
    protected virtual bool InitPerform()
    {
        skillStartFlag = true;
        if (agent.target == null)
        {
            return false;
        }
        else
        {
            performDirection = (agent.target.GetCentralPosition() - agent.GetCentralPosition()).normalized;
            return true;
        }
    }
    protected virtual bool BeforePerform()
    {
        //agent.Animator.SetTrigger("before_attack");
        performTimer += Time.deltaTime;
        agent.rigidbody2d.velocity = Vector3.zero;
        if (performTimer > beforePerformTime)
        {
            performTimer = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    protected virtual bool DuringPerform() { return true; }
    protected virtual bool AfterPerform()
    {
        //agent.Animator.SetTrigger("after_attack");
        performTimer += Time.deltaTime;
        agent.rigidbody2d.velocity = Vector3.zero;
        if (performTimer > afterPerformTime)
        {
            performTimer = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    protected virtual void EndPerform()
    {
        skillStartFlag = false;
        nowPerformStage = 0;
        CDTimer = 0;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

abstract public class TargetSkill : Skill
{
    protected override bool InitPerform()
    {
        skillStartFlag = true;
        if (agent.target == null)
        {
            return false;
        }
        else
        {
            float distance = Vector3.Distance(agent.GetCentralPosition(), agent.target.GetCentralPosition());
            if (distance > agent.ActualMonster.AttackRadius)
            {
                return false;
            }
            else
            {
                performDirection = (agent.target.GetCentralPosition() - agent.GetCentralPosition()).normalized;
                return true;
            }
        }
    }
}
abstract public class AttackSkill : TargetSkill
{
    protected override bool DuringPerform() => DuringPerform(null);
    protected bool DuringPerform(Action<LivingBaseAgent> extraEffect)
    {
        agent.Animator.SetTrigger("attack");
        AttackAt(performDirection, extraEffect);
        performTimer = 0;
        return true;
    }
    /// <summary>
    /// 向指定方向攻击
    /// </summary>
    /// <param name="attackDirection">攻击方向</param>
    /// <param name="extraEffect">攻击附带的额外效果（lambda）</param>
    /// <param name="agent"></param>
    public abstract bool AttackAt(Vector3 attackDirection, Action<LivingBaseAgent> extraEffect);
}
/// <summary>
/// 远程武器攻击
/// </summary>
public class MissleAttackSkill : AttackSkill
{
    public override bool AttackAt(Vector3 attackDirection, Action<LivingBaseAgent> extraEffect)
    {
        GameObject projectileObject = GameObject.Instantiate(agent.bulletPrefab, agent.GetCentralPosition(), Quaternion.identity);
        Bullet bullet = projectileObject.GetComponent<Bullet>();
        bullet.SetBullet(agent, agent.actualLiving.AttackAmount, extraEffect);
        bullet.Shoot(attackDirection, agent.ActualMonster.BulletSpeed);
        return true;
    }

    protected override bool DuringPerform() => DuringPerform(null);
}



/// <summary>
/// 近战普攻技能
/// </summary>
public class MeleeAttackSkill : AttackSkill
{
    public override bool AttackAt(Vector3 attackDirection, Action<LivingBaseAgent> extraEffect)
    {
        bool attackFlag = false;
        IEnumerable<GameObject> targetObjects = agent.GetAttackRangeObjects(agent.GetCentralPosition(), attackDirection, agent.ActualMonster.AttackRadius, agent.ActualMonster.AttackAngle, "Player");
        foreach (var targetObject in targetObjects)
        {
            LivingBaseAgent targetAgent = targetObject.GetComponent<LivingBaseAgent>();
            targetAgent.ChangeHealth(-agent.ActualMonster.AttackAmount);
            extraEffect?.Invoke(targetAgent);
            attackFlag = true;
        }
        return attackFlag;
    }

    protected override bool DuringPerform() => DuringPerform(null);
}

/// <summary>
/// 近战普攻减速技能
/// </summary>
public class MeleeSlowAttackSkill : MeleeAttackSkill
{

    protected override bool DuringPerform() => DuringPerform(target => target.actualLiving.State.AddStatus(new SlowState(), 2));
}


/// <summary>
/// 分裂技能
/// </summary>
public class SplitSkill : Skill
{
    public int splitNum = 3;

    public override void Init(MonsterAgent agent)
    {
        this.agent = agent;
        CD = 0;
        CDTimer = 0;
        performTimer = 0;
        nowPerformStage = 0;
        skillStartFlag = false;
        performStage = new List<Func<bool>>
        {
            DuringPerform,
        };
        Debug.Log($"{GetType()} {performStage.Count}");
    }
    protected override bool InitPerform()
    {
        skillStartFlag = true;
        return true;
    }
    protected override bool DuringPerform()
    {
        GameObject prefab = Global.GetPrefab($"微{agent.living.Name}");
        for (int i = 0; i < splitNum; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(0, 1.0f), Random.Range(0, 1.0f));
            GameObject.Instantiate(prefab, agent.GetCentralPosition() + randomOffset, Quaternion.identity);
        }
        return true;
    }
}

public class FierceSkill : Skill
{
    protected float bloodLine;
    public override void Init(MonsterAgent agent)
    {
        base.Init(agent);
        bloodLine = agent.ActualMonster.BloodLine;
    }
    protected override bool DuringPerform()
    {
        if (agent.ActualMonster.CurrentHealth * 1.0f / agent.ActualMonster.MaxHealth <= bloodLine)
        {
            agent.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            agent.ActualMonster.State.AddStatus(new FierceState(), 1);
        }
        return true;
    }
}

public class LaserSkill : Skill
{
    protected Lasers lasers;
    protected bool finished;
    public override void Init(MonsterAgent agent)
    {
        base.Init(agent);
        BossAgent boss = agent as BossAgent;
        lasers = boss.GetLasers();
    }

    protected override bool DuringPerform()
    {
        return lasers.AutoPerform();
    }

    protected override void EndPerform()
    {
        base.EndPerform();
        lasers.EndLasers();
    }
}

public class BarrageSkill : Skill
{
    protected Barrage barrage;

    public override void Init(MonsterAgent agent)
    {
        base.Init(agent);
        BossAgent boss = agent as BossAgent;
        barrage = boss.GetBarrage();
    }

    protected override bool DuringPerform()
    {
        return barrage.Perform();
    }
}

public class ObstacleSkill : TargetSkill
{
    private float randomTime;
    private Vector3 obstaclePosition;
    public override void Init(MonsterAgent agent)
    {
        base.Init(agent);
        randomTime = Random.Range(0, 0.3f);
        beforePerformTime = randomTime;
    }

    protected override bool InitPerform()
    {
        skillStartFlag = true;
        if (agent.target == null)
        {
            return false;
        }
        else
        {
            obstaclePosition = agent.target.GetCentralPosition();
            return true;
        }
    }
    protected override bool DuringPerform()
    {
        GameObject.Instantiate(agent.bulletPrefab, obstaclePosition, Quaternion.identity);
        performTimer = 0;
        return true;
    }
}

public class MissleBleedAttackSkill : MissleAttackSkill
{
    protected override bool DuringPerform()
    {
        return DuringPerform(target => target.actualLiving.State.AddStatus(new BleedState(), 10));
    }
}
public class MeleeChargeAttackSkill : MeleeAttackSkill
{
    private float chargeTime = 1;
    private int speedTimes = 3;
    private float beatDistance = 3;
    private bool attackFlag;
    public override void Init(MonsterAgent agent)
    {
        base.Init(agent);
        attackFlag = false;
    }

    protected override bool InitPerform()
    {
        bool flag = base.InitPerform();
        agent.actualLiving.MoveSpeed = agent.living.MoveSpeed * speedTimes;
        agent.actualLiving.AttackRadius = 1.5f;
        return flag;
    }
    protected override bool DuringPerform()
    {
        performTimer += Time.deltaTime;
        agent.MoveTowards(performDirection);
        if (!attackFlag)
        {
            if (AttackAt(performDirection, BeatBack))
            {
                attackFlag = true;
            }
        }

        if (chargeTime < performTimer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void EndPerform()
    {
        base.EndPerform();
        attackFlag = false;
        agent.actualLiving.MoveSpeed = agent.living.MoveSpeed;
        agent.actualLiving.AttackRadius = agent.living.AttackRadius;
    }

    public void BeatBack(LivingBaseAgent target)
    {
        Vector3 beatPosition = target.GetCentralPosition() + performDirection * beatDistance;
        RaycastHit2D dashHit = Physics2D.Raycast(target.rigidbody2d.position, performDirection, beatDistance, LayerMask.GetMask("Obstacle"));
        if (dashHit.collider != null)
        {
            beatPosition = dashHit.point;
        }
        target.rigidbody2d.MovePosition(new Vector2(beatPosition.x, beatPosition.y));
    }
}

