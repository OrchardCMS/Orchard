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
                .Token("SumInt:*", T("SumInt:<Tokenized text>"), T("Sum each element in the list by applying the tokenized text."))
                .Token("SumFloat:*", T("SumFloat:<Tokenized text>"), T("Sum each element in the list by applying the tokenized text."))
                .Token("SumDecimal:*", T("SumDecimal:<Tokenized text>"), T("Sum each element in the list by applying the tokenized text."))
                .Token("First:*", T("First:<Tokenized text>"), T("Apply the tokenized text to the first element."))
                .Token("Last:*", T("Last:<Tokenized text>"), T("Apply the tokenized text to the last element."))
                .Token("Count", T("Count"), T("Get the list count."))
                ;
	    }
	 
	    public void Evaluate(EvaluateContext context) {
            context.For<IList<IContent>>("List", () => new List<IContent>())
                    .Token( // {List.ForEach:<string>[,<separator>]}
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
	                    return String.Join(separator, collection.Select(content => _tokenizer().Replace(text, new {content}, new ReplaceOptions {Encoding = ReplaceOptions.NoEncode})));
	                })
                    .Token( // {List.SumInt:<string>}
	                token => {
	                    if (token.StartsWith("SumInt:", StringComparison.OrdinalIgnoreCase)) {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumInt:".Length));
	                    }
	                    return null;
	                },
	                (token, collection) => collection.Sum(i => long.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
                    .Token( // {List.SumFloat:<string>}
	                token => {
	                    if (token.StartsWith("SumFloat:", StringComparison.OrdinalIgnoreCase)) {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumFloat:".Length));
	                    }
	                    return null;
	                },
	                (token, collection) => collection.Sum(i => double.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
	                .Token( // {List.SumDecimal:<string>}
	                token => {
	                    if (token.StartsWith("SumDecimal:", StringComparison.OrdinalIgnoreCase)) {
	                        // html decode to stop double encoding.
	                        return HttpUtility.HtmlDecode(token.Substring("SumDecimal:".Length));
	                    }
	                    return null;
	                },
                    (token, collection) => collection.Sum(i => decimal.Parse(_tokenizer().Replace(token, new { Content = i }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }))))
                    .Token( // {List.First:<string>}
                    token => {
                        if (token.StartsWith("First:", StringComparison.OrdinalIgnoreCase)) {
                            // html decode to stop double encoding.
                            return HttpUtility.HtmlDecode(token.Substring("First:".Length));
                        }
                        return null;
                    },
                    (token, list) => list.Any() ? _tokenizer().Replace(token, new { Content = list.First() }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }) : String.Empty)
                    .Token( // {List.Last:<string>}
                    token => {
                        if (token.StartsWith("Last:", StringComparison.OrdinalIgnoreCase)) {
                            // html decode to stop double encoding.
                            return HttpUtility.HtmlDecode(token.Substring("Last:".Length));
                        }
                        return null;
                    },
                    (token, list) => list.Any() ? _tokenizer().Replace(token, new { Content = list.Last() }, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }) : String.Empty)
                    .Token( // {List.Count}
                    "Count",
                    list => list.Count)
	            ;
	    }
	}
}