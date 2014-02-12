using System;
using System.Collections.Generic;

namespace AlTypes
{
	public struct PhonemeData
	{
		public String Phoneme;
		public String Mneumonic;
		public String MneumonicTwo;
		public int Id;
		public String Grapheme;
		public String LetterInWord;
	}

	public struct DataPhonemeData
	{
		public String Phoneme;
		public String Grapheme;
		public bool IsTarget;
		public bool IsDummy;
		public int PhonemeId;
		public int LinkingIndex;
	}

	public struct DataWordData
	{
		//from the data word -- e.g. section level
		public bool IsTargetWord;
		public bool IsDummyWord;
		public String LinkingIndex;
		
		//from the word itself
		public String Word;
		public int WordId;
		public bool Nonsense;

	}

	public struct DataSentenceData
	{
		public String Sentence;
		public String LinkingIndex;
	}

	public struct StoryPageData
	{
		public String AnchorPoint;
		public String PageText;
		public String AudioName;
		public String ImageName;
	}
	
	public struct CorrectSentenceInfo
	{
		public String Word;
		public List<String> DummySentences;
		public String TargetSentence;
		public int index;
	}
}