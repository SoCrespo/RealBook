﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class SongParser
    {
        public Song Parse(string songUrl)
        {
            var components = SplitSongUrl(songUrl);

            if (components.Length != 6)
            {
                throw new Exception($"Song url has {components.Length} components - expecting 6");
            }

            return new Song
            {
                SongTitle = components[0],
                Composer = components[1],
                Style = components[2],
                KeySignature = ToKeySignature(components[3]),
                // components[4] is ignored
                SongChart = ToSongChart(components[5]),
            };
        }

        private string[] SplitSongUrl(string songUrl)
        {
            if (!songUrl.StartsWith("irealbook://"))
            {
                throw new Exception($"Song Uri has invalid scheme: {songUrl}");
            }

            return songUrl
                .Substring("irealbook://".Length)
                .Split("=".ToCharArray());
        }

        private KeySignature ToKeySignature(string component)
        {
            switch (component)
            {
                case "C": return KeySignature.CMajor;
                case "C#": return KeySignature.DFlatMajor;
                case "Db": return KeySignature.DFlatMajor;
                case "D": return KeySignature.DMajor;
                case "D#": return KeySignature.EFlatMajor;
                case "Eb": return KeySignature.EFlatMajor;
                case "E": return KeySignature.EMajor;
                case "F": return KeySignature.FMajor;
                case "F#": return KeySignature.GFlatMajor;
                case "Gb": return KeySignature.GFlatMajor;
                case "G": return KeySignature.GMajor;
                case "G#": return KeySignature.AFlatMajor;
                case "Ab": return KeySignature.AFlatMajor;
                case "A": return KeySignature.AMajor;
                case "A#": return KeySignature.BFlatMajor;
                case "Bb": return KeySignature.BFlatMajor;
                case "B": return KeySignature.BMajor;
                case "A-": return KeySignature.AMinor;
                case "A#-": return KeySignature.BFlatMinor;
                case "Bb-": return KeySignature.BFlatMinor;
                case "B-": return KeySignature.BMinor;
                case "C-": return KeySignature.CMinor;
                case "C#-": return KeySignature.CSharpMinor;
                case "Db-": return KeySignature.CSharpMinor;
                case "D-": return KeySignature.DMinor;
                case "D#-": return KeySignature.EFlatMinor;
                case "Eb-": return KeySignature.EFlatMinor;
                case "E-": return KeySignature.EMinor;
                case "F-": return KeySignature.FMinor;
                case "F#-": return KeySignature.FSharpMinor;
                case "Gb-": return KeySignature.FSharpMinor;
                case "G-": return KeySignature.GMinor;
                case "G#-": return KeySignature.GSharpMinor;
                case "Ab-": return KeySignature.GSharpMinor;
                default: throw new Exception($"Unknown key signature {component}");
            };
        }

        private SongChart ToSongChart(string component)
        {
            var tokens = ToTokens(component).ToArray();
            return new SongChart
            {
                Tokens = tokens,
            };
        }

        private IEnumerable<Token> ToTokens(string component)
        {
            if (component.Length == 0)
            {
                return new Token[0];
            }

            var matchingTokenizers = Tokenizers
                .Where(tokenizer => component.StartsWith(tokenizer.Key));
            
            if (!matchingTokenizers.Any())
            {
                return ToTokens(component.Substring(1))
                    .Prepend(new Token { Type = TokenType.Unknown, Symbol = component.Substring(0, 1) });
            }
            else
            {
                var longestMatchingTokenizer = matchingTokenizers
                    .OrderByDescending(tokenizer => tokenizer.Key.Length)
                    .First();

                var thisComponent = component.Substring(0, longestMatchingTokenizer.Key.Length);
                var nextComponent = component.Substring(longestMatchingTokenizer.Key.Length);
                return ToTokens(nextComponent)
                    .Prepend(longestMatchingTokenizer.Value(thisComponent));
            }
        }

        private static Token BarLine(string symbol) => new Token { Type = TokenType.BarLine, Symbol = symbol };
        private static Token TimeSignature(string symbol) => new Token { Type = TokenType.TimeSignature, Symbol = symbol };
        private static Token RehearsalMark(string symbol) => new Token { Type = TokenType.RehearsalMark, Symbol = symbol };
        private static Token Ending(string symbol) => new Token { Type = TokenType.Ending, Symbol = symbol };
        private static Token StaffText(string symbol) => new Token { Type = TokenType.StaffText, Symbol = symbol };
        private static Token Repeat(string symbol) => new Token { Type = TokenType.Repeat, Symbol = symbol };
        private static Token VerticalSpace(string symbol) => new Token { Type = TokenType.VerticalSpace, Symbol = symbol };
        private static Token Chord(string symbol) => new Token { Type = TokenType.Chord, Symbol = symbol };
        private static Token ChordQuality(string symbol) => new Token { Type = TokenType.ChordQuality, Symbol = symbol };
        private static Token AlternateChord(string symbol) => new Token { Type = TokenType.AlternateChord, Symbol = symbol };
        private static Token NoChord(string symbol) => new Token { Type = TokenType.NoChord, Symbol = symbol };
        private static Token RepeatSymbol(string symbol) => new Token { Type = TokenType.RepeatSymbol, Symbol = symbol };
        private static Token ChordSize(string symbol) => new Token { Type = TokenType.ChordSize, Symbol = symbol };
        private static Token Divider(string symbol) => new Token { Type = TokenType.Divider, Symbol = symbol };
        private static Token EmptyCell(string symbol) => new Token { Type = TokenType.EmptyCell, Symbol = symbol };

        private static Dictionary<string, Func<string, Token>> Tokenizers = new Dictionary<string, Func<string, Token>>
        {
            // Bar Lines
            { "|", BarLine }, // single bar line
            { "[", BarLine }, // opening double bar line
            { "]", BarLine }, // closing double bar line
            { "{", BarLine }, // opening repeat bar line
            { "}", BarLine }, // closing repeat bar line
            { "Z", BarLine }, // Final thick double bar line

            // Time signatures: to be placed before a bar line
            { "T44", TimeSignature }, // 4/4
            { "T34", TimeSignature }, // 3/4
            { "T24", TimeSignature }, // 2/4
            { "T54", TimeSignature },  // 5/4
            { "T64", TimeSignature }, // 6/4
            { "T74", TimeSignature }, // 7/4
            { "T22", TimeSignature }, // 2/2
            { "T32", TimeSignature }, // 3/2
            { "T58", TimeSignature }, // 5/8
            { "T68", TimeSignature }, // 6/8
            { "T78", TimeSignature }, // 7/8
            { "T98", TimeSignature }, // 9/8
            { "T12", TimeSignature }, // 12/8

            // Rehearsal Marks
            { "*A", RehearsalMark }, // A section
            { "*B", RehearsalMark }, // B section
            { "*C", RehearsalMark }, // C Section
            { "*D", RehearsalMark }, // D Section
            { "*V", RehearsalMark }, // Verse
            { "*i", RehearsalMark }, // Intro
            { "S", RehearsalMark }, // Segno
            { "Q", RehearsalMark }, // Coda
            { "f", RehearsalMark }, // Fermata

            // Endings
            { "N1", Ending }, // First ending
            { "N2", Ending }, // Second Ending
            { "N3", Ending }, // Third Ending
            { "N0", Ending }, // No text Ending

            // Staff Text
            // Staff text appears under the current chords and needs to be enclosed in angle brackets
            // <Some staff text>
            // You can move the text upwards relative to the current chord by adding a * followed by two digit number between 00 (below the system) and 74 (above the system):
            // <*36Some raised staff text>
            // There are a number of specific staff text phrases that are recognized by the player in iReal Pro:
            { "<D.C. al Coda>", StaffText },
            { "<D.C. al Fine>", StaffText },
            { "<D.C. al 1st End.>", StaffText },
            { "<D.C. al 2nd End.>", StaffText },
            { "<D.C. al 3rd End.>", StaffText },
            { "<D.S. al Coda>", StaffText },
            { "<D.S. al Fine>", StaffText },
            { "<D.S. al 1st End.>", StaffText },
            { "<D.S. al 2nd End.>", StaffText },
            { "<D.S. al 3rd End.>", StaffText },
            { "<Fine>", StaffText },

            // TODO: Optimistically just ignore anything else,
            // hopefully no collisions with other symbols
            { "<", StaffText },
            { ">", StaffText },

            // If you have a section of the song that is enclosed in repeat bar lines { } you can add in the staff text a number followed by ‘x’ to indicate that the section should repeat that number of times instead of the default 2 times:
            // "<8x>",
            // TODO: Just enumerate?
            { "<1x>", Repeat },
            { "<2x>", Repeat },
            { "<3x>", Repeat },
            { "<4x>", Repeat },
            { "<5x>", Repeat },
            { "<6x>", Repeat },
            { "<7x>", Repeat },
            { "<8x>", Repeat },

            // Vertical Space
            // You can add a small amount of vertical space between staves by adding between 1 and 3 ‘Y’ at the beginning of a system
            { "Y", VerticalSpace },
            { "YY", VerticalSpace },
            { "YYY", VerticalSpace },

            // Chords
            //
            // Chord symbol format: Root + an optional chord quality + an optional inversion

            // For example just a root:
            // C
            // or a root plus a chord quality
            // C-7
            // or a root plus in inversion inversion
            // C/E
            // or a root plus a quality plus an inversion
            // C-7/Bb

            // All valid roots and inversions:
            { "C", Chord },
            { "C#", Chord },
            { "Db", Chord },
            { "D", Chord },
            { "D#", Chord },
            { "Eb", Chord },
            { "E", Chord },
            { "F", Chord },
            { "F#", Chord },
            { "Gb", Chord },
            { "G", Chord },
            { "G#", Chord },
            { "Ab", Chord },
            { "A", Chord },
            { "A#", Chord },
            { "Bb", Chord },
            { "B", Chord },
 
            // All valid qualities:
            { "5", ChordQuality },
            { "2", ChordQuality },
            { "add9",  ChordQuality },
            { "+",  ChordQuality },
            { "o",  ChordQuality },
            { "h",  ChordQuality },
            { "sus", ChordQuality },
            { "^", ChordQuality },
            { "-", ChordQuality },
            { "^7", ChordQuality },
            { "-7", ChordQuality },
            { "7", ChordQuality },
            { "7sus", ChordQuality },
            { "h7", ChordQuality },
            { "o7", ChordQuality },
            { "^9", ChordQuality },
            { "^13", ChordQuality },
            { "6", ChordQuality },
            { "69", ChordQuality },
            { "^7#11", ChordQuality },
            { "^9#11", ChordQuality },
            { "^7#5", ChordQuality },
            { "-6", ChordQuality },
            { "-69", ChordQuality },
            { "-^7", ChordQuality },
            { "-^9", ChordQuality },
            { "-9", ChordQuality },
            { "-11", ChordQuality },
            { "-7b5", ChordQuality },
            { "h9", ChordQuality },
            { "-b6", ChordQuality },
            { "-#5", ChordQuality },
            { "9", ChordQuality },
            { "7b9", ChordQuality },
            { "7#9", ChordQuality },
            { "7#11", ChordQuality },
            { "7b5", ChordQuality },
            { "7#5", ChordQuality },
            { "9#11", ChordQuality },
            { "9b5", ChordQuality },
            { "9#5", ChordQuality },
            { "7b13", ChordQuality },
            { "7#9#5", ChordQuality },
            { "7#9b5", ChordQuality },
            { "7#9#11", ChordQuality },
            { "7b9#11", ChordQuality },
            { "7b9b5", ChordQuality },
            { "7b9#5", ChordQuality },
            { "7b9#9", ChordQuality },
            { "7b9b13", ChordQuality },
            { "7alt", ChordQuality },
            { "13", ChordQuality },
            { "13#11", ChordQuality },
            { "13b9", ChordQuality },
            { "13#9", ChordQuality },
            { "7b9sus", ChordQuality },
            { "7susadd3", ChordQuality },
            { "9sus", ChordQuality },
            { "13sus", ChordQuality },
            { "7b13sus", ChordQuality },
            { "11", ChordQuality },

            // Alternate Chords:
            // iReal Pro can also display smaller “alternate” chords above the regular chords. All the same rules apply for the format of the chord and to mark them as alternate chords you enclose them in round parenthesis:
            // (Db^7/F)
            { "(", AlternateChord },
            { ")", AlternateChord },

            // No Chord: n
            // Adds a N.C. symbol in the chart which makes the player skip harmony and bass for that measure or beats
            { "n", NoChord },

            // Repeat Symbols:
            { "x", RepeatSymbol }, // This is the “Repeat one measure” % symbol and is usually inserted in the middle of an empty measure:
            { "r", RepeatSymbol }, // This is the “Repeat the previous two measures” symbol and is usually inserted across two empty measures:

            // Slash:
            // Sometimes we might want to add slash symbol to indicate that we want to repeat the preceding chord:
            // |C7ppF7|
            { "p", RepeatSymbol },

            // Chord size:
            // When trying to squeeze many chords in one measure you might want to make them narrower.
            // To do this insert an s in the chord progression and all the following chord symbols will be narrower until a l symbol is encountered that restores the normal size.
            { "s", ChordSize },
            { "l", ChordSize },

            // Dividers:
            // One or more space characters are usually used to separate chords but sometimes we want to pack many chords in one measure in which case we use the comma , to separate the chords without adding empty cells to the chord progression.
            { ",", Divider },

            // Empty cell
            //
            { " ", EmptyCell },
        };
    }
}