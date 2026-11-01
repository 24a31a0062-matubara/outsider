using UnityEngine;
using System.Collections.Generic;

public class FilmDamageMotion : MonoBehaviour
{
    [Header("ƒtƒBƒ‹ƒ€‚Ì")]
    [SerializeField] private float scratchSpeed = 8f;
    [SerializeField] private float scratchJitter = 1f;

    [Header("šº")]
    [SerializeField] private float dustSpeed = 12f;
    [SerializeField] private float dustJitter = 3f;

    [Header("ˆÚ“®”ÍˆÍ")]
    [SerializeField] private float screenHeight = 1080f;

    private class FilmParticle
    {
        public RectTransform rect;
        public float speed;
        public float phase;
        public float jitter;
        public bool isDust;
        public float originalX;
        public float currentY;
    }

    private readonly List<FilmParticle> particles =
        new List<FilmParticle>();

    private void Start()
    {
        InitializeParticles();
    }

    private void InitializeParticles()
    {
        particles.Clear();

        foreach (RectTransform child in
                 GetComponentsInChildren<RectTransform>())
        {
            if (child == transform)
                continue;

            bool isDust = child.name.StartsWith("Dust");
            bool isScratch = child.name.StartsWith("Scratch");

            if (!isDust && !isScratch)
                continue;

            FilmParticle particle = new FilmParticle();

            particle.rect = child;
            particle.isDust = isDust;

            particle.speed = isDust
                ? Random.Range(dustSpeed * 0.5f, dustSpeed * 1.5f)
                : Random.Range(scratchSpeed * 0.5f, scratchSpeed * 1.5f);

            particle.jitter = isDust
                ? dustJitter
                : scratchJitter;

            particle.phase = Random.Range(0f, 100f);
            particle.originalX = child.anchoredPosition.x;
            particle.currentY = child.anchoredPosition.y;

            particles.Add(particle);
        }
    }

    private void Update()
    {
        float halfHeight = screenHeight / 2f;

        foreach (FilmParticle particle in particles)
        {
            if (particle.rect == null)
                continue;

            // c•ûŒü‚ÉˆÚ“®
            particle.currentY -= particle.speed * Time.deltaTime;

            // ‰æ–Ê‰º‚Ü‚ÅˆÚ“®‚µ‚½‚çã‚É–ß‚·
            if (particle.currentY < -halfHeight)
            {
                particle.currentY = halfHeight;
            }

            // ¶‰E‚É‚í‚¸‚©‚É—h‚ç‚·
            float horizontalMovement =
                Mathf.Sin(Time.time * 0.7f + particle.phase)
                * particle.jitter;

            particle.rect.anchoredPosition = new Vector2(
                particle.originalX + horizontalMovement,
                particle.currentY
            );
        }
    }
}