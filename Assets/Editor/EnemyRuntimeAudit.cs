using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EnemyRuntimeAudit
{
    private const string RunningKey = "EnemyRuntimeAudit.Running";
    private static double startedAt;
    private static bool sawIdle;
    private static bool sawWalk;
    private static bool sawGround;
    private static bool sawClearWall;
    private static bool sawVelocity;
    private static float startX;

    static EnemyRuntimeAudit()
    {
        EditorApplication.update += Tick;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(RunningKey, true);
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(RunningKey, false) || !EditorApplication.isPlaying)
            return;

        if (startedAt == 0)
            startedAt = EditorApplication.timeSinceStartup;

        GameObject enemyObject = GameObject.Find("Enemy");
        EnemyController enemy = enemyObject != null ? enemyObject.GetComponent<EnemyController>() : null;
        if (enemy == null || enemy.StateMachine == null || enemy.Rb == null)
        {
            Fail("EnemyController, StateMachine, or Rigidbody2D missing at runtime");
            return;
        }

        if (!sawIdle)
        {
            sawIdle = enemy.StateMachine.CurrentState is EnemyIdleState;
            startX = enemy.transform.position.x;
            Debug.Log("ENEMY_AUDIT ENTER IDLE=" + sawIdle);
        }

        sawGround |= enemy.IsGroundDetect;
        sawClearWall |= !enemy.IsWallDetected;
        bool isWalk = enemy.StateMachine.CurrentState is EnemyWalkState;
        if (isWalk && !sawWalk)
        {
            sawWalk = true;
            Debug.Log("ENEMY_AUDIT IDLE -> WALK; ENTER WALK");
        }

        if (isWalk && Mathf.Abs(enemy.Rb.linearVelocity.x) > 0.1f)
            sawVelocity = true;

        if (EditorApplication.timeSinceStartup - startedAt < 3.5)
            return;

        bool freeX = (enemy.Rb.constraints & RigidbodyConstraints2D.FreezePositionX) == 0;
        bool moved = Mathf.Abs(enemy.transform.position.x - startX) > 0.1f;
        bool facingMatchesScale = Mathf.Sign(enemy.transform.localScale.x) == enemy.FacingDirection;
        Debug.Log($"ENEMY_AUDIT IsGroundDetect={enemy.IsGroundDetect} IsWallDetected={enemy.IsWallDetected} FacingDirection={enemy.FacingDirection} scaleX={enemy.transform.localScale.x} linearVelocityX={enemy.Rb.linearVelocity.x} movedX={enemy.transform.position.x - startX} freeX={freeX}");
        bool passed = sawIdle && sawWalk && sawGround && sawClearWall && sawVelocity && moved && freeX && facingMatchesScale;
        Debug.Log("ENEMY_AUDIT " + (passed ? "PASS" : "FAIL") + $" idle={sawIdle} walk={sawWalk} ground={sawGround} clearWall={sawClearWall} velocity={sawVelocity} moved={moved}");
        SessionState.SetBool(RunningKey, false);
        EditorApplication.Exit(passed ? 0 : 1);
    }

    private static void Fail(string reason)
    {
        Debug.LogError("ENEMY_AUDIT FAIL " + reason);
        SessionState.SetBool(RunningKey, false);
        EditorApplication.Exit(1);
    }
}
