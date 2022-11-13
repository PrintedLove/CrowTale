using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class SummonStone : MonoBehaviour
{
    public Transform shooter, firePoint;
    public LineRenderer lineRenderer;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private GameObject startVFX, endVFX;
    [SerializeField] private Material lazerMat, warrningMat;
    [SerializeField] private float rayDistance = 100f, warnningBoxWidth;
    [SerializeField] private int lazerDamage;

    private GameObject player;
    private Sprite startSpr;
    private Light2D lt;
    private float lazerWidth;
    private List<ParticleSystem> particles = new List<ParticleSystem>();
    private bool isHit;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        startSpr = GetComponent<SpriteRenderer>().sprite;
        lt = GetComponent<Light2D>();
        lazerWidth = lineRenderer.startWidth;
        isHit = false;

        FillList();
    }

    private void Reset()
    {
        StopAllCoroutines();

        isHit = false;
        lineRenderer.enabled = false;
        lt.intensity = 0f;
        GetComponent<Animator>().SetBool("transform", false);
        GetComponent<SpriteRenderer>().sprite = startSpr;

        for (int i = 0; i < particles.Count; i++)
            particles[i].Stop();
    }

    private void Update()
    {
        ShootLazer();
    }

    private void ShootLazer()
    {
        if (Physics2D.Raycast(shooter.position, shooter.transform.right, rayDistance, LayerMask.GetMask("Platform")))
        {
            RaycastHit2D hit = Physics2D.Raycast(shooter.position, shooter.transform.right, rayDistance, LayerMask.GetMask("Platform"));
            DrawLazer(firePoint.position, hit.point);
        }
        else
        {
            DrawLazer(firePoint.position, firePoint.transform.right * rayDistance);
        }

        //set partical system position
        endVFX.transform.position = lineRenderer.GetPosition(1);

        //lazer hit player
        if(isHit && Physics2D.Raycast(shooter.position, shooter.transform.right, rayDistance, LayerMask.GetMask("Player")))
        {
            if (!GameManager.Instance.isPlayerDie && !GameManager.Instance.damageImmune)
            {
                RaycastHit2D hit = Physics2D.Raycast(shooter.position, shooter.transform.right, rayDistance, LayerMask.GetMask("Player"));
                player.GetComponent<PlayerManager>().Knockback(0.5f, hit.point, 0.5f);
                player.GetComponent<PlayerManager>().SoundPlay_GetDamage();
                GameManager.Instance.GetDamage(lazerDamage, 1f);
                Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
            }
        }
    }

    private void DrawLazer(Vector2 startPos, Vector2 endPos)
    {
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private void FillList()
    {
        for(int i = 0; i < startVFX.transform.childCount; i++)
        {
            var ps = startVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if(ps != null)
                particles.Add(ps);
        }

        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            var ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
                particles.Add(ps);
        }
    }

    public void StartLazer()
    {
        lineRenderer.enabled = true;
        StartCoroutine(RunStartLazer());
    }

    public void EndLazer()
    {
        isHit = false;
        lineRenderer.enabled = false;

        for (int i = 0; i < particles.Count; i++)
            particles[i].Stop();
    }

    IEnumerator RunStartLazer()
    {
        lineRenderer.material = warrningMat;
        lineRenderer.startWidth = warnningBoxWidth;
        lineRenderer.startColor = new Color(0.9150943f, 0.1093508f, 0.1093508f, 0.18f);
        lineRenderer.endColor = new Color(0.9150943f, 0.1093508f, 0.1093508f, 0.18f);

        float _length = warnningBoxWidth;
        while (_length > 0)
        {
            yield return new WaitForSeconds(0.01f);
            lineRenderer.startWidth -= warnningBoxWidth / 20f;
            _length -= warnningBoxWidth / 20f;
        }

        yield return new WaitForSeconds(0.25f);

        isHit = true;
        lineRenderer.material = lazerMat;
        lineRenderer.startWidth = lazerWidth;
        lineRenderer.startColor = new Color(0.9607844f, 0.8352942f, 0.7372549f, 0f);
        lineRenderer.endColor = new Color(0.9607844f, 0.8352942f, 0.7372549f, 1f);

        for (int i = 0; i < particles.Count; i++)
            particles[i].Play();
    }

    public void ResetSummonStone()
    {
        transform.localPosition = startPosition;
        Reset();
    }
}
