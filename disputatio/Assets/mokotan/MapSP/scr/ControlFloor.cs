using UnityEngine;

public class ControlFloor : MonoBehaviour
{
    [Header("층별 오브젝트 설정")]
    [SerializeField] private GameObject floor1Object;
    [SerializeField] private GameObject floor2Object;

    [Header("버튼")]
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;

    // 1층 활성화 함수
    public void ActivateFloor1()
    {
        SetFloorActive(1);
        if (up != null) up.SetActive(true);
        if (down != null) down.SetActive(false);
    }

    // 2층 활성화 함수
    public void ActivateFloor2()
    {
        SetFloorActive(2);
        if (up != null) up.SetActive(false);
        if (down != null) down.SetActive(true);
    }

    // 공통 로직: 선택한 층만 켜고 나머지는 끕니다.
    private void SetFloorActive(int floorNumber)
    {
        if (floor1Object != null) floor1Object.SetActive(floorNumber == 1);
        if (floor2Object != null) floor2Object.SetActive(floorNumber == 2);
        
        Debug.Log($"{floorNumber}층 오브젝트가 활성화되었습니다.");
    }
}
