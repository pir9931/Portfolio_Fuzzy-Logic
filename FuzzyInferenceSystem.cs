using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzySet
{
    private float a, b, c;

    public FuzzySet(float a, float b, float c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public float GetMembership(float value)
    {
        float membership = 0;

        if (value <= a)
        {
            membership = 0;
        }
        else if (value > a && value <= b)
        {
            membership = (value - a) / (b - a);
        }
        else if (value > b && value <= c)
        {
            membership = (c - value) / (c - b);
        }
        else
        {
            membership = 0;
        }

        return membership;
    }

    public float GetCentroid()
    {
        return (a + b + c) / 3.0f;
    }
}

public class FuzzyInferenceSystem
{
    private List<FuzzyRule> evasionRules;
    private List<FuzzyRule> attackRules;

    public FuzzyInferenceSystem()
    {
        evasionRules = new List<FuzzyRule>();
        attackRules = new List<FuzzyRule>();
    }

    // 회피 규칙 추가
    public void AddEvasionRule(FuzzySet distanceSet, FuzzySet healthSet, FuzzySet evasionSet)
    {
        evasionRules.Add(new FuzzyRule(distanceSet, healthSet, evasionSet));
    }

    // 공격 규칙 추가
    public void AddAttackRule(FuzzySet readinessSet, FuzzySet aggressionSet, FuzzySet attackSet)
    {
        attackRules.Add(new FuzzyRule(readinessSet, aggressionSet, attackSet));
    }

    // 회피 확률 계산
    public float DefuzzifyEvasion(float distance, float health)
    {
        float evasionSum = 0;
        float weightSum = 0;

        foreach (var rule in evasionRules)
        {
            float distanceMembership = rule.DistanceSet.GetMembership(distance);
            float healthMembership = rule.HealthSet.GetMembership(health);
            float weight = Mathf.Min(distanceMembership, healthMembership);

            evasionSum += weight * rule.EvasionSet.GetCentroid();
            weightSum += weight;

            Debug.Log($"Evasion Rule: Distance {distanceMembership}, Health {healthMembership}, Weight {weight}, Centroid {rule.EvasionSet.GetCentroid()}");
        }

        float evasionProbability = (weightSum != 0) ? evasionSum / weightSum : 0;
        Debug.Log($"Evasion Sum: {evasionSum}, Weight Sum: {weightSum}, Evasion Probability: {evasionProbability}");

        return evasionProbability;
    }

    // 공격 확률 계산
    public float DefuzzifyAttack(float readiness, float aggression)
    {
        float attackSum = 0;
        float weightSum = 0;

        foreach (var rule in attackRules)
        {
            float readinessMembership = rule.DistanceSet.GetMembership(readiness);
            float aggressionMembership = rule.HealthSet.GetMembership(aggression);
            float weight = Mathf.Min(readinessMembership, aggressionMembership);

            attackSum += weight * rule.EvasionSet.GetCentroid();
            weightSum += weight;

            Debug.Log($"Attack Centroid {rule.EvasionSet.GetCentroid()}");
        }

        float attackProbability = (weightSum != 0) ? attackSum / weightSum : 0;
        Debug.Log($"멤버십: {attackSum}, 가중치: {weightSum}, 공격 확률: {attackProbability}");

        return attackProbability;
    }
}

public class FuzzyRule
{
    public FuzzySet DistanceSet { get; }
    public FuzzySet HealthSet { get; }
    public FuzzySet EvasionSet { get; }

    public FuzzyRule(FuzzySet distanceSet, FuzzySet healthSet, FuzzySet evasionSet)
    {
        DistanceSet = distanceSet;
        HealthSet = healthSet;
        EvasionSet = evasionSet;
    }
}