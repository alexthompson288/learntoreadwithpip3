﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System.Linq;

public class JumbleSentenceCoordinator : GameCoordinator 
{
    [SerializeField]
    private List<Transform> m_locators;
    [SerializeField]
    private GameObject m_wordPrefab;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private Transform m_textPosition;
    [SerializeField]
    private UISprite m_sentenceBackground;
    [SerializeField]
    private GameObject m_sentenceParent;
    [SerializeField]
    private UITexture m_picture;
    [SerializeField]
    private GameObject m_pictureParent;

    List<GameWidget> m_spawnedWords = new List<GameWidget>();
    List<Transform> m_spawnedWordLocations = new List<Transform>();

    List<string> m_remainingWords = new List<string>();

    IEnumerator Start()
    {
        m_pictureParent.transform.localScale = Vector3.zero;
        m_sentenceParent.transform.localScale = Vector3.zero;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_dataPool = DataHelpers.GetCorrectCaptions();

        m_dataPool.RemoveAll(x => DataHelpers.GetCorrectCaptionWords(x).Length > 5);

        m_dataPool.RemoveAll(x => x["good_sentence"] == null || !x["good_sentence"].ToString().Contains(" "));

        ClampTargetScore();

        m_scoreKeeper.SetTargetScore(m_targetScore);


        if (m_dataPool.Count > 0)
        {
            m_startTime = Time.time;
            AskQuestion();
        } 
        else
        {
            StartCoroutine(CompleteGame());
        }
    }

    void AskQuestion()
    {
        m_currentData = GetRandomData();
        m_dataPool.Remove(m_currentData);

        m_remainingWords = DataHelpers.GetCorrectCaptionWords(m_currentData).ToList();
        m_remainingWords.RemoveAll(x => x == " ");

        List<Transform> locators = new List<Transform>();
        locators.AddRange(m_locators);
        CollectionHelpers.Shuffle(locators);

        float length = 0;

        for (int i = 0; i < m_remainingWords.Count && i < locators.Count; ++i)
        {
            GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, locators[i]);

            // Set up GameWidget
            GameWidget wordBehaviour = newWord.GetComponent<GameWidget>() as GameWidget;
            wordBehaviour.SetUp(m_remainingWords[i], true);
            wordBehaviour.AllReleaseInteractions += OnWordClick;
            wordBehaviour.EnableDrag(false);
            m_spawnedWords.Add(wordBehaviour);

            // Instantiate and position locations that words will take in sentence
            GameObject newWordLocation = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, m_textPosition);
            m_spawnedWordLocations.Add(newWordLocation.transform);
            newWordLocation.name = "wordLocation_" + i.ToString();

            float halfLocationWidth = (wordBehaviour.labelWidth / 2) + 30;

            length += halfLocationWidth;

            newWordLocation.transform.localPosition = new Vector3(length, 0, 0);

            length += halfLocationWidth;
        }

        m_textPosition.localPosition = new Vector3(-length / 2, m_textPosition.localPosition.y, m_textPosition.localPosition.z);

        m_sentenceBackground.width = (int)(length + 50);

        float tweenDuration = m_spawnedWords[0].scaleTweenDuration;

        iTween.ScaleTo(m_sentenceParent, Vector3.one, tweenDuration);

        bool hasSetPicture = false;
        if (m_currentData ["correct_image_name"] != null)
        {
            m_picture.mainTexture = DataHelpers.GetPicture("correctcaptions", m_currentData);

            if(m_picture.mainTexture != null)
            {
                hasSetPicture = true;
                iTween.ScaleTo(m_pictureParent, Vector3.one, tweenDuration);           
            }
        }
        m_pictureParent.gameObject.SetActive(hasSetPicture);
    }

    void OnWordClick(GameWidget widget)
    {
        StartCoroutine(OnWordClickCo(widget));
    }

    IEnumerator OnWordClickCo(GameWidget widget)
    {
        if (widget.labelText == m_remainingWords [0])
        {
            if(!m_audioSource.isPlaying)
            {
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE word='" + StringHelpers.Edit(widget.labelText) + "'");
                if(dt.Rows.Count > 0)
                {
                    PlayShortAudio(dt.Rows[0]);
                }
            }

            widget.EnableCollider(false);

            widget.TweenToPos(m_spawnedWordLocations[0].position);

            m_remainingWords.RemoveAt(0);
            m_spawnedWordLocations.RemoveAt(0);

            widget.transform.parent = m_textPosition;

            widget.FadeBackground(true);

            foreach(GameWidget spawnedWidget in m_spawnedWords)
            {
                spawnedWidget.TintWhite();
            }

            if(m_remainingWords.Count == 0)
            {
                yield return new WaitForSeconds(widget.positionTweenDuration + 1);

                float tweenDuration = m_spawnedWords[0].scaleTweenDuration;

                iTween.ScaleTo(m_pictureParent, Vector3.zero, tweenDuration);
                iTween.ScaleTo(m_sentenceParent, Vector3.zero, tweenDuration);

                for(int i = m_spawnedWords.Count - 1; i > -1; --i)
                {
                    m_spawnedWords[i].Off();
                }
                m_spawnedWords.Clear();

                for(int i = m_spawnedWordLocations.Count - 1; i > -1; --i)
                {
                    Destroy(m_spawnedWordLocations[i]);
                }
                m_spawnedWordLocations.Clear();

                WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
                PlayLongAudio();

                yield return new WaitForSeconds(tweenDuration);

                ++m_score;
                m_scoreKeeper.UpdateScore();

                if(m_score < m_targetScore)
                {
                    AskQuestion();
                }
                else
                {
                    StartCoroutine(CompleteGame());
                }
            }
        } 
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_EXHALE");
            widget.TintGray();
            widget.Shake();
        }

        yield break;
    }

    protected override IEnumerator CompleteGame()
    {
        float timeTaken = Time.time - m_startTime;

        float twoStarPerQuestion = 12.5f;
        float threeStarPerQuestion = 8;

        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);
        
        ScoreInfo.Instance.NewScore(timeTaken, m_score, m_targetScore, stars);

        //yield return new WaitForSeconds(1f);

        yield return StartCoroutine(CelebrationCoordinator.Instance.ExplodeLetters());

        GameManager.Instance.CompleteGame();
    }
}
