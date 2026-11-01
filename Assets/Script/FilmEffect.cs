using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FilmEffectController : MonoBehaviour
{
    [Header("基本画像")]
    [SerializeField] private Image background;
    [SerializeField] private Image filmGrain;

    [Header("ノイズ設定")]
    [SerializeField] private int grainWidth = 512;
    [SerializeField] private int grainHeight = 288;
    [SerializeField] private float grainUpdateInterval = 0.06f;
    [SerializeField] private byte grainMaxAlpha = 45;

    [Header("フィルム傷")]
    [SerializeField] private float scratchMinInterval = 0.3f;
    [SerializeField] private float scratchMaxInterval = 3.0f;
    [SerializeField] private float scratchVisibleTime = 0.12f;

    [Header("埃")]
    [SerializeField] private float dustMinInterval = 0.1f;
    [SerializeField] private float dustMaxInterval = 0.8f;
    [SerializeField] private float dustVisibleTime = 0.15f;

    [Header("映像の揺れ")]
    [SerializeField] private float jitterAmount = 1.2f;
    [SerializeField] private float jitterSpeed = 8f;

    private class FilmElement
    {
        public Image image;
        public Color originalColor;
        public RectTransform rect;
        public Vector2 originalPosition;
        public bool active;
    }

    private readonly List<FilmElement> scratches = new();
    private readonly List<FilmElement> dusts = new();

    private Texture2D grainTexture;
    private Sprite grainSprite;

    private RectTransform backgroundRect;
    private RectTransform grainRect;

    private Vector2 backgroundOriginalPosition;
    private Vector2 grainOriginalPosition;

    private float grainTimer;
    private float jitterTime;

    private void Start()
    {
        FindImages();
        CreateGrainTexture();
        InitializeElements();
        StartCoroutine(ScratchRoutine());
        StartCoroutine(DustRoutine());
    }

    private void FindImages()
    {
        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (Image img in images)
        {
            if (img.name == "backscene")
            {
                background = img;
            }
            else if (img.name == "FilmGrain")
            {
                filmGrain = img;
            }
            else if (img.name.StartsWith("Scratch"))
            {
                AddElement(img, scratches);
            }
            else if (img.name.StartsWith("Dust"))
            {
                AddElement(img, dusts);
            }
        }

        if (background != null)
        {
            backgroundRect = background.rectTransform;
            backgroundOriginalPosition =
                backgroundRect.anchoredPosition;
        }

        if (filmGrain != null)
        {
            grainRect = filmGrain.rectTransform;
            grainOriginalPosition =
                grainRect.anchoredPosition;
        }
    }

    private void AddElement(Image img, List<FilmElement> list)
    {
        FilmElement element = new FilmElement();

        element.image = img;
        element.rect = img.rectTransform;
        element.originalColor = img.color;
        element.originalPosition = img.rectTransform.anchoredPosition;
        element.active = false;

        Color color = img.color;
        color.a = 0f;
        img.color = color;

        img.raycastTarget = false;

        list.Add(element);
    }

    private void CreateGrainTexture()
    {
        if (filmGrain == null)
        {
            Debug.LogError("FilmGrainが見つかりません。");
            return;
        }

        grainTexture = new Texture2D(
            grainWidth,
            grainHeight,
            TextureFormat.RGBA32,
            false
        );

        grainTexture.wrapMode = TextureWrapMode.Repeat;
        grainTexture.filterMode = FilterMode.Point;

        grainSprite = Sprite.Create(
            grainTexture,
            new Rect(0, 0, grainWidth, grainHeight),
            new Vector2(0.5f, 0.5f)
        );

        filmGrain.sprite = grainSprite;
        filmGrain.color = Color.white;
        filmGrain.raycastTarget = false;

        UpdateGrain();
    }

    private void UpdateGrain()
    {
        if (grainTexture == null)
            return;

        Color32[] pixels =
            new Color32[grainWidth * grainHeight];

        for (int i = 0; i < pixels.Length; i++)
        {
            byte alpha = (byte)Random.Range(
                0,
                grainMaxAlpha + 1
            );

            pixels[i] = new Color32(
                255, 255, 255, alpha
            );
        }

        grainTexture.SetPixels32(pixels);
        grainTexture.Apply(false);
    }

    private void InitializeElements()
    {
        foreach (FilmElement element in scratches)
        {
            element.active = false;
        }

        foreach (FilmElement element in dusts)
        {
            element.active = false;
        }
    }

    private void Update()
    {
        grainTimer += Time.deltaTime;

        if (grainTimer >= grainUpdateInterval)
        {
            grainTimer = 0f;
            UpdateGrain();
        }

        UpdateJitter();
    }

    private void UpdateJitter()
    {
        jitterTime += Time.deltaTime * jitterSpeed;

        float x = Mathf.PerlinNoise(jitterTime, 0f) - 0.5f;
        float y = Mathf.PerlinNoise(0f, jitterTime) - 0.5f;

        Vector2 offset = new Vector2(x, y) * jitterAmount;

        if (backgroundRect != null)
        {
            backgroundRect.anchoredPosition =
                backgroundOriginalPosition + offset;
        }

        if (grainRect != null)
        {
            grainRect.anchoredPosition =
                grainOriginalPosition + offset;
        }
    }

    private IEnumerator ScratchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(
                    scratchMinInterval,
                    scratchMaxInterval
                )
            );

            if (scratches.Count == 0)
                continue;

            FilmElement element =
                scratches[Random.Range(0, scratches.Count)];

            if (element.active)
                continue;

            StartCoroutine(ShowElement(
                element,
                scratchVisibleTime
            ));
        }
    }

    private IEnumerator DustRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(
                    dustMinInterval,
                    dustMaxInterval
                )
            );

            if (dusts.Count == 0)
                continue;

            FilmElement element =
                dusts[Random.Range(0, dusts.Count)];

            if (element.active)
                continue;

            MoveDust(element);

            StartCoroutine(ShowElement(
                element,
                dustVisibleTime
            ));
        }
    }

    private void MoveDust(FilmElement element)
    {
        RectTransform parent =
            element.rect.parent as RectTransform;

        if (parent == null)
            return;

        // Canvas全体の範囲を基準にする
        RectTransform canvasRect =
            GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        float width = canvasRect.rect.width;
        float height = canvasRect.rect.height;

        Vector2 canvasPosition = new Vector2(
            Random.Range(-width / 2f, width / 2f),
            Random.Range(-height / 2f, height / 2f)
        );

        Vector3 worldPosition =
            canvasRect.TransformPoint(canvasPosition);

        Vector3 localPosition =
            parent.InverseTransformPoint(worldPosition);

        element.rect.anchoredPosition = localPosition;
    }

    private IEnumerator ShowElement(
        FilmElement element,
        float duration
    )
    {
        element.active = true;

        Color color = element.originalColor;
        color.a = element.originalColor.a;

        element.image.color = color;

        yield return new WaitForSeconds(duration);

        color.a = 0f;
        element.image.color = color;

        element.active = false;
    }

    private void OnDestroy()
    {
        if (grainSprite != null)
            Destroy(grainSprite);

        if (grainTexture != null)
            Destroy(grainTexture);
    }
}