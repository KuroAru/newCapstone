using UnityEngine;
using System.Collections;

public class SpriteDissolver : MonoBehaviour
{
    private SpriteRenderer sr;
    public Material dissolveMat;
    
    [Header("설정")]
    public float dissolveDuration = 1.5f; // 사라지는 시간
    
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // 인스펙터에서 넣은 메테리얼을 복제하여 개별 오브젝트만 변하게 합니다.
        dissolveMat = sr.material; 
    }

    [ContextMenu("테스트 실행")] // 에디터에서 우클릭으로 테스트 가능
    public void StartDissolve()
    {
        StartCoroutine(DissolveRoutine());
    }

    IEnumerator DissolveRoutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            // 0에서 1까지 서서히 증가
            float amount = Mathf.Lerp(0f, 1f, elapsedTime / dissolveDuration);
            
            // 셰이더 그래프에서 만든 변수명(_DissolveAmount)과 일치해야 합니다.
            dissolveMat.SetFloat("_DissolveAmount", amount);
            
            yield return null;
        }

        // 완전히 사라지면 오브젝트 삭제
        Destroy(gameObject);
    }
}