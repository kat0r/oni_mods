using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PeterHan;
using PeterHan.PLib.UI;

namespace ImprovedFilteredStorage
{
    public class ImprovedTreeFilterableSideScreen : SideScreenContent
    {
        //private GameObject target;
        private ImprovedTreeFilterable improvedTreeFilterable;

        private char OnValidate_TextField(string text, int charIndex, char addedChar)
        {
            if (Char.IsDigit(addedChar)/* && float.TryParse(text, out float _)*/)
                return addedChar;

            return '\0';
        }
        private void OnTextChanged_TextField(Tag tag, string text)
        {
            if (float.TryParse(text, out float value))
            {
                improvedTreeFilterable.AddTagToFilter(tag, value);
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
                baseLayout.Params = new BoxLayoutParams()
                {
                    Margin = new RectOffset(4, 4, 4, 4),
                    Direction = PanelDirection.Vertical,
                    Alignment = TextAnchor.MiddleLeft,
                    //Spacing = 8, 
                };

            ContentContainer = gameObject;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
        }

        public void Refresh()
        {
            ClearContent();
            if (ContentContainer == null)
                return;

            if (improvedTreeFilterable != null && improvedTreeFilterable.Enabled)
            {
                var rootPanel = new PPanel("rootPanel") { /*BackColor = Color.green,*/ FlexSize = Vector2.left, Spacing = 2, Alignment = TextAnchor.MiddleLeft };

                if (improvedTreeFilterable.GetAcceptedElements().Count == 0)
                {
                    var noContent = new PLabel("nocontent")
                    {
                        Text = Strings.NOCONTENT,
                        TextStyle = PUITuning.Fonts.TextDarkStyle,
                    };
                    rootPanel = rootPanel.AddChild(noContent);
                }
                else
                {
                    foreach (var acceptedTag in improvedTreeFilterable.GetAcceptedElements())
                    {
                        var elementRoot = new PPanel("panel" + acceptedTag.Key.ProperNameStripLink())
                        {
                            Direction = PanelDirection.Horizontal,
                            Margin = new RectOffset(5, 5, 0, 0),
                            Alignment = TextAnchor.UpperLeft,
                            Spacing = 10,
                            FlexSize = Vector2.left,
                        };

                        var textField = new PTextField("amount" + acceptedTag.Key.ProperNameStripLink())
                        {
                            MinWidth = 50,
                            Text = acceptedTag.Value.ToString(),
                            OnValidate = OnValidate_TextField,
                            OnTextChanged = (GameObject _, string text) => OnTextChanged_TextField(acceptedTag.Key, text),
                            FlexSize = Vector2.left,
                        };

                        elementRoot = elementRoot.AddChild(textField);

                        var label = new PLabel("label" + acceptedTag.Key.ProperNameStripLink())
                        {
                            Text = acceptedTag.Key.ProperNameStripLink(),
                            TextStyle = PUITuning.Fonts.TextDarkStyle,
                            FlexSize = Vector2.left,
                        };
                        elementRoot = elementRoot.AddChild(label);
                        rootPanel = rootPanel.AddChild(elementRoot);
                    }
                }
                rootPanel.AddTo(ContentContainer);
                //PUIUtils.DebugObjectTree(ContentContainer);
            }
        }
        private void ClearContent()
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
                GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        public override void ClearTarget()
        {
            if (improvedTreeFilterable != null)
                improvedTreeFilterable.OnUpdateFilters -= ImprovedTreeFilterable_OnUpdateFilters;

            improvedTreeFilterable = null;

            base.ClearTarget();
        }

        public override void SetTarget(GameObject targetGO)
        {
            this.gameObject.SetActive(true);
            improvedTreeFilterable = targetGO.GetComponent<ImprovedTreeFilterable>();
            if (improvedTreeFilterable != null)
                improvedTreeFilterable.OnUpdateFilters += ImprovedTreeFilterable_OnUpdateFilters;

            Refresh();
        }

        private void ImprovedTreeFilterable_OnUpdateFilters(object sender, EventArgs e)
        {
            Refresh();
        }

        public override string GetTitle()
        {
            return "Partitioned Storage";
        }
        //public override int GetSideScreenSortOrder() => 1;

        public override bool IsValidForTarget(GameObject target)
        {
            ImprovedTreeFilterable improvedTreeFilterable = target.GetComponent<ImprovedTreeFilterable>();
            return improvedTreeFilterable != null && improvedTreeFilterable.Enabled;
        }
    }
}
