using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;

public enum StatType
{
    Hitpoint,
    MovingSpeed,
    RotateWeight,
    VisionRange,
    AttackDuration,
    AttackDamageBoost
}

public enum StatOperationType
{
    AddValue,
    AddPercent
}

[Serializable]
public class StatModifier
{
    public StatOperationType operationType;
    public float value;

    public StatModifier(StatOperationType operationType, float value)
    {
        this.operationType = operationType;
        this.value = value;
    }
}

[Serializable]
public class EntityStats
{
    [SerializedDictionary] public SerializedDictionary<StatType, float> baseStats = _initStats();
    [SerializedDictionary] public SerializedDictionary<StatType, List<StatModifier>> statModifiers = new();

    private Dictionary<StatType, float> _finalStats = new();

    private static SerializedDictionary<StatType, float> _initStats()
    {
        SerializedDictionary<StatType, float> stats = new();
        foreach (var type in Enum.GetValues(typeof(StatType)).Cast<StatType>())
        {
            stats.Add(type, 0);
        }
        return stats;
    }

    // Add a modifier. Not recommended to create new modifier instance unless you want it permanent.
    public void AddModifier(StatType type, StatModifier modifier)
    {
        if (!statModifiers.ContainsKey(type))
        {
            statModifiers[type] = new();
        }

        if (statModifiers[type].Contains(modifier))
        {
            return;
        }
        statModifiers[type].Add(modifier);
            
    }

    // Remove specified modifier instance.
    public void RemoveModifier(StatType type, StatModifier modifier)
    {
        if (!statModifiers.ContainsKey(type))
        {
            return;
        }
        statModifiers[type].Remove(modifier);
    }

    // Calculate entity stats when adding / removing modifiers to reduce unnecessary summing process
    public void UpdateStats()
    {
        foreach (var type in Enum.GetValues(typeof(StatType)).Cast<StatType>())
        {
            _finalStats[type] = baseStats[type];

            if (!statModifiers.TryGetValue(type, out var modifiers)) continue;
            foreach (var modifier in modifiers)
            {
                if (modifier.operationType == StatOperationType.AddValue)
                {
                    _finalStats[type] += modifier.value;
                }
                else if (modifier.operationType == StatOperationType.AddPercent)
                {
                    _finalStats[type] += modifier.value * baseStats[type];
                }
            }
        }
    }

    public float hitpoint
    {
        get
        {
            if (!_finalStats.ContainsKey(StatType.Hitpoint))
            {
                UpdateStats();
            }
            return _finalStats[StatType.Hitpoint];
        }
    }

    public float movingSpeedRatio
    {
        get 
        {
            if (!_finalStats.ContainsKey(StatType.MovingSpeed))
            {
                UpdateStats();
            }
            return _finalStats[StatType.MovingSpeed];
        }
    }

    public float rotateWeight
    {
        get 
        {
            if (!_finalStats.ContainsKey(StatType.RotateWeight))
            {
                UpdateStats();
            }
            return _finalStats[StatType.RotateWeight];
        }
    }
    
    public float visionRange
    {
        get 
        {
            if (!_finalStats.ContainsKey(StatType.VisionRange))
            {
                UpdateStats();
            }
            return _finalStats[StatType.VisionRange];
        }
    }
    
    public float attackDuration
    {
        get 
        {
            if (!_finalStats.ContainsKey(StatType.AttackDuration))
            {
                UpdateStats();
            }
            return _finalStats[StatType.AttackDuration];
        }
    }
    
    public float attackDamageMultiplier
    {
        get 
        {
            if (!_finalStats.ContainsKey(StatType.AttackDamageBoost))
            {
                UpdateStats();
            }
            return Math.Max(1 + _finalStats[StatType.AttackDamageBoost], 0);
        }
    }
}