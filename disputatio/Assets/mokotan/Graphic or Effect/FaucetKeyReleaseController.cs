using System.Collections;
using Fungus;
using UnityEngine;

public class FaucetKeyReleaseController : MonoBehaviour
{
    [SerializeField] private Flowchart targetFlowchart;
    [SerializeField] private string faucetBoolName = "FaucetClicked";
    [SerializeField] private string keySpawnBlockName = "addKey";
    [SerializeField] private float delaySeconds = 1f;

    private bool hasTriggered;

    private void Update()
    {
        if (hasTriggered)
            return;

        Flowchart flowchart = ResolveFlowchart();
        if (flowchart == null)
            return;

        if (!flowchart.GetBooleanVariable(faucetBoolName))
            return;

        hasTriggered = true;
        StartCoroutine(SpawnKeyAfterDelay(flowchart));
    }

    private Flowchart ResolveFlowchart()
    {
        if (targetFlowchart != null)
            return targetFlowchart;

        targetFlowchart = FindFirstObjectByType<Flowchart>();
        return targetFlowchart;
    }

    private IEnumerator SpawnKeyAfterDelay(Flowchart flowchart)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        if (!string.IsNullOrEmpty(keySpawnBlockName))
            flowchart.ExecuteBlock(keySpawnBlockName);
    }
}
