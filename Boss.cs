using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Boss : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform player;
    public GameObject attackIndicatorPrefab;

    public Transform[] leftHandPathPoints;
    public Transform[] rightHandPathPoints;

    public float avoidanceHeight = 3.0f;
    public float detectionRange = 5.0f;
    public float avoidanceDetectionRange = 2.0f;
    public float attackCooldownTime = 4.0f;
    public float avoidanceCooldownTime = 6.0f;
    public float attackDelay = 2.0f;
    public float rotationSpeed = 2.0f;

    private bool isAttackCooldown = false;
    private bool isAvoidanceCooldownLeftHand = false;
    private bool isAvoidanceCooldownRightHand = false;
    private bool isAvoiding = false;

    private float avoidanceProbabilityScale = 1.0f;
    private float attackProbabilityScale = 1.0f; 

    private GameObject currentIndicator;

    public int maxHp = 300;
    public int nowHp;

    private Vector3 leftHandInitialPosition;
    private Vector3 rightHandInitialPosition;

    // 퍼지 시스템 관련 변수들
    private FuzzyInferenceSystem evasionFuzzySystem;
    private FuzzyInferenceSystem attackFuzzySystem;
    private FuzzySet close;
    private FuzzySet medium;
    private FuzzySet far;
    private FuzzySet lowReadiness;
    private FuzzySet mediumReadiness;
    private FuzzySet highReadiness;
    private FuzzySet lowPlayerAggression;
    private FuzzySet mediumPlayerAggression;
    private FuzzySet highPlayerAggression;
    private FuzzySet lowAttack;
    private FuzzySet mediumAttack;
    private FuzzySet highAttack;

    void Start()
    {
        nowHp = maxHp;
        leftHandInitialPosition = leftHand.position;
        rightHandInitialPosition = rightHand.position;
        StartHandRotation(leftHand, leftHandPathPoints);
        StartHandRotation(rightHand, rightHandPathPoints);

        InitializeFuzzySystems();
    }

    void Update()
    {
        if (!isAttackCooldown)
        {
            Transform selectedHand = Random.value > 0.5f ? rightHand : leftHand;
            if (Vector3.Distance(selectedHand.position, player.position) < detectionRange && ShouldAttack(selectedHand))
            {
                StartCoroutine(PerformAttack(selectedHand));
            }
        }

        if (!isAvoiding)
        {
            if (!isAvoidanceCooldownLeftHand && ShouldAvoid(leftHand))
            {
                StartCoroutine(PerformAvoidance(leftHand));
            }
            if (!isAvoidanceCooldownRightHand && ShouldAvoid(rightHand))
            {
                StartCoroutine(PerformAvoidance(rightHand));
            }
        }
    }

    void StartHandRotation(Transform hand, Transform[] pathPoints)
    {
        Vector3[] path = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            path[i] = pathPoints[i].position;
        }

        hand.DOKill();
        hand.DOPath(path, rotationSpeed, PathType.CatmullRom)
            .SetOptions(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    IEnumerator PerformAvoidance(Transform hand)
    {
        isAvoiding = true;
        if (hand == leftHand)
        {
            isAvoidanceCooldownLeftHand = true;
        }
        else
        {
            isAvoidanceCooldownRightHand = true;
        }

        Vector3 originalPosition = hand.position;
        Vector3 avoidancePosition = new Vector3(hand.position.x, hand.position.y + avoidanceHeight, hand.position.z);

        hand.DOKill();
        hand.DOMove(avoidancePosition, 1.0f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(1.0f);

        hand.DOMove(originalPosition, 1.0f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(1.0f);

        if (hand == leftHand)
        {
            yield return new WaitForSeconds(avoidanceCooldownTime);
            isAvoidanceCooldownLeftHand = false;
        }
        else
        {
            yield return new WaitForSeconds(avoidanceCooldownTime);
            isAvoidanceCooldownRightHand = false;
        }

        StartHandRotation(hand, hand == leftHand ? leftHandPathPoints : rightHandPathPoints);
        isAvoiding = false;
    }

    IEnumerator PerformAttack(Transform attackingHand)
    {
        isAttackCooldown = true;

        Vector3 attackPosition = player.position;
        currentIndicator = Instantiate(attackIndicatorPrefab, attackPosition, Quaternion.identity);
        currentIndicator.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);

        attackingHand.DOKill();
        yield return new WaitForSeconds(attackDelay);
        StartCoroutine(SmashAttack(attackingHand, attackPosition));

        yield return new WaitForSeconds(attackCooldownTime);
        isAttackCooldown = false;

        StartHandRotation(attackingHand, attackingHand == leftHand ? leftHandPathPoints : rightHandPathPoints);
    }

    IEnumerator SmashAttack(Transform hand, Vector3 position)
    {
        Vector3 originalPosition = hand.position;
        float smashSpeed = 10.0f;

        while (Vector3.Distance(hand.position, position) > 0.05f)
        {
            hand.position = Vector3.MoveTowards(hand.position, position, smashSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        while (Vector3.Distance(hand.position, originalPosition) > 0.05f)
        {
            hand.position = Vector3.MoveTowards(hand.position, originalPosition, smashSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(currentIndicator);
    }

    public void TakeDamage(int damage)
    {
        nowHp -= damage;

        if (nowHp <= 0)
        {
            gameObject.layer = 12;
            gameObject.tag = "EnemyDead";
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftHand.position, avoidanceDetectionRange);
        Gizmos.DrawWireSphere(rightHand.position, avoidanceDetectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(leftHand.position, detectionRange);
        Gizmos.DrawWireSphere(rightHand.position, detectionRange);
    }

    void InitializeFuzzySystems()
    {
        evasionFuzzySystem = new FuzzyInferenceSystem();
        attackFuzzySystem = new FuzzyInferenceSystem();

        // 퍼지 집합 정의
        close = new FuzzySet(0, 0, 5);
        medium = new FuzzySet(3, 7, 10);
        far = new FuzzySet(8, 15, 20);

        // 새로운 퍼지 집합 정의
        lowReadiness = new FuzzySet(0, 0, 0.3f);
        mediumReadiness = new FuzzySet(0.2f, 0.5f, 0.8f);
        highReadiness = new FuzzySet(0.7f, 1, 1);

        lowPlayerAggression = new FuzzySet(0, 0, 0.3f);
        mediumPlayerAggression = new FuzzySet(0.2f, 0.5f, 0.8f);
        highPlayerAggression = new FuzzySet(0.7f, 1, 1);

        lowAttack = new FuzzySet(0, 0, 0.3f);
        mediumAttack = new FuzzySet(0.2f, 0.5f, 0.8f);
        highAttack = new FuzzySet(0.7f, 1, 1);

        // 회피 퍼지 규칙 추가
        evasionFuzzySystem.AddEvasionRule(close, lowReadiness, highAttack);
        evasionFuzzySystem.AddEvasionRule(close, mediumReadiness, mediumAttack);
        evasionFuzzySystem.AddEvasionRule(close, highReadiness, lowAttack);
        evasionFuzzySystem.AddEvasionRule(medium, lowReadiness, mediumAttack);
        evasionFuzzySystem.AddEvasionRule(medium, mediumReadiness, mediumAttack);
        evasionFuzzySystem.AddEvasionRule(medium, highReadiness, lowAttack);
        evasionFuzzySystem.AddEvasionRule(far, lowReadiness, lowAttack);
        evasionFuzzySystem.AddEvasionRule(far, mediumReadiness, lowAttack);
        evasionFuzzySystem.AddEvasionRule(far, highReadiness, lowAttack);

        // 공격 퍼지 규칙 추가
        attackFuzzySystem.AddAttackRule(lowReadiness, lowPlayerAggression, lowAttack);
        attackFuzzySystem.AddAttackRule(lowReadiness, mediumPlayerAggression, lowAttack);
        attackFuzzySystem.AddAttackRule(lowReadiness, highPlayerAggression, mediumAttack);
        attackFuzzySystem.AddAttackRule(mediumReadiness, lowPlayerAggression, lowAttack);
        attackFuzzySystem.AddAttackRule(mediumReadiness, mediumPlayerAggression, mediumAttack);
        attackFuzzySystem.AddAttackRule(mediumReadiness, highPlayerAggression, highAttack);
        attackFuzzySystem.AddAttackRule(highReadiness, lowPlayerAggression, mediumAttack);
        attackFuzzySystem.AddAttackRule(highReadiness, mediumPlayerAggression, highAttack);
        attackFuzzySystem.AddAttackRule(highReadiness, highPlayerAggression, highAttack);
    }

    bool ShouldAvoid(Transform hand)
    {
        float distanceToPlayer = Vector3.Distance(hand.position, player.position);
        if (distanceToPlayer > avoidanceDetectionRange)
        {
            return false;
        }

        float healthFactor = (float)nowHp / maxHp;
        float evasionProbability = evasionFuzzySystem.DefuzzifyEvasion(distanceToPlayer, healthFactor) * avoidanceProbabilityScale;

        return Random.value < evasionProbability;
    }

    bool ShouldAttack(Transform hand)
    {
        float readinessFactor = GetReadinessFactor();
        float playerAggression = player.GetComponent<Player>().GetAggressionLevel();
        float attackProbability = attackFuzzySystem.DefuzzifyAttack(readinessFactor, playerAggression) * attackProbabilityScale;

        return Random.value < attackProbability;
    }

    float GetReadinessFactor()
    {
        return 0.5f;
    }
}
