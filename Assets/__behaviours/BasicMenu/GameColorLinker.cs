using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;

// TODO: This needs to separate maths games from reading games
public class GameColorLinker : Singleton<GameColorLinker> 
{
    [SerializeField]
    private string m_filename = "/BasicGameColors.csv";

    void Awake()
    {   
        string path = Application.streamingAssetsPath + m_filename;
        
        using (CsvFileReader reader = new CsvFileReader(path))
        {
            CsvRow row = new CsvRow();
            while (reader.ReadRow(row))
            {
                string[] gameColors = row.LineText.Split(',');
                
                if(gameColors.Length > 1)
                {
                    try
                    {
                        ColorInfo.PipColor pipColor = ColorInfo.GetPipColor(gameColors[0]);

                        List<string> gameNames = new List<string>();

                        for(int i = 1; i < gameColors.Length; ++i)
                        {
                            if(!System.String.IsNullOrEmpty(gameColors[i]))
                            {
                                gameNames.Add(gameColors[i]);
                            }
                        }

                        m_links.Add(new GameColorLink("PlaceholderProgramme", pipColor, gameNames));
                    }
                    catch
                    {
                        D.Log("Failed to link GameColors for: " + gameColors[0]);
                    }
                }
            }
        }

        foreach (GameColorLink link in m_links)
        {
            D.Log(link.m_programme + " - " + link.m_pipColor);
            foreach(string s in link.m_gameNames)
            {
                D.Log(s);
            }
        }
    }

    List<GameColorLink> m_links = new List<GameColorLink>();

    class GameColorLink
    {
        public string m_programme;
        public ColorInfo.PipColor m_pipColor;
        public List<string> m_gameNames = new List<string>();

        public GameColorLink(string myProgramme, ColorInfo.PipColor myPipColor, List<string> myGameNames)
        {
            m_programme = myProgramme;
            m_pipColor = myPipColor;
            m_gameNames = myGameNames;
        }
    }

    public List<string> GetGameNames(ColorInfo.PipColor pipColor)
    {
        GameColorLink link = m_links.Find(x => x.m_programme == GameManager.Instance.programme && x.m_pipColor == pipColor);
        return link != null ? link.m_gameNames : new List<string>();
    }
}
