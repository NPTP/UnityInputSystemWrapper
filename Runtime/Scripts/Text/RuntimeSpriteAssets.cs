#if ISW_TEXTMESHPRO
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// Sprite assets built at runtime, so loaded binding sprites can be written into text as sprite tags.
    /// A sprite asset draws from one texture, so the first texture's group is the asset the text uses and
    /// the rest are its fallbacks.
    /// </summary>
    internal sealed class RuntimeSpriteAssets : IDisposable
    {
        private const string SPRITE_SHADER_NAME = "TextMeshPro/Sprite";

        /// <summary>The asset to put on the text component. Its fallbacks hold the rest.</summary>
        internal TMP_SpriteAsset Primary { get; }

        /// <summary>Everything created here, so it can all be destroyed together.</summary>
        private readonly List<Object> created;

        private bool disposed;

        private RuntimeSpriteAssets(TMP_SpriteAsset primary, List<Object> created)
        {
            Primary = primary;
            this.created = created;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            foreach (Object createdObject in created)
            {
                if (createdObject != null)
                {
                    Object.Destroy(createdObject);
                }
            }

            created.Clear();
        }

        /// <summary>
        /// Sprite assets covering the given sprites, and the name each answers to in a sprite tag. Names
        /// are positional, so two sprites sharing a project name cannot shadow one another. Null when there
        /// is nothing to show.
        /// </summary>
        internal static RuntimeSpriteAssets Build(IReadOnlyList<Sprite> sprites, out string[] spriteNames)
        {
            spriteNames = new string[sprites.Count];

            // Grouped by texture because a sprite asset draws every character from a single sheet.
            Dictionary<Texture, List<int>> spriteIndicesByTexture = new();
            for (int i = 0; i < sprites.Count; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                {
                    continue;
                }

                spriteNames[i] = $"isw_{i.ToString()}";
                if (!spriteIndicesByTexture.TryGetValue(sprite.texture, out List<int> indices))
                {
                    indices = new List<int>();
                    spriteIndicesByTexture.Add(sprite.texture, indices);
                }

                indices.Add(i);
            }

            if (spriteIndicesByTexture.Count == 0)
            {
                return null;
            }

            List<Object> created = new();
            TMP_SpriteAsset primary = null;

            foreach (KeyValuePair<Texture, List<int>> group in spriteIndicesByTexture)
            {
                TMP_SpriteAsset spriteAsset = CreateSpriteAsset(group.Key, group.Value, sprites, spriteNames, created);
                if (primary == null)
                {
                    primary = spriteAsset;
                    primary.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
                }
                else
                {
                    primary.fallbackSpriteAssets.Add(spriteAsset);
                }
            }

            return new RuntimeSpriteAssets(primary, created);
        }

        private static TMP_SpriteAsset CreateSpriteAsset(Texture texture, List<int> spriteIndices,
            IReadOnlyList<Sprite> sprites, string[] spriteNames, List<Object> created)
        {
            TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            spriteAsset.name = $"ISW Runtime Sprite Asset ({texture.name})";
            spriteAsset.hideFlags = HideFlags.HideAndDontSave;
            spriteAsset.spriteSheet = texture;

            // The legacy list an upgrade would read from, which must not be null.
            spriteAsset.spriteInfoList = new List<TMP_Sprite>();
            created.Add(spriteAsset);

            for (int i = 0; i < spriteIndices.Count; i++)
            {
                Sprite sprite = sprites[spriteIndices[i]];
                Rect textureRect = sprite.textureRect;

                TMP_SpriteGlyph spriteGlyph = new()
                {
                    index = (uint)i,
                    metrics = new GlyphMetrics(textureRect.width, textureRect.height, -sprite.pivot.x,
                        textureRect.height - sprite.pivot.y, textureRect.width),
                    glyphRect = new GlyphRect(textureRect),
                    scale = 1f,
                    sprite = sprite
                };

                spriteAsset.spriteGlyphTable.Add(spriteGlyph);

                // 0xFFFE is the unicode for a sprite addressed by name rather than standing in for a character.
                TMP_SpriteCharacter spriteCharacter = new(0xFFFE, spriteGlyph)
                {
                    name = spriteNames[spriteIndices[i]],
                    scale = 1f
                };

                spriteAsset.spriteCharacterTable.Add(spriteCharacter);
            }

            // The material goes on after the lookups are built: a material on an asset with no version
            // stamp reads as one saved by an older TextMeshPro, which clears the tables to rebuild them.
            spriteAsset.UpdateLookupTables();

            Material material = new(Shader.Find(SPRITE_SHADER_NAME))
            {
                name = spriteAsset.name,
                hideFlags = HideFlags.HideAndDontSave
            };

            material.SetTexture(ShaderUtilities.ID_MainTex, texture);
            spriteAsset.material = material;
            created.Add(material);

            return spriteAsset;
        }
    }
}
#endif
