﻿using System;
using TMPro;
using UnityEngine;

public class WeaponAgent : ItemAgent
{
    protected Transform aimTransform;
    protected CharacterAgent user;

    public GameObject bulletPrefab;
    public Weapon Weapon { get; set; }

    protected Vector3 aimDir = new Vector3(1,0,0);

    protected bool leftFlag = false;
    protected bool upFlag = false;

    protected enum State
    {
        Normal,
        MeleeAttack
    }

    protected State state = State.Normal;

    private void Awake()
    {
        Weapon = Global.items[itemIndex].Clone() as Weapon;
        Weapon.bulletPrefab = bulletPrefab;
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();

        aimTransform = transform;
        try
        {
            user = transform.parent.gameObject.GetComponent<CharacterAgent>();
        }
        catch (Exception)
        {
            user = null;
        }
    }

    protected void Update()
    {
        //Debug.Log($"{GetType()} {state}");
        switch (state)
        {
            case State.Normal:
                if (user != null)
                {
                    HandleAiming();
                    HandlePosition();
                    //Debug.Log("Normal");
                }
                break;
            case State.MeleeAttack:
                //Debug.Log("Weapon Attacking");
                break;
        }
        transform.position = user.gameObject.transform.Find("weaponPosition").gameObject.transform.position;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vec.z = 0f;
        return vec;
    }

    protected float GetCurrentAngle()
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        aimDir = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        return angle;
    }

    private void HandleAiming()
    {
        HandleAiming(GetCurrentAngle(), 0f, true);
    }

    protected void HandleAiming(float offSet, float angle,  bool leftFlag)
    {
        
            if (leftFlag)
        {
            aimTransform.eulerAngles = new Vector3(0, 0, offSet - angle);
         }
        else
        {
            aimTransform.eulerAngles = new Vector3(0, 0, offSet + angle);
        }
    }


    public virtual bool Attack()
    {
        CharacterAgent agent = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterAgent>();
        agent.AudioSource.PlayOneShot(agent.MissleAttackClip);
        Weapon.Attack(user, aimDir);
        return true;
    }

    public virtual bool SwordLightAttack()//刀光攻击
    {
        Weapon.SwordLightAttack(user, aimDir);
        return true;
    }


    public override void InteractWith(GameObject gameObject)
    {
        CharacterAgent character = gameObject.GetComponent<CharacterAgent>();
        if (character != null)
        {
            //只捡没有的武器
           if ((character.WeaponPrefab == null || character.WeaponPrefab.GetComponent<WeaponAgent>().itemIndex != itemIndex) && !character.HasWeapon(Item))
            {
                character.WeaponColumnAddItem(Item);
                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// 远程武器人物朝上或朝下时，调整武器的左右位置，使得武器始终在左手上
    /// </summary>
    protected virtual void HandlePosition()
    {
        //Debug.Log("角色编号：");
        //Debug.Log(user.characterIndex);
        if (user.characterIndex == 0)
        {
            Vector3 weaponPosition = transform.position;

            if (upFlag && aimDir.y <= 0)
            {
                //Debug.Log("向下看！"); 
                upFlag = false;
                float deltaX = transform.position.x - user.GetPosition().x;
                transform.position = Vector3.MoveTowards(weaponPosition, new Vector3(user.GetPosition().x - deltaX, weaponPosition.y), 10000f);
                Weapon.offset = new Vector3(1, 1, 0);
                //transform.Translate(new Vector3(user.GetPosition().x, weaponPosition.y));

            }
            else if (!upFlag && aimDir.y > 0)
            {
                //Debug.Log("向上看！");

                upFlag = true;
                float deltaX = transform.position.x - user.GetPosition().x;

                transform.position = Vector3.MoveTowards(weaponPosition, new Vector3(user.GetPosition().x - deltaX, weaponPosition.y), 10000f);
                Weapon.offset = new Vector3(-1, 1, 0);
                //transform.Translate(new Vector3(user.GetPosition().x, weaponPosition.y));

            }

            if (!leftFlag && aimDir.x < 0)
            {
                leftFlag = true;
                Vector3 scale = transform.localScale;
                scale.y = -scale.y;
                transform.localScale = scale;

            }
            else if (leftFlag && aimDir.x >= 0)
            {
                leftFlag = false;
                Vector3 scale = transform.localScale;
                scale.y = -scale.y;
                transform.localScale = scale;

            }
        }

        else if(user.characterIndex == 1)
        {
            Vector3 weaponPosition = transform.position;

            if (upFlag && aimDir.y <= 0)
            {

                upFlag = false;
               // Vector3 scale = transform.localScale;
                //scale.y = -scale.y;
                //transform.localScale = scale;
            }
            else if (!upFlag && aimDir.y > 0)
            {

                upFlag = true;
                //Vector3 scale = transform.localScale;
                //scale.y = -scale.y;
                //transform.localScale = scale;
            }

            if (!leftFlag && aimDir.x < 0)
            {
                leftFlag = true;
                float deltaX = transform.position.x - user.GetPosition().x;
                transform.position = Vector3.MoveTowards(weaponPosition, new Vector3(user.GetPosition().x - deltaX, weaponPosition.y), 10000f);
                Weapon.offset= new Vector3(-1, 1, 0); 

            }
            else if (leftFlag && aimDir.x >= 0)
            {
                leftFlag = false;
                float deltaX = transform.position.x - user.GetPosition().x;

                transform.position = Vector3.MoveTowards(weaponPosition, new Vector3(user.GetPosition().x - deltaX, weaponPosition.y), 10000f);
                Weapon.offset =new Vector3(1, 1, 0);
            }
        }
        else
        {

        }
    }

    /// <summary>
    /// 以下的函数为了测试使用
    /// </summary>
    public virtual void TestAwake(CharacterAgent _user)
    {
        Weapon = Global.items[itemIndex].Clone() as Weapon;
        Weapon.bulletPrefab = bulletPrefab;
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();

        aimTransform = transform;
        user = _user;
    }
}

