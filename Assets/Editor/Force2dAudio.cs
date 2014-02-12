using UnityEngine;
using System.Collections;

using UnityEditor;

public class Force2dAudio  : AssetPostprocessor {
    void OnPreprocessAudio () {
       AudioImporter importer = (AudioImporter) assetImporter;
       importer.threeD = false;
       importer.loadType=AudioImporterLoadType.StreamFromDisc;
       importer.format=AudioImporterFormat.Compressed;
    }
}
