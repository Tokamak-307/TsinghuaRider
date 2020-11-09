﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

interface IWeapon
{
    void Attack(GameObject user, Vector3 direction);
}
/// <summary>
/// 远程武器类
/// </summary> 
public class MeleeWeapon : Item, IWeapon
{
    /// <summary>
    /// 子弹prefab
    /// </summary>
    public GameObject bulletPrefab;

    public void Attack(GameObject user, Vector3 direction)
    {
        bulletPrefab.GetComponent<Bullets>().Direction = direction;
        bulletPrefab.GetComponent<Bullets>().AttackAmount = user.GetComponent<LivingBaseAgent>().living.AttackAmount;

        GameObject.Instantiate(bulletPrefab, user.transform.position, Quaternion.identity);
    }
}

public class MissleWeapon : Item, IWeapon
{
    public void Attack(GameObject user, Vector3 Direction)
    {
        string layerMask;
        LivingBaseAgent agent = user.GetComponent<LivingBaseAgent>();
        // 判断进行攻击的是角色还是怪物
        if (agent is CharacterAgent)
        {
            layerMask = "Monster";
        }
        else
        {
            layerMask = "Player";
        }
        IEnumerable<GameObject> targetObjects = agent.GetAttackRangeObjects(user.transform.position, agent.living.AttackDirection, layerMask);
        foreach (var targetObject in targetObjects)
        {
            LivingBaseAgent targetAgent = targetObject.GetComponent<LivingBaseAgent>();
            targetAgent.ChangeHealth(-agent.living.AttackAmount);
            Debug.Log($"{agent.living.Name} attack {targetAgent.living.Name}, {targetAgent.living.Name} currentHealth is {targetAgent.living.CurrentHealth}");
        }
    }
}