using UnityEngine;
using System.Collections;
using System.IO;

public static class LoaderHelpers {

    public static AudioClip LoadAudioForWord(string word)
    {
        AudioClip loadedAudio = (AudioClip)Resources.Load("audio/words/" + word);
        if (loadedAudio == null)
        {
            loadedAudio = (AudioClip)Resources.Load("audio/words/alex_" + word);
        }
        return loadedAudio;
    }

    public static AudioClip LoadMnemonic(DataRow phonemeData)
    {
		string audioFilename = string.Format("{0}_{1}",
		                                     phonemeData["phoneme"],
		                                     phonemeData["mneumonic"].ToString().Replace(" ", "_"));
	
        AudioClip loadedAudio = (AudioClip)Resources.Load("audio/benny_mnemonics_master/" + audioFilename);
        if (loadedAudio == null)
        {
            audioFilename = string.Format("alex_mnemonic_{0}_{1}_{2}",
                phonemeData["grapheme"],
                phonemeData["phoneme"],
                phonemeData["mneumonic"].ToString().Replace(" ", "_"));
            loadedAudio = (AudioClip)Resources.Load("audio/benny_mnemonics_master/" + audioFilename);
        }

        return loadedAudio;
    }

    public static T LoadObject<T>(string filename) where T : Object
    {
        string shortFilename = Path.GetFileNameWithoutExtension(filename);

		//Debug.Log("filename: " + filename);
		//Debug.Log("shortFilename: " + shortFilename);
        T r = AssetBundleLoader.Instance.FindAsset<T>(shortFilename);
        if (r == null)
        {
           // Debug.Log(shortFilename + " not found in asset bundles, loading from resources!");

			try // There are some resources of different types with identical names; this causes a bad cast exception if the wrong file type is found first
			{
				r = (T)Resources.Load(shortFilename);

				if(r == null)
				{
					//Debug.Log(shortFilename + " not found in resources short");
					r = (T)Resources.Load(filename);

					if(r == null)
					{
						//Debug.Log(filename + " not found in resources long");
					}
				}
			}
			catch // There are some resources of different types with identical names; this causes a bad cast exception if the wrong file type is found first
			{
				//Debug.Log("LoadObject catch");
			}

            if (r == null)
            {
                //Debug.Log(shortFilename + " not found anywhere!");
            }
			else
			{
				//Debug.Log("Found - r: " + r);
			}
        }
        return r;
    }

}
