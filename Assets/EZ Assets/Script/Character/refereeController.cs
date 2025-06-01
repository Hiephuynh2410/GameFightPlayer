using UnityEngine;

public class RefereeController : MonoBehaviour
{
    public Transform playerTransform;  // Gán Player ở Inspector hoặc tìm trong Start
    public Transform botTransform;     // Gán Bot ở Inspector hoặc tìm trong Start

    public float moveSpeed = 5f;

    // Giới hạn vùng di chuyển (boxing ring)
    public Vector3 ringCenter = Vector3.zero;
    public float ringRadius = 7f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody component found on " + gameObject.name);
        }
        else
        {
            rb.freezeRotation = true;
        }

        // Nếu chưa gán transform Player hoặc Bot, bạn có thể tìm như sau (ví dụ dùng tag)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        if (botTransform == null)
        {
            GameObject bot = GameObject.FindGameObjectWithTag("Bot");
            if (bot != null)
                botTransform = bot.transform;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || playerTransform == null || botTransform == null) return;

        // Tính vị trí trung điểm giữa Player và Bot
        Vector3 midpoint = (playerTransform.position + botTransform.position) / 2f;

        // Giữ trọng tài di chuyển trên mặt phẳng ngang (không thay đổi y)
        Vector3 targetPos = new Vector3(midpoint.x, rb.position.y, midpoint.z);

        Vector3 dirToMidpoint = targetPos - rb.position;
        float distanceToMidpoint = dirToMidpoint.magnitude;

        float keepDistance = 0.05f; // 5cm

        if (distanceToMidpoint > keepDistance)
        {
            // Tính vị trí cách midpoint đúng 5cm (giữ khoảng cách)
            Vector3 desiredPos = targetPos - dirToMidpoint.normalized * keepDistance;

            // Giới hạn vị trí trong ring
            Vector3 offsetFromCenter = desiredPos - ringCenter;
            if (offsetFromCenter.magnitude > ringRadius)
            {
                desiredPos = ringCenter + offsetFromCenter.normalized * ringRadius;
            }

            // Di chuyển trọng tài mượt mà về vị trí desiredPos
            Vector3 newPos = Vector3.MoveTowards(rb.position, desiredPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else
        {
            // Khoảng cách đủ gần, đứng yên
        }
    }

}
