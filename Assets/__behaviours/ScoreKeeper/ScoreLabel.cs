using UnityEngine;
using System.Collections;

public class ScoreLabel : ScoreKeeper 
{
    [SerializeField]
    private UILabel m_label;

    public override void SetTargetScore(int myTargetScore)
    {
        base.SetTargetScore(myTargetScore);

        SetLabelText();
    }

    public override void UpdateScore(int delta = 1)
    {
        base.UpdateScore(delta);
        D.Log("score: " + m_score);

        SetLabelText();
    }

    void SetLabelText()
    {
        m_label.text = m_targetScore > 0 ? System.String.Format("{0} / {1}", m_score, m_targetScore) : m_score.ToString();
    }

    public override IEnumerator On()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_WAHOO");

        float tweenDuration = 0.3f;
        iTween.ScaleTo(gameObject, transform.localScale * 1.5f, tweenDuration);

        Hashtable moveTweenArgs = new Hashtable();
        moveTweenArgs.Add("position", Vector3.zero);
        moveTweenArgs.Add("islocal", true);

        iTween.MoveTo(gameObject, moveTweenArgs);

        yield return new WaitForSeconds(tweenDuration + 1.5f);
    }
}
