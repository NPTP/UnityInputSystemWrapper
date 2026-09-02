using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Text;
using NUnit.Framework;

namespace NPTP.InputSystemWrapper.Tests
{
    /// <summary>
    /// Covers reading "&lt;isw ...&gt;" elements out of a string. Nothing here touches the input system, so
    /// these run in the editor without a player, a device or an action asset.
    /// </summary>
    public class InlineGlyphTagParserTests
    {
        private static InlineGlyphTag ParseSingle(string text)
        {
            List<InlineGlyphTag> tags = InlineGlyphTagParser.Parse(text);
            Assert.AreEqual(1, tags.Count, $"Expected exactly one glyph in \"{text}\".");
            return tags[0];
        }

        #region The shortest and longest forms

        [Test]
        public void Parse_ActionAlone_TakesTheDefaultsForEverythingElse()
        {
            InlineGlyphTag tag = ParseSingle("Look at this glyph: <isw action=\"Fire\">. Isn't it cool?");

            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(string.Empty, tag.ActionMapName);
            Assert.AreEqual(InlineGlyphType.Sprite, tag.GlyphType);
            Assert.AreEqual(0, tag.PlayerID);
            Assert.AreEqual(CompositePart.DontIsolatePart, tag.CompositePart);
            Assert.AreEqual(0, tag.UIIndex);
        }

        [Test]
        public void Parse_EveryAttribute_ReadsEachOne()
        {
            InlineGlyphTag tag = ParseSingle(
                "Look at this glyph: <isw type=\"sprite\" player=1 action=\"Gameplay.Fire\" composite=\"positive\" index=2>. Isn't it cool?");

            Assert.AreEqual(InlineGlyphType.Sprite, tag.GlyphType);
            Assert.AreEqual(1, tag.PlayerID);
            Assert.AreEqual("Gameplay", tag.ActionMapName);
            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(CompositePart.Positive, tag.CompositePart);
            Assert.AreEqual(2, tag.UIIndex);
        }

        [Test]
        public void Parse_TypeText_ShowsTheDisplayNameInsteadOfTheSprite()
        {
            Assert.AreEqual(InlineGlyphType.Text, ParseSingle("<isw type=\"text\" action=\"Fire\">").GlyphType);
        }

        #endregion

        #region How an element may be written

        [Test]
        public void Parse_AttributesInAnyOrder_ReadsTheSameElement()
        {
            InlineGlyphTag tag = ParseSingle("<isw index=3 action=\"Fire\" player=2 composite=\"up\" type=\"text\">");

            Assert.AreEqual(3, tag.UIIndex);
            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(2, tag.PlayerID);
            Assert.AreEqual(CompositePart.Up, tag.CompositePart);
            Assert.AreEqual(InlineGlyphType.Text, tag.GlyphType);
        }

        [Test]
        public void Parse_UnquotedValues_AreReadAsWritten()
        {
            InlineGlyphTag tag = ParseSingle("<isw action=Gameplay.Fire player=2 composite=down index=1>");

            Assert.AreEqual("Gameplay", tag.ActionMapName);
            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(2, tag.PlayerID);
            Assert.AreEqual(CompositePart.Down, tag.CompositePart);
            Assert.AreEqual(1, tag.UIIndex);
        }

        [Test]
        public void Parse_SingleQuotedValues_AreReadTheSameAsDoubleQuoted()
        {
            InlineGlyphTag tag = ParseSingle("<isw action='Gameplay.Fire' composite='left'>");

            Assert.AreEqual("Gameplay", tag.ActionMapName);
            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(CompositePart.Left, tag.CompositePart);
        }

        [Test]
        public void Parse_NamesInAnyCase_AreStillUnderstood()
        {
            InlineGlyphTag tag = ParseSingle("<ISW ACTION=\"Fire\" COMPOSITE=\"UP\" TYPE=\"TEXT\">");

            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(CompositePart.Up, tag.CompositePart);
            Assert.AreEqual(InlineGlyphType.Text, tag.GlyphType);
        }

        [Test]
        public void Parse_SpacesAroundTheAttributes_AreIgnored()
        {
            InlineGlyphTag tag = ParseSingle("<isw   action = \"Gameplay.Fire\"    index =  4   >");

            Assert.AreEqual("Gameplay", tag.ActionMapName);
            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(4, tag.UIIndex);
        }

        [Test]
        public void Parse_SpacesAroundTheActionNames_AreTrimmed()
        {
            InlineGlyphTag tag = ParseSingle("<isw action=\" Gameplay . Fire \">");

            Assert.AreEqual("Gameplay", tag.ActionMapName);
            Assert.AreEqual("Fire", tag.ActionName);
        }

        #endregion

        #region Where an element sits in the line

        [Test]
        public void Parse_TagSpan_CoversExactlyTheElement()
        {
            const string text = "Press <isw action=\"Fire\"> to shoot.";
            InlineGlyphTag tag = ParseSingle(text);

            Assert.AreEqual("<isw action=\"Fire\">", text.Substring(tag.StartIndex, tag.Length));
        }

