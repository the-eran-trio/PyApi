using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OnBase.Phoenix.M.Core.Services
{
	public class SummaryService
	{

		private static readonly int _baseSentenceCount = 5;
		private static readonly int _idealSentenceLength = 20;

		/// <summary>
		/// List of words not considered key words
		/// </summary>
		private static string[] _blacklistWords = new string[]{
			"", "a", "e", "i", "o", "u", "t", "about", "above",
			"above", "across", "after", "afterwards", "again", "against", "all",
			"almost", "alone", "along", "already", "also", "although", "always",
			"am", "among", "amongst", "amoungst", "amount", "an", "and",
			"another", "any", "anyhow", "anyone", "anything", "anyway",
			"anywhere", "are", "around", "as", "at", "back", "be", "became",
			"because", "become", "becomes", "becoming", "been", "before",
			"beforehand", "behind", "being", "below", "beside", "besides",
			"between", "beyond", "both", "bottom", "but", "by", "call", "can",
			"cannot", "can't", "co", "con", "could", "couldn't", "comment", "de",
			"describe", "detail", "did", "do", "doc", "docx", "dollar",
			"dollars", "done", "down", "due", "during", "document", "documents",
			"each", "eg", "eight", "either", "eleven", "else", "elsewhere",
			"empty", "enough", "etc", "even", "ever", "every", "everyone",
			"everything", "everywhere", "except", "few", "fifteen", "fifty",
			"fill", "find", "fire", "first", "five", "foot", "feet", "for", "former",
			"formerly", "forty", "found", "four", "from", "front", "ft", "full",
			"further", "get", "give", "go", "got", "had", "has", "hasnt",
			"have", "he", "hence", "her", "here", "hereafter", "hereby",
			"herein", "hereupon", "hers", "herself", "him", "himself", "his",
			"how", "however", "hundred", "i", "ie", "if", "in", "inc", "inch", "indeed",
			"into", "is", "it", "its", "it's", "itself", "just", "keep", "last",
			"latter", "latterly", "lb", "lbs", "least", "less", "like", "ltd", "made", "make",
			"many", "may", "me", "meanwhile", "might", "mill", "mine", "more",
			"moreover", "most", "mostly", "move", "much", "must", "my", "myself",
			"name", "namely", "neither", "never", "nevertheless", "new", "next",
			"nine", "no", "nobody", "none", "noone", "nor", "not", "nothing",
			"now", "nowhere", "of", "off", "often", "on", "once", "one", "only",
			"onto", "or", "other", "others", "otherwise", "our", "ours",
			"ourselves", "out", "over", "own", "page", "part", "pdf", "people", "per",
			"perhaps", "please", "pound", "pounds", "put", "rather", "re", "said", "same", "see",
			"seem", "seemed", "seeming", "seems", "several", "she", "should",
			"show", "side", "since", "sincere", "six", "sixty", "so", "some",
			"somehow", "someone", "something", "sometime", "sometimes",
			"somewhere", "sq", "square", "still", "such", "take", "tax", "ten", "than", "that", "the",
			"their", "them", "themselves", "then", "thence", "there",
			"thereafter", "thereby", "therefore", "therein", "thereupon",
			"these", "they", "thickv", "thin", "third", "this", "those",
			"though", "three", "through", "throughout", "thru", "thus", "to",
			"together", "too", "top", "toward", "towards", "twelve", "twenty",
			"two", "un", "under", "until", "up", "upon", "us", "use", "very",
			"via", "want", "was", "we", "well", "were", "what", "whatever",
			"when", "whence", "whenever", "where", "whereafter", "whereas",
			"whereby", "wherein", "whereupon", "wherever", "whether", "which",
			"while", "whither", "who", "whoever", "whole", "whom", "whose",
			"why", "will", "with", "within", "without", "would", "yet", "you",
			"your", "yours", "yourself", "yourselves", "the", "reuters", "news",
			"monday", "tuesday", "wednesday", "thursday", "friday", "saturday",
			"sunday", "mon", "tue", "wed", "thu", "fri", "sat", "sun",
			"rappler", "rapplercom", "inquirer", "yahoo", "home", "sports",
			"sa", "says", "said", "tweet", "pm", "home", "homepage",
			"sports", "section", "newsinfo", "stories", "story", "photo",
			 "na", "ng", "ang", "year", "years", "percent",
			"time", "january", "february", "march", "april", "may", "june", "july",
			"august", "september", "october", "november", "december", "high", "medium",
			"low", "none", "link"};

		/// <summary>
		/// These are considered for the abbreviation filtering.
		/// They are any 2 letter word that can end a sentence and are NOT an abbreviation.
		/// </summary>
		private static string[] _exceptionWords = new string[] { "it", "to", "at", "is", "as" };

		public IEnumerable<string> SummarizeFullText(string title, string text)
		{
			// Split the text into sentences
			var sentences = splitSentences(text);

			// If there are only a few sentences, we will return that
			if (sentences.Count() <= _baseSentenceCount)
			{
				return sentences;
			}

			// Figure out key words (and their scores) from the text, ignoring blacklisted words
			var keywords = getKeywordsWithScore(sentences);

			// Split the title text into separate words that can be used as keywords 
			var titleKeywords = splitWordsExcludingBlacklistWords(title);

			// Otherwise, we will score the sentences and use the top ones
			var ranks = scoreText(sentences, keywords, titleKeywords).ToList();
			ranks.Sort((l, r) => r.Value.CompareTo(l.Value));

			return ranks.Take(determineTotalSentences(sentences.Count())).Select(k => k.Key);
		}

		/// <summary>
		/// Increases the number of summary sentences based on the length of the original text,
		/// up to a maximum of 10
		/// </summary>
		private int determineTotalSentences(int sentenceCount)
		{
			var adjustedCount = _baseSentenceCount + sentenceCount / 100;
			return adjustedCount > 10 ? 10 : adjustedCount;
		}

		private string[] splitSentences(string text)
		{
			var sb = new StringBuilder();
			var sentences = new List<string>();

			var sentenceExceptions = new char[] { '/', '`', '’', '-', '\'' };
			var topLevelDomains = new string[] { "com", "net", "gov", "org" };

			/* Custom regex that tries to play nice with FullTextSearch.
			 * Guesses at where periods are and then where spaces should be, based on the formatting.
			 * If a mistake is made here, a fix is attempted when building up sentences.
			 */
			var customText = Regex.Replace(Regex.Replace(text, @"(((\\)*\r(\\)*\n)((\\)*\r(\\)*\n)+)", "."),
				@"((\\)*\r(\\)*\n)|( ( )+)", " ").Trim(new char[] { '\\' });

			foreach (var c in customText)
			{
				if ((c >= 32 && c <= 127 && c != '.') || sentenceExceptions.Contains(c))
				{
					if (c == ' ' && topLevelDomains.Contains(sb.ToString().ToLower()))
					{
						// We found a Top-Level Domain
						var lastSentence = sentences.Last();
						sentences.RemoveAt(sentences.Count - 1);

						sb.Insert(0, lastSentence + ".");
					}
					sb.Append(c);
				}
				else if (sb.Length > 2)
				{
					// Work-around for abbreviations
					var shortWordCandidate = sb.ToString().Substring(sb.Length - 2, 2);

					if (c == '.' && !_exceptionWords.Contains(shortWordCandidate) && (sb.ToString()[sb.Length - 3] == ' ' && shortWordCandidate.All(letter => Char.IsLetter(letter)) || (shortWordCandidate.Contains(' ') && Char.IsLetter(shortWordCandidate[1]))))
					{
						// Period found, but this isn't a complete sentence yet
						// Found either an abbreviation or a middle initial
						sb.Append(c);
					}
					else
					{
						// Sentence is complete, add it to the sentences and clear the builder
						sb.Append(c);
						sentences.Add(sb.ToString());
						sb.Clear();
					}
				}
			}
			return sentences.ToArray();
		}

		/// <summary>
		/// Gets an array of the top 20 most important words from a text.  Ignores any blacklisted words.
		/// </summary>
		private Dictionary<string, double> getKeywordsWithScore(string[] sentences)
		{
			var textBuilder = new StringBuilder();

			foreach (var sentence in sentences)
			{
				textBuilder.Append(sentence + " ");
			}

			var text = textBuilder.ToString();

			var keywordCandidates = splitWordsExcludingBlacklistWords(text);

			var wordCount = keywordCandidates.Count();
			var wordDictionary = new Dictionary<string, int>();

			foreach (var word in keywordCandidates)
			{
				if (word.All(c => Char.IsLetter(c) || Char.IsSymbol(c)))
				{
					if (wordDictionary.TryGetValue(word, out var count))
					{
						wordDictionary[word]++;
					}
					else
					{
						wordDictionary.Add(word, 1);
					}
				}
			}

			var wordList = wordDictionary.ToList();

			wordList.Sort((l, r) => r.Value.CompareTo(l.Value));

			// Retrieve the top 20 highest occurring words from the text
			var keywordsWithScore = new Dictionary<string, double>(20);

			foreach (var wordWithCount in wordList.Take(20))
			{
				// This score is incorporated into the final scoring
				keywordsWithScore.Add(wordWithCount.Key, (wordWithCount.Value * 1.0 / wordCount) * 2.0);
			}

			return keywordsWithScore;
		}

		/// <summary>
		/// Splits the sentence into separate, lowercase words without blacklisted words 
		/// </summary>
		private IEnumerable<string> splitWordsExcludingBlacklistWords(string words)
		{
			return words.ToLower().Split(new char[] { '_', '.', '-', '*', '>', '<', ' ' }).Where(x => !_blacklistWords.Contains(x)).ToArray();
		}

		private Dictionary<string, double> scoreText(IEnumerable<string> sentences, Dictionary<string, double> keywords, IEnumerable<string> titleWords)
		{
			var lengthOfText = sentences.Sum(x => x.Length);
			var ranks = new Dictionary<string, double>();

			int i = 1;
			foreach (var sentence in sentences)
			{
				var words = splitWordsExcludingBlacklistWords(sentence);
				var titleScore = scoreFromTitle(titleWords, words);
				var lengthScore = scoreFromLength(words.Count());
				var positionScore = scoreFromPosition(i, sentences.Count());
				var keywordFrequencyScore = scoreFromKeywordFrequency(words, keywords);

				var totalScore = (titleScore * 1.5 + keywordFrequencyScore * 2.0 + lengthScore * 0.5 + positionScore * 1.0) / 4.0;

				if (!ranks.ContainsKey(sentence))
				{
					ranks.Add(sentence, totalScore);
				}
				i++;
			}

			return ranks;
		}

		private double scoreFromPosition(int i, int length)
		{
			var normalized = i * 1.0 / length;
			if (normalized > 1.0)
			{
				return 0.0;
			}
			else if (normalized > 0.9)
			{
				return 0.15;
			}
			else if (normalized > 0.8)
			{
				return 0.04;
			}
			else if (normalized > 0.7)
			{
				return 0.04;
			}
			else if (normalized > 0.6)
			{
				return 0.06;
			}
			else if (normalized > 0.5)
			{
				return 0.04;
			}
			else if (normalized > 0.4)
			{
				return 0.05;
			}
			else if (normalized > 0.3)
			{
				return 0.08;
			}
			else if (normalized > 0.2)
			{
				return 0.14;
			}
			else if (normalized > 0.1)
			{
				return 0.23;
			}
			else if (normalized > 0)
			{
				return 0.17;
			}
			return 0.0;
		}

		private double scoreFromLength(int numberOfWords)
		{
			return 1 - (Math.Abs((float) (_idealSentenceLength - numberOfWords)) / _idealSentenceLength);
		}

		private double scoreFromTitle(IEnumerable<string> titleWords, IEnumerable<string> words)
		{
			var count = 0;

			foreach (var word in words)
			{
				if (titleWords.Contains(word))
				{
					count++;
				}
			}

			return titleWords.Count() > 0 ? (double) count / titleWords.Count() : 0.0;
		}

		private double scoreFromKeywordFrequency(IEnumerable<string> words, Dictionary<string, double> keywords)
		{
			return (DBS(words, keywords) + SBS(words, keywords)) / 2.0 * 10.0;
		}

		/// <summary>
		/// Summation-Based Selection. Scores based on frequency of keywords in a sentence.
		/// </summary>
		private double SBS(IEnumerable<string> words, Dictionary<string, double> keywords)
		{
			if (words.Count() == 0)
			{
				return 0.0;
			}

			double summation = 0.0;

			foreach (var word in words)
			{
				summation += keywords.ContainsKey(word) ? keywords[word] : 0;
			}

			return summation;
		}

		/// <summary>
		/// Density-Based Selection. Scores based on adjacency of keywords in a sentence.
		/// </summary>
		private double DBS(IEnumerable<string> words, Dictionary<string, double> keywords)
		{
			if (words.Count() == 0)
			{
				return 0.0;
			}

			// Determine how many words in the sentence are a keyword
			var overlap = words.Intersect(keywords.Keys).Count() + 1;
			var summation = 0.0;

			// Represents adjacent words in the sentence
			var firstWord = new KeyValuePair<int, double>();
			var secondWord = new KeyValuePair<int, double>();

			var wordsArray = words.ToArray();

			for (int i = 0; i < wordsArray.Length; i++)
			{
				if (keywords.ContainsKey(wordsArray[i]))
				{
					var score = keywords[wordsArray[i]];

					if (firstWord.Key == 0)
					{
						firstWord = new KeyValuePair<int, double>(i, score);
					}
					else
					{
						secondWord = firstWord;
						firstWord = new KeyValuePair<int, double>(i, score);

						// Adjacent score divided by the square of their distance from each other
						summation += (firstWord.Value * secondWord.Value) / Math.Pow((firstWord.Key - secondWord.Key), 2);
					}

				}
			}

			return summation / (overlap * (overlap + 1));
		}

	}
}

