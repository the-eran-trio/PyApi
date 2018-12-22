blackListedWords = ["", "a", "e", "i", "o", "u", "t", "about", "above",
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
			"low", "none", "link"]
exceptionWords = ["it", "to", "at", "is", "as" ]
_baseSentenceCount = 5;
_minSentenceWordCount = 5;
_idealSentenceWordCount = 20;
_keywordCount = 20;

class scoredSentence:
    def __init__(self, sentence, keywordCandidates = [], score = 0):
        self.sentence = sentence
        self.keywordCandidates = keywordCandidates
        self.score = score

    # public scoredSentence(string sentence)
	# 		{
	# 			Sentence = sentence;
	# 		}


def summarize(txt):
    sentences = splitSentences(txt)

    if sentences.count <= _baseSentenceCount:
        return sentences
    
    keywordDictionary = getKeywordsWithScore(sentences);

def splitSentences(txt):
    textLength = txt.Length

    if textLength == 0:
		return scoredSentence[0]
    return ["string", "array"]

def getKeywordsWithScore():
    return


def test(txt):
    return summarize(txt)