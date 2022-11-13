using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MovingSpike : _Object
{
    public bool isAttack = true;
    [SerializeField] private float delTime = 0f, upTime = 1f, downTime = 1f;

    private Vector3 startPos;
    private Vector3 moveDis, moveDis_s;

    private void Awake()
    {
        startPos = transform.position;
        moveDis = transform.up * 0.38f;
        moveDis_s = moveDis / 5;

        StartCoroutine(RunSpikeMoving());
    }

    private void OnEnable()
    {
        StartCoroutine(RunSpikeMoving());
    }

    IEnumerator RunSpikeMoving()     //moving cycle
    {
        transform.position = startPos -= moveDis;

        yield return new WaitForSeconds(delTime + 1.5f);

        while (isAttack)
        {
            for(int i = 0; i < 5; i++)
            {
                transform.position += moveDis_s;
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(upTime);

            for (int i = 0; i < 5; i++)
            {
                transform.position -= moveDis_s;
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(downTime);
        }
    }
}