        [Test]
        public void Parse_SeveralElements_ComeBackInTheOrderTheyAppear()
        {
            const string text = "<isw action=\"Jump\"> then <isw action=\"Fire\"> then <isw action=\"Crouch\">";
            List<InlineGlyphTag> tags = InlineGlyphTagParser.Parse(text);

            Assert.AreEqual(3, tags.Count);
            Assert.AreEqual("Jump", tags[0].ActionName);
            Assert.AreEqual("Fire", tags[1].ActionName);
            Assert.AreEqual("Crouch", tags[2].ActionName);

            foreach (InlineGlyphTag tag in tags)
            {
                Assert.AreEqual("<isw", text.Substring(tag.StartIndex, 4));
                Assert.AreEqual('>', text[tag.StartIndex + tag.Length - 1]);
            }
        }

        [Test]
        public void Parse_ClosingBracketInsideAValue_DoesNotEndTheElementEarly()
        {
            const string text = "<isw action=\"Fi>re\" index=1>";
            InlineGlyphTag tag = ParseSingle(text);

            Assert.AreEqual("Fi>re", tag.ActionName);
            Assert.AreEqual(1, tag.UIIndex);
            Assert.AreEqual(text.Length, tag.Length);
        }

        #endregion

        #region What is not an element

        [Test]
        public void Parse_NoElements_FindsNothing()
        {
            Assert.IsEmpty(InlineGlyphTagParser.Parse("Just a line of text with no glyphs in it."));
        }

        [Test]
        public void Parse_ALongerNameStartingTheSameWay_IsNotAnElement()
        {
            Assert.IsEmpty(InlineGlyphTagParser.Parse("<iswitch action=\"Fire\">"));
        }

        [Test]
        public void Parse_ALongerNameFollowedByARealElement_StillFindsTheRealOne()
        {
            InlineGlyphTag tag = ParseSingle("<iswitch> and <isw action=\"Fire\">");
            Assert.AreEqual("Fire", tag.ActionName);
        }

        [Test]
        public void Parse_NoAction_LeavesTheElementInTheText()
        {
            Assert.IsEmpty(InlineGlyphTagParser.Parse("<isw type=\"sprite\" player=1>"));
        }

        [Test]
        public void Parse_ElementNeverClosed_FindsNothingAndDoesNotHang()
        {
            Assert.IsEmpty(InlineGlyphTagParser.Parse("Press <isw action=\"Fire\" to shoot."));
        }

        [Test]
        public void Parse_OtherRichTextElements_AreLeftAlone()
        {
            InlineGlyphTag tag = ParseSingle("<b>Press <isw action=\"Fire\"></b>");
            Assert.AreEqual("Fire", tag.ActionName);
        }

        #endregion

        #region Values that make no sense

        [Test]
        public void Parse_PlayerThatIsNotANumber_FallsBackToTheFirstPlayer()
        {
            Assert.AreEqual(0, ParseSingle("<isw action=\"Fire\" player=\"two\">").PlayerID);
        }

        [Test]
        public void Parse_NegativeIndex_FallsBackToTheFirstBinding()
        {
            Assert.AreEqual(0, ParseSingle("<isw action=\"Fire\" index=-3>").UIIndex);
        }

        [Test]
        public void Parse_CompositePartThatIsNotOne_ShowsTheWholeBinding()
        {
            Assert.AreEqual(CompositePart.DontIsolatePart, ParseSingle("<isw action=\"Fire\" composite=\"sideways\">").CompositePart);
        }

        [Test]
        public void Parse_TypeThatIsNotOne_ShowsTheSprite()
        {
            Assert.AreEqual(InlineGlyphType.Sprite, ParseSingle("<isw action=\"Fire\" type=\"hologram\">").GlyphType);
        }

        [Test]
        public void Parse_AttributeItDoesNotKnow_IsIgnoredAndTheRestIsRead()
        {
            InlineGlyphTag tag = ParseSingle("<isw action=\"Fire\" colour=\"red\" index=2>");

            Assert.AreEqual("Fire", tag.ActionName);
            Assert.AreEqual(2, tag.UIIndex);
        }

        #endregion

        #region Whether a line is worth parsing

        [Test]
        public void ContainsTag_LineWithAnElement_IsWorthParsing()
        {
            Assert.IsTrue(InlineGlyphTagParser.ContainsTag("Press <isw action=\"Fire\"> to shoot."));
        }

        [Test]
        public void ContainsTag_LineWithout_IsNot()
        {
            Assert.IsFalse(InlineGlyphTagParser.ContainsTag("Press the button to shoot."));
        }

        [Test]
        public void ContainsTag_NothingAtAll_IsNot()
        {
            Assert.IsFalse(InlineGlyphTagParser.ContainsTag(null));
            Assert.IsFalse(InlineGlyphTagParser.ContainsTag(string.Empty));
        }

        [Test]
        public void Parse_NothingAtAll_FindsNothing()
        {
            Assert.IsEmpty(InlineGlyphTagParser.Parse(null));
            Assert.IsEmpty(InlineGlyphTagParser.Parse(string.Empty));
        }

        #endregion
    }
}
