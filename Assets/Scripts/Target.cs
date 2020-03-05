using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{

    public Vector3 centerPoint;
    public float radius;
    public float rotationSpeed;

    public ParticleSystem shatterParticles;

    public Text scoreText;

    private Vector3 startPos;
    private int direction;

    private MeshRenderer _renderer;
    private ParticleSystem.MainModule _shatterMain;
    

    // Start is called before the first frame update
    void Start()
    {
        //setup distance from centerpoint
        transform.position = (transform.position - centerPoint).normalized * radius + centerPoint;
        startPos = transform.position;
        shatterParticles.Stop();
        _renderer = GetComponent<MeshRenderer>();
        scoreText.text = "0";
        _shatterMain = shatterParticles.main;
        direction = Random.Range(0, 2) * 2 - 1;  //picks -1 or 1
    }

    // Update is called once per frame
    void Update()
    {
        //rotate around centerpoint
        transform.RotateAround(centerPoint, transform.forward, direction * rotationSpeed * Time.deltaTime);
        var desiredPosition = (transform.position - centerPoint).normalized * radius + centerPoint;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * rotationSpeed);
    }

    private void resetTarget()
    {
        //setup distance from centerpoint
        transform.RotateAround(centerPoint, transform.forward, Random.Range(0, 360));
        direction = Random.Range(0, 2) * 2 - 1;  //picks -1 or 1
        shatterParticles.Stop();
        _renderer.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Arrow"))
        {
            Vector3 arrowPos = other.GetComponent<Arrow>().tip.position;
            float distanceFromCenter = Vector3.Distance(transform.position, arrowPos);

            int score;
            Color col;
            if(distanceFromCenter < 1f)
            {
                score = 100;
                col = Color.red;
            }
            else if (distanceFromCenter < 1.5f)
            {
                score = 50;
                col = Color.yellow;
            }
            else
            {
                score = 25;
                col = Color.white;
            }

            scoreText.text = (int.Parse(scoreText.text) + score).ToString();
            StartCoroutine(ShatterRoutine(col));
        }
    }

    private IEnumerator ShatterRoutine(Color shatterColor)
    {
        _renderer.enabled = false;
        shatterParticles.Play();
        _shatterMain.startColor = shatterColor;
        yield return new WaitForSeconds(2.0f);
        resetTarget();
    }
}
