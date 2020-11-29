﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 背包类(Inventory)的实现
/// </summary>
public class Inventory 
{
    /// <summary>
    /// 这个OnItemListChanged用来记录每一次背包内物品的变动
    /// </summary>
    public event EventHandler OnItemListChanged;

    public List<Item> ItemList { get; private set; }
    //private Action<Item> useItemAction;

    /// <summary>
    /// 初始化背包，传入使用方法。带有一些物品
    /// </summary>
    /// <param name="useItemAction"></param>
    public Inventory() 
    {
        ItemList = new List<Item>();
    }

    /// <summary>
    /// 用来处理背包内物品的增加，并且记录在事件中。
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(Item item)
    {
        if (item.IsStackable)
        {
            var inventoryItem = ItemList.FirstOrDefault(e => e.Equals(item));
            if (inventoryItem == null)
            {
                ItemList.Add(item);
            }
            else
            {
                inventoryItem.Amount += item.Amount;
            }
        }
        else
        {
            ItemList.Add(item);
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(Item item)
    {
        var inventoryItem = ItemList.FirstOrDefault(e => e.Equals(item));
        inventoryItem.Amount -= item.Amount;
        if (inventoryItem.Amount <= 0)
        {
            ItemList.Remove(inventoryItem);
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasItem(Item item)
    {
        var inventoryItem = ItemList.FirstOrDefault(e => e.Equals(item));
        if (inventoryItem != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UseItem(Item item, CharacterAgent character)
    {
        if (HasItem(item))
        {
            item.Use(character);
            RemoveItem(item);
        }
        else
        {
            Debug.Log("无了啊！");
        }
    }

}
