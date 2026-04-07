using UnityEngine;

namespace XmqqyBackpack
{
    public class PawnManager : MonoBehaviour
    {
        [Header("目标坐标")]
        public Vector2 targetPosition = new Vector2(5, 0);

        [Header("移动速度")]
        public float speed = 3f;

        [Header("方向模型")]
        public GameObject frontObj;   // 正面
        public GameObject backObj;    // 背面
        public GameObject leftObj;    // 左侧
        public GameObject rightObj;   // 右侧 

        [Header("世界引用")]
        public InfiniteWorld infiniteWorld;

        // 当前显示的方向模型
        private GameObject currentActiveObj;
        // 上一帧的移动方向
        private Vector2 lastMoveDir;

        private void Awake()
        {
            if (infiniteWorld == null)
                infiniteWorld = FindObjectOfType<InfiniteWorld>();

            // 初始显示正面
            SetActiveDirection(frontObj);
            lastMoveDir = Vector2.zero;
        }

        private void Update()
        {
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPosition, speed * Time.deltaTime);
            transform.position = newPos;

            // 计算实际移动方向（归一化）
            Vector2 moveDelta = newPos - currentPos;
            Vector2 moveDir = moveDelta.magnitude > 0.01f ? moveDelta.normalized : Vector2.zero;

            // 根据移动方向更新模型显示（水平优先）
            if (moveDir != Vector2.zero)
            {
                if (Mathf.Abs(moveDir.x) > 0.01f) // 水平移动
                {
                    if (moveDir.x > 0)
                        SetActiveDirection(rightObj);
                    else if (moveDir.x < 0)
                        SetActiveDirection(leftObj);
                }
                else if (Mathf.Abs(moveDir.y) > 0.01f) // 垂直移动
                {
                    if (moveDir.y > 0)
                        SetActiveDirection(backObj);   // 向上 → 背面
                    else if (moveDir.y < 0)
                        SetActiveDirection(frontObj);  // 向下 → 正面
                }
                lastMoveDir = moveDir;
            }
            // 如果 moveDir == Vector2.zero，不改变当前显示（保持上次移动方向）

            // 更新世界
            if (infiniteWorld != null)
            {
                Vector3 posForWorld = new Vector3(transform.position.x, 0, transform.position.y);
                infiniteWorld.SetPlayerPosition(posForWorld);
            }
        }

        /// <summary>
        /// 外部调用：移动结束，将角色重置为正面显示。
        /// </summary>
        public void OnMoveEnd()
        {
            SetActiveDirection(frontObj);
        }

        /// <summary>
        /// 设置显示指定的方向模型，隐藏其他三个。
        /// </summary>
        private void SetActiveDirection(GameObject activeObj)
        {
            if (activeObj == null) return;
            if (currentActiveObj == activeObj) return;

            frontObj?.SetActive(activeObj == frontObj);
            backObj?.SetActive(activeObj == backObj);
            leftObj?.SetActive(activeObj == leftObj);
            rightObj?.SetActive(activeObj == rightObj);

            currentActiveObj = activeObj;
        }
    }
}