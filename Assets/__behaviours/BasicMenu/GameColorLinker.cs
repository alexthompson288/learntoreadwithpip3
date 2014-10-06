using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;

// TODO: This needs to separate maths games from reading games
public class GameColorLinker : Singleton<GameColorLinker> 
{
    [SerializeField]
    private string m_filename = "/BasicGameColors.csv";

    void Start()
    {   
        string path = Application.streamingAssetsPath + m_filename;
        
        using (CsvFileReader reader = new CsvFileReader(path))
        {
            string programme = "";

            CsvRow row = new CsvRow();
            while (reader.ReadRow(row))
            {
                string[] cells = row.LineText.Split(',');
                
                if(cells.Length > 0)
                {
                    if(cells[0] == ProgrammeInfo.basicMaths || cells[0] == ProgrammeInfo.basicReading)
                    {
                        programme = cells[0];
                    }
                    else
                    {
                        try
                        {
                            ColorInfo.PipColor pipColor = ColorInfo.GetPipColor(cells[0]);
                            
                            List<string> gameNames = new List<string>();
                            
                            for(int i = 1; i < cells.Length; ++i)
                            {
                                if(!System.String.IsNullOrEmpty(cells[i]))
                                {
                                    gameNames.Add(cells[i]);
                                }
                            }
                            
                            m_links.Add(new GameColorLink(programme, pipColor, gameNames));
                        }
                        catch
                        {
                            D.Log("Failed to link GameColors for: " + cells[0]);
                        }
                    }
                }
            }
        }

//        foreach (GameColorLink link in m_links)
//        {
//            D.Log(link.m_programme + " - " + link.m_pipColor);
//            foreach(string s in link.m_gameNames)
//            {
//                D.Log(s);
//            }
//        }
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
