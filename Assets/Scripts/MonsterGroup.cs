﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGroup : MonoBehaviour
{
    /// <summary>
    /// 可生成的怪物prefab
    /// </summary>
    public GameObject[] monsterObjects;
    /// <summary>
    /// 参考生成概率
    /// </summary>
    public float[] probability;
    /// <summary>
    /// 生成延迟
    /// </summary>
    public float delay;
    /// <summary>
    /// 额外词条数量
    /// </summary>
    public int extraState;
    public GameObject visualEffect;

    bool generating = false;
    float timer = 0;
    float difficulty;
    GameObject effect;
    Animator effectAnimator;

    public void Generate(float _difficulty)
    {
        difficulty = _difficulty;
        generating = true;
        timer = delay;

        // 生成特效
        effect = Instantiate(visualEffect, transform);
        // effect.transform.localScale *= Mathf.Sqrt(difficulty);
        effect.transform.localScale *= 10;
        effectAnimator = effect.GetComponent<Animator>();
        effectAnimator.speed = 1.0f / (delay + 1e-4f);
    }

    private void Update()
    {
        if (generating)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                // 随机生成词条
                float speedFactor = 1, attackSpeedFactor = 1, agilityFactor = 1;
                float healthFactor = 1, attackAmountFactor = 1, attackRadiusFactor = 1;
                int infestedNumber = 0;
                for (int i=0; i<extraState; i++)
                {
                    int state;
                    state = Random.Range(0, 7);
                    //Debug.Log("加词条" + state);
                    switch (state)
                    {
                        case 0:
                            speedFactor += 0.5f;
                            break;
                        case 1:
                            attackSpeedFactor += 1f;
                            break;
                        case 2:
                            agilityFactor += 1f;
                            break;
                        case 3:
                            healthFactor += 1.3f;
                            break;
                        case 4:
                            attackAmountFactor += 0.6f;
                            break;
                        case 5:
                            attackRadiusFactor += 0.5f;
                            break;
                        case 6:
                            infestedNumber += 1;
                            break;
                        default:
                            break;
                    }
                }

                // 随机生成怪物，直到难度用完。生成后摧毁自身。
                float[] monsterDifficulty = new float[monsterObjects.Length];
                float usedDifficulty = 0.0f;
                float totalProbability = 0.0f;

                for (int i = 0; i < monsterObjects.Length; i++)
                {
                    Monster monster = Global.monsters[monsterObjects[i].GetComponent<MonsterAgent>().monsterIndex];
                    monsterDifficulty[i] = monster.Difficulty;

                    totalProbability += probability[i];
                }

                while (usedDifficulty < difficulty)
                {
                    float tmp = Random.Range(0, totalProbability);
                    float current = 0.0f;
                    int i;
                    for (i = 0; i < monsterObjects.Length; i++)
                    {
                        current += probability[i];
                        if (tmp < current)
                        {
                            MonsterAgent agent = Instantiate(monsterObjects[i], transform.position, Quaternion.identity).GetComponent<MonsterAgent>();
                            usedDifficulty += monsterDifficulty[i];

                            if (agent.living.MaxHealth > 900)
                            {
                                //Debug.Log("拉格朗日增强了！");
                                agent.actualLiving.State.AddStatus(new BossEnhanceState(), float.NaN);
                            }
                            // 为新生成的怪物增加词条
                            if (!Mathf.Approximately(speedFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new SpeedState(speedFactor), float.NaN);
                            }
                            if (!Mathf.Approximately(attackSpeedFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new AttackSpeedState(attackSpeedFactor), float.NaN);
                            }
                            if (!Mathf.Approximately(agilityFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new AgilityState(agilityFactor), float.NaN);
                            }
                            if (!Mathf.Approximately(healthFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new HealthState(healthFactor), float.NaN);
                            }
                            if (!Mathf.Approximately(attackAmountFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new AttackAmountState(attackAmountFactor), float.NaN);
                            }
                            if (!Mathf.Approximately(attackRadiusFactor, 1.0f))
                            {
                                agent.actualLiving.State.AddStatus(new AttackRadiusState(attackRadiusFactor), float.NaN);
                            }
                            if (infestedNumber != 0)
                            {
                                agent.actualLiving.State.AddStatus(new InfestedState(infestedNumber), float.NaN);
                            }

                            break;
                        }
                    }

                    //Debug.Log("used difficulty" + usedDifficulty);
                    //Debug.Log("total difficulty" + difficulty);
                }

                Destroy(gameObject);
            }
        }

    }

}
