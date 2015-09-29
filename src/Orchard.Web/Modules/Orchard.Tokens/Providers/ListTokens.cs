using System;
using System.Collections.Generic;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Localization;
using System.Linq;
	 
namespace Orchard.Tokens.Providers {
	 
	public class ListTokens : ITokenProvider {
	    private readonly Func<ITokenizer> _tokenizer;
	 
	    public ListTokens(Func<ITokenizer> tokenizer) {
	        _tokenizer = tokenizer;
	        T = NullLocalizer.Instance;
	    }
	 
	    public Localizer T { get; set; }
	 
	    public void Describe(DescribeContext context) {
	        context.For("List", T("Lists"), T("Handles lists of Content Items"))
                .Token("Join:*", T("Join:<Tokenized text>[,<separator>]"), T("Join each element in the list by applying the tokenized text and concatenating the output with the optional separator."))
                .Token("Sum:*", T("Sum:<Tokenized text>"), T("Sum each element in the list by applying the tokenized text."))
                .Token("First:*", T("First:<Tokenized text>"), T("Apply the tokenized text to the first element."))
                .Token("Last:*", T("Last:<Tokenized text>"), T("Apply the tokenized text to the last element."))
                .Token("Count", T("Count"), T("Get the list count."))
                ;
	    }
	 
	    public void Evaluate(EvaluateContext context) {
			Func<string, IContent, string> getValue = (t, i) => _tokenizer().Replace(t, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

		    context.For<IList<IContent>>("List", () => new List<IContent>())
			    .Token( // {List.Join:<string>[,<separator>]}
				    token => {
					    if (token.StartsWith("Join:", StringComparison.OrdinalIgnoreCase)) {
						    // html decode to stop double encoding.
						    return HttpUtility.HtmlDecode(token.Substring("Join:".Length));
					    }
					    return null;
				    },
				    (token, collection) => {
					    if (String.IsNullOrEmpty(token)) {
						    return String.Empty;
					    }
					    // Split the params to get the tokenized text and optional separator.
					    var index = token.IndexOf(',');
					    var text = index == -1 ? token : token.Substring(0, index);
					    if (String.IsNullOrEmpty(text)) {
						    return String.Empty;
					    }
					    var separator = index == -1 ? String.Empty : token.Substring(index + 1);
					    return String.Join(separator, collection.Select(content => getValue(text, content)));
				    })
			    .Token( // {List.Sum:<string>}
				    token => {
					    if (token.StartsWith("Sum:", StringComparison.OrdinalIgnoreCase)) {
						    // html decode to stop double encoding.
						    return HttpUtility.HtmlDecode(token.Substring("Sum:".Length));
					    }
					    return null;
				    },
				    (token, collection) => {
					    if (!collection.Any()) return "0";

					    // try long.
					    long longValue;
					    if (long.TryParse(collection.Select(i => getValue(token, i)).First(), out longValue)) {
						    return collection.Sum(i => long.Parse(getValue(token, i)));
					    }
					    // try float.
					    float floatValue;
					    if (float.TryParse(collection.Select(i => getValue(token, i)).First(), out floatValue)) {
						    return collection.Sum(i => float.Parse(getValue(token, i)));
					    }
					    // try decimal.
					    decimal decimalValue;
					    if (decimal.TryParse(collection.Select(i => getValue(token, i)).First(), out decimalValue)) {
						    return collection.Sum(i => decimal.Parse(getValue(token, i)));
					    }
					    return "";
				    })
			    .Token( // {List.First:<string>}
				    token => {
					    if (token.StartsWith("First:", StringComparison.OrdinalIgnoreCase)) {
						    // html decode to stop double encoding.
						    return HttpUtility.HtmlDecode(token.Substring("First:".Length));
					    }
					    return null;
				    },
				    (token, list) => list.Any() ? list.Select(i => getValue(token, i)).First() : String.Empty)
			    .Token( // {List.Last:<string>}
				    token => {
					    if (token.StartsWith("Last:", StringComparison.OrdinalIgnoreCase)) {
						    // html decode to stop double encoding.
						    return HttpUtility.HtmlDecode(token.Substring("Last:".Length));
					    }
					    return null;
				    },
				    (token, list) => list.Any() ? list.Select(i => getValue(token, i)).Last() : String.Empty)
			    .Token( // {List.Count}
				    "Count",
				    list => list.Count)
			    ;
	    }
	}
}