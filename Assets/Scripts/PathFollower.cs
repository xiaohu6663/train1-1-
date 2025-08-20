using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathFollower : MonoBehaviour
{
    public List<Transform> waypoints;
    public float minSpeed = 4f;
    public float maxSpeed = 7f;
    public bool loop = true;
    public bool orientAlongPath = true;
    // 仅绕Y轴朝向（保持竖直，不随上下坡俯仰）
    public bool yawOnly = true;
    public float arriveThreshold = 0.2f;
    public bool startFromNearestNext = true;
    public bool startFromFirst = false;
    public bool pingPong = false; // 往返模式：到最后一个点后返回到第一个点
    public bool useSegmentDuration = true; // 根据路段长度自动计算速度，保证每段用时相近
    public float minSegmentDuration = 6f;
    public float maxSegmentDuration = 10f;
    [Range(0.1f, 10f)] public float segmentSpeedScale = 1f; // 段内速度缩放，可在运行时调整
    public List<float> segmentSpeedScales = new List<float>(); // 按段缩放，index i 代表 i->(i+1)
    private int currentWaypoint = 0;
    private float currentSpeed;
    private Vector3 initialPosition;
    private int direction = 1; // 1 向前，-1 向后（用于 ping-pong）
    public bool debugLogs = false;
    private int lastLegTargetIndex = -1; // 记录已计算速度的目标索引
    
    void Start()
    {
        float minS = Mathf.Max(0.01f, Mathf.Min(minSpeed, maxSpeed));
        float maxS = Mathf.Max(minS, Mathf.Max(minSpeed, maxSpeed));
        currentSpeed = Random.Range(minS, maxS);
        if (waypoints != null)
        {
            // 移除空引用与重复位置的连续点
            waypoints.RemoveAll(w => w == null);
        }
        initialPosition = transform.position;

        // 若强制从第一个开始
        if (startFromFirst && waypoints != null && waypoints.Count >= 1)
        {
            currentWaypoint = 0;
            direction = 1;
        }
        // 否则从距离最近的路点的“下一个”开始，避免起点重叠导致原地等待
        else if (startFromNearestNext && waypoints != null && waypoints.Count >= 2)
        {
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < waypoints.Count; i++)
            {
                float d = Vector3.SqrMagnitude(waypoints[i].position - transform.position);
                if (d < best)
                {
                    best = d;
                    nearest = i;
                }
            }
            currentWaypoint = (nearest + 1) % waypoints.Count;
            direction = 1;
        }

        if (debugLogs)
        {
            Debug.Log($"[PathFollower] Init on {name}: waypoints={waypoints?.Count ?? 0}, startIndex={currentWaypoint}, speed={currentSpeed:F2}, pingPong={pingPong}");
        }
    }
    
    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // 特判：只有一个路点，则在初始位置与该路点之间往返
        if (waypoints.Count == 1)
        {
            Vector3 targetPos = (currentWaypoint % 2 == 0) ? waypoints[0].position : initialPosition;
            if (useSegmentDuration && lastLegTargetIndex != currentWaypoint)
            {
                currentSpeed = ComputeLegSpeed(transform.position, targetPos);
                lastLegTargetIndex = currentWaypoint;
            }
            float effectiveSpeed = currentSpeed * Mathf.Max(0.01f, segmentSpeedScale);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveSpeed * Time.deltaTime);
            if (orientAlongPath)
            {
                Vector3 dir = targetPos - transform.position;
                if (yawOnly) dir.y = 0f; // 仅水平朝向
                if (dir != Vector3.zero)
                {
                    Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, look, 5f * Time.deltaTime);
                }
            }
            if (Vector3.Distance(transform.position, targetPos) <= arriveThreshold)
            {
                currentWaypoint++;
            }
            return;
        }

        // 处理可能的零长度段：连续相同位置的路点直接跳过
        int safety = 0;
        while (safety++ < waypoints.Count)
        {
            Transform tgt = waypoints[currentWaypoint];
            if (Vector3.Distance(transform.position, tgt.position) <= arriveThreshold)
            {
                int prev = currentWaypoint;
                if (pingPong)
                {
                    // 到达端点则反向
                    if (currentWaypoint == waypoints.Count - 1)
                    {
                        direction = -1;
                    }
                    else if (currentWaypoint == 0)
                    {
                        direction = 1;
                    }

                    int nextIndex = currentWaypoint + direction;
                    if (nextIndex >= 0 && nextIndex < waypoints.Count)
                    {
                        currentWaypoint = nextIndex;
                    }
                }
                else
                {
                    if (currentWaypoint + 1 < waypoints.Count)
                        currentWaypoint++;
                    else if (loop)
                        currentWaypoint = 0;
                    else
                        break;
                }
                if (debugLogs && prev != currentWaypoint)
                {
                    Debug.Log($"[PathFollower] Arrived index {prev}, next index {currentWaypoint}, dir={direction}");
                }
            }
            else break;
        }

        Transform target = waypoints[currentWaypoint];
        // 仅在进入新段时计算一次速度，避免段内抖动
        if (useSegmentDuration && lastLegTargetIndex != currentWaypoint)
        {
            currentSpeed = ComputeLegSpeed(transform.position, target.position);
            currentSpeed *= GetLegScale(currentWaypoint);
            lastLegTargetIndex = currentWaypoint;
        }
        float effSpeed = currentSpeed * Mathf.Max(0.01f, segmentSpeedScale);
        transform.position = Vector3.MoveTowards(transform.position, target.position, effSpeed * Time.deltaTime);
        
        // 朝向目标点
        if (orientAlongPath && (target.position - transform.position != Vector3.zero))
        {
            Vector3 dir = target.position - transform.position;
            if (yawOnly) dir.y = 0f; // 仅水平朝向
            if (dir != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 5f * Time.deltaTime);
            }
        }
        
        // 检查是否到达路径点
        if (Vector3.Distance(transform.position, target.position) <= arriveThreshold)
        {
            int prev = currentWaypoint;
            if (pingPong)
            {
                if (currentWaypoint == waypoints.Count - 1)
                {
                    direction = -1;
                }
                else if (currentWaypoint == 0)
                {
                    direction = 1;
                }
                int nextIndex = currentWaypoint + direction;
                if (nextIndex >= 0 && nextIndex < waypoints.Count)
                {
                    // 进入下一个 leg：current -> next
                    int prevIdx = currentWaypoint;
                    currentWaypoint = nextIndex;
                    if (useSegmentDuration)
                    {
                        currentSpeed = ComputeLegSpeed(waypoints[prevIdx].position, waypoints[currentWaypoint].position);
                        currentSpeed *= GetLegScale(currentWaypoint);
                        lastLegTargetIndex = currentWaypoint;
                    }
                }
            }
            else
            {
                if (currentWaypoint + 1 < waypoints.Count)
                {
                    int prevIdx = currentWaypoint;
                    currentWaypoint++;
                    if (useSegmentDuration)
                    {
                        currentSpeed = ComputeLegSpeed(waypoints[prevIdx].position, waypoints[currentWaypoint].position);
                        currentSpeed *= GetLegScale(currentWaypoint);
                        lastLegTargetIndex = currentWaypoint;
                    }
                }
                else
                {
                    if (loop)
                    {
                        int prevIdx = currentWaypoint;
                        currentWaypoint = 0;
                        if (useSegmentDuration)
                        {
                            currentSpeed = ComputeLegSpeed(waypoints[prevIdx].position, waypoints[currentWaypoint].position);
                            currentSpeed *= GetLegScale(currentWaypoint);
                            lastLegTargetIndex = currentWaypoint;
                        }
                    }
                }
            }
            if (debugLogs && prev != currentWaypoint)
            {
                Debug.Log($"[PathFollower] Reached target, move index {prev} -> {currentWaypoint}, dir={direction}");
            }
            if (debugLogs && useSegmentDuration)
            {
                Debug.Log($"[PathFollower] Next segment base speed {currentSpeed/Mathf.Max(0.01f, GetLegScale(currentWaypoint)):F2}, legScale={GetLegScale(currentWaypoint):F2}, globalScale={segmentSpeedScale:F2}, effective={currentSpeed * segmentSpeedScale:F2}");
            }
        }

    }

    private float GetLegScale(int legIndex)
    {
        if (segmentSpeedScales == null || segmentSpeedScales.Count == 0) return 1f;
        if (legIndex < 0) return 1f;
        if (legIndex >= segmentSpeedScales.Count)
        {
            // 如果数量不足，使用最后一个的值
            return segmentSpeedScales[segmentSpeedScales.Count - 1];
        }
        float s = segmentSpeedScales[legIndex];
        return Mathf.Max(0.01f, s);
    }

    private float ComputeLegSpeed(Vector3 from, Vector3 to)
    {
        if (!useSegmentDuration)
        {
            // 退化为区间随机速度
            float minS = Mathf.Max(0.01f, Mathf.Min(minSpeed, maxSpeed));
            float maxS = Mathf.Max(minS, Mathf.Max(minSpeed, maxSpeed));
            return Random.Range(minS, maxS);
        }
        float dist = Vector3.Distance(from, to);
        float dur = Mathf.Clamp(Random.Range(minSegmentDuration, maxSegmentDuration), 0.1f, 9999f);
        return Mathf.Max(0.01f, dist / dur);
    }
}