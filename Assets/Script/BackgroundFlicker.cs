using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundFlicker : MonoBehaviour
{
    [SerializeField] private Image background;

    [SerializeField] private float minTime = 0.1f;
    [SerializeField] private float maxTime = 0.3f;

    private void Start()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            background.color = Color.white;

            yield return new WaitForSeconds(
                Random.Range(1.5f, 4.0f)
            );

            background.color = Color.black;

            yield return new WaitForSeconds(0.05f);

            background.color = Color.white;

            if (Random.value < 0.4f)
            {
                yield return new WaitForSeconds(0.08f);

                background.color = Color.black;

                yield return new WaitForSeconds(0.04f);

                background.color = Color.white;
            }
        }
    }
}