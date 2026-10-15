using System;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class SegmentAnchorVerificationTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VerifiedAnchorStillRequiresSurroundingPositions(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (var example in new[]
        {
            (Pattern: "*ab?needle??c*tail*", Core: "abQneedleXYc"),
            (Pattern: "ab?needle??c*tail", Core: "abQneedleXYc"),
            (Pattern: "*??needle?xy*tail*", Core: "QQneedleRxy"),
            (Pattern: "*a\nb?needle??c*tail*", Core: "a\nbQneedleXYc"),
            (Pattern: "*\0a?needle??\ud800*tail*", Core: "\0aQneedleXY\ud800"),
            (Pattern: "*" + new string('a', 70) + "?b*tail*", Core: new string('a', 70) + "Qb")
        })
        {
            var matcher = new WildcardMatch(example.Pattern, options);
            string value = example.Core + new string('x', 64) + "tail";
            foreach (string input in new[]
            {
                value, value + "\n", "\n" + value,
                value.Replace('Q', '\n'), value.Replace('X', '\n'),
                value.Replace('b', 'z'), value.Replace('c', 'z'),
                value.Replace("tail", "fail"), value.Replace("needle", "needlf"),
                example.Core.Replace('c', 'z') + "--" + value,
                example.Core.Replace('c', 'z') + "\n" + value,
                new string('x', 64) + value
            })
                ShortStarBoundaryTest.Check(matcher, input, example.Pattern, options, singleline);
        }
    }
}
