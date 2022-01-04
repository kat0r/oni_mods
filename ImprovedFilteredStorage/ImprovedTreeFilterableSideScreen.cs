using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PeterHan;
using PeterHan.PLib.UI;
using UnityEngine.UI;

namespace ImprovedFilteredStorage
{
    public class ImprovedTreeFilterableSideScreen : SideScreenContent
    {
        //private GameObject target;
        private ImprovedTreeFilterable improvedTreeFilterable;

        private ImprovedTreeFilterableSideScreenRow rowPrefab;
        private UIPool<ImprovedTreeFilterableSideScreenRow> rowPool;
        private Dictionary<Tag, ImprovedTreeFilterableSideScreenRow> addedRows = new Dictionary<Tag, ImprovedTreeFilterableSideScreenRow>();

        private GameObject goNoContent;


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
            var rootpanel = new PPanel("rootPanel")
            {
                Direction = PanelDirection.Vertical,
                Spacing = 2,
            };
            ContentContainer = rootpanel.AddTo(gameObject);

            var noContent = new PLabel("nocontent")
            {
                Text = Strings.NOCONTENT,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
            };
            goNoContent = noContent.AddTo(ContentContainer);
            goNoContent.SetActive(false);

            var go = new GameObject("prefab");
            rowPrefab = go.AddOrGet<ImprovedTreeFilterableSideScreenRow>();
            DestroyImmediate(rowPrefab.transform.Find("amount").gameObject);
            DestroyImmediate(rowPrefab.transform.Find("label").gameObject);
            rowPool = new UIPool<ImprovedTreeFilterableSideScreenRow>(rowPrefab);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
        }

        public void Refresh()
        {
            //clicking the "all" button in TreeFilterable calls refresh n amount of times, so completely rebuilding is slow as fuck

            if (ContentContainer == null || rowPool == null)
                return;

            if (improvedTreeFilterable != null && improvedTreeFilterable.Enabled)
            {
                if (improvedTreeFilterable.GetAcceptedElements().Count == 0)
                {
                    ClearContent();
                    goNoContent.SetActive(true);
                }
                else
                {
                    goNoContent.SetActive(false);
                    foreach (var acceptedTag in improvedTreeFilterable.GetAcceptedElements().OrderBy(x => x.Key.ProperNameStripLink()))
                    {
                        if (!addedRows.ContainsKey(acceptedTag.Key)) // is missing, add it
                        {
                            var freeElement = rowPool.GetFreeElement(ContentContainer, true);
                            //freeElement.transform.SetAsLastSibling();
                            freeElement.SetContent(improvedTreeFilterable, acceptedTag.Key, acceptedTag.Value);
                            addedRows[acceptedTag.Key] = freeElement;
                        }
                    }

                    var tagsToRemove = addedRows.Where(x => !improvedTreeFilterable.GetAcceptedElements().ContainsKey(x.Key)).ToList();
                    foreach (var tagToRemove in tagsToRemove)
                    {
                        rowPool.ClearElement(tagToRemove.Value);
                        addedRows.Remove(tagToRemove.Key);
                    }
                }
            }
        }
        private void ClearContent()
        {
            addedRows.Clear();

            if (rowPool != null)
                rowPool.ClearAll();
        }

        public override void ClearTarget()
        {
            if (improvedTreeFilterable != null)
                improvedTreeFilterable.OnUpdateFilters -= ImprovedTreeFilterable_OnUpdateFilters;

            improvedTreeFilterable = null;
            ClearContent();

            base.ClearTarget();
        }

        public override void SetTarget(GameObject targetGO)
        {
            this.gameObject.SetActive(true);
            improvedTreeFilterable = targetGO.GetComponent<ImprovedTreeFilterable>();
            if (improvedTreeFilterable != null)
                improvedTreeFilterable.OnUpdateFilters += ImprovedTreeFilterable_OnUpdateFilters;

            ClearContent();
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
