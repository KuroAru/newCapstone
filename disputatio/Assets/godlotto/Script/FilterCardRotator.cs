using UnityEngine;

public class FilterCardRotator : MonoBehaviour
{
    // 현재 카드의 Z축 회전값을 저장할 변수
    private float currentZRotation = 0f;

    // 오른쪽으로 90도 회전시키는 함수 (오른쪽 버튼에 연결할 예정)
    public void RotateRight()
    {
        // 현재 각도에서 -90도를 합니다 (시계 방향 회전).
        currentZRotation -= 90f;
        // Z축을 기준으로 회전값을 적용합니다.
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);
    }

    // 왼쪽으로 90도 회전시키는 함수 (왼쪽 버튼에 연결할 예정)
    public void RotateLeft()
    {
        // 현재 각도에서 +90도를 합니다 (반시계 방향 회전).
        currentZRotation += 90f;
        // Z축을 기준으로 회전값을 적용합니다.
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);
    }
}