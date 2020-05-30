using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenTitle : MonoBehaviour
{
    public Text title;
    public Text credit;
    public AnimationCurve alphaAnimation;
    void Start()
    {
        title.color = new Color(title.color.r, title.color.g, title.color.b, 0f);
        credit.color = new Color(credit.color.r, credit.color.g, credit.color.b, 0f);

        StartCoroutine(ScreenTitleAnimation());
    }

    private IEnumerator ScreenTitleAnimation()
    {
        yield return new WaitForSeconds(0.5f);

        for (float i = 0f; i <= 1f; i+=0.1f)
        {
            title.color = new Color(title.color.r, title.color.g, title.color.b, alphaAnimation.Evaluate(i));
            yield return new WaitForSeconds(0.1f);
        }

        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            credit.color = new Color(credit.color.r, credit.color.g, credit.color.b, alphaAnimation.Evaluate(i));
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        for (float i = 1f; i >= 0f; i -= 0.1f)
        {
            title.color = new Color(title.color.r, title.color.g, title.color.b, alphaAnimation.Evaluate(i));
            yield return new WaitForSeconds(0.1f);
        }

        for(float i = 1f; i >= 0f; i -= 0.1f)
        {
            credit.color = new Color(credit.color.r, credit.color.g, credit.color.b, alphaAnimation.Evaluate(i));
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadSceneAsync(1);
    }


}
