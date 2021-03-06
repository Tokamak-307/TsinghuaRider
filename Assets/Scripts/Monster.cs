﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 怪物属性类，继承自LivingBase，存储角色所有基本属性
/// </summary>
public class Monster : LivingBase
{
    /// <summary>
    /// 敏捷性，决定前后摇时间长短
    /// </summary>
    public float Agility { get; set; }
    /// <summary>
    /// 视野角度
    /// </summary>
    public float ViewAngle { get; set; }
    /// <summary>
    /// 视野半径，玩家进入视野半径以内时追击玩家
    /// </summary>
    public float ViewRadius { get; set; }
    /// <summary>
    /// 击杀奖励
    /// </summary>
    [JsonConverter(typeof(TJsonConverter<Item>))]
    public List<Item> Rewards { get; set; }
    public float[] Possibility { get; set; }
    /// <summary>
    /// 难度
    /// </summary>
    public int Difficulty { get; set; }

    public float BulletSpeed { get; set; }

    public float BloodLine { get; set; }

    /// <summary>
    /// 技能列表，存储实体可施放的技能
    /// </summary>
    [JsonConverter(typeof(TJsonConverter<Skill>))]
    public List<Skill> Skills { get; set; }
    [JsonConverter(typeof(TJsonConverter<Skill>))]
    public List<Skill> SkillOrder { get; set; }
    public AttackSkill AttackSkill => Skills[0] as AttackSkill;
    public override object Clone()
    {
        Monster monster =  MemberwiseClone() as Monster;
        List<Skill> newSkills = new List<Skill>();
        monster.Skills = monster.Skills.Select(skill => skill.Clone() as Skill).ToList();
        monster.SkillOrder = monster.SkillOrder.Select(skill => skill.Clone() as Skill).ToList();
        monster.State = new State();
        return monster;
    }

    /// <summary>
    /// 加载怪物配置文件，游戏初始化时调用
    /// </summary>
    /// <returns>monster list</returns>
    public static List<Monster> LoadMonster()
    {
        string path = Settings.MONSTER_CONFIG_PATH;
        string json = Resources.Load<TextAsset>(path).text;
        List<Monster> monsters = JsonSerializer.CreateDefault().Deserialize<List<Monster>>(new JsonTextReader(new StringReader(json)));
        return monsters;
    }

    public static void SaveMonster(List<Monster> monsters)
    {
        string path = Settings.MONSTER_CONFIG_PATH;
        using (StreamWriter sw = new StreamWriter(path))
        {
            JsonSerializer.CreateDefault().Serialize(new JsonTextWriter(sw), monsters);
        }
    }
}

class TJsonConverter<T> : JsonConverter<List<T>>
{
    public override List<T> ReadJson(JsonReader reader, Type objectType, List<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (existingValue == null) existingValue = new List<T>();
        JArray arr = JArray.Load(reader);
        foreach (var s in arr)
        {
            existingValue.Add((T)Activator.CreateInstance(typeof(T).Assembly.GetType(s.ToString())));
        }
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, List<T> value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        if (value != null)
        {
            foreach (var s in value)
            {
                string name = s.GetType().Name;
                writer.WriteValue(name.Substring(0, name.Length - 5));
            }
        }
        writer.WriteEndArray();
    }
}