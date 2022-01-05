using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using PeterHan.PLib.UI;

namespace ImprovedFilteredStorage
{
    internal class ImprovedTreeFilterableSideScreenRow : KMonoBehaviour
    {
        private LocText locLabel = null;
        private TMPro.TextMeshProUGUI uiAmount = null;

        ImprovedTreeFilterable m_target;
        Tag m_tag;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            var layoutGrp = gameObject.AddOrGet<HorizontalLayoutGroup>();
            layoutGrp.spacing = 3;

            var textField = new PTextField("amount")
            {
                Text = "0",
                MinWidth = 50,
                OnValidate = OnValidate_TextField,
                OnTextChanged = (GameObject _, string text) => OnTextChanged_TextField(text),
            };
            var go_amount = textField.AddTo(layoutGrp.gameObject);

            var label = new PLabel("label")
            {
                Text = "null",
                TextStyle = PUITuning.Fonts.TextDarkStyle,
            };

            var go_label = label.AddTo(layoutGrp.gameObject);

            uiAmount = transform.Find("amount")?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            locLabel = transform.Find("label")?.GetComponentInChildren<LocText>();
        }

        public void SetContent(ImprovedTreeFilterable target, Tag tag, float amount)
        {
            m_target = target;
            m_tag = tag;

            if (locLabel != null)
            {
                locLabel.SetText(tag.ProperNameStripLink());
                locLabel.alignment = TMPro.TextAlignmentOptions.Left;
            }
            if (uiAmount != null)
            {
                //this fixes a weird crash/UX issue, dont ask me why
                uiAmount.text = "0";
                uiAmount.text = amount.ToString();
                uiAmount.SetAllDirty();
            }
        }

        private char OnValidate_TextField(string text, int charIndex, char addedChar)
        {
            if (Char.IsDigit(addedChar)/* && float.TryParse(text, out float _)*/)
                return addedChar;

            return '\0';
        }
        private void OnTextChanged_TextField(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                uiAmount.text = "0";
                uiAmount.SetAllDirty();
                return;
            }

            if (float.TryParse(text, out float value))
            {
                m_target.AddTagToFilter(m_tag, value);
            }
        }
    }
}
