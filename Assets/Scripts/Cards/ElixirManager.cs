using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElixirManager : MonoBehaviour
{
    public static ElixirManager Instance;

    [Header("UI")]
    public Slider elixirSlider;
    public JellyElixir jelly;

    [Header("Settings")]
    public int maxElixir = 10;
    public float regenInterval = 2f;
    public int regenAmount = 1;

    private int currentElixir;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentElixir = 5;
        elixirSlider.maxValue = maxElixir;
        elixirSlider.value = currentElixir;

        StartCoroutine(RegenElixir());
    }

    IEnumerator RegenElixir()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (currentElixir < maxElixir)
            {
                currentElixir += regenAmount;
                if (currentElixir > maxElixir)
                    currentElixir = maxElixir;

                elixirSlider.value = currentElixir;
            }
            if (jelly != null && currentElixir != maxElixir)
                jelly.PlayJelly();
        }
    }

    public bool TrySpend(int amount)
    {
        if (currentElixir < amount)
            return false;

        currentElixir -= amount;
        elixirSlider.value = currentElixir;

        if (jelly != null)
            jelly.PlayJelly();
        return true;
    }

    public int GetElixir() => currentElixir;
}
